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
    class CModel
    {
        public Vector3 _modelPosition{ get ; set; }
        public Vector3 _modelRotation { get; set; }
        public Vector3 _modelScale { get; set; }
 
        public Model _model { get; private set; }
 
        private Matrix[] _modelTransforms;
 
        private GraphicsDevice _graphicsDevice;
 
        public CModel(Model model, Vector3 modelPos, Vector3 modelRotation, Vector3 modelScale, GraphicsDevice device)
            // Constructor : Used to initialize all the value, position, scale of a model
        {
            this._model = model;
 
            this._modelPosition = modelPos;
            this._modelRotation = modelRotation;
            this._modelScale = modelScale;
 
            _modelTransforms = new Matrix[model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_modelTransforms);
 
            this._graphicsDevice = device;
        }

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
