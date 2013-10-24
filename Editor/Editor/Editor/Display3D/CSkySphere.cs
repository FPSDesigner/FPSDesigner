using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Editor.Display3D
{
    public class SkySphere
    {
        CModel model;
        Effect effect;
        GraphicsDevice graphics;

        public SkySphere(ContentManager Content, GraphicsDevice GraphicsDevice,
            TextureCube Texture)
        {
            model = new CModel(Content.Load<Model>("skysphere_mesh"), Vector3.Zero,
                Vector3.Zero, Vector3.One, GraphicsDevice);

            effect = Content.Load<Effect>("skysphere_effect");
            effect.Parameters["CubeMap"].SetValue(Texture);

            model.SetModelEffect(effect, false);

            this.graphics = GraphicsDevice;
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            // Disable the depth buffer
            graphics.DepthStencilState = DepthStencilState.None;

            // Move the model with the sphere
            model.Position = CameraPosition;

            model.Draw(View, Projection, CameraPosition);

            graphics.DepthStencilState = DepthStencilState.Default;
        }
    }
}
