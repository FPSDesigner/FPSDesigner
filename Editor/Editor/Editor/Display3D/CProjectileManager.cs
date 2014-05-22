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
        CProjectile _currentProjectile; // Store the current projectle, when thrown, we add it to the list
        List<CProjectile> _thrownProjectiles; // All the projectiles that have been thrown

        public void AddProjectile(CProjectile projectile)
        {
            _thrownProjectiles.Add(projectile);
        }

        public void updateProjectileList()
        {

        }

        // Delete a projectile with its index or after a time span on the terrain
        public void DeleteProjectile(int index = -1)
        {
            if (index >= 0)
            {

            }
        }
    }

    class CProjectile
    {
        public Vector3 _position { get; set; }
        public Matrix _rotation { get; private set; }

        private CModel _model;

        private bool _isThrown;

        public CProjectile(CModel Model, Texture2D Texture, Vector3 Pos, Matrix Rot)
        {
            this._position = Pos;
            this._rotation = Rot;

            this._model = Model;

            this._modelTexture = Texture;

            this._isThrown = false;
        }

        public void Update(GameTime gameTime)
        {
            if (_isThrown)
                _position += Vector3.Forward * (float)gameTime.ElapsedGameTime.TotalSeconds;

        }

        public void Draw()
        {

        }

        public void ThrowProjectile()
        {
            _isThrown = true;

            _position += Vector3.Forward;
        }


    }
}
