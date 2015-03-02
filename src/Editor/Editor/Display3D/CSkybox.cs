using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Engine.Display3D
{
    // Make SkySphere IRenderable
    public class CSkybox : IRenderable
    {
        CModel model;
        private Effect effect;
        private GraphicsDevice graphics;

        float Intensity;
        public float ColorIntensity
        {
            get { return Intensity; }
            set
            {
                Intensity = value;
                effect.Parameters["ColorIntensity"].SetValue(value);
            }
        }

        public CSkybox(ContentManager Content, GraphicsDevice GraphicsDevice, TextureCube Texture)
        {
            model = new CModel(Content.Load<Model>("3D/Skysphere"), Vector3.Zero, Vector3.Zero, new Vector3(2000), GraphicsDevice);
            model.shouldNotUpdateTriangles = true;

            effect = Content.Load<Effect>("Effects/Skysphere");
            effect.Parameters["CubeMap"].SetValue(Texture);

            model.SetModelEffect(effect, false);

            this.graphics = GraphicsDevice;

            ColorIntensity = 1.0f;
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            graphics.DepthStencilState = DepthStencilState.None;

            model._modelPosition = CameraPosition;

            model.Draw(View, Projection, CameraPosition);

            graphics.DepthStencilState = DepthStencilState.Default;
        }

        public void SetClipPlane(Vector4? Plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

            if (Plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(Plane.Value);
        }
    }
}
