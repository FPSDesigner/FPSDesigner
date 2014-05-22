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
    class CProjectileManager
    {
    }

    class CProjectile
    {
        public Matrix _rotation{ get; set; }

        public Vector3 _position { get; set; }

        public Model _model { get; private set; }

        public CProjectile(Vector3 Pos, Model Model)
        {
            this._position = Pos;

            this._model = Model;

        }

        public void Update(GameTime gameTime)
        {


        }

        public void ThrowProjectile(Vector3 direction)
        {
            // Normalize the direction, we just want to have a unitary vector
            direction.Normalize();


        }

    }
}
