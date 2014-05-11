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

        public static void AddEnemy(CEnemy enemy)
        {
            _enemyList.Add(enemy);
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
    }

    class CEnemy
    {
        public float _life;
        private float _runningVelocity; // Displacement velocity

        private Display3D.MeshAnimation _model; //The 3Dmodel and all animations

        private Vector3 _position; // The character positon
        private Vector3 _targetPos; // The target position
        private Vector3 _scale;

        private Vector3 _deathPosition; // The coordinates where the enemy died
        private Matrix _deathRotation;

        private Matrix _rotation; // Model rotation
        private float rotationValue; // We give this float to the rotation mat

        private Game.CPhysics _physicEngine; // Ennemy will be submitted to forces

        public bool _isAgressive; // Pacifiste or not

        private bool _isMoving;
        private bool _isDead;
        public bool _isFrozen; // Use to stop translation

        // Animation Boolean
        private bool _isWaitAnimPlaying;
        private bool _isWalkAnimPlaying;
        private bool _isDyingAnimPlaying;
        private bool _isFrozenAnimPlaying;

        private Dictionary<string, Display3D.Triangle> hitBoxesTriangles;

        public CEnemy(string ModelName, Texture2D[] Textures, Vector3 Position, Matrix Rotation, float Life, float Velocity,bool isAgressive)
        {
            _position = Position;
            _scale = new Vector3(0.5f);
            _deathPosition = Vector3.Zero;

            _rotation = Rotation;
            _rotation = Matrix.Identity;

            this._life = Life;

            this._runningVelocity = Velocity;

            // We Create the Enemy, giving its textures, models...

            _model = new Display3D.MeshAnimation(ModelName, 1, 1, 1.0f, _position,
                Matrix.CreateRotationX(-1 * MathHelper.PiOver2), _scale.X, Textures, 10, 0.0f, true);

            this._isAgressive = isAgressive;

            _isMoving = false;
            _isDyingAnimPlaying = false;
            _isDead = false;
            _isFrozen = true;

            hitBoxesTriangles = new Dictionary<string, Display3D.Triangle>();
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

        public void Update(GameTime gameTime)
        {
            //Play Anims
            if (!_isFrozen && !_isDead && !_isDyingAnimPlaying) // Player is not frozen by the administrator
            {
                // The character is running
                if (!_isMoving && !_isWaitAnimPlaying)
                {
                    _model.ChangeAnimSpeed(0.6f);
                    _model.ChangeAnimation("wait", true, 0.5f);

                    _isWalkAnimPlaying = false;
                    _isWaitAnimPlaying = true;

                }

                // The Character is running
                if (_isMoving && !_isWalkAnimPlaying)
                {
                    _model.ChangeAnimSpeed(2.0f);
                    _model.ChangeAnimation("walk", true, 0.7f);

                    _isWaitAnimPlaying = false;
                    _isWalkAnimPlaying = true;
                }

                _isFrozenAnimPlaying = false;
            }

            else if(_isFrozen && !_isFrozenAnimPlaying)
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

            _model.Update(gameTime, _position, _rotation);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Matrix view, Matrix projection, bool drawHitbox = false)
        {
            //GenerateHitBoxesTriangles();
            //GetRealTriangles();

            // We draw the enemy entirely if he is alive
            if (!_isDead)
                _model.Draw(gameTime, spriteBatch, view, projection);
            else
            {
                string[] undrawable = new string [1];
                undrawable[0] = "Head";
                _model.Draw(gameTime, spriteBatch, view, projection, undrawable);
            }

            if (drawHitbox)
                GetRealTriangles(true);
        }

        public void MoveTo(Vector3 newPos, GameTime gameTime)
        {
            if (!_isFrozen && !_isDead)
            {
                _targetPos = new Vector3(newPos.X - _position.X,
                    newPos.Y - _position.Y, newPos.Z - _position.Z);
                _targetPos.Normalize();

                rotationValue = (float)Math.Atan2(newPos.X - _position.X,
                    newPos.Z - _position.Z);

                Vector3 translation = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(rotationValue));
                translation.Normalize();

                if (Vector3.DistanceSquared(_position, newPos) > 10.0f)
                {
                    translation *= _runningVelocity;
                    _position = _physicEngine.GetNewPosition(gameTime, _position, translation, false);
                    _isMoving = true;
                }
                else
                    _isMoving = false;
            }
        }

        // This function allows us to find the good rotation to apply on the enemy when it is died
        // With this, we can rotate the mesh to make it aligned with the terrain.
        private Matrix GetTerrainNormalRotation(Vector3 position)
        {
            // Get the normal of the plane
            Vector3 normal = _physicEngine.GetNormal(position);
            Vector3 currentNormal = _rotation.Up;

            Vector3 axis = Vector3.Normalize(Vector3.Cross(currentNormal, normal));

            //float xRot = (float)Math.Acos((Vector3.Dot(new Vector3(1f,0f,0f), normal)) / normal.Length());
            //float yRot = (float)Math.Acos((Vector3.Dot(new Vector3(0f, 1f, 0f), normal)) / normal.Length());
            //float zRot = (float)Math.Acos((Vector3.Dot(new Vector3(0f, 0f, 1f), normal)) / normal.Length());

            float angle = (float)Math.Acos(Vector3.Dot(currentNormal, normal));

            return Matrix.CreateFromAxisAngle(axis, angle);
        }

        // launch the appropriate def animation
        public void ReceivedDamages(float damages, string animToPlay)
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
                        // We save the death pos and rot
                        _deathPosition = _position;
                        _deathRotation = 
                            Matrix.CreateRotationX(MathHelper.PiOver2) * _rotation * GetTerrainNormalRotation(_position);

                        _model.ChangeAnimSpeed(2f);
                        _model.ChangeAnimation(animToPlay, false, 0.75f);
                        _isDyingAnimPlaying = true;
                    }
                    _life = 0;
                }
            }
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
                triangles.Add(tri.Value.NewByMatrix(world));
                if(drawHitbox)
                    Display3D.CSimpleShapes.AddTriangle(tri.Value.NewByMatrix(world).V0, tri.Value.NewByMatrix(world).V1, tri.Value.NewByMatrix(world).V2, Color.Black);
            }
            return triangles;
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
                if (mesh.Name.Length > 3 && mesh.Name.Substring(0, 3) == "Bb_")
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

                            hitBoxesTriangles.Add(mesh.Name + "_" + x, new Display3D.Triangle(v0, v1, v2, mesh.Name));
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
