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

using Engine.Game.Settings;

namespace Engine.Game
{
    class CCharacter
    {
        private Display3D.MeshAnimation _handAnimation; //The 3Dmodel + all animation
        private Texture2D[] _handTexture; //ALl the texture storaged in an array
        private Matrix _handRotation;

        private Display3D.CCamera _cam; //Will back up all camera's attributes

        private Model _muzzleFlash; // The plane containing the muzzle flash texture

        private Random _muzzleRandom; // Randomize muzzle flashes

        private int _previousScrollWheelValue; // Help us to determine if he is changing weapon

        private bool _isWalkAnimPlaying = false;
        private bool _isWaitAnimPlaying = false;
        private bool _isShoting = false;
        private bool _isRunning = false;
        private bool _isUnderWater = false;
        private bool _isSwimAnimationPlaying = false;
        private bool _isSwitchingWeapon = false;

        private bool _isAiming = false; // Check if he was aiming to change the FOV just one time

        private float _velocity = 0.3f;
        public float _initSpeed = 0.2f;


        public void Initialize()
        {
            _handTexture = new Texture2D[1];
            _previousScrollWheelValue = 0;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics, Game.CWeapon weap)
        {
            _handTexture[0] = content.Load<Texture2D>("Textures\\Uv_Hand");
            _handAnimation = new Display3D.MeshAnimation("Arm_Animation(Smoothed)", 1, 1, 1.0f, new Vector3(0, 0, 0), _handRotation, 0.03f, _handTexture, 8, 0.05f, true);

            _handRotation = Matrix.CreateRotationX(MathHelper.ToRadians(90));
            _handRotation = Matrix.CreateFromYawPitchRoll(0, -90, 0);

            _handAnimation.LoadContent(content);

            _handAnimation.ChangeAnimSpeed(0.7f);
            _handAnimation.BeginAnimation(weap._weaponsArray[weap._selectedWeapon]._weapAnim[2], true);

            //Initialize the weapon attributes
            foreach (ModelMesh mesh in weap._weaponsArray[weap._selectedWeapon]._wepModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.TextureEnabled = true;
                    effect.Texture = weap._weaponsArray[weap._selectedWeapon]._weapTexture;

                    effect.SpecularColor = new Vector3(0.3f);
                    effect.SpecularPower = 32;
                }
            }

            // Load the muzzle flash
            _muzzleFlash = content.Load<Model>("Models\\Plane");
            foreach (ModelMesh mesh in _muzzleFlash.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = content.Load<Texture2D>("Textures\\PistolFlash001");

                    effect.SpecularColor = new Vector3(0.0f);
                    effect.SpecularPower = 8;
                }
            }

