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

        private float _initSpeed = 0.2f;
        private float _velocity = 0.3f;


        private bool _isWalkAnimPlaying = false;
        private bool _isWaitAnimPlaying = false;
        private bool _isRunning = false;
        private bool _isShoting = false;

        public void Initialize()
        {
            this._gameSettings = Game.Settings.CGameSettings.getInstance();
            _handTexture = new Texture2D[1];
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics, CWeapon weap)
        {
            _handTexture[0] = content.Load<Texture2D>("Textures\\Uvw_Hand");
            _handRotation = Matrix.CreateRotationX(MathHelper.ToRadians(90));
            _handRotation = Matrix.CreateFromYawPitchRoll(0, -90, 0);
            _handAnimation = new Display3D.MeshAnimation("Arm_Animation", 1, 1, 1.0f, new Vector3(0, 0, 0),_handRotation ,0.03f,_handTexture, true);
            _handAnimation.LoadContent(content);
            _handAnimation.ChangeAnimSpeed(0.7f);
            _handAnimation.BeginAnimation(weap.GetAnims(weap._selectedWeapon, 2), true);
        }

        public void Update(MouseState mouseState, MouseState oldMouseState, KeyboardState kbState,CWeapon weapon, GameTime gameTime, Display3D.CCamera cam)
        {
            _cam = cam;

            //We place the hand like we want
            _handRotation = Matrix.CreateFromYawPitchRoll(_cam._yaw - MathHelper.Pi, -cam._pitch - MathHelper.PiOver2, 0);

            Matrix rotation = Matrix.CreateFromYawPitchRoll(cam._yaw, cam._pitch, 0);
            Vector3 _handPos = new Vector3(cam._cameraPos.X, cam._cameraPos.Y, cam._cameraPos.Z) + 0.015f * Vector3.Transform(Vector3.Forward, rotation)
                + 0.025f * Vector3.Transform(Vector3.Down, rotation);

            //All arround weapons, the shot, animations ..
            WeaponHandle(weapon, gameTime, mouseState, oldMouseState, cam);

            _handAnimation.Update(gameTime, _handPos, _handRotation);

            
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gametime, Matrix view, Matrix projection, Vector3 camPos)
        {
            _handAnimation.Draw(gametime, spriteBatch, view, projection);
        }


        //This function allows the player to Run
            //the fallVelocity argument is used to prevent the player from speed in the air
        public float Run(KeyboardState state, Vector3 fallVelocity)
        {
            if ((_gameSettings.useGamepad && _gameSettings.gamepadState.IsButtonDown(_gameSettings._gameSettings.KeyMapping.GPRun)) || state.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MSprint))
            {
                if ((_velocity < _initSpeed + 0.25f) && fallVelocity.Y <= 0.0f)
                {
                    _velocity += .01f;
                    //_velocity += 2f;
                }
                _isRunning = true;
            }
            else
            {
                if (_velocity > _initSpeed)
                {
                    _velocity -= .01f;
                }

                _isRunning = false;
            }
            return _velocity;
        }

        public void WeaponHandle(Game.CWeapon weapon, GameTime gameTime, MouseState mouseState, MouseState oldMouseState, Display3D.CCamera cam)
        {
            //If He is doing nothing, we stop him
            if ((!cam._isMoving && !_isWaitAnimPlaying) && !_isShoting)
            {
                _handAnimation.ChangeAnimSpeed(0.7f);
                _handAnimation.ChangeAnimation(weapon.GetAnims(weapon._selectedWeapon, 2), true);
                _isWalkAnimPlaying = false;
                _isWaitAnimPlaying = true;
            }

            //We wanted to know if the shoting animation is finished
            if (_isShoting && _handAnimation.HasFinished())
            {
                _handAnimation.ChangeAnimSpeed(0.7f);
                _handAnimation.ChangeAnimation(weapon.GetAnims(weapon._selectedWeapon, 2), true);
                _isWalkAnimPlaying = false;
                _isWaitAnimPlaying = true;
                _isShoting = false;
            }

            //If player move, we play the walk anim
            if ((cam._isMoving && !_isWalkAnimPlaying) && !_isShoting)
            {
                _handAnimation.ChangeAnimSpeed(1.6f);
                _handAnimation.ChangeAnimation(weapon.GetAnims(weapon._selectedWeapon, 0),true);
                _isWaitAnimPlaying = false;
                _isWalkAnimPlaying = true;
            }

            if (_isRunning)
            {
                _handAnimation.ChangeAnimSpeed(2.4f);
            }

            if ((mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released) ||
                (_gameSettings.useGamepad && _gameSettings.gamepadState.IsButtonDown(_gameSettings._gameSettings.KeyMapping.GPShot) && _gameSettings.oldGamepadState.IsButtonUp(_gameSettings._gameSettings.KeyMapping.GPShot)))
            {
                if (!_isShoting)
                {
                    weapon.Shot(true, gameTime);
                    _handAnimation.ChangeAnimSpeed(3.8f);
                    _handAnimation.ChangeAnimation(weapon.GetAnims(weapon._selectedWeapon, 1), false);
                    _isWalkAnimPlaying = false;
                    _isWaitAnimPlaying = false;
                    _isShoting = true;
                }
            }
            else if (mouseState.LeftButton == ButtonState.Pressed || (_gameSettings.useGamepad && _gameSettings.gamepadState.IsButtonDown(_gameSettings._gameSettings.KeyMapping.GPShot)))
                weapon.Shot(false, gameTime);
        }

        public void WeaponDrawing(Game.CWeapon weap, SpriteBatch spritebatch, Matrix view, Matrix projection)
        {
            Matrix[] bonesMatrix = new Matrix[_handAnimation.GetModel().Model.Bones.Count];
            _handAnimation.GetModel().Model.CopyAbsoluteBoneTransformsTo(bonesMatrix);

            foreach (ModelMesh mesh in weap.GetModel(weap._selectedWeapon).Meshes) 
            {  
                foreach (BasicEffect effect in mesh.Effects)  
                {
                    Matrix model2Transform = Matrix.CreateScale(1.0f) * Matrix.CreateFromYawPitchRoll(0, 0, 0);
                    //effect.World = model2Transform * bonesMatrix;

                    effect.View = view;    
                    effect.Projection = projection;
                }
            }
        }

    }
}
