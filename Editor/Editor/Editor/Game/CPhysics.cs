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

        private Display3D.CTerrain _terrain;

        private double _lastFreeFall = 0;
        private bool _isJumping = false;

        public float _entityHeight { get; private set; }
        public float _gravityConstant = -9.81f / 500;
        public float _maxFreeFallSpeed = -5f;

        public Vector3 _velocity;

        public BoundingSphere _BoundingSphere { get; private set; }
        public List<Display3D.CModel> _modelsList { get; set; }

        public CPhysics(float gravCoef, Display3D.CTerrain map, float entityHeight)
        {
            this._terrain = map;
            this._entityHeight = entityHeight;
            this._gravityConstant = -gravCoef;

            _velocity = Vector3.Zero;

            _BoundingSphere = new BoundingSphere(Vector3.Zero, entityHeight / 2);
        }

        public Vector3 checkCollisions(GameTime gameTime, Vector3 position, Vector3 translation)
        {
            Vector3 newPosition = position + translation;

            // Check terrain collision
            float terrainHeight = _terrain.GetHeightAtPosition(newPosition.X, newPosition.Z);

            if (newPosition.Y >= terrainHeight + _entityHeight || _isJumping)
            {
                float dt = (float)(gameTime.TotalGameTime.TotalSeconds - _lastFreeFall);
                if(_velocity.Y > _maxFreeFallSpeed)
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

            Vector3 triangleNormal = Vector3.Zero;

            BoundingSphere _BoundingSphereTranslation = new BoundingSphere(newPosition, _entityHeight / 2);
            for (int i = 0; i < _modelsList.Count; i++)
            {
                Vector3 intersectionPoint;
                if (_modelsList[i].IsBoundingSphereIntersecting(_BoundingSphereTranslation, out triangleNormal))
                {
                    newPosition -= translation;
                    Display3D.CSimpleShapes.AddLine(triangleNormal, 2 * triangleNormal, Color.Red);
                    //translation = Vector3.Cross(translation, triangleNormal);

                    translation.Y = -triangleNormal.Y;
                    if (Vector3.Dot(translation, triangleNormal) < -0.5f)
                        newPosition += translation;
                    Console.WriteLine(Vector3.Dot(translation, triangleNormal));
                    break;
                }
            }

            newPosition += _velocity;

            BoundingSphere _BoundingSphereGravity = new BoundingSphere(newPosition, _entityHeight / 2);
            for (int i = 0; i < _modelsList.Count; i++)
            {
                Vector3 intersectionPoint;
                if (_modelsList[i].IsBoundingSphereIntersecting(_BoundingSphereGravity, out triangleNormal))
                {
                    newPosition -= _velocity;
                    //newPosition.Y = intersectionPoint.Y + _entityHeight;
                    _velocity = Vector3.Zero;
                    _lastFreeFall = gameTime.TotalGameTime.TotalSeconds;
                    break;
                }
            }
            Console.WriteLine(_velocity);
            return newPosition;
        }

        public void Jump(Vector3 Position)
        {
            if (_velocity.Y == 0)
            {
                _velocity.Y += 0.6f;
                _isJumping = true;
            }
        }

    }
}
