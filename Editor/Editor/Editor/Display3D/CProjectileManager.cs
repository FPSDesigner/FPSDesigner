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

        public static void ThrowProjectile(CProjectile projectile)
        {
            projectile._isThrown = true;
            _thrownProjectiles.Add(projectile);
        }

        public static void updateProjectileList()
        {

        }

        // Delete a projectile with its index or after a time span on the terrain
        public static void DeleteProjectile(int index = -1)
        {
            if (index >= 0)
            {

            }
        }
    }

    class CProjectile
    {
        public Vector3 _position { get; set ; }
        private Matrix _rotation { get; set; }

        private CModel _model;

        public bool _isThrown;

        public CProjectile(CModel Model, Vector3 Pos, Matrix Rot)
        {
            this._position = Pos;
            this._rotation = Rot;

            this._model = Model;

            this._isThrown = false;
        }

        public void Update(GameTime gameTime)
        {
            if (_isThrown)
                _position += Vector3.Forward * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(Matrix view, Matrix projection, Vector3 camPos)
        {
            _model.Draw(view, projection, camPos);
        }

        public void UpdatePosition(Vector3 newPos)
        {
            _position = newPos;
            _model._modelPosition = newPos;
        }

        public void UpdateRotation(Matrix rot)
        {
            //_rotation = rot;
            //_model._modelRotation = _rotation;
        }
    }
}
