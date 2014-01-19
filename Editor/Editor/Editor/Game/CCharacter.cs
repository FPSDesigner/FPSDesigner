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

using XNAnimation;
using XNAnimation.Controllers;

namespace Editor.Game
{
    class CCharacter
    {
        private Game.Settings.CGameSettings _gameSettings;

        private Display3D.MeshAnimation _handAnimation; //The 3Dmodel + all animation
        private Texture2D[] _handTexture; //ALl the texture storaged in an array
        private Matrix _handRotation;

        private Display3D.CCamera _cam; //Will back up all camera's attributes

        float _initSpeed = 0.2f;
        float _velocity = 0.3f;

        public void Initialize()
        {
            this._gameSettings = Game.Settings.CGameSettings.getInstance();
            _handTexture = new Texture2D[1];
            
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics)
        {
            _handTexture[0] = content.Load<Texture2D>("Textures\\Uvw_Hand");
            _handRotation = Matrix.CreateRotationX(MathHelper.ToRadians(90));
            _handRotation = Matrix.CreateFromYawPitchRoll(0, -90, 0);
            _handAnimation = new Display3D.MeshAnimation("Arm_Animation", 1, 1, 1.0f, new Vector3(0, 0, 0),_handRotation ,0.04f,_handTexture, true);
            _handAnimation.LoadContent(content);
        }

        public void Update(MouseState mouseState, MouseState oldMouseState, KeyboardState kbState,CWeapon weapon, GameTime gameTime, Display3D.CCamera cam)
        {
            _cam = cam;

            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
            {
                weapon.Shot(true, gameTime);
                _handAnimation.StartAnimation("run", true);
            }
            else if (mouseState.LeftButton == ButtonState.Pressed)
                weapon.Shot(false, gameTime);

            _handRotation = Matrix.CreateFromYawPitchRoll(_cam._yaw, cam._pitch + MathHelper.PiOver2, 0);
            _handAnimation.Update(gameTime, cam._cameraPos, _handRotation);
            
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gametime, Matrix view, Matrix projection, Vector3 camPos)
        {
            _handAnimation.Draw(gametime, spriteBatch, view, projection);
        }


        //This function allows the player to Run
            //the fallVelocity argument is used to prevent the player from speed in the air
        public float Run(KeyboardState state, Vector3 fallVelocity)
        {
            if (state.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MSprint))
            {
                if ((_velocity < _initSpeed + 0.25f) && fallVelocity.Y <= 0.0f)
                {
                    _velocity += .01f;
                    //_velocity += 2f;
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
