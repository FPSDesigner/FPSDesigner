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

        private CWeapon _weaponPossessed;

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

        // Animation Boolean
        private bool _isWaitAnimPlaying;
        private bool _isWalkAnimPlaying;
        private bool _isDyingAnimPlaying;
        private bool _isFrozenAnimPlaying;
        private bool _isReloading;

        private float _height = 1.9f;

        public float _multiSpeed = 0f;
        public Vector3 _multiDirection = Vector3.Zero;

        private Dictionary<string, Display3D.Triangle> hitBoxesTriangles;

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
            _isWaitAnimPlaying = false;
            _isDead = false;
            _isFrozen = true;
            _isHeadShot = false;
            _isShoting = false;
            _isFollowingPlayer = false;
            _isCrouch = false;
            _isJumping = false;
            _isReloading = false;

            _targetPos = _position; // The AI is not moving

            hitBoxesTriangles = new Dictionary<string, Display3D.Triangle>();

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
        }

        public void Update(GameTime gameTime, Display3D.CCamera cam)
        {
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

                // The character is waiting
                if (!_isMoving && !_isWaitAnimPlaying && !_isCrouch && !_isJumping)
                {
                    _model.ChangeAnimSpeed(0.4f);
                    _model.ChangeAnimation("machete_wait", true, 0.8f);

                    _isWalkAnimPlaying = false;
                    _isWaitAnimPlaying = true;
                }

                // The Character is running
                if (_isMoving && !_isWalkAnimPlaying && !_isCrouch && !_isJumping)
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation("heavy_walk", true, 0.5f);

                    _isWaitAnimPlaying = false;
                    _isWalkAnimPlaying = true;
                }

                // The character is waiting crouched
                if (_isCrouch && !_isMoving && !_isWaitAnimPlaying && !_isJumping)
                {
                    _model.ChangeAnimSpeed(0.8f);
                    _model.ChangeAnimation("machete_wait-crouch", true, 0.8f);

                    _isWalkAnimPlaying = false;
                    _isWaitAnimPlaying = true;
                }

                // The Character is walking crouched
                if (_isCrouch && _isMoving && !_isWalkAnimPlaying && !_isJumping)
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation("machete_walk-crouch", true, 0.3f);

                    _isWaitAnimPlaying = false;
                    _isWalkAnimPlaying = true;
                }

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

            if (_isJumping && _model.HasFinished())
            {
                _model.ChangeAnimSpeed(2.5f);
                _model.ChangeAnimation("machete_walk", true, 0.5f);

                _isJumping = false;
            }

            if (_isReloading && _model.HasFinished())
            {
                if (_isCrouch)
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation("machete_walk-crouch", true, 0.5f);
                }
                else
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation("machete_walk", true, 0.5f);
                }
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
        }

        public void AttackPlayer(float distSquared, CCharacter player)
        {
            if (!_isShoting)
            {
                if (_isAgressive && IsAnyPlayerInSight())
                {
                    _model.ChangeAnimSpeed(5.0f);
                    _model.ChangeAnimation("machete_attack", false, 0.2f);

                    if (distSquared <= (_weaponPossessed._weaponsArray[_weaponPossessed._selectedWeapon]._range) * (_weaponPossessed._weaponsArray[_weaponPossessed._selectedWeapon]._range))
                    {
                        Random random = new Random();
                        int rand = random.Next();

                        // Shoot at the player
                        if (rand == prob[0])
                        {
                            player._life -= _weaponPossessed._weaponsArray[_weaponPossessed._selectedWeapon]._damagesPerBullet;
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
            if (!_isFrozen && !_isFollowingPlayer)
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

                _model.ChangeAnimSpeed(2.5f);
                _model.ChangeAnimation(typeName + "_wait", true, 0.8f);

                _isCrouch = false;
            }
        }

        public void SetJump(string type)
        {
            if (!_isJumping)
            {
                _model.ChangeAnimSpeed(2.0f);
                _model.ChangeAnimation(type + "_jump", false, 0.65f);

                _isJumping = true;
            }
        }

        public void SetReload(string typeName)
        {
            if (!_isReloading)
            {
                if (_isCrouch)
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(typeName + "_reload-crouch", true, 0.55f);
                }
                else
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(typeName + "_reload", true, 0.55f);
                }

                _isReloading = true;
                _isWalkAnimPlaying = false;
                _isWaitAnimPlaying = false;
            }
        }

        public void SetWalk(bool toggle, string typeName)
        {
            if (toggle)
            {
                if (_isCrouch)
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(typeName + "_walk-crouch", true, 0.55f);
                }
                else
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(typeName + "_walk", true, 0.55f);
                }

                _isWalkAnimPlaying = true;
                _isWaitAnimPlaying = false;
            }

            else
            {
                if (_isCrouch)
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(typeName + "_wait-crouch", true, 0.55f);
                }
                else
                {
                    _model.ChangeAnimSpeed(2.5f);
                    _model.ChangeAnimation(typeName + "_wait", true, 0.55f);
                }

                _isWalkAnimPlaying = false;
                _isWaitAnimPlaying = true;
            }
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
    }
}
