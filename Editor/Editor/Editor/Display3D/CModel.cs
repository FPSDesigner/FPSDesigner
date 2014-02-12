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

namespace Editor.Display3D
{
    /// <summary>
    /// Load a new model to draw on the world
    /// </summary>
    class CModel : IRenderable
    {
        public Vector3 _modelPosition { get; set; }
        public Vector3 _modelRotation { get; set; }
        public Vector3 _modelScale { get; set; }
        public Vector3 _lightDirection { get; set; }

        public Model _model { get; private set; }

        public float Alpha = 1.0f;

        private Matrix[] _modelTransforms;
        private GraphicsDevice _graphicsDevice;
        private BoundingSphere _boundingSphere;

        public Materials.Material Material { get; set; }

        public List<Triangle> _trianglesPositions = new List<Triangle>();
        public List<Vector3> _trianglesNormal = new List<Vector3>();

        private string collisionShapeName = "collision_shape";

        public BoundingSphere BoundingSphere
        {
            get
            {
                Matrix worldTransform = Matrix.CreateScale(_modelScale)
                    * Matrix.CreateTranslation(_modelPosition);

                BoundingSphere transformed = _boundingSphere;
                transformed = transformed.Transform(worldTransform);

                return transformed;
            }
        }

        /// <summary>
        /// Initialize the model class
        /// </summary>
        /// <param name="model">Model element</param>
        /// <param name="modelPos">Position of the model</param>
        /// <param name="modelRotation">Rotation of the model</param>
        /// <param name="modelScale">Scale of the model (size)</param>
        /// <param name="device">GraphicsDevice class</param>
        public CModel(Model model, Vector3 modelPos, Vector3 modelRotation, Vector3 modelScale, GraphicsDevice device, float alpha = 1.0f)
        {
            this._model = model;

            this._modelPosition = modelPos;
            this._modelRotation = modelRotation;
            this._modelScale = modelScale;
            this.Alpha = alpha;

            _modelTransforms = new Matrix[model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_modelTransforms);

            buildBoundingSphere();
            generateTags();
            generateModelTriangles();

            this._graphicsDevice = device;

            this.Material = new Materials.Material();
        }

        /// <summary>
        /// Draw the model in the world
        /// </summary>
        /// <param name="view">View Matrix used in CCamera class</param>
        /// <param name="projection">Projection Matrix used in CCamera class</param>
        /// <param name="cameraPosition">Vector representing the camera position</param>
        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            // Matrix which display the model in the world
            Matrix world = Matrix.CreateScale(_modelScale) *
                Matrix.CreateFromYawPitchRoll(_modelRotation.Y, _modelRotation.X, _modelRotation.Z) *
                Matrix.CreateTranslation(_modelPosition);

