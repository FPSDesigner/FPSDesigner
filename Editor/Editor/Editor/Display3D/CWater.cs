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
        public static int selectedWater = -1;

        private static bool _isUnderWater = false;

        public static bool isUnderWater
        {
            set
            {
                foreach (CWater water in listWater)
                    water.isUnderWater = value;
                _isUnderWater = value;
            }
            get
            {
                return _isUnderWater;
            }
        }

        public static int AddWater(CWater water)
        {
            listWater.Add(water);
            return listWater.IndexOf(water);
        }

        public static void PreDraw(CCamera camera, GameTime gameTime)
        {
            foreach(CWater water in listWater)
                water.PreDraw(camera, gameTime);
        }

        public static void Draw(Matrix View, Matrix Projection, Vector3 camPos)
        {
            for (int i = 0; i < listWater.Count; i++)
            {
                listWater[i].Draw(View, Projection, camPos);
                if (selectedWater == i)
                    CSimpleShapes.AddBoundingBox(listWater[i].RealBoundingBox, Color.Black);
            }
        }

        public static void SetDebugActivated(bool toggle)
        {
            foreach (CWater water in listWater)
                water.debugActivated = toggle;
        }

        public static bool IsPositionUnderwater(Vector3 pos)
        {
            foreach (CWater water in listWater)
                if (water.isPositionUnderWater(pos))
                    return true;

            return false;
        }

        public static float GetWaterHeight(Vector3 pos)
        {
            if (!isUnderWater)
                return 0f;
            else
                foreach (CWater water in listWater)
                    if (water.isPositionUnderWater(pos))
                        return water.waterPosition.Y;

            return 0f;
        }

        public static void DrawDebug(SpriteBatch spriteBatch)
        {
            foreach (CWater water in listWater)
                water.DrawDebug(spriteBatch);
        }

        public static float? CheckRayIntersectsAnyPickup(Ray ray, out int idIntersected)
        {
            for (int i = 0; i < listWater.Count; i++)
            {
                float? distance = listWater[i].RayIntersectsPickup(ray);
                if (distance.HasValue)
                {
                    idIntersected = i;
                    return distance;
                }
            }
            idIntersected = 0;
            return null;
        }

        public static void UpdateGameLevel(ref Game.LevelInfo.LevelData lvl)
        {
            for (int i = 0; i < listWater.Count; i++)
            {
                CWater water = listWater[i];

                lvl.Water.Water[i].Alpha = water.WaterAlpha;
                lvl.Water.Water[i].Coordinates = new Game.LevelInfo.Coordinates(water.waterPosition);
                lvl.Water.Water[i].SizeX = water.waterSize.X;
                lvl.Water.Water[i].SizeY = water.waterSize.Y;
            }

            while (lvl.Water.Water.Count != listWater.Count)
                lvl.Water.Water.RemoveAt(lvl.Water.Water.Count - 1);
        }

    }

    class CWater
    {
        public CModel waterMesh;
        Effect waterEffect;

        ContentManager content;
        GraphicsDevice graphics;

        public RenderTarget2D reflectionTarg;
        public List<IRenderable> Objects = new List<IRenderable>();

        public Vector3 _waterDeepestPoint;

        private Vector3 _waterPosition;
        public Vector3 waterPosition
        {
            get { return _waterPosition; }
            set
            {
                _waterPosition = value;
                waterMesh._modelPosition = _waterPosition;
                GenerateBoundingBoxes();
            }
        }

        private Vector2 _waterSize;
        public Vector2 waterSize
        {
            get { return _waterSize; }
            set
            {
                _waterSize = value;
                waterMesh._modelScale = new Vector3(_waterSize.X, 1, _waterSize.Y);
            }
        }


        public CCamera reflectionCamera;

        private Vector3 modelRotationUnderwater = new Vector3(0, 0, MathHelper.Pi);

        private BoundingBox BoundingBoxChunk;
        public BoundingBox RealBoundingBox;
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

            this._waterPosition = position;
            this._waterSize = size;
            this._waterDeepestPoint = deepestPoint;

            waterMesh = new CModel(content.Load<Model>("3D/plane"), position, Vector3.Zero, new Vector3(size.X, 1, size.Y), graphics);

            waterEffect = content.Load<Effect>("Effects/WaterEffect");
            waterMesh.SetModelEffect(waterEffect, false);

            waterEffect.Parameters["viewportWidth"].SetValue(graphics.Viewport.Width);
            waterEffect.Parameters["viewportHeight"].SetValue(graphics.Viewport.Height);
            waterEffect.Parameters["WaterNormalMap"].SetValue(content.Load<Texture2D>("Textures/water_normal"));

            this.WaterAlpha = alpha;

            reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);

            reflectionCamera = new CCamera(graphics, Vector3.Zero, Vector3.Zero, 0.1f, 10000.0f, true);

            pickWorld = Matrix.CreateTranslation(Vector3.Zero);
        }

        public void GenerateBoundingBoxes()
        {
            List<Vector3> list = new List<Vector3>();
            list.Add(new Vector3(_waterPosition.X - _waterSize.X, _waterPosition.Y, _waterPosition.Z - _waterSize.Y));
            list.Add(new Vector3(_waterPosition.X + _waterSize.X, _waterPosition.Y, _waterPosition.Z - _waterSize.Y));
            list.Add(new Vector3(_waterPosition.X - _waterSize.X, _waterPosition.Y, _waterPosition.Z + _waterSize.Y));
            list.Add(new Vector3(_waterPosition.X + _waterSize.X, _waterPosition.Y, _waterPosition.Z + _waterSize.Y));
            BoundingBoxChunk = BoundingBox.CreateFromPoints(list);
            list.Add(new Vector3(_waterPosition.X, _waterDeepestPoint.Y, _waterPosition.Z));
            RealBoundingBox = BoundingBox.CreateFromPoints(list);
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

        /// <summary>
        /// Check if the ray intersects the water
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public float? RayIntersectsPickup(Ray ray)
        {
            return ray.Intersects(RealBoundingBox);
        }
    }
}
