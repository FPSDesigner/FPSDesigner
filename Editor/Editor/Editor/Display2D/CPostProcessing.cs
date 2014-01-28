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
    /// <summary>
    /// The post processing technique allows the applications of different effects
    /// after the world has been displayed, directly by modifying the rendered pixels.
    /// </summary>
    public class CPostProcessor
    {

        /// <summary>
        /// Texture to process, usually the screen captured by CRenderCapture
        /// </summary>
        public Texture2D Input { get; set; }

        protected GraphicsDevice graphicsDevice;
        protected static SpriteBatch spriteBatch;

        /// <summary>
        /// List of the different loaded effects
        /// </summary>
        public Dictionary<string, Effect> effectList = new Dictionary<string, Effect>();

        /// <summary>
        /// Initialize the post processor technique
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice class</param>
        public CPostProcessor(GraphicsDevice graphicsDevice)
        {
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(graphicsDevice);
            this.graphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Load a new post processed effect
        /// </summary>
        /// <param name="effectName">The effect name used as a key in the dictionnary</param>
        /// <param name="effect">The effect to load</param>
        public void LoadEffect(string effectName, Effect effect)
        {
            if (!effectList.ContainsKey(effectName))
                effectList.Add(effectName, effect); 
        }

        /// <summary>
        /// Check if an effect is loaded
        /// </summary>
        /// <param name="effectName">The key name of the effect</param>
        /// <returns>True if the effect is loaded, False otherwise</returns>
        public bool isEffectLoaded(string effectName)
        {
            return effectList.ContainsKey(effectName);
        }

        /// <summary>
        /// Remove an effect from the post processor
        /// </summary>
        /// <param name="effectName"></param>
        public void removeEffect(string effectName)
        {
            effectList.Remove(effectName);
        }

        /// <summary>
        /// Check if at least one effect is loaded
        /// </summary>
        /// <returns>True if at least one effect is loaded, false otherwise</returns>
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

        // Color Filter vars
        public float[] cfColors { get; set; }

        /// <summary>
        /// Draws the input textures using the pixel shade post processor
        /// </summary>
        /// <param name="gaussianBlurStop">Only used internal, so the blur can be drawn twice (horizontally & vertically)</param>
        public virtual void Draw(bool gaussianBlurStop = false)
        {
            if (!gaussianBlurStop && effectList.ContainsKey("GaussianBlur"))
            {
                gbCapture.Begin();
                // Set values for horizontal pass
                effectList["GaussianBlur"].Parameters["Offsets"].SetValue(gboffsetsH);
                effectList["GaussianBlur"].Parameters["Weights"].SetValue(gbweightsH);
            }

            if (effectList.ContainsKey("ColorFilter"))
            {
                effectList["ColorFilter"].Parameters["redPercent"].SetValue(cfColors[0]);
                effectList["ColorFilter"].Parameters["greenPercent"].SetValue(cfColors[1]);
                effectList["ColorFilter"].Parameters["bluePercent"].SetValue(cfColors[2]);
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
            graphicsDevice.BlendState = BlendState.AlphaBlend;

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

    /// <summary>
    /// Render Capture class captures what is being drawn so it can be used to post process effects
    /// </summary>
    public class CRenderCapture
    {
        public RenderTarget2D renderTarget;
        private GraphicsDevice graphicsDevice;


        /// <summary>
        /// Initialize the render capture class
        /// </summary>
        /// <param name="GraphicsDevice">Graphics Device class</param>
        public CRenderCapture(GraphicsDevice GraphicsDevice)
        {
            this.graphicsDevice = GraphicsDevice;
            renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 4, RenderTargetUsage.PreserveContents);
        }

        /// <summary>
        /// Begins the capture with the graphic device
        /// </summary>
        public void Begin()
        {
            graphicsDevice.SetRenderTarget(renderTarget);
        }

        /// <summary>
        /// Stop capturing
        /// </summary>
        public void End()
        {
            graphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Returns what has been captured
        /// </summary>
        /// <returns>The capture</returns>
        public Texture2D GetTexture()
        {
            return renderTarget;
        }
    }
}
