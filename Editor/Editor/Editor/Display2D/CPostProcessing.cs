using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Editor.Display2D
{
    // Post Processor class
    public class CPostProcessor
    {
        // Pixel shader
        //public Effect Effect { get; protected set; }

        // Texture to process
        public Texture2D Input { get; set; }

        // GraphicsDevice and SpriteBatch for drawing
        protected GraphicsDevice graphicsDevice;
        protected static SpriteBatch spriteBatch;

        public Dictionary<string, Effect> effectList = new Dictionary<string, Effect>();

        public CPostProcessor(GraphicsDevice graphicsDevice)
        {
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(graphicsDevice);
            this.graphicsDevice = graphicsDevice;
        }

        public void LoadEffect(string effectName, Effect effect)
        {
            if (!effectList.ContainsKey(effectName))
                effectList.Add(effectName, effect); 
        }

        public bool isEffectLoaded(string effectName)
        {
            return effectList.ContainsKey(effectName);
        }

        public void removeEffect(string effectName)
        {
            effectList.Remove(effectName);
        }

        public bool isAnyEffectLoaded()
        {
            return (effectList.Count > 0);
        }

        // Gaussian Blur vars
        public float[] gbweightsH { get; set; }
        public float[] gbweightsV { get; set; }
        public Vector2[] gboffsetsH { get; set; }
        public Vector2[] gboffsetsV { get; set; }
        public CRenderCapture gbCapture { get; set; }

        // Draws the input texture using the pixel shader postprocessor
        public virtual void Draw(bool gaussianBlurStop = false)
        {
            if (!gaussianBlurStop && effectList.ContainsKey("GaussianBlur"))
            {
                gbCapture.Begin();
                // Set values for horizontal pass
                effectList["GaussianBlur"].Parameters["Offsets"].SetValue(gboffsetsH);
                effectList["GaussianBlur"].Parameters["Weights"].SetValue(gbweightsH);
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            foreach (KeyValuePair<string, Effect> effectPair in effectList)
            {
                Effect effect = effectPair.Value;
                // Set effect parameters if necessary
                if (effect.Parameters["ScreenWidth"] != null)
                    effect.Parameters["ScreenWidth"].
                    SetValue(graphicsDevice.Viewport.Width);
                if (effect.Parameters["ScreenHeight"] != null)
                    effect.Parameters["ScreenHeight"].
                    SetValue(graphicsDevice.Viewport.Height);

                effect.CurrentTechnique.Passes[0].Apply();
            }

            // Draw the input texture
            graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            spriteBatch.Draw(Input, Vector2.Zero, Color.White);
            spriteBatch.End();

            // Clean up render states changed by the spritebatch
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;

            if (!gaussianBlurStop && effectList.ContainsKey("GaussianBlur"))
            {
                
                gbCapture.End();

                // Get the results of the first pass
                Input = gbCapture.GetTexture();

                // Set values for the vertical pass
                effectList["GaussianBlur"].Parameters["Offsets"].SetValue(gboffsetsV);
                effectList["GaussianBlur"].Parameters["Weights"].SetValue(gbweightsV);

                // Render the final pass
                this.Draw(true);
            }
        }
    }

    // Render Capture class - Capture what is being drawn
    public class CRenderCapture
    {
        RenderTarget2D renderTarget;
        GraphicsDevice graphicsDevice;

        public CRenderCapture(GraphicsDevice GraphicsDevice)
        {
            this.graphicsDevice = GraphicsDevice;
            renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        // Begins capturing from the graphics device
        public void Begin()
        {
            graphicsDevice.SetRenderTarget(renderTarget);
        }

        // Stop capturing
        public void End()
        {
            graphicsDevice.SetRenderTarget(null);
        }

        // Returns what was captured
        public Texture2D GetTexture()
        {
            return renderTarget;
        }
    }
}
