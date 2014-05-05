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

        public static string RayIntersectsHitbox(Ray ray)
        {
            foreach (CEnemy enemy in _enemyList)
            {
                string enemytest = enemy.RayIntersectsHitbox(ray);
                if (enemytest != "")
                    return enemytest;
            }
            return "";
        }
    }

    class CEnemy
    {
        float _life;

        private Display3D.MeshAnimation _model; //The 3Dmodel and all animations

        private Vector3 _position; // The character positon
        private Vector3 _targetPos; // The target position
        private Vector3 _scale;

        private float rotationValue; // We give this float to the rotation mat
        private Matrix _rotation; // Model rotation

        private Game.CPhysics _physicEngine; // Ennemy will be submitted to forces

        private bool _isMoving;

        private Dictionary<string, Display3D.Triangle> hitBoxesTriangles;


        public CEnemy(string ModelName, Texture2D[] Textures, Vector3 Position, Matrix Rotation)
        {
            _position = Position;
            _rotation = Rotation;
            _scale = new Vector3(0.5f);

            // We Create the Enemy, giving its textures, models...

            _model = new Display3D.MeshAnimation(ModelName, 1, 1, 1.0f, _position,
                Matrix.CreateRotationX(-1 * MathHelper.PiOver2), _scale.X, Textures, 10, 0.0f, true);

            _isMoving = false;

            hitBoxesTriangles = new Dictionary<string, Display3D.Triangle>();
        }

        public void LoadContent(ContentManager content, Display3D.CCamera cam)
        {

            // We load the ennemy content
            _model.LoadContent(content);
            _model.ChangeAnimSpeed(2f);
            _model.BeginAnimation("walk", true);

            // We Create the forces application on the Ennemy
            _physicEngine = new CPhysics();
            _physicEngine.LoadContent(0.2f, new bool[] { true, true });
            _physicEngine._triangleList = cam._physicsMap._triangleList;
            _physicEngine._triangleNormalsList = cam._physicsMap._triangleNormalsList;
            _physicEngine._terrain = cam._physicsMap._terrain;
            _physicEngine._waterHeight = cam._physicsMap._waterHeight;

            GenerateHitBoxesTriangles();
        }

        public void Update(GameTime gameTime)
        {
            // Apply the physic on the character
            _position = _physicEngine.GetNewPosition(gameTime, _position, Vector3.Zero, false);

            // We update the character pos, rot...
            _rotation = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationY((rotationValue));
            _model.Update(gameTime, _position, _rotation);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Matrix view, Matrix projection)
        {
            GetRealTriangles();
            _model.Draw(gameTime, spriteBatch, view, projection);
        }

        public void MoveTo(Vector3 newPos)
        {
            _targetPos = newPos + _position;
            _targetPos.Normalize();

            rotationValue = (float)Math.Atan2(newPos.X - _position.X,
                newPos.Z - _position.Z);
        }

        public Matrix GetModelMatrix()
        {
            return Matrix.CreateScale(_scale) *
               _rotation *
               Matrix.CreateTranslation(_position);
        }

        public List<Display3D.Triangle> GetRealTriangles()
        {
            Matrix world = GetModelMatrix();
            List<Display3D.Triangle> triangles = new List<Display3D.Triangle>();
            foreach (KeyValuePair<string, Display3D.Triangle> tri in hitBoxesTriangles)
            {
                triangles.Add(tri.Value.NewByMatrix(world));
                Display3D.CSimpleShapes.AddTriangle(tri.Value.NewByMatrix(world).V0, tri.Value.NewByMatrix(world).V1, tri.Value.NewByMatrix(world).V2, Color.Red);
            }
            return triangles;
        }

        public string RayIntersectsHitbox(Ray ray)
        {
            GenerateHitBoxesTriangles();
            foreach (Display3D.Triangle tri in GetRealTriangles())
            {
                Display3D.Triangle triangle = tri;
                float? dist = Display3D.TriangleTest.Intersects(ref ray, ref triangle);
                if (dist.HasValue)
                    return tri.TriName;
            }
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

                            hitBoxesTriangles.Add(mesh.Name+"_"+x, new Display3D.Triangle(v0, v1, v2, mesh.Name));
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
