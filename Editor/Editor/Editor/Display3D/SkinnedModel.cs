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
using SkinnedModel;

namespace Editor.Display3D
{
    class SkinnedModel
    {
        Model model; 
        GraphicsDevice graphicsDevice; 
        ContentManager content;
        SkinningData skinningData;
        public Vector3 Position, Rotation, Scale;

        public Model Model { get { return model; } }
        public SkinnedModel(Model Model, Vector3 Position, Vector3 Rotation, Vector3 Scale, GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            this.model = Model; this.graphicsDevice = GraphicsDevice; this.content = Content; this.Position = Position; this.Rotation = Rotation; this.Scale = Scale;
            this.skinningData = model.Tag as SkinningData;
            setNewEffect();
        }

        void setNewEffect()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    SkinnedEffect newEffect = new SkinnedEffect(graphicsDevice); 
                    BasicEffect oldEffect = ((BasicEffect)part.Effect);
                    newEffect.EnableDefaultLighting(); 
                    newEffect.SpecularColor = Color.Black.ToVector3();
                    newEffect.AmbientLightColor = oldEffect.AmbientLightColor; 
                    newEffect.DiffuseColor = oldEffect.DiffuseColor; 
                    newEffect.Texture = oldEffect.Texture;
                    part.Effect = newEffect;
                }
            }
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            Matrix world = Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects) 
                { 
                    effect.World = world; effect.View = View; effect.Projection = Projection; 
                }
                mesh.Draw();
            }
        } 
    }
}
