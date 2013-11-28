using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Display3D.Materials
{
    public class MultiLightingMaterial : Material
    {
        public Vector3 AmbientColor { get; set; }
        public Vector3[] LightDirection { get; set; }
        public Vector3[] LightColor { get; set; }
        public Vector3 SpecularColor { get; set; }

        public MultiLightingMaterial()
        {
            AmbientColor = new Vector3(.1f, .1f, .1f);
            LightDirection = new Vector3[3];
            LightColor = new Vector3[] { Vector3.One, Vector3.One, 
                Vector3.One };
            SpecularColor = new Vector3(1, 1, 1);
        }

        public override void SetEffectParameters(Effect effect)
        {
            if (effect.Parameters["AmbientColor"] != null)
                effect.Parameters["AmbientColor"].SetValue(AmbientColor);

            if (effect.Parameters["LightDirection"] != null)
                effect.Parameters["LightDirection"].SetValue(LightDirection);

            if (effect.Parameters["LightColor"] != null)
                effect.Parameters["LightColor"].SetValue(LightColor);

            if (effect.Parameters["SpecularColor"] != null)
                effect.Parameters["SpecularColor"].SetValue(SpecularColor);
        }
    }
}