            foreach (ModelMesh mesh in _model.Meshes)
            {
                Matrix localWorld = _modelTransforms[mesh.ParentBone.Index] * world;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    Effect effect = meshPart.Effect;

                    if (effect is BasicEffect)
                    {
                        ((BasicEffect)effect).World = localWorld;
                        ((BasicEffect)effect).View = view;
                        ((BasicEffect)effect).Projection = projection;
                        ((BasicEffect)effect).EnableDefaultLighting();
                    }
                    else
                    { 
                        setEffectParameter(effect, "World", localWorld);
                        setEffectParameter(effect, "View", view);
                        setEffectParameter(effect, "Projection", projection);
                        setEffectParameter(effect, "CameraPosition", cameraPosition);

                        Material.SetEffectParameters(effect);
                    }
                }
                if (mesh.Name != collisionShapeName)
                    mesh.Draw();
            }

        }

        /// <summary>
        /// Creates the bounding sphere associated to the model
        /// </summary>
        private void buildBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 0);

            // Merge all the model's built in bounding spheres
            foreach (ModelMesh mesh in _model.Meshes)
            {
                if (mesh.Name != collisionShapeName)
                {
                    BoundingSphere transformed = mesh.BoundingSphere.Transform(
                        _modelTransforms[mesh.ParentBone.Index]);
                    sphere = BoundingSphere.CreateMerged(sphere, transformed);
                }
            }

            this._boundingSphere = sphere;
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
        /// Store references to all of the model's current effecs
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

        /// <summary>
        /// Sets the clip plane for the water rendering reflection
        /// </summary>
        /// <param name="Plane">The plane</param>
        public void SetClipPlane(Vector4? Plane)
        {
            foreach (ModelMesh mesh in _model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.Effect.Parameters["ClipPlaneEnabled"] != null)
                        part.Effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

                    if (Plane.HasValue)
                        if (part.Effect.Parameters["ClipPlane"] != null)
                            part.Effect.Parameters["ClipPlane"].SetValue(Plane.Value);
                }
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

        /// <summary>
        /// Check whether or not a sphere 
        /// </summary>
        /// <param name="Sphere">The sphere to test</param>
        /// <returns>True if the sphere intersects, false otherwise.</returns>
        public bool IsBoundingSphereIntersecting(BoundingSphere Sphere, out Vector3 triangleNormal)
        {
            triangleNormal = Vector3.Zero;
            for (int i = 0; i < _trianglesPositions.Count; i++)
            {
                Triangle triangleToTest = _trianglesPositions[i];
                if (TriangleTest.Intersects(ref Sphere, ref triangleToTest))
                {
                    triangleNormal = _trianglesNormal[i];
                    return true;
                }
            }
            return false;
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

    /// <summary>
    /// Triangle-based collision tests
    /// </summary>
    public static class TriangleTest
    {
        const float EPSILON = 1e-20F;

        #region Triangle-Sphere

        /// <summary>
        /// Returns true if the given sphere intersects the triangle (v0,v1,v2).
        /// </summary>
        public static bool Intersects(ref BoundingSphere sphere, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            Vector3 p = NearestPointOnTriangle(ref sphere.Center, ref v0, ref v1, ref v2);
            return Vector3.DistanceSquared(sphere.Center, p) < sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Returns true if the given sphere intersects the given triangle.
        /// </summary>
        public static bool Intersects(ref BoundingSphere sphere, ref Triangle t)
        {
            Vector3 p = NearestPointOnTriangle(ref sphere.Center, ref t.V0, ref t.V1, ref t.V2);
            return Vector3.DistanceSquared(sphere.Center, p) < sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Determines whether the given sphere contains/intersects/is disjoint from the triangle
        /// (v0,v1,v2)
        /// </summary>
        public static ContainmentType Contains(ref BoundingSphere sphere, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            float r2 = sphere.Radius * sphere.Radius;
            if (Vector3.DistanceSquared(v0, sphere.Center) <= r2 &&
                Vector3.DistanceSquared(v1, sphere.Center) <= r2 &&
                Vector3.DistanceSquared(v2, sphere.Center) <= r2)
                return ContainmentType.Contains;

            return Intersects(ref sphere, ref v0, ref v1, ref v2)
                   ? ContainmentType.Intersects : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether the given sphere contains/intersects/is disjoint from the
        /// given triangle.
        /// </summary>
        public static ContainmentType Contains(ref BoundingSphere sphere, ref Triangle triangle)
        {
            return Contains(ref sphere, ref triangle.V0, ref triangle.V1, ref triangle.V2);
        }
        #endregion

        #region Triangle-Ray

        /// <summary>
        /// Determine whether the triangle (v0,v1,v2) intersects the given ray. If there is intersection,
        /// returns the parametric value of the intersection point on the ray. Otherwise returns null.
        /// </summary>
        public static float? Intersects(ref Ray ray, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;

            Vector3 p = Vector3.Cross(ray.Direction, e2);

            float det = Vector3.Dot(e1, p);

            float t;
            if (det >= EPSILON)
            {
                // Determinate is positive (front side of the triangle).
                Vector3 s = ray.Position - v0;
                float u = Vector3.Dot(s, p);
                if (u < 0 || u > det)
                    return null;

                Vector3 q = Vector3.Cross(s, e1);
                float v = Vector3.Dot(ray.Direction, q);
                if (v < 0 || ((u + v) > det))
                    return null;

                t = Vector3.Dot(e2, q);
                if (t < 0)
                    return null;
            }
            else if (det <= -EPSILON)
            {
                // Determinate is negative (back side of the triangle).
                Vector3 s = ray.Position - v0;
                float u = Vector3.Dot(s, p);
                if (u > 0 || u < det)
                    return null;

                Vector3 q = Vector3.Cross(s, e1);
                float v = Vector3.Dot(ray.Direction, q);
                if (v > 0 || ((u + v) < det))
                    return null;

                t = Vector3.Dot(e2, q);
                if (t > 0)
                    return null;
            }
            else
            {
                // Parallel ray.
                return null;
            }

            return t / det;
        }

        /// <summary>
        /// Determine whether the given triangle intersects the given ray. If there is intersection,
        /// returns the parametric value of the intersection point on the ray. Otherwise returns null.
        /// </summary>
        public static float? Intersects(ref Ray ray, ref Triangle tri)
        {
            return Intersects(ref ray, ref tri.V0, ref tri.V1, ref tri.V2);
        }

        #endregion

        #region Common utility methods

        /// <summary>
        /// Return the point on triangle (v0,v1,v2) closest to point p.
        /// </summary>
        public static Vector3 NearestPointOnTriangle(ref Vector3 p, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            Vector3 r0 = p - v0;
            Vector3 r1 = v1 - v0;
            Vector3 r2 = v2 - v0;

            //q1 = (r1 dot r0)/length(r1)^2
            //q2 = (r2 dot r0)/length(r2)^2

            float q1 = (Vector3.Dot(r1, r0)) / r1.LengthSquared();
            float q2 = (Vector3.Dot(r2, r0)) / r2.LengthSquared();

            if (q1 > 0 && q2 > 0 && q1 + q2 < 1)
                return v0 + r1 * q1 + r2 * q2;
            else
                return Vector3.Zero;

            /*Vector3 D = p - v0;
            Vector3 E1 = (v1 - v0);
            Vector3 E2 = (v2 - v0);
            float dot11 = E1.LengthSquared();
            float dot12 = Vector3.Dot(E1, E2);
            float dot22 = E2.LengthSquared();
            float dot1d = Vector3.Dot(E1, D);
            float dot2d = Vector3.Dot(E2, D);
            float dotdd = D.LengthSquared();

            float s = dot1d * dot22 - dot2d * dot12;
            float t = dot2d * dot11 - dot1d * dot12;
            float d = dot11 * dot22 - dot12 * dot12;

            if (dot1d <= 0 && dot2d <= 0)
            {
                return v0;
            }
            if (s <= 0 && dot2d >= 0 && dot2d <= dot22)
            {
                return v0 + E2 * (dot2d / dot22);
            }
            if (t <= 0 && dot1d >= 0 && dot1d <= dot11)
            {
                return v0 + E1 * (dot1d / dot11);
            }
            if (s >= 0 && t >= 0 && s + t <= d)
            {
                float dr = 1.0f / d;
                return v0 + (s * dr) * E1 + (t * dr) * E2;
            }

            float u12_num = dot2d - dot1d - dot12 + dot11;
            float u12_den = dot22 + dot11 - 2 * dot12;
            if (u12_num <= 0)
            {
                return v1;
            }
            if (u12_num >= u12_den)
            {
                return v2;
            }
            return v1 + (v2 - v1) * (u12_num / u12_den);*/
        }

        /// <summary>
        /// Check if an origin-centered, axis-aligned box with the given half extents contains,
        /// intersects, or is disjoint from the given triangle. This is used for the box and
        /// frustum vs. triangle tests.
        /// </summary>
        public static ContainmentType OriginBoxContains(ref Vector3 halfExtent, ref Triangle tri)
        {
            BoundingBox triBounds = new BoundingBox(); // 'new' to work around NetCF bug
            triBounds.Min.X = Math.Min(tri.V0.X, Math.Min(tri.V1.X, tri.V2.X));
            triBounds.Min.Y = Math.Min(tri.V0.Y, Math.Min(tri.V1.Y, tri.V2.Y));
            triBounds.Min.Z = Math.Min(tri.V0.Z, Math.Min(tri.V1.Z, tri.V2.Z));

            triBounds.Max.X = Math.Max(tri.V0.X, Math.Max(tri.V1.X, tri.V2.X));
            triBounds.Max.Y = Math.Max(tri.V0.Y, Math.Max(tri.V1.Y, tri.V2.Y));
            triBounds.Max.Z = Math.Max(tri.V0.Z, Math.Max(tri.V1.Z, tri.V2.Z));

            Vector3 triBoundhalfExtent;
            triBoundhalfExtent.X = (triBounds.Max.X - triBounds.Min.X) * 0.5f;
            triBoundhalfExtent.Y = (triBounds.Max.Y - triBounds.Min.Y) * 0.5f;
            triBoundhalfExtent.Z = (triBounds.Max.Z - triBounds.Min.Z) * 0.5f;

            Vector3 triBoundCenter;
            triBoundCenter.X = (triBounds.Max.X + triBounds.Min.X) * 0.5f;
            triBoundCenter.Y = (triBounds.Max.Y + triBounds.Min.Y) * 0.5f;
            triBoundCenter.Z = (triBounds.Max.Z + triBounds.Min.Z) * 0.5f;

            if (triBoundhalfExtent.X + halfExtent.X <= Math.Abs(triBoundCenter.X) ||
                triBoundhalfExtent.Y + halfExtent.Y <= Math.Abs(triBoundCenter.Y) ||
                triBoundhalfExtent.Z + halfExtent.Z <= Math.Abs(triBoundCenter.Z))
            {
                return ContainmentType.Disjoint;
            }

            if (triBoundhalfExtent.X + Math.Abs(triBoundCenter.X) <= halfExtent.X &&
                triBoundhalfExtent.Y + Math.Abs(triBoundCenter.Y) <= halfExtent.Y &&
                triBoundhalfExtent.Z + Math.Abs(triBoundCenter.Z) <= halfExtent.Z)
            {
                return ContainmentType.Contains;
            }

            Vector3 edge1, edge2, edge3;
            Vector3.Subtract(ref tri.V1, ref tri.V0, out edge1);
            Vector3.Subtract(ref tri.V2, ref tri.V0, out edge2);

            Vector3 normal;
            Vector3.Cross(ref edge1, ref edge2, out normal);
            float triangleDist = Vector3.Dot(tri.V0, normal);
            if (Math.Abs(normal.X * halfExtent.X) + Math.Abs(normal.Y * halfExtent.Y) + Math.Abs(normal.Z * halfExtent.Z) <= Math.Abs(triangleDist))
            {
                return ContainmentType.Disjoint;
            }

            // Worst case: we need to check all 9 possible separating planes
            // defined by Cross(box edge,triangle edge)
            // Check for separation in plane containing an axis of box A and and axis of box B
            //
            // We need to compute all 9 cross products to find them, but a lot of terms drop out
            // since we're working in A's local space. Also, since each such plane is parallel
            // to the defining axis in each box, we know those dot products will be 0 and can
            // omit them.
            Vector3.Subtract(ref tri.V1, ref tri.V2, out edge3);
            float dv0, dv1, dv2, dhalf;

            // a.X ^ b.X = (1,0,0) ^ edge1
            // axis = Vector3(0, -edge1.Z, edge1.Y);
            dv0 = tri.V0.Z * edge1.Y - tri.V0.Y * edge1.Z;
            dv1 = tri.V1.Z * edge1.Y - tri.V1.Y * edge1.Z;
            dv2 = tri.V2.Z * edge1.Y - tri.V2.Y * edge1.Z;
            dhalf = Math.Abs(halfExtent.Y * edge1.Z) + Math.Abs(halfExtent.Z * edge1.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.X ^ b.Y = (1,0,0) ^ edge2
            // axis = Vector3(0, -edge2.Z, edge2.Y);
            dv0 = tri.V0.Z * edge2.Y - tri.V0.Y * edge2.Z;
            dv1 = tri.V1.Z * edge2.Y - tri.V1.Y * edge2.Z;
            dv2 = tri.V2.Z * edge2.Y - tri.V2.Y * edge2.Z;
            dhalf = Math.Abs(halfExtent.Y * edge2.Z) + Math.Abs(halfExtent.Z * edge2.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.X ^ b.Y = (1,0,0) ^ edge3
            // axis = Vector3(0, -edge3.Z, edge3.Y);
            dv0 = tri.V0.Z * edge3.Y - tri.V0.Y * edge3.Z;
            dv1 = tri.V1.Z * edge3.Y - tri.V1.Y * edge3.Z;
            dv2 = tri.V2.Z * edge3.Y - tri.V2.Y * edge3.Z;
            dhalf = Math.Abs(halfExtent.Y * edge3.Z) + Math.Abs(halfExtent.Z * edge3.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,1,0) ^ edge1
            // axis = Vector3(edge1.Z, 0, -edge1.X);
            dv0 = tri.V0.X * edge1.Z - tri.V0.Z * edge1.X;
            dv1 = tri.V1.X * edge1.Z - tri.V1.Z * edge1.X;
            dv2 = tri.V2.X * edge1.Z - tri.V2.Z * edge1.X;
            dhalf = Math.Abs(halfExtent.X * edge1.Z) + Math.Abs(halfExtent.Z * edge1.X);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,1,0) ^ edge2
            // axis = Vector3(edge2.Z, 0, -edge2.X);
            dv0 = tri.V0.X * edge2.Z - tri.V0.Z * edge2.X;
            dv1 = tri.V1.X * edge2.Z - tri.V1.Z * edge2.X;
            dv2 = tri.V2.X * edge2.Z - tri.V2.Z * edge2.X;
            dhalf = Math.Abs(halfExtent.X * edge2.Z) + Math.Abs(halfExtent.Z * edge2.X);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,1,0) ^ bX
            // axis = Vector3(edge3.Z, 0, -edge3.X);
            dv0 = tri.V0.X * edge3.Z - tri.V0.Z * edge3.X;
            dv1 = tri.V1.X * edge3.Z - tri.V1.Z * edge3.X;
            dv2 = tri.V2.X * edge3.Z - tri.V2.Z * edge3.X;
            dhalf = Math.Abs(halfExtent.X * edge3.Z) + Math.Abs(halfExtent.Z * edge3.X);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,0,1) ^ edge1
            // axis = Vector3(-edge1.Y, edge1.X, 0);
            dv0 = tri.V0.Y * edge1.X - tri.V0.X * edge1.Y;
            dv1 = tri.V1.Y * edge1.X - tri.V1.X * edge1.Y;
            dv2 = tri.V2.Y * edge1.X - tri.V2.X * edge1.Y;
            dhalf = Math.Abs(halfExtent.Y * edge1.X) + Math.Abs(halfExtent.X * edge1.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,0,1) ^ edge2
            // axis = Vector3(-edge2.Y, edge2.X, 0);
            dv0 = tri.V0.Y * edge2.X - tri.V0.X * edge2.Y;
            dv1 = tri.V1.Y * edge2.X - tri.V1.X * edge2.Y;
            dv2 = tri.V2.Y * edge2.X - tri.V2.X * edge2.Y;
            dhalf = Math.Abs(halfExtent.Y * edge2.X) + Math.Abs(halfExtent.X * edge2.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,0,1) ^ edge3
            // axis = Vector3(-edge3.Y, edge3.X, 0);
            dv0 = tri.V0.Y * edge3.X - tri.V0.X * edge3.Y;
            dv1 = tri.V1.Y * edge3.X - tri.V1.X * edge3.Y;
            dv2 = tri.V2.Y * edge3.X - tri.V2.X * edge3.Y;
            dhalf = Math.Abs(halfExtent.Y * edge3.X) + Math.Abs(halfExtent.X * edge3.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            return ContainmentType.Intersects;
        }

        #endregion
    }

}
