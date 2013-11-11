﻿using System;
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
        public Vector3 _modelPosition{ get ; set; }
        public Vector3 _modelRotation { get; set; }
        public Vector3 _modelScale { get; set; }
        public Vector3 _lightDirection { get; set; }
 
        public Model _model { get; private set; }

        public float Alpha = 1.0f;
 
        private Matrix[] _modelTransforms;
        private GraphicsDevice _graphicsDevice;
        private BoundingSphere boundingSphere;

        public Materials.Material Material { get; set; }

        public BoundingSphere BoundingSphere
        {
            get
            {
                Matrix worldTransform = Matrix.CreateScale(_modelScale)
                    * Matrix.CreateTranslation(_modelPosition);

                BoundingSphere transformed = boundingSphere;
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
                        //((BasicEffect)effect).EnableDefaultLighting();
                        ((BasicEffect)effect).LightingEnabled = true; // turn on the lighting subsystem.
                        ((BasicEffect)effect).DirectionalLight0.DiffuseColor = new Vector3(1, 1, 1); // a red light
                        ((BasicEffect)effect).DirectionalLight0.Direction = _lightDirection;  // coming along the x-axis
                        ((BasicEffect)effect).DirectionalLight0.SpecularColor = new Vector3(1, 1, 1); // with green highlights
                        ((BasicEffect)effect).Alpha = Alpha;
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
                BoundingSphere transformed = mesh.BoundingSphere.Transform(
                    _modelTransforms[mesh.ParentBone.Index]);

                sphere = BoundingSphere.CreateMerged(sphere, transformed);
            }

            this.boundingSphere = sphere;
        }


        /// <summary>
        /// Set to the specified effet the parameter given
        /// </summary>
        /// <param name="effect">The effect the parameter is applied to</param>
        /// <param name="paramName">The parameter name</param>
        /// <param name="val">The parameter value</param>
        void setEffectParameter(Effect effect, string paramName, object val)
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
}
