using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Display3D
{
    class CLightsManager
    {
        public static List<Materials.PPPointLight> lights = new List<Materials.PPPointLight>();

        public static void AddLight(Vector3 position, Color color, float attenuation)
        {
            lights.Add(new Materials.PPPointLight(position, color, attenuation));
        }

        public static void AddToRenderer()
        {
            Display3D.CModelManager.renderer.Lights = lights;
        }

    }
}