            _muzzleRandom = new Random();
        }

        public void Update(MouseState mouseState, MouseState oldMouseState, KeyboardState kbState, KeyboardState oldKbState, CWeapon weapon, GameTime gameTime, Display3D.CCamera cam,
            bool isUnderWater)
        {
            _cam = cam;
            this._isUnderWater = isUnderWater;

            // We place the hand like we want
            _handRotation = Matrix.CreateFromYawPitchRoll(_cam._yaw - MathHelper.Pi, -cam._pitch - MathHelper.PiOver2, 0);

            Matrix rotation = Matrix.CreateFromYawPitchRoll(cam._yaw, cam._pitch, 0);
            Vector3 _handPos = new Vector3(cam._cameraPos.X, cam._cameraPos.Y, cam._cameraPos.Z) + 0.015f * Vector3.Transform(Vector3.Forward, rotation)
                + 0.025f * Vector3.Transform(Vector3.Down, rotation);

            //All arround weapons, the shot, animations ..
            WeaponHandle(weapon, gameTime, mouseState, oldMouseState, cam);

            _handAnimation.Update(gameTime, _handPos, _handRotation);

            // If he changed weapon
            ChangeWeapon(mouseState, weapon);

            // The reloading method
            Reloading(weapon, kbState, oldKbState);

            // Aim
            Aim(weapon, mouseState, cam);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gametime, Matrix view, Matrix projection, Vector3 camPos, CWeapon weap)
        {
            // Draw the animation mesh
            _handAnimation.Draw(gametime, spriteBatch, view, projection);

            // Draw the weapon attached to the mesh
            if (!_isSwimAnimationPlaying)
            {
                WeaponDrawing(weap, spriteBatch, view, projection);
            }
        }


        /// <summary>
        /// Get the speed of the camera following what the player is doing
        /// </summary>
        /// <param name="kbState">Keyboard State</param>
        /// <param name="fallVelocity">Vertical velocity</param>
        /// <returns>The camera velocity</returns>
        public float SpeedModification(KeyboardState kbState, float fallVelocity, CWeapon weapon, Display3D.CCamera cam)
        {
            // If he runs
            if ((CGameSettings.useGamepad && CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPRun)) || kbState.IsKeyDown(CGameSettings._gameSettings.KeyMapping.MSprint))
            {
                if ((_velocity < _initSpeed + 0.25f) && fallVelocity <= 0.0f)
                {
                    _velocity += .012f;
                }
                if (!_isShoting && cam._isMoving && !_isAiming)
                {
                    _handAnimation.ChangeAnimSpeed(weapon._weaponsArray[weapon._selectedWeapon]._animVelocity[0] * 1.8f);
                }
                _isRunning = true;
            }
            else
            {
                if (_velocity > _initSpeed)
                {
                    _velocity -= .014f;
                }

                if (_isRunning) _handAnimation.ChangeAnimSpeed(weapon._weaponsArray[weapon._selectedWeapon]._animVelocity[0]);
                _isRunning = false;
            }

            // If the player is Aiming
            if (_isAiming)
            {
                _velocity = 0.04f;
            }
            else if (!_isAiming && !_isRunning)
            {
                _velocity = _initSpeed;
            }

            Console.WriteLine(_velocity);

            return _velocity;
        }

        public void WeaponHandle(Game.CWeapon weapon, GameTime gameTime, MouseState mouseState, MouseState oldMouseState, Display3D.CCamera cam)
        {
            //If He is doing nothing, we stop him
            if ((!cam._isMoving && !_isWaitAnimPlaying) && (!_isShoting && !_isUnderWater))
            {
                _handAnimation.ChangeAnimSpeed(weapon._weaponsArray[weapon._selectedWeapon]._animVelocity[2]);
                _handAnimation.ChangeAnimation(weapon._weaponsArray[weapon._selectedWeapon]._weapAnim[2], true);

                //Just the wait animation is playing
                _isWalkAnimPlaying = false;
                _isSwimAnimationPlaying = false;
                _isWaitAnimPlaying = true;
            }

            //We wanted to know if the shoting animation is finished
            if (_isShoting && _handAnimation.HasFinished())
            {
                _handAnimation.ChangeAnimSpeed(weapon._weaponsArray[weapon._selectedWeapon]._animVelocity[2]);
                _handAnimation.ChangeAnimation(weapon._weaponsArray[weapon._selectedWeapon]._weapAnim[2], true);

                //just the wait animation is playing
                _isWalkAnimPlaying = false;
                _isSwimAnimationPlaying = false;
                _isShoting = false;
                _isWaitAnimPlaying = true;
            }

            //If player move, we play the walk anim
            if ((cam._isMoving && !_isWalkAnimPlaying) && (!_isShoting && !_isUnderWater))
            {
                _handAnimation.ChangeAnimSpeed(weapon._weaponsArray[weapon._selectedWeapon]._animVelocity[0]);
                _handAnimation.ChangeAnimation(weapon._weaponsArray[weapon._selectedWeapon]._weapAnim[0], true);

                //just the walk animation is playing
                _isWaitAnimPlaying = false;
                _isSwimAnimationPlaying = false;
                _isWalkAnimPlaying = true;
            }

            if (_isUnderWater && !_isSwimAnimationPlaying)
            {
                _handAnimation.ChangeAnimSpeed(1.25f);
                _handAnimation.ChangeAnimation("Swim", true);

                // Just the swim animation is playing
                _isWaitAnimPlaying = false;
                _isWalkAnimPlaying = false;
                _isShoting = false;
                _isSwimAnimationPlaying = true;
            }

            if ((mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released) ||
                (CGameSettings.useGamepad && CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPShot) && CGameSettings.oldGamepadState.IsButtonUp(CGameSettings._gameSettings.KeyMapping.GPShot)))
            {
                if (!_isShoting && !_isUnderWater)
                {
                    // If he does not use a machete AND if he has bullet in a magazine
                    if (weapon._weaponsArray[weapon._selectedWeapon]._actualClip != 0)
                    {

                        _handAnimation.ChangeAnimSpeed(weapon._weaponsArray[weapon._selectedWeapon]._animVelocity[1]);
                        _handAnimation.ChangeAnimation(weapon._weaponsArray[weapon._selectedWeapon]._weapAnim[1], false);


                        _isShoting = true;
                        _isWalkAnimPlaying = false;
                        _isWaitAnimPlaying = false;
                    }
                    weapon.Shot(true, _isShoting, gameTime);
                }
            }
            else if (mouseState.LeftButton == ButtonState.Pressed || (CGameSettings.useGamepad && CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPShot)))
                weapon.Shot(false, _isShoting, gameTime);
        }

        private void WeaponDrawing(Game.CWeapon weap, SpriteBatch spritebatch, Matrix view, Matrix projection)
        {
            // Get the hand position attached to the bone
            Matrix world = _handAnimation.GetBoneMatrix("hand_R", weap._weaponsArray[weap._selectedWeapon]._rotation,
                weap._weaponsArray[weap._selectedWeapon]._scale, weap._weaponsArray[weap._selectedWeapon]._offset);

            foreach (ModelMesh mesh in weap._weaponsArray[weap._selectedWeapon]._wepModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }

            // Draw the muzzle flash
            if (_isShoting && weap._weaponsArray[weap._selectedWeapon]._wepType != 2)
            {
                float randomScale = (float)_muzzleRandom.NextDouble() / 4f;
                Matrix muzzleDestination = _handAnimation.GetBoneMatrix("hand_R",
                    Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2),
                    0.33f + randomScale, new Vector3(-1f, -2.0f + randomScale, -2.85f));

                foreach (ModelMesh mesh in _muzzleFlash.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = muzzleDestination;
                        effect.View = view;
                        effect.Projection = projection;
                    }
                    mesh.Draw();
                }
            }
        }

        // Check the key entered to change the weapon
        private void ChangeWeapon(MouseState mouseState, CWeapon weapon)
        {
            // If he scrolls down
            if (mouseState.ScrollWheelValue > _previousScrollWheelValue)
            {
                int newWeap = (weapon._selectedWeapon + 1) % weapon._weaponsArray.Length;
                weapon.ChangeWeapon(newWeap);

                // Draw the weapon texture
                foreach (ModelMesh mesh in weapon._weaponsArray[weapon._selectedWeapon]._wepModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.TextureEnabled = true;
                        effect.Texture = weapon._weaponsArray[weapon._selectedWeapon]._weapTexture;

                        effect.SpecularColor = new Vector3(0.5f);
                        effect.SpecularPower = 32;
                    }
                }

                // Change the futur animation speed
                _isSwitchingWeapon = true;

                _handAnimation.ChangeAnimSpeed(weapon._weaponsArray[weapon._selectedWeapon]._animVelocity[2]);
                _handAnimation.ChangeAnimation(weapon._weaponsArray[weapon._selectedWeapon]._weapAnim[2], true);
            }
            else if (mouseState.ScrollWheelValue < _previousScrollWheelValue)
            {
                int newWeap = (weapon._selectedWeapon <= 0) ? weapon._weaponsArray.Length - 1 : weapon._selectedWeapon - 1;
                weapon.ChangeWeapon(newWeap);

                // Draw the weapon texture
                foreach (ModelMesh mesh in weapon._weaponsArray[weapon._selectedWeapon]._wepModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.TextureEnabled = true;
                        effect.Texture = weapon._weaponsArray[weapon._selectedWeapon]._weapTexture;

                        effect.SpecularColor = new Vector3(0.5f);
                        effect.SpecularPower = 32;
                    }
                }

                // Change the futur animation speed
                _isSwitchingWeapon = true;

                _handAnimation.ChangeAnimSpeed(weapon._weaponsArray[weapon._selectedWeapon]._animVelocity[2]);
                _handAnimation.ChangeAnimation(weapon._weaponsArray[weapon._selectedWeapon]._weapAnim[2], true);
            }

            /*if (_isSwitchingWeapon = true && _handAnimation.HasFinished())
            {
                _handAnimation.ChangeAnimSpeed(weapon._weaponsArray[weapon._selectedWeapon]._animVelocity[2]);
                _handAnimation.ChangeAnimation(weapon._weaponsArray[weapon._selectedWeapon]._weapAnim[2], true);
                _isSwitchingWeapon = false;
            }*/

            _previousScrollWheelValue = mouseState.ScrollWheelValue;
        }

        // Reloading function
        private void Reloading(CWeapon weapon, KeyboardState kbState, KeyboardState oldKbState)
        {
            if ((kbState.IsKeyDown(Keys.R) && oldKbState.IsKeyUp(Keys.R)) && weapon.Reloading())
            {
                _handAnimation.ChangeAnimSpeed(weapon._weaponsArray[weapon._selectedWeapon]._animVelocity[3]);
                _handAnimation.ChangeAnimation(weapon._weaponsArray[weapon._selectedWeapon]._weapAnim[3], false);
            }
        }

        private void Aim(CWeapon weapon, MouseState mstate, Display3D.CCamera cam)
        {
            // If he presse the right mouse button
            if (mstate.RightButton == ButtonState.Pressed && !_isAiming)
            {

                if (weapon._weaponsArray[weapon._selectedWeapon]._wepType != 2)
                {
                    cam.ChangeFieldOfView(MathHelper.Lerp(MathHelper.ToRadians(40), MathHelper.ToRadians(30), 0.5f));
                    _isAiming = true;
                }
            }

            else if (mstate.RightButton == ButtonState.Released && _isAiming)
            {
                cam.ChangeFieldOfView(MathHelper.Lerp(MathHelper.ToRadians(30), MathHelper.ToRadians(40), 0.8f));
                _isAiming = false;
            }
        }

    }
}
