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
    class CEnemyManager
    {
        public static List<CEnemy> _enemyList = new List<CEnemy>();
        private static List<CEnemy> enemiesToAddQueue = new List<CEnemy>(); // MultiThread pb hack
        private static List<CEnemy> enemiesToRemoveQueue = new List<CEnemy>(); // MultiThread pb hack

        public static SpriteFont _hudFont;

        public static int selectedBot = -1;

        public static void LoadContent(ContentManager content)
        {
            _hudFont = content.Load<SpriteFont>("2D/consoleFont");
        }

        public static void AddEnemy(ContentManager content, Display3D.CCamera cam, CEnemy enemy)
        {
            enemiesToAddQueue.Add(enemy);
            enemy.LoadContent(content, cam);
        }

        public static void Draw(GameTime gameTime, SpriteBatch spriteBatch, Display3D.CCamera cam)
        {
            for (int i = 0; i < _enemyList.Count; i++)
                _enemyList[i].Draw(gameTime, spriteBatch, cam._view, cam._projection, (selectedBot == i));
        }

        public static void RemoveBot(int eltId)
        {
            RemoveBot(_enemyList[eltId]);
        }

        public static void RemoveBot(CEnemy enemy)
        {
            enemiesToRemoveQueue.Add(enemy);
        }

        public static void Update(GameTime gameTime, Display3D.CCamera cam)
        {
            FixQueuedBots();

            foreach (CEnemy enemy in _enemyList)
                enemy.Update(gameTime, cam);
        }

        public static void AddEnemyHud(SpriteBatch sb, Display3D.CCamera cam)
        {
            foreach (CEnemy enemy in _enemyList)
                enemy.AddEnemyHUD(sb, cam);
        }

        public static void FixQueuedBots()
        {
            if (enemiesToAddQueue.Count > 0)
            {
                _enemyList.AddRange(enemiesToAddQueue);
                enemiesToAddQueue.Clear();
            }
            if (enemiesToRemoveQueue.Count > 0)
            {
                foreach (CEnemy enemy in enemiesToRemoveQueue)
                    _enemyList.Remove(enemy);
                enemiesToRemoveQueue.Clear();
            }
        }

        public static string RayIntersectsHitbox(Ray ray, out float? distance, out CEnemy enemyVal)
        {
            foreach (CEnemy enemy in _enemyList)
            {
                string enemytest = enemy.RayIntersectsHitbox(ray, out distance);
                enemyVal = enemy;
                if (enemytest != "")
                    return enemytest;
            }
            distance = null;
            enemyVal = null;
            return "";
        }

        public static float? CheckRayIntersectsAnyBot(Ray ray, out int idIntersected)
        {
            for (int i = 0; i < _enemyList.Count; i++)
            {
                float? distance;
                idIntersected = i;
                string enemytest = _enemyList[i].RayIntersectsHitbox(ray, out distance);
                if (distance != null)
                    return distance;
            }
            idIntersected = 0;
            return null;
        }

        public static void UpdateGameLevel(ref Game.LevelInfo.LevelData lvl)
        {
            FixQueuedBots();
            for (int i = 0; i < _enemyList.Count; i++)
            {
                CEnemy enemy = _enemyList[i];

                lvl.Bots.Bots[i].IsAggressive = enemy._isAgressive;
                lvl.Bots.Bots[i].Life = enemy._life;
                lvl.Bots.Bots[i].Name = enemy._hudText;
                lvl.Bots.Bots[i].RangeOfAttack = enemy._rangeAttack;
                lvl.Bots.Bots[i].SpawnPosition = new Game.LevelInfo.Coordinates(enemy._model._position);
                lvl.Bots.Bots[i].Type = enemy._type;
                lvl.Bots.Bots[i].Velocity = enemy._runningVelocity;
            }

            while (lvl.Bots.Bots.Count != _enemyList.Count)
                lvl.Bots.Bots.RemoveAt(lvl.Bots.Bots.Count - 1);
        }
    }

    class CEnemy
    {
        private int[] prob = new int[2]; // Store probabilities (shoot, roll...)

        public float _life;
        public float _runningVelocity; // Displacement velocity
        private float _collisionHeight; // Collision Height between terrain and the AI

        public float _rangeAttack; // AI : Minimal distance to attack

        public Display3D.MeshAnimation _model; //The 3Dmodel and all animations

        private int _selectedWeap; // ID of the selected weapon

        public Vector3 _position; // The character positon
        private Vector3 _targetPos; // The target position
        public Vector3 _scale;

        private Vector3 _deathPosition; // The coordinates where the enemy died
        private Matrix _deathRotation;

        private float _hudTestSizeX;
        public string _hudText;

        public Matrix _rotation; // Model rotation
        public float rotationValue; // We give this float to the rotation mat

        private Game.CPhysics _physicEngine; // Ennemy will be submitted to forces

        public bool _isAgressive; // Pacifist or not
        public int _type; // 0: Friendly / 1: Enemy

        private bool _isMoving;
        private bool _isDead;
        private bool _isShoting;
        private bool _isHeadShot; // The character took an headshot
        private bool _isFollowingPlayer;
        private bool _isCrouch;
        private bool _isJumping;

        public bool _isFrozen; // Use to stop translation
        public bool _isMultiPlayer;

        // Animation Boolean
        private bool _isWaitAnimPlaying;
        private bool _isWalkAnimPlaying;
        private bool _isDyingAnimPlaying;
        private bool _isSwitchingWeapon;
        private bool _isFrozenAnimPlaying;
        private bool _isReloading;

        private float _height = 1.9f;

        public float _multiSpeed = 0f;
        public Vector3 _multiDirection = Vector3.Zero;

        private Dictionary<string, Display3D.Triangle> hitBoxesTriangles;

        // DEBUG 
        float px, py, pz;
        float rx, ry, rz;

        public CEnemy(string ModelName, Texture2D[] Textures, Vector3 Position, Matrix Rotation, float Life, float Velocity, float RangeToAttack, bool isAgressive = false, string name = "Enemy", int type = 1)
        {
            _position = Position;
            _scale = new Vector3(0.3f);
            _deathPosition = Vector3.Zero;
            _collisionHeight = 0.2f;
            this._rangeAttack = RangeToAttack;

            //this._weaponPossessed = weap;

            _rotation = Rotation;
            _rotation = Matrix.Identity;

            float Yaw, Pitch, Roll;
            Display3D.CGizmos.RotationMatrixToYawPitchRoll(ref Rotation, out Yaw, out Pitch, out Roll);
            rotationValue = Yaw;

            this._life = Life;

            this._runningVelocity = Velocity;

            // We Create the Enemy, giving its textures, models...

            _model = new Display3D.MeshAnimation(ModelName, 1, 1, 1.0f, _position,
                Matrix.CreateRotationX(-1 * MathHelper.PiOver2), _scale.X, Textures, 10, 0.0f, true);

            this._isAgressive = isAgressive;

            _isMoving = false;
            _isDyingAnimPlaying = false;
            _isWaitAnimPlaying = true;
            _isWalkAnimPlaying = false;
            _isDead = false;
            _isFrozen = false;
            _isHeadShot = false;
            _isShoting = false;
            _isFollowingPlayer = false;
            _isCrouch = false;
            _isJumping = false;
            _isReloading = false;
            _isSwitchingWeapon = false;

            _targetPos = _position; // The AI is not moving

            hitBoxesTriangles = new Dictionary<string, Display3D.Triangle>();

            _selectedWeap = 5;

            // Initialize probability
            prob[0] = 1; // 1/2 chance to shot the player

            SetEnemyName(name);
        }

        public void SetEnemyName(string name)
        {
            _hudText = name;
            _hudTestSizeX = CEnemyManager._hudFont.MeasureString(name).X / 2;
        }

        public void LoadContent(ContentManager content, Display3D.CCamera cam)
        {
            // We load the ennemy content
            _model.LoadContent(content);

            // We Create the forces application on the Ennemy
            _physicEngine = new CPhysics();
            _physicEngine.LoadContent(0.2f, new bool[] { true, true });
            _physicEngine._triangleList = cam._physicsMap._triangleList;
            _physicEngine._triangleNormalsList = cam._physicsMap._triangleNormalsList;
            _physicEngine._terrain = cam._physicsMap._terrain;
            _physicEngine._waterHeight = cam._physicsMap._waterHeight;
            _physicEngine.heightCorrection = 0.85f;

            GenerateHitBoxesTriangles();

            // Play first anim
            _model.ChangeAnimSpeed(0.5f);
            _model.BeginAnimation(CConsole._Weapon._weaponsArray[_selectedWeap].MultiType + "_wait", true);
        }

        public void Update(GameTime gameTime, Display3D.CCamera cam)
        {
            if (!_isMultiPlayer)
            {
                if (_model.animationController == null)
                    return;

                // Interpolation
                if (_multiSpeed > 0)
                {
                    float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    _position += _multiDirection * _multiSpeed * elapsedTime;
                }
                // Play Anims
                if (!_isFrozen && !_isDead && !_isDyingAnimPlaying)
                {
                    _model.RotateBone(2, cam._pitch);

                    // The character is waiting or running
                    SetWalk(_isMoving, CConsole._Weapon._weaponsArray[_selectedWeap].MultiType);

                    _isFrozenAnimPlaying = false;
                }

                else if (_isFrozen && !_isFrozenAnimPlaying)
                {
                    _model.ChangeAnimation("frozen", true, 0.8f);
                    _isFrozenAnimPlaying = true;
                }

                // We update the character pos, rot...
                _rotation = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationY((rotationValue));

                // He was playing the anim, now the character is really dead
                if (_isDyingAnimPlaying && _model.HasFinished())
                {
                    _position = _deathPosition;
                    _rotation = _deathRotation;
                    _isDead = true;
                }

                if (_isDead)
                {
                    _position = _deathPosition;
                    Matrix world = Matrix.CreateWorld(_position, Vector3.Right, Vector3.Up);
                    _rotation = _deathRotation;

                    _isMoving = false;
                    _isWaitAnimPlaying = false;
                    _isWalkAnimPlaying = false;
                }

                if ((_isJumping || _isSwitchingWeapon || _isReloading) && _model.HasFinished())
                {
                    if (_isCrouch)
                    {
                        _model.ChangeAnimSpeed(2.5f);
                        _model.ChangeAnimation(CConsole._Weapon._weaponsArray[_selectedWeap].MultiType + "_walk-crouch", true, 0.5f);
                    }
                    else
                    {
                        _model.ChangeAnimSpeed(2.5f);
                        _model.ChangeAnimation(CConsole._Weapon._weaponsArray[_selectedWeap].MultiType + "_walk", true, 0.5f);
                    }

                    _isJumping = false;
                    _isReloading = false;
                    _isSwitchingWeapon = false;
                }

                _model.Update(gameTime, _position, _rotation);

                // new target is the camera position
                if (_isFollowingPlayer)
                {
                    _targetPos = cam._cameraPos;
                }

                if (!_isFrozen && Vector3.DistanceSquared(_position, _targetPos) > 10.0f)
                {
                    Vector3 translation = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(rotationValue));
                    translation.Normalize();

                    rotationValue = (float)Math.Atan2(_targetPos.X - _position.X,
                    _targetPos.Z - _position.Z);

                    translation = _runningVelocity * translation;
                    _position = _physicEngine.GetNewPosition(gameTime, _position, translation, false);
                    _isMoving = true;
                }
                else
                {
                    _isMoving = false;
                }

            }
            else
            {
                UpdateMultiplayer(gameTime, cam);
            }

        }

        public void UpdateMultiplayer(GameTime gameTime, Display3D.CCamera cam)
        {
            if (_model.animationController == null)
                return;

            // Interpolation
            if (_multiSpeed > 0)
            {
                float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _position += _multiDirection * _multiSpeed * elapsedTime;
            }
            else if (_isFrozen && !_isFrozenAnimPlaying)
            {
                _model.ChangeAnimation("frozen", true, 0.8f);
                _isFrozenAnimPlaying = true;
                _isMoving = false;
            }
            // We update the character pos, rot...
            _rotation = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationY((rotationValue));

            // He was playing the anim, now the character is really dead
            if (_isDyingAnimPlaying && _model.HasFinished())
            {
                _position = _deathPosition;
                _rotation = _deathRotation;
                _isDead = true;
                _isMoving = false;
            }

            if (_isDead)
            {
                _position = _deathPosition;
                Matrix world = Matrix.CreateWorld(_position, Vector3.Right, Vector3.Up);
                _rotation = _deathRotation;

                _isMoving = false;
                _isWaitAnimPlaying = false;
                _isWalkAnimPlaying = false;
            }

            if ((_isJumping || _isSwitchingWeapon || _isReloading) && _model.HasFinished())
            {
                if (_isCrouch)
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(CConsole._Weapon._weaponsArray[_selectedWeap].MultiType + "_walk-crouch", true, 0.5f);
                    _isWaitAnimPlaying = false;
                    _isWalkAnimPlaying = true;
                }
                else
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(CConsole._Weapon._weaponsArray[_selectedWeap].MultiType + "_walk", true, 0.5f);
                    _isWaitAnimPlaying = false;
                    _isWalkAnimPlaying = true;
                }

                _isMoving = true;
                _isJumping = false;
                _isReloading = false;
                _isSwitchingWeapon = false;
            }

            _model.Update(gameTime, _position, _rotation);

            
            if (CConsole._Character._justShot)
            {
                Console.WriteLine("Wait : " + _isWaitAnimPlaying + "\n Walk: " + _isWalkAnimPlaying);
                Console.WriteLine("isMoving : " + _isMoving);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Matrix view, Matrix projection, bool drawHitbox = false)
        {
            //GenerateHitBoxesTriangles();
            //GetRealTriangles();

            // We draw the enemy entirely if he is alive
            if (!_isHeadShot)
                _model.Draw(gameTime, spriteBatch, view, projection);
            else
            {
                string[] undrawable = new string[1];
                undrawable[0] = "Head";
                _model.Draw(gameTime, spriteBatch, view, projection, undrawable);
            }

            if (drawHitbox)
                GetRealTriangles(true);

            DrawWeapon(view, projection, gameTime);
        }

        public void DrawWeapon(Matrix view, Matrix projection, GameTime gameTime)
        {
            foreach (ModelMesh mesh in CConsole._Weapon._weaponsArray[_selectedWeap]._wepModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.TextureEnabled = true;
                    effect.Texture = CConsole._Weapon._weaponsArray[_selectedWeap]._weapTexture;

                    effect.World = ComputeWeaponWorldMatrix();
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
                //            // Draw the muzzle flash
                //if (weap._weaponPossessed[weap._selectedWeapon]._wepType != 2 &&
                //    (_isShoting && _elapsedTimeMuzzle < 50))
                //{
                //    Matrix muzzleDestination = Matrix.Identity;
                //    float randomScale = (float)_muzzleRandom.NextDouble() / 2f;

                //    switch (weap._weaponPossessed[weap._selectedWeapon]._name)
                //    {
                //        case "M1911":
                //            muzzleDestination = _handAnimation.GetBoneMatrix("hand_R",
                //            Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2),
                //            0.25f + randomScale * 0.5f, new Vector3(-1f, -2.0f + randomScale, -2.85f));
                //            break;
                //        case "AK47":
                //            randomScale = (float)_muzzleRandom.NextDouble() * 1.4f;
                //            muzzleDestination = _handAnimation.GetBoneMatrix("hand_R",
                //            Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2),
                //            0.7f + randomScale * 1.4f, new Vector3(-3.6f, -1.6f + randomScale * 1.1f, -2.85f + 0.1f * randomScale));
                //            break;
                //        case "Deagle":
                //            muzzleDestination = _handAnimation.GetBoneMatrix("hand_R",
                //            Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2),
                //            0.5f + randomScale * 0.05f, new Vector3(-0.7f, -1.4f, -2.85f));
                //            break;
                //        case "M40A5":
                //            randomScale = (float)_muzzleRandom.NextDouble() * 1.4f;
                //            muzzleDestination = _handAnimation.GetBoneMatrix("hand_R",
                //            Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2),
                //            0.7f + randomScale * 1.4f, new Vector3(-3f, -1.6f + randomScale * 1.1f, -2.85f + 0.1f * randomScale));
                //            break;
                //    }


                //    graphicsDevice.BlendState = BlendState.Additive;
                //    foreach (ModelMesh mesh in _muzzleFlash.Meshes)
                //    {
                //        foreach (BasicEffect effect in mesh.Effects)
                //        {
                //            effect.World = muzzleDestination;
                //            effect.View = view;
                //            effect.Projection = projection;
                //        }
                //        mesh.Draw();
                //    }
                //    graphicsDevice.BlendState = BlendState.Opaque;

                //                // We increment the muzzleTime or we reinit it
                //if (_isShoting)
                //{
                //    _elapsedTimeMuzzle += gameTime.ElapsedGameTime.Milliseconds;
                //}

        }

        public void AttackPlayer(float distSquared, CCharacter player)
        {
            if (!_isShoting)
            {
                if (_isAgressive && IsAnyPlayerInSight())
                {
                    _model.ChangeAnimSpeed(5.0f);
                    _model.ChangeAnimation(CConsole._Weapon._weaponsArray[_selectedWeap].MultiType + "_attack", false, 0.2f);

                    if (distSquared <= (CConsole._Weapon._weaponsArray[_selectedWeap]._range) * (CConsole._Weapon._weaponsArray[_selectedWeap]._range))
                    {
                        Random random = new Random();
                        int rand = random.Next();

                        // Shoot at the player
                        if (rand == prob[0])
                        {
                            player._life -= CConsole._Weapon._weaponsArray[_selectedWeap]._damagesPerBullet;
                        }

                    }
                }
                _isShoting = true;
            }

        }

        public bool IsAnyPlayerInSight()
        {
            return false;
        }

        public void MoveTo(Vector3 position, GameTime gameTime)
        {
            if (!_isMultiPlayer && !_isFrozen && !_isFollowingPlayer)
            {
                _targetPos = position;

                _isMoving = true;

                rotationValue = (float)Math.Atan2(position.X - _position.X,
                    position.Z - _position.Z);

                Vector3 translation = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(rotationValue));
                translation.Normalize();
                translation *= _runningVelocity;

                _position = _physicEngine.GetNewPosition(gameTime, _position, translation, false);
            }
        }

        public void FollowPlayer(GameTime gameTime)
        {
            if (!_isFrozen && !_isDead)
            {
                _isFollowingPlayer = !_isFollowingPlayer;
            }
        }

        public void Crouch(bool toggle, string typeName)
        {
            if (!_isCrouch && toggle)
            {
                _physicEngine._entityHeight /= 2f;
                _runningVelocity /= 2f;

                _model.ChangeAnimSpeed(1.5f);
                _model.ChangeAnimation(typeName + "_walk-crouch", true, 0.8f);

                _isCrouch = true;
            }
            else if (_isCrouch && !toggle)
            {
                _physicEngine._entityHeight *= 2;
                _runningVelocity *= 2;

                _model.ChangeAnimSpeed(0.7f);
                _model.ChangeAnimation(typeName + "_wait", true, 0.8f);

                _isCrouch = false;
            }
        }

        public void SetJump(string type)
        {
            if (!_isJumping)
            {
                _model.ChangeAnimSpeed(3.0f);
                _model.ChangeAnimation(type + "_jump", false, 0.65f);

                _isJumping = true;
            }
        }

        public void SetReload(string typeName)
        {
            if (!_isReloading && !_isShoting)
            {
                if (_isCrouch)
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(typeName + "_reload-crouch", false, 0.55f);
                }
                else
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(typeName + "_reload", false, 0.55f);
                }

                _isReloading = true;
            }
        }

        public void SetWalk(bool toggle, string typeName)
        {
            //if (_model.animationController == null)
            //    return;

            if (!_isReloading && !_isSwitchingWeapon & !_isJumping)
            {
                if (toggle && !_isWalkAnimPlaying )
                {
                    if (_isCrouch)
                    {
                        _model.ChangeAnimSpeed(2.5f);
                        _model.ChangeAnimation(typeName + "_walk-crouch", true, 0.35f);
                    }
                    else
                    {
                        _model.ChangeAnimSpeed(2.5f);
                        _model.ChangeAnimation(typeName + "_walk", true, 0.35f);
                    }

                    _isWalkAnimPlaying = true;
                    _isWaitAnimPlaying = false;

                }
                if (!toggle && !_isWaitAnimPlaying)
                {
                    if (_isCrouch)
                    {
                        _model.ChangeAnimSpeed(0.7f);
                        _model.ChangeAnimation(typeName + "_wait-crouch", true, 0.35f);
                    }
                    else
                    {
                        _model.ChangeAnimSpeed(0.7f);
                        _model.ChangeAnimation(typeName + "_wait", true, 0.35f);
                    }


                    _isWalkAnimPlaying = false;
                    _isWaitAnimPlaying = true;
                }

            }
        }

        public void SetSwitchingWeapon(string typeName)
        {
            if (_model.animationController == null)
                return;

            if (!_isSwitchingWeapon && !_isReloading && !_isJumping && !_isShoting)
            {
                if (_isCrouch)
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(typeName + "_switch-crouch", false, 0.65f);
                }
                else
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(typeName + "_switch", false, 0.65f);
                }

                _isSwitchingWeapon = true;
            }

        }

        public void SetShot(string typeName)
        {
            if (_model.animationController == null)
                return;
            if (!_isShoting && !_isReloading && !_isSwitchingWeapon && !_isJumping)
            {
                if (_isCrouch)
                {
                    _model.ChangeAnimSpeed(6f);
                    _model.ChangeAnimation(typeName + "_attack-crouch", false, 0.3f);
                }
                else
                {
                    _model.ChangeAnimSpeed(6f);
                    _model.ChangeAnimation(typeName + "_attack", false, 0.3f);
                }

                _isShoting = true;
            }
        }

        public void PlayFireSound(Vector3 PlayerPos)
        {
            string key = "WEP.MULTI." + CConsole._Weapon._weaponsArray[_selectedWeap]._shotSound;
            if (CSoundManager.soundList.ContainsKey(key) && CSoundManager.soundList[key]._audioEmitter != null)
            {
                CSoundManager.soundList[key]._audioEmitter.Position = _position;
                CSoundManager.soundList[key]._audioListener.Position = PlayerPos;
                CSoundManager.soundList[key]._soundInstance.Play();
            }
        }

        public void ChangeWeapon(int iD)
        {
            _selectedWeap = iD;
        }

        // Handle the damages and the character death
        public float GetDamages(float damages, string hitbox)
        {
            hitbox = hitbox.Split('.')[0];
            switch (hitbox)
            {
                case "bb_Head":
                    damages *= 1.75f;
                    break;
                case "bb_Arm":
                    damages *= 0.9f;
                    break;
                case "bb_Leg":
                    damages *= 0.9f;
                    break;
                case "bb_Body":
                    damages = 1.35f;
                    break;
            }

            return damages;
        }

        public void HandleDamages(float damages, string hitbox)
        {
            // If the player is not already dead
            if (!_isDead)
            {
                _life -= damages;
                if (_life <= 0)
                {
                    // The player is not 
                    if (!_isDyingAnimPlaying)
                    {
                        _collisionHeight = 0.05f;

                        // We save the death pos and rot
                        _deathPosition = _position;
                        _deathRotation =
                            _rotation * GetTerrainNormalRotation(_position);

                        _model.ChangeAnimSpeed(2f);
                        // Back Death
                        if (hitbox.Contains('.'))
                            _model.ChangeAnimation("death_back", false, 0.75f);
                        else
                            _model.ChangeAnimation("death_front", false, 0.75f);

                        _isDyingAnimPlaying = true;
                    }
                    _life = 0;
                }
            }
        }

        // This function allows us to find the good rotation to apply on the enemy when it is died
        // With this, we can rotate the mesh to make it aligned with the terrain.
        private Matrix GetTerrainNormalRotation(Vector3 position)
        {
            // Get the normal of the plane
            Vector3 normal = _physicEngine.GetNormal(position);
            Vector3 currentNormal = new Vector3(0f, 1f, 0f);

            Vector3 axis = Vector3.Normalize(Vector3.Cross(currentNormal, normal));

            float angle = (float)Math.Acos(Vector3.Dot(currentNormal, normal));

            return Matrix.CreateFromAxisAngle(axis, angle) * Matrix.CreateRotationX(MathHelper.Pi);
        }

        public Matrix GetModelMatrix()
        {
            return Matrix.CreateScale(_scale) *
               _rotation *
               Matrix.CreateTranslation(_position);
        }

        public List<Display3D.Triangle> GetRealTriangles(bool drawHitbox = false)
        {
            Matrix world = GetModelMatrix();
            List<Display3D.Triangle> triangles = new List<Display3D.Triangle>();
            foreach (KeyValuePair<string, Display3D.Triangle> tri in hitBoxesTriangles)
            {
                //world = tri.Value.transformMatrix;
                Display3D.Triangle newTri = tri.Value.NewByMatrix(world);
                triangles.Add(newTri);
                if (drawHitbox)
                {
                    Display3D.CSimpleShapes.AddTriangle(newTri.V0, newTri.V1, newTri.V2, Color.Black);
                }
            }
            return triangles;
        }

        public void AddEnemyHUD(SpriteBatch sb, Display3D.CCamera cam)
        {
            Ray ray = new Ray(cam._cameraPos, _position - cam._cameraPos);
            int mdlid;
            if (Display3D.CModelManager.CheckRayIntersectsModel(ray, out mdlid) == null)
            {
                if (Vector3.Distance(cam._cameraPos, _position) < 50)
                {
                    Vector3 Pos = Display2D.C2DEffect._graphicsDevice.Viewport.Project(new Vector3(_position.X, _position.Y + _height, _position.Z), cam._projection, cam._view, Matrix.Identity);
                    sb.Begin();
                    if (Pos.Z < 1)
                        sb.DrawString(CEnemyManager._hudFont, _hudText, new Vector2(Pos.X - _hudTestSizeX, Pos.Y), Color.Red);
                    sb.End();
                }
            }
        }

        public string RayIntersectsHitbox(Ray ray, out float? distance)
        {
            GenerateHitBoxesTriangles();
            foreach (Display3D.Triangle tri in GetRealTriangles())
            {
                Display3D.Triangle triangle = tri;
                float? dist = Display3D.TriangleTest.Intersects(ref ray, ref triangle);
                if (dist.HasValue)
                {
                    distance = dist;
                    return tri.TriName;
                }
            }
            distance = null;
            return "";
        }

        public void GenerateHitBoxesTriangles()
        {
            hitBoxesTriangles.Clear();
            foreach (ModelMesh mesh in _model.skinnedModel.Model.Meshes)
            {
                if (mesh.Name.Length > 3 && mesh.Name.Substring(0, 3) == "bb_")
                {
                    Matrix localWorld = _model._modelTransforms[mesh.ParentBone.Index];
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        List<Vector3> indices = new List<Vector3>();
                        List<Display3D.TriangleVertexIndices> triangles = new List<Display3D.TriangleVertexIndices>();
                        ExtractModelMeshPartData(meshPart, ref localWorld, indices, triangles);

                        for (int x = 0; x < triangles.Count; x++)
                        {
                            Vector3 v0 = indices[triangles[x].A];
                            Vector3 v1 = indices[triangles[x].B];
                            Vector3 v2 = indices[triangles[x].C];
                            if (hitBoxesTriangles.ContainsKey(mesh.Name + "_" + x))
                                hitBoxesTriangles.Add(mesh.Name + "_" + x, new Display3D.Triangle(v0, v1, v2, mesh.Name, mesh.ParentBone.Transform));
                            //Display3D.CSimpleShapes.AddTriangle(v0, v1, v2, Color.Red,20.0f);
                        }
                    }
                }
            }
        }

        public void ExtractModelMeshPartData(ModelMeshPart meshPart, ref Matrix transform, List<Vector3> vertices, List<Display3D.TriangleVertexIndices> indices)
        {
            int offset = vertices.Count;

            /* Vertices */
            VertexDeclaration declaration = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] vertexElements = declaration.GetVertexElements();
            VertexElement vertexPosition = new VertexElement();

            foreach (VertexElement vert in vertexElements)
            {
                if (vert.VertexElementUsage == VertexElementUsage.Position && vert.VertexElementFormat == VertexElementFormat.Vector3)
                {
                    vertexPosition = vert;
                    break;
                }
            }

            if (vertexPosition == null ||
                vertexPosition.VertexElementUsage != VertexElementUsage.Position ||
                vertexPosition.VertexElementFormat != VertexElementFormat.Vector3)
            {
                throw new Exception("Model uses unsupported vertex format!");
            }

            Vector3[] allVertex = new Vector3[meshPart.NumVertices];

            meshPart.VertexBuffer.GetData<Vector3>(
                meshPart.VertexOffset * declaration.VertexStride + vertexPosition.Offset,
                allVertex,
                0,
                meshPart.NumVertices,
                declaration.VertexStride);

            for (int i = 0; i != allVertex.Length; ++i)
            {
                Vector3.Transform(ref allVertex[i], ref transform, out allVertex[i]);
            }

            vertices.AddRange(allVertex);

            /* Indices */

            if (meshPart.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
                throw new Exception("Model uses 32-bit indices, which are not supported.");

            short[] indexElements = new short[meshPart.PrimitiveCount * 3];
            meshPart.IndexBuffer.GetData<short>(
                meshPart.StartIndex * 2,
                indexElements,
                0,
                meshPart.PrimitiveCount * 3);

            Display3D.TriangleVertexIndices[] tvi = new Display3D.TriangleVertexIndices[meshPart.PrimitiveCount];
            for (int i = 0; i != tvi.Length; ++i)
            {
                tvi[i].A = indexElements[i * 3 + 0] + offset;
                tvi[i].B = indexElements[i * 3 + 1] + offset;
                tvi[i].C = indexElements[i * 3 + 2] + offset;
            }

            indices.AddRange(tvi);
        }

        private Matrix ComputeWeaponWorldMatrix()
        {
            Matrix world = Matrix.Identity;

            switch (_selectedWeap)
            {
                case 0:
                    world = _model.GetBoneMatrix(40, Matrix.CreateRotationX(0f) * Matrix.CreateRotationY(3.089f) * Matrix.CreateRotationZ(1.2799f),
                    0.65f, new Vector3(-0.34f, 0.0f, 0.13f));
                    break;
                case 1:
                    world = _model.GetBoneMatrix(40, Matrix.CreateRotationX(0f) * Matrix.CreateRotationY(1.6899f) * Matrix.CreateRotationZ(3.08f),
                    0.72f, new Vector3(0.2f, -0.45f, 0.049f));
                    break;
                case 2:
                    world = _model.GetBoneMatrix(40, Matrix.CreateRotationX(0.2f) * Matrix.CreateRotationY(1.28f) * Matrix.CreateRotationZ(3.179f),
                    0.28f, new Vector3(0.559f, -1.579f, -0.09f));
                    break;
                case 3:
                    world = _model.GetBoneMatrix(40, Matrix.CreateRotationX(rx) * Matrix.CreateRotationY(1.8599f) * Matrix.CreateRotationZ(1.4999f),
                    0.585f, new Vector3(0.039f, 0.2f, 0.709f));
                    break;
                case 4:
                    world = _model.GetBoneMatrix(40, Matrix.CreateRotationX(3.12f) * Matrix.CreateRotationY(-0.09f) * Matrix.CreateRotationZ(-1.549f),
                    0.65f, new Vector3(-1.469f, 0.08f, 0.2f));
                    break;
                case 5:
                    world = _model.GetBoneMatrix(17, Matrix.CreateRotationX(1.4999f) * Matrix.CreateRotationY(4.78f) * Matrix.CreateRotationZ(-4.38f),
                    0.625f, new Vector3(0.01f, 1.697f, -0.27f));
                    break;

            }


            return world;
        }
    }
}
