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

namespace Editor.Game
{
    class CPhysics
    {
        #region "Singleton"

        // Singleton Code
        private static CPhysics instance = null;
        private static readonly object myLock = new object();

        // Singelton Methods
        private CPhysics() { }
        public static CPhysics getInstance()
        {
            lock (myLock)
            {
                if (instance == null) instance = new CPhysics();
                return instance;
            }
        }
        #endregion

        List<Model> modelsList;
        Display3D.CTerrain terrain;

        private double lastFreeFall = 0;

        float entityHeight = 1f;
        Vector3 velocity;
        float gravityConstant = -9.81f / 500;
        bool isJumping = false;

        public void Initialize(Display3D.CTerrain terrain, GraphicsDevice graphicsDevice)
        {
            this.terrain = terrain;

            Display3D.CSimpleShapes.Initialize(graphicsDevice);

            Vector3 acceleration = new Vector3(0, 0, 0);
        }

        public Vector3 checkCollisions(Vector3 position, GameTime gameTime)
        {
            // Check terrain collision
            float Steepness;
            float terrainHeight = terrain.GetHeightAtPosition(position.X, position.Z, out Steepness);

            if (position.Y >= terrainHeight + entityHeight || isJumping)
            {
                float dt = (float)(gameTime.TotalGameTime.TotalSeconds - lastFreeFall);
                velocity.Y += gravityConstant * dt;
                isJumping = false;
            }
            else
            {
                position.Y = terrainHeight + entityHeight;
                lastFreeFall = gameTime.TotalGameTime.TotalSeconds;

                Vector3 normal = terrain.getNormalAtPoint(position.X, position.Z);
                velocity = Vector3.Zero;
                if(normal.Y > -0.6)
                {
                    velocity = -0.5f*normal;
                    velocity.Y = -0.1f;
                }
            }
            return position + velocity;
        }

        public void Jump(Vector3 Position, float Yaw)
        {
            float Steepness;
            float terrainHeight = terrain.GetHeightAtPosition(Position.X, Position.Z, out Steepness);

            if (Position.Y <= terrainHeight + entityHeight)
            {
                velocity.Y += 0.3f;
                isJumping = true;
            }
        }

    }
}
