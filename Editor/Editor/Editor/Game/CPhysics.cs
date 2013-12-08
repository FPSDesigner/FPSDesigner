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

        public List<Model> _modelsList;
        private Display3D.CTerrain _terrain;

        private double _lastFreeFall = 0;
        private bool _isJumping = false;

        public float _entityHeight { get; private set; }
        public float _gravityConstant = -9.81f / 500;

        public Vector3 _velocity;

        public CPhysics(float gravCoef, Display3D.CTerrain map, float entityHeight)
        {
            this._terrain = map;
            this._entityHeight = entityHeight;
            this._gravityConstant = -gravCoef;

            _velocity = Vector3.Zero;
        }

        public Vector3 checkCollisions(GameTime gameTime, Vector3 position, Vector3 translation, bool applyTranslation, Vector3 triangleNormal)
        {
            Vector3 newPosition = position;

            if (applyTranslation)
                newPosition += translation;
            else
                newPosition -= triangleNormal;

            // Check terrain collision
            float terrainHeight = _terrain.GetHeightAtPosition(newPosition.X, newPosition.Z);

            if (newPosition.Y >= terrainHeight + _entityHeight || _isJumping)
            {
                float dt = (float)(gameTime.TotalGameTime.TotalSeconds - _lastFreeFall);
                _velocity.Y += _gravityConstant * dt;
                _isJumping = false;
            }
            else
            {
                newPosition.Y = terrainHeight + _entityHeight;
                _lastFreeFall = gameTime.TotalGameTime.TotalSeconds;

                Vector3 normal = _terrain.getNormalAtPoint(newPosition.X, newPosition.Z);
                _velocity = Vector3.Zero;
                if (normal.Y > -0.6)
                {
                    _velocity = -0.5f * normal;
                    _velocity.Y = -0.1f;
                }
            }
            return newPosition + _velocity;
        }

        public void Jump(Vector3 Position)
        {
            float terrainHeight = _terrain.GetHeightAtPosition(Position.X, Position.Z);

            if (Position.Y <= terrainHeight + _entityHeight)
            {
                _velocity.Y += 0.6f;
                _isJumping = true;
            }
        }

    }
}
