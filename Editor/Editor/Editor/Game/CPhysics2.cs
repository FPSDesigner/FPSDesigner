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
    class CPhysics2
    {
        public float _gravityConstant = 9.81f;
        public float maxFallingVelocity; // The maximum velocity of an entity during its fall

        private List<Display3D.Triangle> _triangleList;

        private Display3D.CTerrain _terrain;
        private float _entityHeight;
        private float _weight;

        private Vector3 _velocity;
        
        public CPhysics2()
        {

        }

        public void LoadContent(Display3D.CTerrain terrain, float entityHeight, List<Display3D.Triangle> triangleList)
        {
            this._terrain = terrain;
            this._entityHeight = entityHeight;

            this._triangleList = triangleList;

            this._weight = _gravityConstant * 80.0f;

        }

        
        public Vector3 GetNewPosition(GameTime gameTime, Vector3 entityPos, Vector3 translation)
        {

            return entityPos;
        }

        private Vector3 GetNewPositionFromFalling(GameTime gameTime, Vector3 entityPos, Vector3 translation, bool isIntersecting)
        {
            return entityPos;
        }
    }
}
