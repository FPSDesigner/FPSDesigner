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

        private Texture2D _lensTexture;

        private Display3D.CCamera _cam; //Will back up all camera's attributes
        private GraphicsDevice _graphicsDevice;

        private Model _muzzleFlash; // The plane containing the muzzle flash texture

        // All variables useful to create an arrow
        private Model _arrowModel; // The model we draw before shoting
        private Texture2D _arrowTexture;
        private Matrix _arrowWorld; // Contain the position and the rotation 

        private Random _muzzleRandom; // Randomize muzzle flashes

        private int _previousScrollWheelValue; // Help us to determine if he is changing weapon
        private int _futurSelectedWeapon = 0; // Used to change the weapon only after the animation

        // All booleans created to handle animations
        private bool _isWalkAnimPlaying = false;
        private bool _isWaitAnimPlaying = false;
        private bool _isShoting = false;
        private bool _isRunning = false;
        private bool _isUnderWater = false;
        private bool _isSwimAnimationPlaying = false;
        private bool _isReloading = false;
        private bool _isSwitchingAnimPlaying = false; // Hands go down
        private bool _isSwitchingAnim2ndPartPlaying = false; // Hands go up

        private float _isRecoilAffected = 0;
        private float _recoilBackIntensity = 0;

        private bool _isAiming = false; // Check if he was aiming to change the FOV just one time
        public bool _isCrouched = false;
        private bool _isStandingUp = false; // Used when the player want to stand up after a crouch
        public bool _justShot = false;

        public Ray _lastShot;

        private bool _isReloadingSoundPlayed = true;

        private bool _isSniping = false; // Player aims with a sniper

        private float _horizontalVelocity;

        public float _life = 100.0f;

        private float elapsedTime; // We get the time to know when play a sound
        private float _elapsedTimeMuzzle; // Used to draw the muzzle flash
        private float _elapsedSnipeShot; // Time between two shots when the player is aiming with a sniper

        public float _sprintSpeed = 3f;
        public float _walkSpeed = 1.6f;
        public float _aimSpeed = 3f;
        public float _freeCamSpeed = 30f;
        public float _crouchSpeed = 3f;
        public float _movementsLerp = 0.25f;

        public float _entityHeight; // Used to crouch the player with the physicsMap in Camera
        public float _entityCrouch; // Used to crouch the player with the physicsMap in Camera

        public float _mpCrouchCorrection = -99f;
        public float _uniqueWeaponIdCarrying = 0;

        public Display3D.CTerrain _terrain;

        public void Initialize()
        {
            _handTexture = new Texture2D[1];
            _previousScrollWheelValue = 0;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics, Game.CWeapon weap, Display3D.CCamera cam, string handTexture)
        {
            _graphicsDevice = graphics;

            _entityHeight = cam._physicsMap._entityHeight; // We save the size of the player, to reuse it when he crouchs
            _entityCrouch = _entityHeight * 0.58f;

            _futurSelectedWeapon = weap._selectedWeapon;
            SetUniqueWepId(weap);

            _handTexture[0] = content.Load<Texture2D>(handTexture);
            _handAnimation = new Display3D.MeshAnimation("Arm_Animation(Smoothed)", 1, 1, 1.0f, new Vector3(0, 0, 0), _handRotation, 0.03f, _handTexture, 8, 0.05f, true);

            _handRotation = Matrix.CreateRotationX(MathHelper.ToRadians(90));
            _handRotation = Matrix.CreateFromYawPitchRoll(0, -90, 0);

            _handAnimation.LoadContent(content);

            _handAnimation.ChangeAnimSpeed(0.7f);
            if (weap._weaponPossessed != null)
                _handAnimation.BeginAnimation(weap._weaponPossessed[weap._selectedWeapon]._weapAnim[2], true);

            // Initialize the weapon attributes
            foreach (ModelMesh mesh in weap._weaponPossessed[weap._selectedWeapon]._wepModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.TextureEnabled = true;
                    effect.Texture = weap._weaponPossessed[weap._selectedWeapon]._weapTexture;

                    effect.SpecularColor = new Vector3(0.01f);
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

            _lensTexture = content.Load<Texture2D>("Textures//Lens");

            // Load the arrow
            _arrowModel = content.Load<Model>("Models\\Arrow");
            _arrowTexture = content.Load<Texture2D>("Textures\\Arrow");

            foreach (ModelMesh arrowMesh in _arrowModel.Meshes)
            {
                foreach (BasicEffect effect in arrowMesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.TextureEnabled = true;
                    effect.Texture = _arrowTexture;

                    effect.SpecularColor = new Vector3(0.0f);
                    effect.SpecularPower = 8;

                }
            }

            _muzzleRandom = new Random();
            _horizontalVelocity = _walkSpeed;
        }

        public void Update(MouseState mouseState, MouseState oldMouseState, KeyboardState kbState, KeyboardState oldKbState, CWeapon weapon, GameTime gameTime, Display3D.CCamera cam,
            bool isUnderWater)
        {
            _cam = cam;

            this._isUnderWater = isUnderWater;

            if (_isSniping && cam.sensibilityMultiplier == 1)
                cam.sensibilityMultiplier = 0.2f;
            else if (!_isSniping && cam.sensibilityMultiplier != 1)
                cam.sensibilityMultiplier = 1;

            // We place the hand like we want
            _handRotation = Matrix.CreateFromYawPitchRoll(_cam._yaw - MathHelper.Pi, -cam._pitch - MathHelper.PiOver2, 0);

            Matrix rotation = Matrix.CreateFromYawPitchRoll(cam._yaw, cam._pitch, cam._roll);
            Vector3 _handPos = new Vector3(cam._cameraPos.X, cam._cameraPos.Y, cam._cameraPos.Z) + 0.015f * Vector3.Transform(Vector3.Forward, rotation)
                + 0.025f * Vector3.Transform(Vector3.Down, rotation);

            //All arround weapons, the shot, animations ..
            WeaponHandle(weapon, gameTime, mouseState, oldMouseState, kbState, oldKbState, cam);

            _handAnimation.Update(gameTime, _handPos, _handRotation);

            //Console.WriteLine(weapon._weaponsArray);
            // If he changed weapon
            ChangeWeapon(weapon, mouseState);

            // The reloading method
            Reloading(weapon, kbState, oldKbState, gameTime);

            // Aim
            Aim(weapon, mouseState, cam);

            // Crouch
            Crouch(kbState, oldKbState, cam, gameTime);

            if (_isRecoilAffected > 0)
            {
                float reduceIntensity = _isRecoilAffected;
                if (_isRecoilAffected > 0.0001f)
                    reduceIntensity = MathHelper.Lerp(_isRecoilAffected, 0, _recoilBackIntensity);
                cam._pitch -= reduceIntensity;
                _isRecoilAffected -= reduceIntensity;
            }
            else if (_isRecoilAffected < 0)
                _isRecoilAffected = 0;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gametime, Matrix view, Matrix projection, Vector3 camPos, CWeapon weap)
        {
            // The player is not looking in the sniper lens
            if (!_isSniping)
            {
                // Draw the animation mesh
                _handAnimation.Draw(gametime, spriteBatch, view, projection);

                // Draw the weapon attached to the mesh
                if (!_isSwimAnimationPlaying)
                {
                    WeaponDrawing(weap, spriteBatch, view, projection, camPos, gametime);
                }
            }
            else
            {
                spriteBatch.Begin();
                spriteBatch.Draw(_lensTexture, new Rectangle(0, 0, _graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
                spriteBatch.End();
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
            if (cam.isFreeCam)
                return _freeCamSpeed;

            // If the player is running
            if ((CGameSettings.useGamepad && CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPRun)) || kbState.IsKeyDown(CGameSettings._gameSettings.KeyMapping.MSprint) && !_isAiming && !_isCrouched)
            {
                _horizontalVelocity = MathHelper.Lerp(_horizontalVelocity, _sprintSpeed, _movementsLerp);

                if (_horizontalVelocity != _sprintSpeed && !_isShoting && !_isReloading && !_isSwitchingAnimPlaying && !_isSwitchingAnim2ndPartPlaying
                    && !_isWaitAnimPlaying && !_isAiming)
                    _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[0] * 1.7f);
                _isRunning = true;

            }
            else
            {
                if (_horizontalVelocity != _walkSpeed)
                {
                    _horizontalVelocity = MathHelper.Lerp(_horizontalVelocity, _walkSpeed, _movementsLerp * 2.5f);

                    if (!_isShoting && !_isReloading && !_isAiming && !_isSwitchingAnimPlaying && !_isSwitchingAnim2ndPartPlaying && !_isWaitAnimPlaying)
                        _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[0]);
                }

                _isRunning = false;
            }

            if (_isAiming && !_isShoting
                && !_isReloading && !_isSwitchingAnimPlaying && !_isSwitchingAnim2ndPartPlaying && !_isCrouched)
            {
                if (_horizontalVelocity != _aimSpeed)
                {
                    _horizontalVelocity = MathHelper.Lerp(_horizontalVelocity, _aimSpeed, _movementsLerp);
                }
            }

            if (_isCrouched)
            {
                if (_horizontalVelocity != _crouchSpeed)
                {
                    _horizontalVelocity = MathHelper.Lerp(_horizontalVelocity, _crouchSpeed, 0.7f);

                    if (!_isShoting && !_isAiming && !_isReloading
                        && !_isSwitchingAnimPlaying && !_isSwitchingAnim2ndPartPlaying)
                        _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[0] * 0.65f);
                }
            }

            return _horizontalVelocity;

        }

        public void WeaponHandle(Game.CWeapon weapon, GameTime gameTime, MouseState mouseState, MouseState oldMouseState,
            KeyboardState kbState, KeyboardState oldKbState, Display3D.CCamera cam)
        {
            // If He is doing nothing, we stop him
            if ((!cam._isMoving && !_isWaitAnimPlaying && !_isShoting) && !_isUnderWater && !_isReloading &&
                (!_isSwitchingAnimPlaying && !_isSwitchingAnim2ndPartPlaying) && !_isAiming)
            {
                _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[2]);
                _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[2], true);

                //Just the wait animation is playing
                _isWalkAnimPlaying = false;
                _isSwimAnimationPlaying = false;
                _isSwitchingAnimPlaying = false;
                _isSwitchingAnim2ndPartPlaying = false;
                _isWaitAnimPlaying = true;
            }

            // If player move, we play the walk anim
            if ((cam._isMoving && !_isWalkAnimPlaying && !_isShoting) && !_isUnderWater && !_isReloading &&
                (!_isSwitchingAnimPlaying && !_isSwitchingAnim2ndPartPlaying) && !_isAiming)
            {
                _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[0]);
                _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[0], true, 0.75f);

                //just the walk animation is playing
                _isWaitAnimPlaying = false;
                _isSwimAnimationPlaying = false;
                _isSwitchingAnimPlaying = false;
                _isSwitchingAnim2ndPartPlaying = false;
                _isWalkAnimPlaying = true;
            }

            // If he is underwater but not swimming
            if (_isUnderWater && !_isSwimAnimationPlaying)
            {
                _handAnimation.ChangeAnimSpeed(1.25f);
                _handAnimation.ChangeAnimation("Swim", true);

                // Just the swim animation is playing
                _isWaitAnimPlaying = false;
                _isWalkAnimPlaying = false;
                _isShoting = false;
                _isAiming = false;
                _isSniping = false;
                _isSwitchingAnimPlaying = false;
                _isSwitchingAnim2ndPartPlaying = false;
                _isSwimAnimationPlaying = true;
            }

            // If he is switching weapon we play the other part of the switch
            if (_isSwitchingAnimPlaying && _handAnimation.HasFinished())
            {
                // Inverse the sens of animation
                weapon.ChangeWeapon(_futurSelectedWeapon);

                _handAnimation.InverseMode("backward");
                _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[4]);
                _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[4], false, 0.99f);

                _isWaitAnimPlaying = false;
                _isWalkAnimPlaying = false;
                _isReloading = false;
                _isShoting = false;
                _isSniping = false;
                _isAiming = false;
                _isSwitchingAnim2ndPartPlaying = true;
                _isSwitchingAnimPlaying = false;
            }

            // The hands finished to go up after the switching
            if ((_isSwitchingAnim2ndPartPlaying && !_isSwitchingAnimPlaying) && _handAnimation.HasFinished())
            {
                // Inverse the sens of animation
                _handAnimation.InverseMode("forward");
                _isWaitAnimPlaying = false;
                _isWalkAnimPlaying = false;
                _isReloading = false;
                _isAiming = false;
                _isSniping = false;
                _isShoting = false;

                // Depending on what is the player doing, after the switch we play an anim
                if (!_isRunning && !_cam._isMoving)
                {
                    _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[2]);
                    _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[2], true);
                    _isWaitAnimPlaying = true;
                }
                else
                {
                    _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[0]);
                    _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[0], true);
                    _isWalkAnimPlaying = true;
                }

                _isSwitchingAnimPlaying = false;
                _isSwitchingAnim2ndPartPlaying = false;
            }

            // We wanted to know if the shoting animation is finished
            if ((_isShoting || _isReloading) && _handAnimation.HasFinished())
            {
                if (!_isAiming)
                {
                    _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[2]);
                    _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[2], true, 0.02f);
                    _isWaitAnimPlaying = true;
                }
                else
                {
                    _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[5]);
                    _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[5], true, 0.02f);
                }

                //just the wait animation is playing
                _isWalkAnimPlaying = false;
                _isSwimAnimationPlaying = false;
                _isSwitchingAnimPlaying = false;
                _isSwitchingAnim2ndPartPlaying = false;

                _isReloading = false;

                _elapsedTimeMuzzle = 0; // Time use to draw the muzzle
                _isShoting = false;
            }

            // We Call the shot function
            Shot(mouseState, oldMouseState, kbState, oldKbState, weapon, cam, gameTime);


            // ***** If he has shot : We play vibrations **** //
            if (CGameSettings.useGamepad)
            {
                if (_isShoting)
                    GamePad.SetVibration(PlayerIndex.One, 0.9f, 0.1f);
                else
                    GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
            }

            _elapsedSnipeShot += gameTime.ElapsedGameTime.Milliseconds;
        }

        private void Shot(MouseState mouseState, MouseState oldMouseState, KeyboardState kbState, KeyboardState oldKbState, CWeapon weapon, Display3D.CCamera cam, GameTime gameTime)
        {
            if (cam.isCamFrozen)
                return;
            // If the player shot
            if (mouseState.LeftButton == ButtonState.Pressed ||
                (CGameSettings.useGamepad && CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPShot)))
            {
                if (!_isUnderWater && (!_isShoting && !_isReloading && (!_isSwitchingAnimPlaying && !_isSwitchingAnim2ndPartPlaying)))
                {
                    if (weapon._weaponPossessed[weapon._selectedWeapon]._isAutomatic || oldMouseState.LeftButton == ButtonState.Released)
                    {
                        // We choose where is the gun sight
                        Point shotPosScreen =
                            new Point(_graphicsDevice.PresentationParameters.BackBufferWidth / 2, _graphicsDevice.PresentationParameters.BackBufferHeight / 2);

                        // The player can shot : enough bullets
                        if (weapon._weaponPossessed[weapon._selectedWeapon]._actualClip > 0)
                        {
                            _justShot = true;
                            _isRecoilAffected += weapon._weaponPossessed[weapon._selectedWeapon]._recoilIntensity;
                            _recoilBackIntensity = weapon._weaponPossessed[weapon._selectedWeapon]._recoilBackIntensity;

                            cam._pitch += _isRecoilAffected;
                            if (!_isAiming && !_isSniping)
                            {
                                _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[1]);
                                _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[1], false, 0.12f);
                                _isShoting = true;
                            }

                            // Player has no sniper
                            else if (_isAiming && !_isSniping
                                && weapon._weaponPossessed[weapon._selectedWeapon]._wepType != 1)
                            {
                                _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[6]);
                                _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[6], false, 0.1f);
                                _isShoting = true;
                            }

                            _isWalkAnimPlaying = false;
                            _isWaitAnimPlaying = false;

                            // RAY DETECTION : KNOW IF SOMEONE IS TOUCHED (WITHOUT BOW)
                            Vector3 nearSource = Display2D.C2DEffect.softwareViewport.Unproject(new Vector3((float)shotPosScreen.X, (float)shotPosScreen.Y, Display2D.C2DEffect.softwareViewport.MinDepth), cam._projection, cam._view, Matrix.Identity);
                            Vector3 farSource = Display2D.C2DEffect.softwareViewport.Unproject(new Vector3((float)shotPosScreen.X, (float)shotPosScreen.Y, Display2D.C2DEffect.softwareViewport.MaxDepth), cam._projection, cam._view, Matrix.Identity);
                            Vector3 direction = farSource - nearSource;

                            direction.Normalize();
                            weapon.Shot(true, _isShoting, gameTime);

                            // The weapon is not a bow
                            if (weapon._weaponPossessed[weapon._selectedWeapon]._wepType != 3)
                            {
                                Ray ray = new Ray(nearSource, direction);
                                _lastShot = ray;

                                Display3D.CSimpleShapes.AddLine(ray.Position, ray.Position + 3 * direction, Color.Green, 255f);
                                float? distance;
                                CEnemy enemy;
                                string boxTouched = CEnemyManager.RayIntersectsHitbox(ray, out distance, out enemy);

                                boxTouched.Split('.');

                                if (distance < weapon._weaponPossessed[weapon._selectedWeapon]._range)
                                {
                                    if (boxTouched != "")
                                    {
                                        Vector3 hitPosition = ray.Position + ray.Direction * distance.Value;
                                        Display3D.CSimpleShapes.AddBoundingSphere(new BoundingSphere(hitPosition, 0.1f), Color.Blue, 255f);
                                        Game.CConsole.addMessage("Hit " + boxTouched);

                                        switch (boxTouched)
                                        {
                                            case "bb_Head":
                                                enemy.ReceivedDamages(weapon._weaponPossessed[weapon._selectedWeapon]._damagesPerBullet, "death_headshot");
                                                break;
                                            case "bb_Body":
                                                enemy.ReceivedDamages(weapon._weaponPossessed[weapon._selectedWeapon]._damagesPerBullet, "death_bodyFront");
                                                break;
                                        }
                                    }
                                }

                                if (weapon._weaponPossessed[weapon._selectedWeapon]._wepType != 2)
                                {
                                    int modelId = -1;
                                    if (Display3D.CModelManager.CheckRayIntersectsModel(ray, out modelId, weapon._weaponPossessed[weapon._selectedWeapon]._damagesPerBullet) != null)
                                    {
                                    }
                                    else if (_terrain != null)
                                    {
                                        bool IsTerrainShot = false;
                                        bool IsWaterShot = false;

                                        Vector3 terrainPos = _terrain.Pick(_cam._view, cam._projection, shotPosScreen.X, shotPosScreen.Y, out IsTerrainShot);

                                        Matrix muzzleMatrix = _handAnimation.GetBoneMatrix("hand_R",
                                            Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2),
                                            0.33f, new Vector3(-1f - 50, -2.0f, -2.85f - 100));
                                        Vector3 gunSmokePos = Vector3.Transform(Vector3.Zero, muzzleMatrix);


                                        Display3D.Particles.ParticlesManager.AddParticle("gunshot_dirt", terrainPos);
                                        Display3D.Particles.ParticlesManager.AddParticle("gun_smoke", gunSmokePos);
                                    }
                                }
                            }

                            // If the player shot with a bow
                            else
                            {
                                // Texture
                                Dictionary<string, Texture2D> arrowDic = new Dictionary<string, Texture2D>();
                                arrowDic.Add("ApplyAllMesh", _arrowTexture);

                                Vector3 scale, pos, rotation;
                                Quaternion rot;
                                _arrowWorld.Decompose(out scale, out rot, out pos);

                                rotation = new Vector3(rot.X, rot.Y, rot.Z);

                                Display3D.CModel arrowModelProjectile = new Display3D.CModel(_arrowModel, pos,
                                    rotation, new Vector3(1f), _graphicsDevice, arrowDic);

                                if (rot.X != 0 && rot.Y != 0 && rot.Z != 0)
                                {
                                    Display3D.CSimpleShapes.AddLine(pos, pos + Vector3.Transform(Vector3.Up, rot), Color.Red, 255f);
                                    direction = -Vector3.Transform(Vector3.Up, rot);
                                    direction.Normalize();
                                }
                                Display3D.CProjectileManager.ThrowProjectile(new Display3D.CProjectile(arrowModelProjectile, pos + direction, rotation, direction));

                                // With a bow -> Reload after a shoot
                                ReloadingBow(weapon);
                            }
                        }

                    }
                }
            }
        }

        private void WeaponDrawing(Game.CWeapon weap, SpriteBatch spritebatch, Matrix view, Matrix projection, Vector3 camPos, GameTime gameTime)
        {
            // We draw the weapon only if the player is not looking in the lens
            if (!_isSniping)
            {
                // Get the hand position attached to the bone for any weapons which is not a special weapon
                Matrix world;

                if (weap._weaponPossessed[weap._selectedWeapon]._wepType != 3)
                {
                    world = _handAnimation.GetBoneMatrix("hand_R", weap._weaponPossessed[weap._selectedWeapon]._rotation,
                     weap._weaponPossessed[weap._selectedWeapon]._scale, weap._weaponPossessed[weap._selectedWeapon]._offset);
                }
                else
                {
                    world = _handAnimation.GetBoneMatrix("hand_L", weap._weaponPossessed[weap._selectedWeapon]._rotation,
                    weap._weaponPossessed[weap._selectedWeapon]._scale, weap._weaponPossessed[weap._selectedWeapon]._offset);
                }

                foreach (ModelMesh mesh in weap._weaponPossessed[weap._selectedWeapon]._wepModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.TextureEnabled = true;
                        effect.Texture = weap._weaponPossessed[weap._selectedWeapon]._weapTexture;

                        // If the mesh is the slide, we anim it
                        if ((_isShoting || _isReloading) && mesh.Name == "Slide")
                        {
                            // Move the slide of a shot by shot weapon
                            switch (weap._weaponPossessed[weap._selectedWeapon]._name)
                            {
                                case "M1911":
                                    world = _handAnimation.GetBoneMatrix("hand_R", weap._weaponPossessed[weap._selectedWeapon]._rotation,
                                        weap._weaponPossessed[weap._selectedWeapon]._scale, weap._weaponPossessed[weap._selectedWeapon]._offset + 0.3f * Vector3.Up);
                                    break;
                                case "AK47":
                                    world = _handAnimation.GetBoneMatrix("hand_R", weap._weaponPossessed[weap._selectedWeapon]._rotation,
                                    weap._weaponPossessed[weap._selectedWeapon]._scale, weap._weaponPossessed[weap._selectedWeapon]._offset + 0.24f * Vector3.Up);
                                    break;
                                case "Deagle":
                                    world = _handAnimation.GetBoneMatrix("hand_R", weap._weaponPossessed[weap._selectedWeapon]._rotation,
                                    weap._weaponPossessed[weap._selectedWeapon]._scale, weap._weaponPossessed[weap._selectedWeapon]._offset + 0.18f * Vector3.Forward);
                                    break;
                            }
                        }
                        else
                        {
                            // Choose the good position according to the wepTy
                            if (weap._weaponPossessed[weap._selectedWeapon]._wepType != 3)
                            {
                                world = _handAnimation.GetBoneMatrix("hand_R", weap._weaponPossessed[weap._selectedWeapon]._rotation,
                                    weap._weaponPossessed[weap._selectedWeapon]._scale, weap._weaponPossessed[weap._selectedWeapon]._offset);
                            }
                            else
                            {
                                world = _handAnimation.GetBoneMatrix("hand_L", weap._weaponPossessed[weap._selectedWeapon]._rotation,
                                weap._weaponPossessed[weap._selectedWeapon]._scale, weap._weaponPossessed[weap._selectedWeapon]._offset);
                            }
                        }

                        effect.World = world;
                        effect.View = view;
                        effect.Projection = projection;
                    }
                    mesh.Draw();
                }

                // Draw the arrow only if the player has the bow
                if (weap._weaponPossessed[weap._selectedWeapon]._wepType == 3
                    && !_isShoting)
                {
                    // Draw the arrow only if he has bullets
                    if (weap._weaponPossessed[weap._selectedWeapon]._actualClip > 0)
                    {
                        _arrowWorld = _handAnimation.GetBoneMatrix("hand_R", Matrix.CreateRotationX(0.12f) * Matrix.CreateRotationZ(0.22f)
                            , 0.8f, new Vector3(-0.1f, -0.72f, 0.1f));

                        foreach (ModelMesh arrowMesh in _arrowModel.Meshes)
                        {
                            foreach (BasicEffect eff in arrowMesh.Effects)
                            {
                                eff.TextureEnabled = true;
                                eff.Texture = _arrowTexture;

                                eff.World = _arrowWorld;
                                eff.View = view;
                                eff.Projection = projection;
                            }
                            arrowMesh.Draw();
                        }
                    }
                }

                // Draw the muzzle flash
                if (weap._weaponPossessed[weap._selectedWeapon]._wepType != 2 &&
                    (_isShoting && _elapsedTimeMuzzle < 50))
                {
                    Matrix muzzleDestination = Matrix.Identity;
                    float randomScale = (float)_muzzleRandom.NextDouble() / 2f;

                    switch (weap._weaponPossessed[weap._selectedWeapon]._name)
                    {
                        case "M1911":
                            muzzleDestination = _handAnimation.GetBoneMatrix("hand_R",
                            Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2),
                            0.25f + randomScale * 0.5f, new Vector3(-1f, -2.0f + randomScale, -2.85f));
                            break;
                        case "AK47":
                            randomScale = (float)_muzzleRandom.NextDouble() * 1.4f;
                            muzzleDestination = _handAnimation.GetBoneMatrix("hand_R",
                            Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2),
                            0.7f + randomScale * 1.4f, new Vector3(-3.6f, -1.6f + randomScale * 1.1f, -2.85f + 0.1f * randomScale));
                            break;
                        case "Deagle":
                            muzzleDestination = _handAnimation.GetBoneMatrix("hand_R",
                            Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2),
                            0.5f + randomScale * 0.05f, new Vector3(-0.7f, -1.4f, -2.85f));
                            break;
                        case "M40A5":
                            randomScale = (float)_muzzleRandom.NextDouble() * 1.4f;
                            muzzleDestination = _handAnimation.GetBoneMatrix("hand_R",
                            Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2),
                            0.7f + randomScale * 1.4f, new Vector3(-3f, -1.6f + randomScale * 1.1f, -2.85f + 0.1f * randomScale));
                            break;
                    }


                    _graphicsDevice.BlendState = BlendState.Additive;
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
                    _graphicsDevice.BlendState = BlendState.Opaque;

                }

                // We increment the muzzleTime or we reinit it
                if (_isShoting)
                {
                    _elapsedTimeMuzzle += gameTime.ElapsedGameTime.Milliseconds;
                }
            }

        }

        // Check the key entered to change the weapon
        public void ChangeWeapon(CWeapon weapon, MouseState mouseState, bool isPickingWeap = false, int futurIndex = 0)
        { // futurIndex is just used when we give a weapon
            // To avoid bugs, we let the player change weapon only if he has more than one
            if (weapon._weaponPossessed.Count > 1 && !isPickingWeap)
            {
                if ((!_isSwitchingAnimPlaying && !_isSwitchingAnim2ndPartPlaying) && !_isReloading && !_isShoting && !_isSwimAnimationPlaying)
                {
                    // If he scrolls down
                    if ((mouseState.ScrollWheelValue > _previousScrollWheelValue) ||
                        CGameSettings.useGamepad && (CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPSwitch) && CGameSettings.oldGamepadState.IsButtonUp(CGameSettings._gameSettings.KeyMapping.GPSwitch)))
                    {
                        _futurSelectedWeapon = (weapon._selectedWeapon + 1) % weapon._weaponPossessed.Count;

                        // Play the sound If the weapon is not something like a machete
                        if (weapon._weaponPossessed[_futurSelectedWeapon]._wepType != 2)
                            CSoundManager.PlaySound("SWITCHWEAPON1");
                        else // Play a special sound
                            CSoundManager.PlaySound("SWITCH_MACHETE");

                        // Change the futur animation speed
                        _isShoting = false;
                        _isWaitAnimPlaying = false;
                        _isReloading = false;
                        _isAiming = false;
                        _isSniping = false;


                        _handAnimation.InverseMode("forward");
                        _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[4]);
                        _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[4], false);
                        _isSwitchingAnimPlaying = true;
                        SetUniqueWepId(weapon);
                    }
                    else if (mouseState.ScrollWheelValue < _previousScrollWheelValue)
                    {
                        _futurSelectedWeapon = (weapon._selectedWeapon <= 0) ? weapon._weaponPossessed.Count - 1 : weapon._selectedWeapon - 1;

                        // Play the sound If the weapon is not something like a machete
                        if (weapon._weaponPossessed[_futurSelectedWeapon]._wepType != 2)
                            CSoundManager.PlaySound("SWITCHWEAPON2");
                        else // Play a special sound
                            CSoundManager.PlaySound("SWITCH_MACHETE");

                        // Change the futur animation speed
                        _isShoting = false;
                        _isWaitAnimPlaying = true;
                        _isReloading = false;
                        _isAiming = false;
                        _isSniping = false;

                        _isReloading = false;
                        _handAnimation.InverseMode("forward");
                        _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[4]);
                        _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[4], false);
                        _isSwitchingAnimPlaying = true;
                        SetUniqueWepId(weapon);
                    }

                    //// Switching weapons with a GamePad
                    //if(CGameSettings.useGamepad &&
                    //    (CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPSwitch) && CGameSettings.oldGamepadState.IsButtonUp(CGameSettings._gameSettings.KeyMapping.GPSwitch)))
                    //{
                    //    int newWeap = (weapon._selectedWeapon + 1) % weapon._weaponPossessed.Length;
                    //    _futurSelectedWeapon = newWeap;
                    //}
                }

            }

            // THE PLAYER IS PICKING A WEAPON
            if (isPickingWeap)
            {
                _futurSelectedWeapon = futurIndex;

                // Change the futur animation speed
                _isShoting = false;
                _isWaitAnimPlaying = false;
                _isReloading = false;
                _isAiming = false;
                _isSniping = false;

                _handAnimation.InverseMode("forward");
                _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[4]);
                _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[4], false);
                _isSwitchingAnimPlaying = true;
                SetUniqueWepId(weapon);
            }
            _previousScrollWheelValue = mouseState.ScrollWheelValue;
        }

        private void SetUniqueWepId(CWeapon weapon)
        {
            for (int i = 0; i < weapon._weaponsArray.Count; i++)
                if (weapon._weaponsArray[i]._name == weapon._weaponPossessed[_futurSelectedWeapon]._name)
                    _uniqueWeaponIdCarrying = i;
        }

        // Reloading function
        private void Reloading(CWeapon weapon, KeyboardState kbState, KeyboardState oldKbState, GameTime gameTime)
        {
            if ((kbState.IsKeyDown(CGameSettings._gameSettings.KeyMapping.Reload))
                || (CGameSettings.useGamepad && CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPReload)))
            {
                // You cannot reload with a bow, so, we avoid any crash
                if (weapon._weaponPossessed[weapon._selectedWeapon]._wepType != 3)
                {
                    if ((weapon.Reloading() && !_isReloading && !_isShoting) && (!_isSwitchingAnimPlaying && !_isSwitchingAnim2ndPartPlaying))
                    {
                        _isWaitAnimPlaying = false;
                        _isWalkAnimPlaying = false;
                        _isSwimAnimationPlaying = false;
                        _isSwitchingAnim2ndPartPlaying = false;
                        _isSwitchingAnimPlaying = false;

                        _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[3]);
                        if (weapon._weaponPossessed[weapon._selectedWeapon]._wepType != 1 || !_isSniping)
                            _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[3], false, 0.12f);
                        else
                            _handAnimation.BeginAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[3], false);

                        _isReloading = true;
                    }

                    _isSniping = false;
                    _isAiming = false;
                    _isReloadingSoundPlayed = false;
                }

            }

            // We play the sound after a delay
            if (!_isReloadingSoundPlayed && (elapsedTime >= weapon._weaponPossessed[weapon._selectedWeapon]._delay))
            {
                CSoundManager.PlayInstance("WEP." + weapon._weaponPossessed[weapon._selectedWeapon]._reloadSound);
                elapsedTime = 0f;
                _isReloadingSoundPlayed = true;
            }

            else if (!_isReloadingSoundPlayed)
            {
                elapsedTime += gameTime.ElapsedGameTime.Milliseconds;
            }
        }

        private void ReloadingBow(CWeapon weapon)
        {
            if (weapon.Reloading())
            {
                _isWaitAnimPlaying = false;
                _isWalkAnimPlaying = false;
                _isSwimAnimationPlaying = false;
                _isSwitchingAnim2ndPartPlaying = false;
                _isSwitchingAnimPlaying = false;
            }
        }

        private void Aim(CWeapon weapon, MouseState mstate, Display3D.CCamera cam)
        {
            if (_isAiming)
            {
                if (weapon._weaponPossessed[weapon._selectedWeapon]._wepType != 1)
                    cam.ChangeFieldOfView(MathHelper.Lerp(cam.fieldOfView, MathHelper.ToRadians(30), 0.5f));
                else if (weapon._weaponPossessed[weapon._selectedWeapon]._wepType == 1 && _isSniping)
                    cam.ChangeFieldOfView(MathHelper.Lerp(cam.fieldOfView, MathHelper.ToRadians(5), 0.5f));
            }
            else
            {
                cam.ChangeFieldOfView(MathHelper.Lerp(cam.fieldOfView, MathHelper.ToRadians(40), 0.65f));
            }

            // If he presse the right mouse button
            if (!_isAiming && !_isReloading && !_isShoting &&
                    !_isSniping && !_isSwitchingAnimPlaying & !_isSwitchingAnim2ndPartPlaying)
            {
                if (mstate.RightButton == ButtonState.Pressed || (CGameSettings.useGamepad && CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPAim)))
                {
                    if (weapon._weaponPossessed[weapon._selectedWeapon]._wepType != 2 && weapon._weaponPossessed[weapon._selectedWeapon]._wepType != 1)
                    {
                        _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[5]);
                        _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[5], true, 0.05f);

                        _isRunning = false;
                        _isAiming = true;
                    }

                    // If the player uses a sniper
                    else if (weapon._weaponPossessed[weapon._selectedWeapon]._wepType == 1)
                    {
                        _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[5]);
                        _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[5], false);

                        _isRunning = false;
                        _isAiming = true;
                    }
                }
            }

            else if (_isAiming)
            {
                if (mstate.RightButton == ButtonState.Released && ((CGameSettings.useGamepad && CGameSettings.gamepadState.IsButtonUp(CGameSettings._gameSettings.KeyMapping.GPAim)) ||
                    !CGameSettings.useGamepad))
                {
                    if (!_isShoting && !_isReloading)
                    {
                        if (weapon._weaponPossessed[weapon._selectedWeapon]._wepType != 1)
                        {
                            _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[0]);
                            _handAnimation.ChangeAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[0], true);
                        }
                        else
                        {
                            _handAnimation.ChangeAnimSpeed(weapon._weaponPossessed[weapon._selectedWeapon]._animVelocity[0]);
                            _handAnimation.BeginAnimation(weapon._weaponPossessed[weapon._selectedWeapon]._weapAnim[0], true);
                        }
                        _isWalkAnimPlaying = true;
                        _isWaitAnimPlaying = false;
                    }
                    _isSniping = false;
                    _isAiming = false;
                }
                else
                {
                    if (weapon._weaponPossessed[weapon._selectedWeapon]._wepType == 1)
                    {
                        // If the player is not yet sniping and the snipr anim is finished
                        if (!_isSniping && _handAnimation.HasFinished())
                        {
                            _isSniping = true;
                        }
                    }
                }
            }
        }

        private void Crouch(KeyboardState kstate, KeyboardState oldKeyState, Display3D.CCamera cam, GameTime gameTime)
        {
            if ((kstate.IsKeyDown(CGameSettings._gameSettings.KeyMapping.MCrouch) && oldKeyState.IsKeyUp(CGameSettings._gameSettings.KeyMapping.MCrouch)
                || (CGameSettings.useGamepad && CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPCrouch) && CGameSettings.oldGamepadState.IsButtonUp(CGameSettings._gameSettings.KeyMapping.GPCrouch))))
            {
                if (_isCrouched && !_isStandingUp)
                {
                    _isStandingUp = true;
                }

                if (!_isCrouched)
                {
                    cam._physicsMap._entityHeight = _entityCrouch;
                    _isCrouched = true;
                    _isStandingUp = false;
                }

            }

            // If he is standing up
            if (_isCrouched && _isStandingUp)
            {
                if (cam._physicsMap._entityHeight < _entityHeight)
                {
                    cam._physicsMap._entityHeight += 0.007f * gameTime.ElapsedGameTime.Milliseconds;
                }
                else
                {
                    _isCrouched = false;
                    _isStandingUp = true;
                }
            }
        }

    }
}
