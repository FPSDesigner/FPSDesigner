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
    class Character
    {
        public Vector3 _position { get; set; }
        public Vector3 _rotation { get; set; }
 
        public Matrix[] _viewMatrix { get; set; }
 
        float _health;

        Character(Vector3 position, float health, GraphicsDevice graphics)
        {
            this._position = position;

            this._health = health;
        }
    }
}
