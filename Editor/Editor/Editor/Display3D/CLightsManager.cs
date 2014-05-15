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
        public static int selectedLight = -1;

        private static Texture2D textureLight;
        private static float scaleTexture = 0.2f;

        public static void LoadContent(ContentManager content)
        {
            textureLight = content.Load<Texture2D>("2D/light");
        }

        public static void AddLight(Vector3 position, Color color, float attenuation)
        {
            lights.Add(new Materials.PPPointLight(position, color, attenuation));
        }

        public static void RemoveLight(int id)
        {
            lights.RemoveAt(id);
        }

        public static void AddToRenderer()
        {
            Display3D.CModelManager.renderer.Lights = lights;
        }

        public static void DrawSelect(SpriteBatch spriteBatch, CCamera cam)
        {
            spriteBatch.Begin();
            for (int i = 0; i < lights.Count; i++)
            {
                Vector3 Pos = Display2D.C2DEffect._graphicsDevice.Viewport.Project(lights[i].Position, cam._projection, cam._view, Matrix.Identity);

                if (Pos.Z < 1)
                {
                    Color col = lights[i].Color;
                    if (selectedLight != i)
                        col *= 0.2f;

                    spriteBatch.Draw(textureLight, new Vector2(Pos.X, Pos.Y), null, col, 0, Vector2.Zero, scaleTexture, SpriteEffects.None, 0);
                }
            }
            spriteBatch.End();
        }

        public static bool PointClicksLight(Vector2 pos, CCamera cam, out int clickedLight)
        {
            clickedLight = -1;

            for(int i = 0; i < lights.Count; i++)
            {
                Vector3 PosLight = Display2D.C2DEffect.softwareViewport.Project(lights[i].Position, cam._projection, cam._view, Matrix.Identity);

                if (PosLight.Z < 1)
                    if(pos.X >= PosLight.X && pos.X <= PosLight.X + textureLight.Width * scaleTexture)
                        if (pos.Y >= PosLight.Y && pos.Y <= PosLight.Y + textureLight.Height * scaleTexture)
                        {
                            clickedLight = i;
                            return true;
                        }
            }
            
            return false;
        }

        public static void UpdateGameLevel(ref Game.LevelInfo.LevelData lvl)
        {
            for (int i = 0; i < lights.Count; i++)
            {
                Materials.PPPointLight light = lights[i];

                lvl.Lights.LightsList[i].Attenuation = light.Attenuation;
                lvl.Lights.LightsList[i].Color = light.Color.R.ToString("X") + light.Color.G.ToString("X") + light.Color.B.ToString("X");
                lvl.Lights.LightsList[i].Position = new Game.LevelInfo.Coordinates(light.Position);
            }

            while (lvl.Lights.LightsList.Count != lights.Count)
                lvl.Lights.LightsList.RemoveAt(lvl.Lights.LightsList.Count - 1);
        }

        public static Color GetColorFromHex(string hexString)
        {
            if (hexString == null)
                return Microsoft.Xna.Framework.Color.White;
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);
            uint hex = uint.Parse(hexString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
            Color color = Microsoft.Xna.Framework.Color.White;
            if (hexString.Length == 8)
            {
                color.A = (byte)(hex >> 24);
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            else if (hexString.Length == 6)
            {
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            return color;
        }

    }
}
