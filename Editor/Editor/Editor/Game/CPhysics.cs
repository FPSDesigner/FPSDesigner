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

namespace Engine.Game
{
    class CPhysics
    {
        
        // Private
        private float _entityHeight;
        private float _intersectionDistanceH = 4f; // Horizontal distance collision with models
        private double _lastFreeFall;
        private bool _isUnderwater = false;

        // Components usage  0: terrain, 1: water
        private bool[] _isGameUsing = new bool[2];
        private Vector3 _velocity;

        // Public
        public List<Display3D.Triangle> _triangleList = new List<Display3D.Triangle>();
        public List<Vector3> _triangleNormalsList = new List<Vector3>();
        public Display3D.CTerrain _terrain;
        public float _gravityConstant = -9.81f / 600f;
        public float _gravityConstantWater = -9.81f / 5000f;
        public float _maxFallingVelocity = -3.5f; // The maximum velocity of an entity during its fall
        public float _maxFallingVelocityWater = -0.05f;
        public float _waterHeight = 0f;
        public bool _isOnWaterSurface = false;
        public float _fallingVelocity { get { return _velocity.Y; } }
        public float _slippingResistance = -0.8f;

        public CPhysics()
        {

        }

        /// <summary>
        /// Loads all the content needed
        /// </summary>
        /// <param name="entityHeight">Height of the entity</param>
        /// <param name="gamesEnvUsages">0: terrain, 1: water</param>
        public void LoadContent(float entityHeight, bool[] gamesEnvUsages)
        {
            this._entityHeight = entityHeight;
            this._isGameUsing = gamesEnvUsages;
        }

        /// <summary>
        /// Gets the new position of the entity by applying it all the physics
        /// </summary>
        public Vector3 GetNewPosition(GameTime gameTime, Vector3 entityPos, Vector3 translation, bool isUnderwater)
        {
            _isUnderwater = isUnderwater;
            bool isVerticalIntersecting = false;

            Vector3 assumedNewPosition = entityPos + translation;
            Ray translationRay = new Ray(entityPos, translation);

            Vector3 horizontalNormalReaction = Vector3.Zero;

            // Water surface check
            if (_isGameUsing[1] && (_isUnderwater || _isOnWaterSurface) && entityPos.Y + translation.Y > _waterHeight)
            {
                _isOnWaterSurface = true;
                _velocity.Y = 0;
                assumedNewPosition.Y = _waterHeight + translation.Y;
            }
            else if(_isOnWaterSurface)
                _isOnWaterSurface = false;

            /* Horizontal check - models */
            for (int i = 0; i < _triangleList.Count; i++)
            {
                Display3D.Triangle triangleToTest = _triangleList[i];
                float? distance = Display3D.TriangleTest.Intersects(ref translationRay, ref triangleToTest);
                if (distance != null && distance <= _intersectionDistanceH)
                {
                    horizontalNormalReaction = -_triangleNormalsList[i] * translation.Length() * (_intersectionDistanceH - (float)distance);
                    //horizontalNormalReaction = -translation;
                    break;
                }
            }
            assumedNewPosition += horizontalNormalReaction;


            /* Vertical check - terrain */
            // Terrain informations
            float Steepness;
            float terrainHeight = (_isGameUsing[0]) ? _terrain.GetHeightAtPosition(assumedNewPosition.X, assumedNewPosition.Z, out Steepness) : 0;
            
            float jumpVelocity = 0f;
            if (_velocity.Y > 0)
                jumpVelocity = _velocity.Y;

            if (_isGameUsing[0] && assumedNewPosition.Y + jumpVelocity <= terrainHeight + _entityHeight + 0.001f)
            {
                isVerticalIntersecting = true;
                assumedNewPosition.Y = terrainHeight + _entityHeight;

                _velocity = Vector3.Zero;
                Vector3 normal = _terrain.getNormalAtPoint(assumedNewPosition.X, assumedNewPosition.Z);
                if (normal.Y > _slippingResistance)
                {
                    _velocity = -0.1f * normal;
                    _velocity.Y = -0.1f;
                }
            }
            else
            {
                isVerticalIntersecting = false;
                float dt = (float)(gameTime.TotalGameTime.TotalSeconds - _lastFreeFall);
                if (_isUnderwater && translation == Vector3.Zero)
                    _velocity.Y += _gravityConstantWater * dt;
                else
                    _velocity.Y += _gravityConstant * dt;

                if (_isUnderwater && _velocity.Y < _maxFallingVelocityWater)
                    _velocity.Y = _maxFallingVelocityWater;
                else if (_velocity.Y < _maxFallingVelocity)
                    _velocity.Y = _maxFallingVelocity;
            }

            Ray newPosTranslationRay = new Ray(assumedNewPosition + _velocity, (_velocity.Y > 0) ? Vector3.Up : Vector3.Down);

            /* Vertical check - models */
            float closestTriangleBelowDistance = 999f;
            int closestTriangle = -1;
            for (int i = 0; i < _triangleList.Count; i++)
            {
                Display3D.Triangle triangleToTest = _triangleList[i];
                float? distance = Display3D.TriangleTest.Intersects(ref newPosTranslationRay, ref triangleToTest);
                if (distance != null)
                {
                    if (distance < closestTriangleBelowDistance)
                    {
                        closestTriangle = i;
                        closestTriangleBelowDistance = (float)distance;
                    }
                }
            }

            if (closestTriangle != -1 && closestTriangleBelowDistance <= (_velocity.Y <= 0 ? _entityHeight : _entityHeight/3))
            {
                // If the player is falling, and not jumping
                if (_velocity.Y <= 0)
                {
                    isVerticalIntersecting = true;
                    assumedNewPosition += _velocity;
                    Display3D.Triangle closestTriangleT = _triangleList[closestTriangle];
                    assumedNewPosition.Y = assumedNewPosition.Y - closestTriangleBelowDistance + _entityHeight;

                    Vector3 normal = _triangleNormalsList[closestTriangle];

                    if (normal.Y > _slippingResistance)
                    {
                        _velocity = -0.1f * normal;
                        _velocity.Y = -0.1f;
                    }
                }
                _velocity.Y = 0;
            }

            if(_isOnWaterSurface)
                _velocity.Y = 0;

            if (isVerticalIntersecting)
            {
                _lastFreeFall = gameTime.TotalGameTime.TotalSeconds;
                _isOnWaterSurface = false;
            }
            return assumedNewPosition + _velocity;
        }

        /// <summary>
        /// Jump function
        /// </summary>
        public void Jump()
        {
            if (_velocity.Y == 0)
            {
                _velocity.Y = 0.125f;
            }
        }
    }
}
