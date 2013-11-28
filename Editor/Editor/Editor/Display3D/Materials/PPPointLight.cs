using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Display3D.Materials
{
    public class PPPointLight
    {
        public Vector3 Position { get; set; }
        public Color Color { get; set; }
        public float Attenuation { get; set; }

        public PPPointLight(Vector3 Position, Color Color, float Attenuation)
        {
            this.Position = Position;
            this.Color = Color;
            this.Attenuation = Attenuation;
        }

        public void SetEffectParameters(Effect effect)
        {
            effect.Parameters["LightPosition"].SetValue(Position);
            effect.Parameters["LightAttenuation"].SetValue(Attenuation);
            effect.Parameters["LightColor"].SetValue(Color.ToVector3());
        }
    }
}
