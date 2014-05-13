using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace Engine.Display3D
{
    public interface IRenderable
    {
        void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition);
        void SetClipPlane(Vector4? Plane);
    }

    class CWaterManager
    {
        public static List<CWater> listWater = new List<CWater>();

        public static void AddWater(CWater water)
        {
            listWater.Add(water);
        }

        public static void PreDraw(CCamera camera, GameTime gameTime)
        {
            foreach(CWater water in listWater)
                water.PreDraw(camera, gameTime);
        }

        public static void Draw(Matrix View, Matrix Projection, Vector3 camPos)
        {
            foreach(CWater water in listWater)
                water.Draw(View, Projection, camPos);
        }

        public static void SetUnderwater(bool underwater)
        {
            foreach (CWater water in listWater)
                water.isUnderWater = underwater;
        }

    }

    class CWater
    {
        CModel waterMesh;
        Effect waterEffect;

        ContentManager content;
        GraphicsDevice graphics;

        public RenderTarget2D reflectionTarg;
        public List<IRenderable> Objects = new List<IRenderable>();

        public Vector3 waterPosition;
        public Vector2 waterSize;
        public CCamera reflectionCamera;

        private Vector3 modelRotationUnderwater = new Vector3(0, 0, MathHelper.Pi);

        private BoundingBox BoundingBoxChunk;
        private BoundingBox RealBoundingBox;
        private bool isInView = true;

        private Matrix pickWorld;

        public bool debugActivated = false;


        public float _waveSpeed = 0.04f;

        private bool _isUnderWater = false;
        public bool isUnderWater
        {
            set
            {
                _isUnderWater = value;
                _waveSpeed = -_waveSpeed;

                waterEffect.Parameters["IsUnderWater"].SetValue(_isUnderWater);
                waterEffect.Parameters["WaveSpeed"].SetValue(_waveSpeed);
            }
            get
            {
                return _isUnderWater;
            }
        }

        private float Alpha;
        public float WaterAlpha
        {
            get { return Alpha; }
            set
            {
                Alpha = value;
                waterEffect.Parameters["Alpha"].SetValue(value);
            }
        }

        public CWater(ContentManager content, GraphicsDevice graphics, Vector3 position, Vector3 deepestPoint, Vector2 size, float alpha)
        {
            this.content = content;
            this.graphics = graphics;

            this.waterPosition = position;
            this.waterSize = size;

            waterMesh = new CModel(content.Load<Model>("3D/plane"), position, Vector3.Zero, new Vector3(size.X, 1, size.Y), graphics);

            waterEffect = content.Load<Effect>("Effects/WaterEffect");
            waterMesh.SetModelEffect(waterEffect, false);

            waterEffect.Parameters["viewportWidth"].SetValue(graphics.Viewport.Width);
            waterEffect.Parameters["viewportHeight"].SetValue(graphics.Viewport.Height);
            waterEffect.Parameters["WaterNormalMap"].SetValue(content.Load<Texture2D>("Textures/water_normal"));

            this.WaterAlpha = alpha;

            reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);

            reflectionCamera = new CCamera(graphics, Vector3.Zero, Vector3.Zero, 0.1f, 10000.0f, true);

            List<Vector3> list = new List<Vector3>();
            list.Add(new Vector3(position.X - size.X, position.Y, position.Z - size.Y));
            list.Add(new Vector3(position.X + size.X, position.Y, position.Z - size.Y));
            list.Add(new Vector3(position.X - size.X, position.Y, position.Z + size.Y));
            list.Add(new Vector3(position.X + size.X, position.Y, position.Z + size.Y));
            BoundingBoxChunk = BoundingBox.CreateFromPoints(list);
            list.Add(deepestPoint);
            RealBoundingBox = BoundingBox.CreateFromPoints(list);

            pickWorld = Matrix.CreateTranslation(Vector3.Zero);
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
            reflectionCamera._cameraPos = reflectedCameraPosition;
            reflectionCamera._cameraTarget = reflectedCameraTarget;
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

            graphics.SetRenderTarget(Display2D.C2DEffect.renderTarget);

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
            isInView = camera.BoundingVolumeIsInView(BoundingBoxChunk);
            if (isInView)
            {
                renderReflection(camera, gameTime);
                waterEffect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch)
        {
            if (debugActivated)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(reflectionTarg, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 0.25f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 1);
                spriteBatch.End();
            }
        }

        /// <summary>
        /// Draw the water plane
        /// </summary>
        /// <param name="View">The view matrix</param>
        /// <param name="Projection">The projection matrix</param>
        /// <param name="CameraPosition">The camera position</param>
        public void Draw(Matrix View, Matrix Projection, Vector3 camPos)
        {
            if (isInView)
            {
                if (_isUnderWater && waterMesh._modelRotation != modelRotationUnderwater)
                    waterMesh._modelRotation = modelRotationUnderwater;
                else if (!_isUnderWater && waterMesh._modelRotation != Vector3.Zero)
                    waterMesh._modelRotation = Vector3.Zero;
                waterMesh.Draw(View, Projection, camPos);
            }
        }

        /// <summary>
        /// Checks if a position is underwater
        /// </summary>
        /// <param name="Position">Vector3 to check</param>
        /// <returns>True if the position is underwater, false otherwise</returns>
        public bool isPositionUnderWater(Vector3 Position, bool checkY = true)
        {
            return RealBoundingBox.Contains(Position) == ContainmentType.Contains;
            /*
            if (checkY && Position.Y > waterPosition.Y)
                return false;

            if ((Position.X >= waterPosition.X - waterSize.X && Position.X <= waterPosition.X + waterSize.X) &&
                (Position.Z >= waterPosition.Z - waterSize.Y && Position.Z <= waterPosition.Z + waterSize.Y))
                return true;
            return false;*/
        }

        public Vector3 Pick(Matrix view, Matrix projection, int X, int Y, out bool isValid)
        {
            Vector3 NearestPoint = Vector3.Zero;

            Vector3 nearSource = graphics.Viewport.Unproject(new Vector3(X, Y, graphics.Viewport.MinDepth), projection, view, pickWorld);
            Vector3 farSource = graphics.Viewport.Unproject(new Vector3(X, Y, graphics.Viewport.MaxDepth), projection, view, pickWorld);
            Vector3 direction = farSource - nearSource;

            float t = 0f;
            while (true)
            {
                t += 0.0001f;

                Vector3 newPos = new Vector3(nearSource.X + direction.X * t, 0, nearSource.Z + direction.Z * t);

                if (newPos.X < waterPosition.X - waterSize.X || newPos.X > waterPosition.X + waterSize.X || newPos.Y < waterPosition.Y - waterSize.Y || newPos.Y > waterPosition.Y + waterSize.Y)
                    break;

                newPos.Y = nearSource.Y + direction.Y * t;

                if (newPos.Y <= waterPosition.Y)
                {
                    isValid = true;
                    return newPos;
                }

                //Display3D.CSimpleShapes.AddBoundingSphere(new BoundingSphere(new Vector3(XPos, YPos, ZPos), 1.0f), Color.Blue, 255f);
            }
            isValid = false;
            return Vector3.Zero;
        }
    }
}
