using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace Editor.Display3D
{
    public interface IRenderable
    {
        void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition);
        void SetClipPlane(Vector4? Plane);
    }

    class CWater
    {
        CModel waterMesh;
        Effect waterEffect;

        ContentManager content;
        GraphicsDevice graphics;

        RenderTarget2D reflectionTarg;
        public List<IRenderable> Objects = new List<IRenderable>();

        //Used to pass as parameter to Cam
        Display3D.CTerrain _map;

        public Vector3 waterPosition;
        public Vector2 waterSize;

        private Vector3 modelRotationUnderwater = new Vector3(0, 0, MathHelper.Pi);

        public bool isUnderWater = false;

        float Alpha;
        public float WaterAlpha
        {
            get { return Alpha; }
            set
            {
                Alpha = value;
                waterEffect.Parameters["Alpha"].SetValue(value);
            }
        }

        public CWater(ContentManager content, GraphicsDevice graphics, Vector3 position, Vector2 size, float alpha, Display3D.CTerrain map)
        {
            this.content = content;
            this.graphics = graphics;

            this.waterPosition = position;
            this.waterSize = size;

            this._map = map;

            waterMesh = new CModel(content.Load<Model>("3D/plane"), position, Vector3.Zero, new Vector3(size.X, 1, size.Y), graphics);

            waterEffect = content.Load<Effect>("Effects/WaterEffect");
            waterMesh.SetModelEffect(waterEffect, false);

            waterEffect.Parameters["viewportWidth"].SetValue(graphics.Viewport.Width);
            waterEffect.Parameters["viewportHeight"].SetValue(graphics.Viewport.Height);
            waterEffect.Parameters["WaterNormalMap"].SetValue(content.Load<Texture2D>("Textures/water_normal"));

            this.Alpha = alpha;

            reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        /// <summary>
        /// Renders the reflection image
        /// </summary>
        /// <param name="camera">Camera class</param>
        /// <param name="gameTime">GameTime snapshot</param>
        public void renderReflection(CCamera camera, GameTime gameTime)
        {
            // Reflect the camera's properties across the water plane
            Vector3 reflectedCameraPosition = camera._cameraPos;
            reflectedCameraPosition.Y = -reflectedCameraPosition.Y + waterMesh._modelPosition.Y * 2;

            Vector3 reflectedCameraTarget = camera._cameraTarget;
            reflectedCameraTarget.Y = -reflectedCameraTarget.Y + waterMesh._modelPosition.Y * 2;

            // Create a temporary camera to render the reflected scene
            CCamera reflectionCamera = new CCamera(graphics, reflectedCameraPosition, reflectedCameraTarget, 0.1f, 1000000.0f,true,_map);

            reflectionCamera.Update(gameTime);

            // Set the reflection camera's view matrix to the water effect
            waterEffect.Parameters["ReflectedView"].SetValue(reflectionCamera._view);

            // Create the clip plane
            Vector4 clipPlane = new Vector4(0, 1, 0, -waterMesh._modelPosition.Y);

            // Set the render target
            graphics.SetRenderTarget(reflectionTarg);
            graphics.Clear(Color.Black);

            // Draw all objects with clip plane
            foreach (IRenderable renderable in Objects)
            {
                renderable.SetClipPlane(clipPlane);

                renderable.Draw(reflectionCamera._view, reflectionCamera._projection, reflectedCameraPosition);

                renderable.SetClipPlane(null);
            }

            graphics.SetRenderTarget(null);

            // Set the reflected scene to its effect parameter in
            // the water effect
            waterEffect.Parameters["ReflectionMap"].SetValue(reflectionTarg);
        }

        /// <summary>
        /// Pre-draw the water effect: gets the reflection image
        /// </summary>
        /// <param name="camera">The camera class</param>
        /// <param name="gameTime">GameTime snapshot</param>
        public void PreDraw(CCamera camera, GameTime gameTime)
        {
            renderReflection(camera, gameTime);
            waterEffect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
        }

        /// <summary>
        /// Draw the water plane
        /// </summary>
        /// <param name="View">The view matrix</param>
        /// <param name="Projection">The projection matrix</param>
        /// <param name="CameraPosition">The camera position</param>
        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            if (isUnderWater)
                waterMesh._modelRotation = modelRotationUnderwater;
            else
                waterMesh._modelRotation = Vector3.Zero;
            waterMesh.Draw(View, Projection, CameraPosition);
        }

        /// <summary>
        /// Checks if a position is underwater
        /// </summary>
        /// <param name="Position">Vector3 to check</param>
        /// <returns>True if the position is underwater, false otherwise</returns>
        public bool isPositionUnderWater(Vector3 Position)
        {
            if (Position.Y <= waterPosition.Y)
            {
                if ((Position.X >= waterPosition.X - waterSize.X && Position.X <= waterPosition.X + waterSize.X) && 
                    (Position.Z >= waterPosition.Z - waterSize.Y && Position.Z <= waterPosition.Z + waterSize.Y))
                        return true;
            }
            return false;
        }
    }
}
