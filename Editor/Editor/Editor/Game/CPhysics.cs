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

        float entityHeight = 1f;

        Vector3 acceleration;

        public void Initialize(Display3D.CTerrain terrain, GraphicsDevice graphicsDevice)
        {
            this.terrain = terrain;

            Display3D.CSimpleShapes.Initialize(graphicsDevice);
        }

        public Vector3 checkCollisions(Vector3 position)
        {
            // Check terrain collision
            float Steepness;
            float terrainHeight = terrain.GetHeightAtPosition(position.X, position.Z, out Steepness);

            Display3D.CSimpleShapes.AddBoundingSphere(new BoundingSphere(new Vector3(position.X, terrainHeight, position.Z), 0.1f), Color.Red);
            Vector3 normal = terrain.getNormalAtPoint(position.X, position.Z);

            Vector2 vert = terrain.positionToTerrain(position.X, position.Z);
            int x2 = (int)(vert.X + 1 == 512 ? vert.X : vert.X + 1);
            int z2 = (int)(vert.Y + 1 == 512 ? vert.Y : vert.Y + 1);
            Display3D.CSimpleShapes.AddLine(terrain.vertices[(int)(vert.Y * 512 + vert.X)].Position, terrain.vertices[z2 * 512 + x2].Position, Color.Blue);


            position.Y = terrainHeight + entityHeight;

            Console.WriteLine(position.Y);


            return position;
        }

        public void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            Display3D.CSimpleShapes.Draw(gameTime, view, projection);
        }



    }
}
