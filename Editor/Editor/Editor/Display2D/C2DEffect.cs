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

using Engine.Game.Script.Embedded;

namespace Engine.Display2D
{
    /// <summary>
    /// A 2D Effects manager
    /// </summary>
    /// 
    /// <remarks>
    /// C2DEffect allows the rendering of different 2D effects used in the game.
    /// Most of theses effects use the post processing (CPostProcessing) technique.
    /// </remarks>
    /// 
    class C2DEffect
    {

        private static SpriteBatch _spriteBatch;
        private static CPostProcessor _postProcessor;
        private static GameTime _actualGameTime;

        public static CRenderCapture _renderCapture;
        public static GraphicsDevice _graphicsDevice;
        public static ContentManager _content;

        public static KeyboardState _kbState;
        public static MouseState _mouseState;

        public static bool isSoftwareEmbedded;
        public static RenderTarget2D renderTarget;

        // fadeEffect
        private static bool _isFading = false;
        private static double _opacityPerMilliSecond;
        private static double _fadeTimeStart;
        private static float _fadeOpacity = 0;
        private static float _fadeTo = 0f;
        private static Color _fadeToColor;
        private static Vector2 _fadeSizeRect;
        private static Vector2 _fadePositionRect;
        private static Texture2D _fadeTexture;
        private static MethodDelegate _fadeCallback;

        // Gaussian Blur Effect
        private static float _gbBlurAmount;
        private static float[] gbWeightsH, gbweightsV;
        private static Vector2[] gboffsetsH, gboffsetsV;
        private static CRenderCapture gbRenderCapture;

        // Underwater effect
        private static Texture2D uwWaterfallTexture;

        // White Pixel (Used to put the weapon aiming)
        private static Texture2D pixel;

        // Private
        private static Dictionary<string, Effect> loadedEffects = new Dictionary<string, Effect>();


        // Scripts:
        public static List<C2DScriptRectangle> ScriptableRectangle = new List<C2DScriptRectangle>();


        /// <summary>
        /// Initialize the class
        /// </summary>
        /// <param name="content">ContentManager class</param>
        /// <param name="graphicsDevice">GraphicsDevice class</param>
        /// <param name="spriteBatch">SpriteBatch class</param>
        /// <param name="postProcessor">CPostProcessor class</param>
        public static void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, CPostProcessor postProcessor, CRenderCapture renderCapture)
        {
            _spriteBatch = spriteBatch;
            _graphicsDevice = graphicsDevice;
            _content = content;
            _postProcessor = postProcessor;
            _renderCapture = renderCapture;

            uwWaterfallTexture = content.Load<Texture2D>("Textures/uw_effect");
            pixel = content.Load<Texture2D>("Textures/Pixel");
        }

        /// <summary>
        /// Creates a new fade effect
        /// </summary>
        /// <param name="fadeTo">Opacity to fade to. 255 = transparent, 0 = opaque</param>
        /// <param name="timeMilliSecs">Duration of the effect</param>
        /// <param name="gameTime">Current GameTime snapshot</param>
        /// <param name="sizeRect">Size of the fade rectangle</param>
        /// <param name="positionRect">Position of the fade rectangle</param>
        /// <param name="fadeToColor">Color to fade to</param>
        /// <param name="callback">Function to be called once the effect is done</param>
        public static void fadeEffect(int fadeTo, int timeMilliSecs, Vector2 sizeRect, Vector2 positionRect, Color fadeToColor, MethodDelegate callback)
        {
            if (_actualGameTime == null)
                return;

            _fadeSizeRect = sizeRect;
            _fadePositionRect = positionRect;
            _fadeToColor = fadeToColor;
            _fadeCallback = callback;

            _isFading = true;
            _fadeTimeStart = _actualGameTime.TotalGameTime.TotalMilliseconds;
            _opacityPerMilliSecond = 255f / timeMilliSecs;
            _fadeTo = fadeTo;
            _fadeOpacity = 255 - _fadeTo;

            _fadeTexture = new Texture2D(_graphicsDevice, 1, 1);

            _fadeTexture.SetData<Color>(new Color[] { Color.White });
        }

        /// <summary>
        /// Creates a new Black and White effect.
        /// </summary>
        public static void BlackAndWhiteEffect()
        {
            _postProcessor.LoadEffect("BlackWhite", GetEffect("BlackWhite_PP"));
        }

