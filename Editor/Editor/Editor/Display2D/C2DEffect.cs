using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Editor.Display2D
{
    class C2DEffect
    {
        // Singleton Code
        private static C2DEffect instance = null;
        private static readonly object myLock = new object();

        private SpriteBatch _spriteBatch;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;
        private CPostProcessor _postProcessor;

        // fadeEffect
        private bool _isFading = false;
        private double _opacityPerMilliSecond;
        private double _fadeTimeStart;
        private float _fadeOpacity = 0;
        private float _fadeTo = 0f;
        private Color _fadeToColor;
        private Vector2 _fadeSizeRect;
        private Vector2 _fadePositionRect;
        private Texture2D _fadeTexture;
        private MethodDelegate _fadeCallback;

        // Gaussin Blur Effect
        private float _gbBlurAmount;
        private float[] gbWeightsH, gbweightsV;
        private Vector2[] gboffsetsH, gboffsetsV;
        private CRenderCapture gbRenderCapture;

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, CPostProcessor postProcessor)
        {
            this._spriteBatch = spriteBatch;
            this._graphicsDevice = graphicsDevice;
            this._content = content;
            this._postProcessor = postProcessor;
        }

        public void fadeEffect(int fadeTo, int timeMilliSecs, GameTime gameTime, Vector2 sizeRect, Vector2 positionRect, Color fadeToColor, MethodDelegate callback)
        {
            this._fadeSizeRect = sizeRect;
            this._fadePositionRect = positionRect;
            this._fadeToColor = fadeToColor;
            this._fadeCallback = callback;

            this._isFading = true;
            this._fadeTimeStart = gameTime.TotalGameTime.TotalMilliseconds;
            this._opacityPerMilliSecond = 255f / timeMilliSecs;
            this._fadeTo = fadeTo;
            this._fadeOpacity = 255 - _fadeTo;

            _fadeTexture = new Texture2D(_graphicsDevice, 1, 1);

            _fadeTexture.SetData<Color>(new Color[] { Color.White });
            
        }

        public void BlackAndWhiteEffect()
        {
            _postProcessor.LoadEffect("BlackWhite", _content.Load<Effect>("Effects/BlackWhite_PP"));
        }


        public void gaussianBlurEffect(float blurAmount)
        {
            this._gbBlurAmount = blurAmount;

            _postProcessor.LoadEffect("GaussianBlur", _content.Load<Effect>("Effects/GaussianBlur_PP"));

            // Calculate weights/offsets for horizontal pass
            gaussianCalcSettings(1.0f / (float)_graphicsDevice.Viewport.Width, 0,
                out gbWeightsH, out gboffsetsH);

            // Calculate weights/offsets for vertical pass
            gaussianCalcSettings(0, 1.0f / (float)_graphicsDevice.Viewport.Height,
                out gbweightsV, out gboffsetsV);

            gbRenderCapture = new CRenderCapture(_graphicsDevice);

            _postProcessor.gbweightsH = gbWeightsH;
            _postProcessor.gboffsetsH = gboffsetsH;
            _postProcessor.gbweightsV = gbweightsV;
            _postProcessor.gboffsetsV = gboffsetsV;
            _postProcessor.gbCapture = gbRenderCapture;
        }

        private void gaussianCalcSettings(float w, float h, out float[] weights, out Vector2[] offsets)
        {
            // 15 Samples
            weights = new float[15];
            offsets = new Vector2[15];

            // Calulate values for center pixel
            weights[0] = gaussianFn(0);
            offsets[0] = new Vector2(0, 0);

            float total = weights[0];

            // Calculate samples in pairs
            for (int i = 0; i < 7; i++)
            {
                // Weight each pair of samples according to Gaussian function
                float weight = gaussianFn(i + 1);
                weights[i * 2 + 1] = weight;
                weights[i * 2 + 2] = weight;
                total += weight * 2;

                // Samples are offset by 1.5 pixels, to make use of
                // filtering halfway between pixels
                float offset = i * 2 + 1.5f;
                Vector2 offsetVec = new Vector2(w, h) * offset;
                offsets[i * 2 + 1] = offsetVec;
                offsets[i * 2 + 2] = -offsetVec;
            }

            // Divide all weights by total so they will add up to 1
            for (int i = 0; i < weights.Length; i++)
                weights[i] /= total;
        }

        private float gaussianFn(float x)
        {
            return (float)((1.0f / Math.Sqrt(2 * Math.PI * _gbBlurAmount * _gbBlurAmount)) * Math.Exp(-(x * x) / (2 * _gbBlurAmount * _gbBlurAmount)));
        }

        public void Update(GameTime gameTime)
        {
            if (_isFading)
            {
                double elapsedMilliSeconds = gameTime.TotalGameTime.TotalMilliseconds - _fadeTimeStart;
                _fadeTimeStart = gameTime.TotalGameTime.TotalMilliseconds;

                if (_fadeTo >= 128)
                    _fadeOpacity += (float)(elapsedMilliSeconds * _opacityPerMilliSecond);
                else
                    _fadeOpacity -= (float)(elapsedMilliSeconds * _opacityPerMilliSecond);

                if (_fadeTo >= 128 && _fadeOpacity >= _fadeTo || _fadeTo < 128 && _fadeOpacity <= _fadeTo)
                {
                    _fadeCallback.DynamicInvoke();
                    _isFading = false;
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (_isFading)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_fadeTexture, _fadePositionRect, null, new Color(_fadeToColor.R, _fadeToColor.G, _fadeToColor.B, _fadeOpacity/255), 0f, Vector2.Zero, _fadeSizeRect, SpriteEffects.None, 0);
                _spriteBatch.End();
            }
        }

        public delegate void MethodDelegate();

        public void nullFunction() { }


        // Singelton Methods
        private C2DEffect() { }
        public static C2DEffect getInstance()
        {
            lock (myLock)
            {
                if (instance == null) instance = new C2DEffect();
                return instance;
            }
        }
    }
}
