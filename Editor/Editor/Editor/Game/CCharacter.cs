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
    class CCharacter
    {
        float _health;

        public void Initialize()
        {

        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Update(MouseState mouseState, MouseState oldMouseState, CWeapon weapon, GameTime gameTime)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                weapon.Shot(true, gameTime);
            else if (mouseState.LeftButton == ButtonState.Pressed)
                weapon.Shot(false, gameTime);
        }

    }
}
