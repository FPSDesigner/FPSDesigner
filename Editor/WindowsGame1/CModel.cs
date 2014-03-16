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

namespace ModelViewer
{
    /// <summary>
    /// Load a new model to draw on the world
    /// </summary>
    class CModel
    {
        public Vector3 _modelPosition { get; set; }
        public Vector3 _modelRotation { get; set; }
        public Vector3 _modelScale { get; set; }
        public Vector3 _lightDirection { get; set; }

        public Model _model { get; private set; }

        public float Alpha = 1.0f;

        private float _specularColor;

        private Matrix[] _modelTransforms;
        private GraphicsDevice _graphicsDevice;

        public List<Triangle> _trianglesPositions = new List<Triangle>();
        public List<Vector3> _trianglesNormal = new List<Vector3>();

        private Dictionary<String, Texture2D> _textures;

        private string collisionShapeName = "collision_shape";

        /// <summary>
        /// Initialize the model class
        /// </summary>
        /// <param name="model">Model element</param>
        /// <param name="modelPos">Position of the model</param>
        /// <param name="modelRotation">Rotation of the model</param>
        /// <param name="modelScale">Scale of the model (size)</param>
        /// <param name="device">GraphicsDevice class</param>
        public CModel(Model model, Vector3 modelPos, Vector3 modelRotation, Vector3 modelScale, GraphicsDevice device, Dictionary<String, Texture2D> textures = null, float specColor = 0.0f, float alpha = 1.0f)
        {
            this._model = model;

            this._modelPosition = modelPos;
            this._modelRotation = modelRotation;
            this._modelScale = modelScale;
            this.Alpha = alpha;

            this._specularColor = specColor;

            this._textures = textures;

            _modelTransforms = new Matrix[model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_modelTransforms);

            // Init the model with textures

            if (_textures != null)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        if (mesh.Name != collisionShapeName)
                        {
                            effect.EnableDefaultLighting();

                            effect.TextureEnabled = true;

                            string newName = mesh.Name.Split('_')[0];; // If there is no * : newName corresponds to the mesh.Name

                            if (_textures.ContainsKey(newName))
                                effect.Texture = _textures[newName];

                            effect.SpecularColor = new Vector3(_specularColor);
                            effect.SpecularPower = 32;
                        }
                    }
                }
            }

            generateTags();
            generateModelTriangles();

            this._graphicsDevice = device;
        }

        /// <summary>
        /// Draw the model in the world
        /// </summary>
        /// <param name="view">View Matrix used in CCamera class</param>
        /// <param name="projection">Projection Matrix used in CCamera class</param>
        /// <param name="cameraPosition">Vector representing the camera position</param>
        public void Draw(Matrix view, Matrix projection)
        {
            Matrix world = Matrix.CreateScale(_modelScale) *
                Matrix.CreateFromYawPitchRoll(_modelRotation.Y, _modelRotation.X, _modelRotation.Z) *
                Matrix.CreateTranslation(_modelPosition);

            if (_textures != null)
            {
                foreach (ModelMesh mesh in _model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        if (mesh.Name != collisionShapeName)
                        {
                            effect.EnableDefaultLighting();

                            effect.TextureEnabled = true;

                            string newName = mesh.Name; // If there is no * : newName corresponds to the mesh.Name

                            if (mesh.Name.Contains('_'))
                                newName = mesh.Name.Split('_')[0];

                            if (_textures.ContainsKey(newName))
                                effect.Texture = _textures[newName];

                            effect.SpecularColor = new Vector3(_specularColor);
                            effect.SpecularPower = 32;

                        }
                    }
                }
            }

            foreach (ModelMesh mesh in _model.Meshes)
            {
                Matrix localWorld = _modelTransforms[mesh.ParentBone.Index] * world;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    Effect effect = meshPart.Effect;

                    if (effect is BasicEffect)
                    {
                        BasicEffect bEffect = (BasicEffect)effect;

                        string newName = mesh.Name.Split('_')[0];
                        if (_textures.ContainsKey(newName))
                            bEffect.Texture = _textures[newName];

                        bEffect.World = localWorld;
                        bEffect.View = view;
                        bEffect.Projection = projection;
                        bEffect.EnableDefaultLighting();
                    }
                    else
                    {
                        setEffectParameter(effect, "World", localWorld);
                        setEffectParameter(effect, "View", view);
                        setEffectParameter(effect, "Projection", projection);
                    }
                }

                // We draw the mesh only if it's not a collision shape and only if we need to draw it
                if (mesh.Name != collisionShapeName)
                    mesh.Draw();
            }

        }


        /// <summary>
        /// Set to the specified effet the parameter given
        /// </summary>
        /// <param name="effect">The effect the parameter is applied to</param>
        /// <param name="paramName">The parameter name</param>
        /// <param name="val">The parameter value</param>
        public void setEffectParameter(Effect effect, string paramName, object val)
        {
            if (effect.Parameters[paramName] == null)
                return;

            if (val is Vector3)
                effect.Parameters[paramName].SetValue((Vector3)val);
            else if (val is bool)
                effect.Parameters[paramName].SetValue((bool)val);
            else if (val is Matrix)
                effect.Parameters[paramName].SetValue((Matrix)val);
            else if (val is Texture2D)
                effect.Parameters[paramName].SetValue((Texture2D)val);
        }

        /// <summary>
        /// Sets a specific effect to a model
        /// </summary>
        /// <param name="effect">The effect to apply to the model</param>
        /// <param name="CopyEffect">Wether or not we copy the effect</param>
        public void SetModelEffect(Effect effect, bool CopyEffect)
        {
            foreach (ModelMesh mesh in _model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Effect toSet = effect;

                    // Copy the effect if necessary
                    if (CopyEffect)
                        toSet = effect.Clone();

                    MeshTag tag = ((MeshTag)part.Tag);

                    // If this ModelMeshPart has a texture, set it to the effect
                    if (tag.Texture != null)
                    {
                        setEffectParameter(toSet, "BasicTexture", tag.Texture);
                        setEffectParameter(toSet, "TextureEnabled", true);
                    }
                    else
                        setEffectParameter(toSet, "TextureEnabled", false);

                    // Set our remaining parameters to the effect
                    setEffectParameter(toSet, "DiffuseColor", tag.Color);
                    setEffectParameter(toSet, "SpecularPower", tag.SpecularPower);

                    part.Effect = toSet;
                }
        }

        /// <summary>
        /// Generate tags
        /// </summary>
        private void generateTags()
        {
            foreach (ModelMesh mesh in _model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (part.Effect is BasicEffect)
                    {
                        BasicEffect effect = (BasicEffect)part.Effect;
                        MeshTag tag = new MeshTag(effect.DiffuseColor,
                            effect.Texture, effect.SpecularPower);
                        part.Tag = tag;
                    }
        }

        /// <summary>
        /// Store references to all of the model's current effects
        /// </summary>
        public void CacheEffects()
        {
            foreach (ModelMesh mesh in _model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    ((MeshTag)part.Tag).CachedEffect = part.Effect;
        }

        /// <summary>
        /// Restore effects referenced by the model's cache
        /// </summary>
        public void RestoreEffects()
        {
            foreach (ModelMesh mesh in _model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (((MeshTag)part.Tag).CachedEffect != null)
                        part.Effect = ((MeshTag)part.Tag).CachedEffect;
        }

        public void generateModelTriangles()
        {
            Matrix world = Matrix.CreateScale(_modelScale) *
                Matrix.CreateFromYawPitchRoll(_modelRotation.Y, _modelRotation.X, _modelRotation.Z) *
                Matrix.CreateTranslation(_modelPosition);

            bool hasCollisionMesh = false;
            ModelMesh collisionMesh = default(ModelMesh);
            foreach (ModelMesh mesh in _model.Meshes)
            {
                if (mesh.Name == collisionShapeName)
                {
                    hasCollisionMesh = true;
                    collisionMesh = mesh;
                    break;
                }
            }

            foreach (ModelMesh mesh in _model.Meshes)
            {
                bool isCollisionOne = (hasCollisionMesh && collisionMesh.Name == mesh.Name);
                if (!hasCollisionMesh || isCollisionOne)
                {
                    Matrix localWorld = _modelTransforms[mesh.ParentBone.Index] * world;
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        List<Vector3> indices = new List<Vector3>();
                        List<TriangleVertexIndices> triangles = new List<TriangleVertexIndices>();
                        ExtractModelMeshPartData(meshPart, ref localWorld, indices, triangles);

                        for (int x = 0; x < triangles.Count; x++)
                        {
                            Vector3 v0 = indices[triangles[x].A];
                            Vector3 v1 = indices[triangles[x].B];
                            Vector3 v2 = indices[triangles[x].C];

                            _trianglesPositions.Add(new Triangle(v0, v1, v2));
                            //Display3D.CSimpleShapes.AddTriangle(v0, v1, v2, Color.Red,20.0f);

                            // Calculate normal
                            Vector3 Vector = Vector3.Cross(v0 - v1, v0 - v2);
                            Vector.Normalize();

                            _trianglesNormal.Add(Vector);
                        }
                    }
                    if (isCollisionOne)
                        break;
                }
            }
        }

        /// <summary>
        /// Get all the triangles from each mesh part (Changed for XNA 4)
        /// </summary>
        /// <param name="meshPart">The meshPart from which we want the datas</param>
        /// <param name="transform">The transform matrix</param>
        /// <param name="vertices">The list which will contains all the vertices</param>
        /// <param name="indices">The list which will contains all the triangles to use with vertices</param>
        public void ExtractModelMeshPartData(ModelMeshPart meshPart, ref Matrix transform, List<Vector3> vertices, List<TriangleVertexIndices> indices)
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

            TriangleVertexIndices[] tvi = new TriangleVertexIndices[meshPart.PrimitiveCount];
            for (int i = 0; i != tvi.Length; ++i)
            {
                tvi[i].A = indexElements[i * 3 + 0] + offset;
                tvi[i].B = indexElements[i * 3 + 1] + offset;
                tvi[i].C = indexElements[i * 3 + 2] + offset;
            }

            indices.AddRange(tvi);
        }
    }

    public class MeshTag
    {
        public Vector3 Color;
        public Texture2D Texture;
        public float SpecularPower;
        public Effect CachedEffect = null;

        public MeshTag(Vector3 Color, Texture2D Texture, float SpecularPower)
        {
            this.Color = Color;
            this.Texture = Texture;
            this.SpecularPower = SpecularPower;
        }
    }

    public struct TriangleVertexIndices
    {
        public int A;
        public int B;
        public int C;
    }

    public struct Triangle
    {
        public Vector3 V0;
        public Vector3 V1;
        public Vector3 V2;

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }
    }

}
