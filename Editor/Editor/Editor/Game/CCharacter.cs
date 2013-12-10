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
        private Game.Settings.CGameSettings _gameSettings;
        float _initSpeed = 0.4f;
        float _velocity = 0.4f;

        public void Initialize()
        {
            this._gameSettings = Game.Settings.CGameSettings.getInstance();
        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Update(MouseState mouseState, MouseState oldMouseState, KeyboardState kbState,CWeapon weapon, GameTime gameTime)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                weapon.Shot(true, gameTime);
            else if (mouseState.LeftButton == ButtonState.Pressed)
                weapon.Shot(false, gameTime);
        }

        public float Run(KeyboardState state)
        {
            if (state.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MSprint))
            {

                if (_velocity < _initSpeed + 0.2f)
                {
                    _velocity += .01f;
                }
            }
            else
            {
                if (_velocity > _initSpeed)
                {
                    _velocity -= .01f;
                }
            }
            return _velocity;
        }

    }
}
