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
        public float _gravityConstant = -9.81f / 5000f;
        public float maxFallingVelocity; // The maximum velocity of an entity during its fall

        public List<Display3D.Triangle> _triangleList;
        public List<Vector3> _triangleNormalsList;

        private Display3D.CTerrain _terrain;
        private float _entityHeight;
        private float _weight;
        private float _intersectionDistanceH = 4f; // Horizontal distance collision with models
        private double _lastFreeFall;

        public Vector3 _velocity;

        public CPhysics2()
        {

        }

        public void LoadContent(Display3D.CTerrain terrain, float entityHeight/*, List<Display3D.Triangle> triangleList, List<Vector3> triangleNormalsList*/)
        {
            this._terrain = terrain;
            this._entityHeight = entityHeight;

            /*this._triangleList = triangleList;
            this._triangleNormalsList = triangleNormalsList;*/

        }

        
        public Vector3 GetNewPosition(GameTime gameTime, Vector3 entityPos, Vector3 translation)
        {
            Console.WriteLine(entityPos);
            bool isVerticalIntersecting = false;

            Vector3 assumedNewPosition = entityPos + translation;
            Ray translationRay = new Ray(entityPos, translation);

            Vector3 horizontalNormalReaction = Vector3.Zero;

            /* Horizontal check - models */
            for (int i = 0; i < _triangleList.Count; i++)
            {
                Display3D.Triangle triangleToTest = _triangleList[i];
                float? distance = Display3D.TriangleTest.Intersects(ref translationRay, ref triangleToTest);
                if (distance != null && distance <= _intersectionDistanceH)
                {
                    Console.WriteLine(distance);
                    horizontalNormalReaction = -_triangleNormalsList[i] * translation.Length() * (_intersectionDistanceH - (float)distance);
                    //horizontalNormalReaction = -translation;
                    break;
                }
            }
            assumedNewPosition += horizontalNormalReaction;


            /* Vertical check - terrain */
            // Terrain informations
            float Steepness;
            float terrainHeight = _terrain.GetHeightAtPosition(assumedNewPosition.X, assumedNewPosition.Z, out Steepness);

            if (assumedNewPosition.Y <= terrainHeight + _entityHeight)
            {
                isVerticalIntersecting = true;
                assumedNewPosition.Y = terrainHeight + _entityHeight;
                _velocity.Y = 0;
            }
            else
            {
                isVerticalIntersecting = false;
                float dt = (float)(gameTime.TotalGameTime.TotalSeconds - _lastFreeFall);
                _velocity.Y += _gravityConstant * dt;
            }

            Ray newPosTranslationRay = new Ray(assumedNewPosition + _velocity, Vector3.Down);

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

            if (closestTriangle != -1 && closestTriangleBelowDistance <= _entityHeight)
            {
                isVerticalIntersecting = true;
                assumedNewPosition += _velocity;
                Display3D.Triangle closestTriangleT = _triangleList[closestTriangle];
                assumedNewPosition.Y = assumedNewPosition.Y - closestTriangleBelowDistance + _entityHeight;
                _velocity.Y = 0;
            }



            if (isVerticalIntersecting)
                _lastFreeFall = gameTime.TotalGameTime.TotalSeconds;

            return assumedNewPosition + _velocity;
        }

        /*private Vector3 GetNewPositionFromFalling(GameTime gameTime, Vector3 entityPos, Vector3 translation, bool isIntersecting)
        {
            if (isIntersecting)
            {

                return entityPos;
            }
            else
            {
                float dt = (float)(gameTime.TotalGameTime.TotalSeconds - _lastFreeFall);
                _velocity.Y += _gravityConstant * dt;
            }
        }

        private float GetDistanceFromIntersection(Vector3 point1, Vector3 point2, Display3D.Triangle triangle, out bool isIntersecting)
        {
        }*/
    }
}
