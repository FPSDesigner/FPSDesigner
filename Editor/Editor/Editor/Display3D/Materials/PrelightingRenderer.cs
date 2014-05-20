using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Engine.Display3D.Materials
{
    class PrelightingRenderer
    {
        // Normal, depth, and light map render targets
        RenderTarget2D depthTarg;
        RenderTarget2D normalTarg;
        RenderTarget2D lightTarg;

        // Depth/normal effect and light mapping effect
        Effect depthNormalEffect;
        Effect lightingEffect;

        // Point light (sphere) mesh
        Model lightMesh;

        // List of models, lights, and the camera
        public List<CModel> Models { get; set; }
        public List<PPPointLight> Lights { get; set; }
        public CCamera Camera { get; set; }

        GraphicsDevice graphicsDevice;
        int viewWidth = 0, viewHeight = 0;

        // Position and target of the shadowing light
        public Vector3 ShadowLightPosition { get; set; }
        public Vector3 ShadowLightTarget { get; set; }

        // Shadow depth target and depth-texture effect
        RenderTarget2D shadowDepthTarg;
        Effect shadowDepthEffect;

        // Depth texture parameters
        int shadowMapSize = 1024;
        int shadowFarPlane = 1000;

        // Shadow light view and projection
        Matrix shadowView, shadowProjection;

        // Shadow properties
        public bool DoShadowMapping { get; set; }
        public float ShadowMult { get; set; }

        SpriteBatch spriteBatch;
        RenderTarget2D shadowBlurTarg;
        Effect shadowBlurEffect;

        public PrelightingRenderer(GraphicsDevice GraphicsDevice,
            ContentManager Content)
        {
            viewWidth = GraphicsDevice.Viewport.Width;
            viewHeight = GraphicsDevice.Viewport.Height;

            // Create the three render targets
            depthTarg = new RenderTarget2D(GraphicsDevice, viewWidth,
                viewHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);

            normalTarg = new RenderTarget2D(GraphicsDevice, viewWidth,
                viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

            lightTarg = new RenderTarget2D(GraphicsDevice, viewWidth,
                viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

            // Load effects
            depthNormalEffect = Content.Load<Effect>("Effects/PPDepthNormal");
            lightingEffect = Content.Load<Effect>("Effects/PPLight");

            // Set effect parameters to light mapping effect
            lightingEffect.Parameters["viewportWidth"].SetValue(viewWidth);
            lightingEffect.Parameters["viewportHeight"].SetValue(viewHeight);

            // Load point light mesh and set light mapping effect to it
            lightMesh = Content.Load<Model>("Effects/PPLightMesh");
            lightMesh.Meshes[0].MeshParts[0].Effect = lightingEffect;

            shadowDepthTarg = new RenderTarget2D(GraphicsDevice, shadowMapSize,
                shadowMapSize, false, SurfaceFormat.HalfVector2, DepthFormat.Depth24);

            shadowDepthEffect = Content.Load<Effect>("Effects/ShadowDepthEffect");
            shadowDepthEffect.Parameters["FarPlane"].SetValue(shadowFarPlane);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            shadowBlurEffect = Content.Load<Effect>("Effects/ShadowGaussianBlur");

            shadowBlurTarg = new RenderTarget2D(GraphicsDevice, shadowMapSize,
                shadowMapSize, false, SurfaceFormat.HalfVector2, DepthFormat.Depth24);

            this.graphicsDevice = GraphicsDevice;
        }

        public void Draw()
        {
            drawDepthNormalMap();
            drawLightMap();

            if (DoShadowMapping)
            {
                drawShadowDepthMap();
                blurShadow(shadowBlurTarg, shadowDepthTarg, 0);
                blurShadow(shadowDepthTarg, shadowBlurTarg, 1);
            }

            prepareMainPass();
        }

        void drawDepthNormalMap()
        {
            // Set the render targets to 'slots' 1 and 2
            graphicsDevice.SetRenderTargets(normalTarg, depthTarg);

            // Clear the render target to 1 (infinite depth)
            graphicsDevice.Clear(Color.White);

            // Draw each model with the PPDepthNormal effect
            foreach (CModel model in Models)
            {
                model.CacheEffects();
                if (model._ModelLife > 0) 
                    model.SetModelEffect(depthNormalEffect, false);
                model.Draw(Camera._view, Camera._projection, Camera._cameraPos);
                model.RestoreEffects();
            }

            // Un-set the render targets
            graphicsDevice.SetRenderTargets(Display2D.C2DEffect.renderTarget);
        }

        void drawShadowDepthMap()
        {
            // Calculate view and projection matrices for the "light"
            // shadows are being calculated for
            shadowView = Matrix.CreateLookAt(ShadowLightPosition,
                ShadowLightTarget, Vector3.Up);

            shadowProjection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), 1, 1, shadowFarPlane);

            // Set render target
            graphicsDevice.SetRenderTarget(shadowDepthTarg);

            // Clear the render target to 1 (infinite depth)
            graphicsDevice.Clear(Color.White);

            // Draw each model with the ShadowDepthEffect effect
            foreach (CModel model in Models)
            {
                model.CacheEffects();
                model.SetModelEffect(shadowDepthEffect, false);
                model.Draw(shadowView, shadowProjection, ShadowLightPosition);
                model.RestoreEffects();
            }

            // Un-set the render targets
            graphicsDevice.SetRenderTarget(null);
        }

        void drawLightMap()
        {
            // Set the depth and normal map info to the effect
            lightingEffect.Parameters["DepthTexture"].SetValue(depthTarg);
            lightingEffect.Parameters["NormalTexture"].SetValue(normalTarg);

            // Calculate the view * projection matrix
            Matrix viewProjection = Camera._view * Camera._projection;

            // Set the inverse of the view * projection matrix to the effect
            Matrix invViewProjection = Matrix.Invert(viewProjection);
            lightingEffect.Parameters["InvViewProjection"].SetValue(
                invViewProjection);

            // Set the render target to the graphics device
            graphicsDevice.SetRenderTarget(lightTarg);

            // Clear the render target to black (no light)
            graphicsDevice.Clear(Color.Black);

            // Set render states to additive (lights will add their influences)
            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthStencilState = DepthStencilState.None;

            foreach (PPPointLight light in Lights)
            {
                // Set the light's parameters to the effect
                light.SetEffectParameters(lightingEffect);

                // Calculate the world * view * projection matrix and set it to 
                // the effect
                Matrix wvp = (Matrix.CreateScale(light.Attenuation)
                    * Matrix.CreateTranslation(light.Position)) * viewProjection;

                lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);

                // Determine the distance between the light and camera
                float dist = Vector3.Distance(Camera._cameraPos, light.Position);

                // If the camera is inside the light-sphere, invert the cull mode
                // to draw the inside of the sphere instead of the outside
                if (dist < light.Attenuation)
                    graphicsDevice.RasterizerState =
                        RasterizerState.CullClockwise;

                // Draw the point-light-sphere
                lightMesh.Meshes[0].Draw();

                // Revert the cull mode
                graphicsDevice.RasterizerState =
                    RasterizerState.CullCounterClockwise;
            }

            // Revert the blending and depth render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Un-set the render target
            graphicsDevice.SetRenderTarget(Display2D.C2DEffect.renderTarget);
        }

        void prepareMainPass()
        {
            foreach (CModel model in Models)
                foreach (ModelMesh mesh in model._model.Meshes)
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // Set the light map and viewport parameters to each model's effect
                        if (part.Effect.Parameters["LightTexture"] != null)
                            part.Effect.Parameters["LightTexture"].SetValue(lightTarg);

                        if (part.Effect.Parameters["viewportWidth"] != null)
                            part.Effect.Parameters["viewportWidth"].SetValue(viewWidth);

                        if (part.Effect.Parameters["viewportHeight"] != null)
                            part.Effect.Parameters["viewportHeight"].SetValue(viewHeight);

                        if (part.Effect.Parameters["DoShadowMapping"] != null)
                            part.Effect.Parameters["DoShadowMapping"].SetValue(DoShadowMapping);

                        if (!DoShadowMapping) continue;

                        if (part.Effect.Parameters["ShadowMap"] != null)
                            part.Effect.Parameters["ShadowMap"].SetValue(shadowDepthTarg);

                        if (part.Effect.Parameters["ShadowView"] != null)
                            part.Effect.Parameters["ShadowView"].SetValue(shadowView);

                        if (part.Effect.Parameters["ShadowProjection"] != null)
                            part.Effect.Parameters["ShadowProjection"].SetValue(shadowProjection);

                        if (part.Effect.Parameters["ShadowLightPosition"] != null)
                            part.Effect.Parameters["ShadowLightPosition"].SetValue(ShadowLightPosition);

                        if (part.Effect.Parameters["ShadowFarPlane"] != null)
                            part.Effect.Parameters["ShadowFarPlane"].SetValue(shadowFarPlane);

                        if (part.Effect.Parameters["ShadowMult"] != null)
                            part.Effect.Parameters["ShadowMult"].SetValue(ShadowMult);
                    }
        }

        void blurShadow(RenderTarget2D to, RenderTarget2D from, int dir)
        {
            // Set the target render target
            graphicsDevice.SetRenderTarget(to);

            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            // Start the Gaussian blur effect
            shadowBlurEffect.CurrentTechnique.Passes[dir].Apply();

            // Draw the contents of the source render target so they can
            // be blurred by the gaussian blur pixel shader
            spriteBatch.Draw(from, Vector2.Zero, Color.White);

            spriteBatch.End();

            // Clean up after the sprite batch
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Remove the render target
            graphicsDevice.SetRenderTarget(null);
        }
    }
}
