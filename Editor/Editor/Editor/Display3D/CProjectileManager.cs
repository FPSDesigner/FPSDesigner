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
namespace Engine.Display3D
{
    static class CProjectileManager
    {
        static List<CProjectile> _thrownProjectiles; // All the projectiles that have been thrown
        static List<CProjectile> _collisionedProjectiles; // All the projectiles which reached the floor

        public static void Initialize()
        {
            _thrownProjectiles = new List<CProjectile>();
            _collisionedProjectiles = new List<CProjectile>();
        }

        public static void ThrowProjectile(CProjectile projectile)
        {
            _thrownProjectiles.Add(projectile);
        }

        public static void updateProjectileList(GameTime gameTime, CTerrain terrain)
        {
            // Change position/rotation of all arrows above the terrain
            foreach(Display3D.CProjectile projectile in _thrownProjectiles)
            {
                projectile.UpdatePos(gameTime, terrain);
            }
        }

        public static void drawThrownProjectiles(GameTime gameTime, Matrix view, Matrix projection, Display3D.CCamera cam)
        {
            foreach (Display3D.CProjectile projectile in _thrownProjectiles)
            {
                projectile.Draw(view, projection, cam._cameraPos);
            }
        }

    }

    class CProjectile
    {
        private CModel _model;

        private Vector3 _pos;
        private Vector3 _rot;
        private Vector3 _direction; // We throw the model in the chosen direction
        public bool _isCollisioned;

        public float _damage;
        public bool _destroy;

        public float _fallElapsedTime;

        public CProjectile(CModel Model, Vector3 Pos, Vector3 Rot, Vector3 Direction, float damage = 20, bool destroy = false)
        {

            this._model = Model;
            this._pos = Pos;
            this._rot = Rot;
            this._direction = Direction;

            this._damage = damage;
            this._destroy = destroy;

            _fallElapsedTime = 0f;

            _model._modelPosition = _pos;
            _model._modelRotation = _rot;
            _model._UseForwardVector = true;

            this._isCollisioned = false;
        }

        public void Draw(Matrix view, Matrix projection, Vector3 camPos)
        {
            _model.Draw(view, projection, camPos);
        }

        public void UpdatePos(GameTime gameTime, CTerrain terrain)
        {
            if (!_isCollisioned)
            {
                _fallElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                _pos += (5.6f * _direction + 0.70f * _fallElapsedTime * Vector3.Down) * (float)gameTime.TotalGameTime.Seconds * 0.018f;

                _model._modelRotation = _pos - _model._modelPosition;
                _model._modelRotation.Normalize();

                _model._modelPosition = _pos;

                Ray ray = new Ray(_pos, _model._modelRotation);

                int modelId;
                float Steepness;

                float? modelIntersectsDist = Display3D.CModelManager.CheckRayIntersectsModel(ray, out modelId, _damage, 0.5f);
                if (modelIntersectsDist != null)
                {
                    _isCollisioned = true;
                    _model._modelPosition = _pos + _model._modelRotation * (float)modelIntersectsDist;
                }
                else if (_pos.Y - terrain.GetHeightAtPosition(_pos.X, _pos.Z, out Steepness, false) <= 0.5f)
                {
                    _isCollisioned = true;
                    _model._modelPosition = new Vector3(_pos.X, terrain.GetHeightAtPosition(_pos.X, _pos.Z, out Steepness, false), _pos.Z);
                }
            }
        }

    }
}
