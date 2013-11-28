using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Display3D.Materials
{
    public class PointLightMaterial : Material
    {
        public Vector3 AmbientLightColor { get; set; }
        public Vector3 LightPosition { get; set; }
        public Vector3 LightColor { get; set; }
        public float LightAttenuation { get; set; }
        public float LightFalloff { get; set; }

        public PointLightMaterial()
        {
            AmbientLightColor = new Vector3(.15f, .15f, .15f);
            LightPosition = new Vector3(0, 0, 0);
            LightColor = new Vector3(.85f, .85f, .85f);
            LightAttenuation = 5000;
            LightFalloff = 2;
        }

        public override void SetEffectParameters(Effect effect)
        {
            if (effect.Parameters["AmbientLightColor"] != null)
                effect.Parameters["AmbientLightColor"].SetValue(
                    AmbientLightColor);

            if (effect.Parameters["LightPosition"] != null)
                effect.Parameters["LightPosition"].SetValue(LightPosition);

            if (effect.Parameters["LightColor"] != null)
                effect.Parameters["LightColor"].SetValue(LightColor);

            if (effect.Parameters["LightAttenuation"] != null)
                effect.Parameters["LightAttenuation"].SetValue(
                    LightAttenuation);

            if (effect.Parameters["LightFalloff"] != null)
                effect.Parameters["LightFalloff"].SetValue(LightFalloff);
        }
    }
}
