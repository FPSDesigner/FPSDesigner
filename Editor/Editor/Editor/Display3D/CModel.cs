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
    class CModel
    {
        public Vector3 _modelPosition{ get ; set; }
        public Vector3 _modelRotation { get; set; }
        public Vector3 _modelScale { get; set; }
 
        public Model _model { get; private set; }
 
        private Matrix[] _modelTransforms;
 
        private GraphicsDevice _graphicsDevice;
 
        /// <summary>
        /// Initialize the model class
        /// </summary>
        /// <param name="model">Model element</param>
        /// <param name="modelPos">Position of the model</param>
        /// <param name="modelRotation">Rotation of the model</param>
        /// <param name="modelScale">Scale of the model (size)</param>
        /// <param name="device">GraphicsDevice class</param>
        public CModel(Model model, Vector3 modelPos, Vector3 modelRotation, Vector3 modelScale, GraphicsDevice device)
        {
            this._model = model;
 
            this._modelPosition = modelPos;
            this._modelRotation = modelRotation;
            this._modelScale = modelScale;
 
            _modelTransforms = new Matrix[model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_modelTransforms);
 
            this._graphicsDevice = device;
        }

        /// <summary>
        /// Draw the model in the world
        /// </summary>
        /// <param name="view">View Matrix used in CCamera class</param>
        /// <param name="projection">Projection Matrix used in CCamera class</param>
        public void Draw(Matrix view, Matrix projection)
        {
            // Matrix which display the model in the world
            Matrix world = Matrix.CreateScale(_modelScale) *
                Matrix.CreateFromYawPitchRoll(_modelRotation.Y, _modelRotation.X, _modelRotation.Z) *
                Matrix.CreateTranslation(_modelPosition);

            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (ModelMeshPart meshParts in mesh.MeshParts)
                {
                    Matrix localWorld = _modelTransforms[mesh.ParentBone.Index] * world;
                    BasicEffect effect = (BasicEffect)meshParts.Effect;
                    effect.World = localWorld;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                }

                mesh.Draw();
            }
        }
    }
}