        /// <summary>
        /// Creates a colored filtered on the screen (red + green + blue = 1f)
        /// </summary>
        /// <param name="redPercent">Percentage of red color</param>
        /// <param name="greenPercent">Percentage of green color</param>
        /// <param name="bluePercent">Percentage of blue color</param>
        public static void ColorFilterEffect(float redPercent, float greenPercent, float bluePercent)
        {
            _postProcessor.LoadEffect("ColorFilter", GetEffect("ColorFilter_PP"));
            _postProcessor.cfColors = new float[3] { redPercent, greenPercent, bluePercent };
        }

        /// <summary>
        /// Creates an underwater effect, distortion
        /// </summary>
        /// <param name="toggle">Toggles the effect</param>
        public static void UnderwaterEffect(bool toggle)
        {
            bool isEffectLoaded = _postProcessor.isEffectLoaded("UWEffect");

            if (!toggle && isEffectLoaded)
                _postProcessor.removeEffect("UWEffect");
            else if (toggle && !isEffectLoaded)
            {
                _postProcessor.LoadEffect("UWEffect", GetEffect("WaterEffect_PP"));
                _postProcessor.uwWaterfallTexture = uwWaterfallTexture;
            }
        }

        public static void DepthOfFieldEffect(float depth, bool toggle = true)
        {
            bool loaded = _postProcessor.isEffectLoaded("dof");
            if (!toggle && loaded)
            {
                _postProcessor.removeEffect("GaussianBlur");
            }
            else if (toggle && !loaded)
            {

            }
        }


        /// <summary>
        /// Creates a new Gaussian Blur Effect
        /// </summary>
        /// <param name="blurAmount">Intensity of the blur</param>
        /// <param name="toggle">If true, then deactivate it if already activated</param>
        public static void gaussianBlurEffect(float blurAmount, bool toggle = false, string effectFileName = "GaussianBlur_PP")
        {
            if (!toggle && _postProcessor.isEffectLoaded("GaussianBlur"))
            {
                _postProcessor.removeEffect("GaussianBlur");
                return;
            }
            else if (toggle && _postProcessor.isEffectLoaded("GaussianBlur"))
                return;

            _gbBlurAmount = blurAmount;

            _postProcessor.LoadEffect("GaussianBlur", GetEffect(effectFileName));

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

        private static void gaussianCalcSettings(float w, float h, out float[] weights, out Vector2[] offsets)
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

        private static float gaussianFn(float x)
        {
            return (float)((1.0f / Math.Sqrt(2 * Math.PI * _gbBlurAmount * _gbBlurAmount)) * Math.Exp(-(x * x) / (2 * _gbBlurAmount * _gbBlurAmount)));
        }

        /// <summary>
        /// Called at each frame to process code frame-per-frame
        /// </summary>
        /// <param name="gameTime">GameTime snapshot</param>
        public static void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState)
        {
            _actualGameTime = gameTime;
            _kbState = kbState;
            _mouseState = mouseState;
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
        
        /// <summary>
        /// Draw the different effects, frame-per-frame
        /// </summary>
        /// <param name="gameTime">GameTime snapshot</param>
        public static void Draw(GameTime gameTime)
        {
            if (ScriptableRectangle.Any())
            {
                for (int i = 0; i < ScriptableRectangle.Count; i++)
                {
                    if (ScriptableRectangle[i].isActive)
                        _spriteBatch.Draw(ScriptableRectangle[i].Texture, ScriptableRectangle[i].Rectangle, ScriptableRectangle[i].SourceRectangle, ScriptableRectangle[i].Color);
                }
            }
            if (_isFading)
            {
                _spriteBatch.Draw(_fadeTexture, _fadePositionRect, null, new Color(_fadeToColor.R, _fadeToColor.G, _fadeToColor.B, _fadeOpacity/255), 0f, Vector2.Zero, _fadeSizeRect, SpriteEffects.None, 0);
            }

            _spriteBatch.Draw(pixel, new Rectangle(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2, 4, 4), Color.Red);
        }

        /// <summary>
        /// Gets an effect without reloading it everytime
        /// </summary>
        /// <param name="effectName"></param>
        /// <returns></returns>
        private static Effect GetEffect(string effectName)
        {
            if (loadedEffects.ContainsKey(effectName))
                return loadedEffects[effectName];
            else
            {
                Effect eff = _content.Load<Effect>("Effects/" + effectName);
                loadedEffects.Add(effectName, eff);
                return eff;
            }
        }

        // Delegation Method
        // Used for callbacks once an effect is done
        // Example: fade effect
        public delegate void MethodDelegate();

        public static void nullFunction() { }

    }
}
