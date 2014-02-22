using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Timers;
using System.Security.Cryptography;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using NLua;

namespace Engine.Game.Script
{
    class CLuaScriptFunctions
    {
        public void SetTimer(string func, double ms)
        {
            if (CLuaVM.VMHandler.GetFunction(func) == null)
            {
                CLuaVM.CallEvent("errorEncountered", new object[] { "SetTimer: Function " + func + " is not defined." });
                return;
            }

            System.Timers.Timer sttime = new System.Timers.Timer(ms);
            sttime.Start();
            sttime.Elapsed += delegate(object sender, ElapsedEventArgs e)
            {
                LuaFunction fnc = CLuaVM.VMHandler.GetFunction(func);
                fnc.Call();
                sttime.Stop();
            };
        }

        // Script related
        public void Print(string msg)
        {
            CConsole.addMessage(msg);
        }

        public DateTime GetDate()
        {
            return DateTime.Now;
        }

        public string GetMD5(string str)
        {
            // Calculate MD5 from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(str);
            byte[] hash = md5.ComputeHash(inputBytes);

            // Convert byte to array string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }

        public string GetFileMD5(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(file))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        public string GetGameState()
        {
            return CGameManagement.currentState;
        }

        public void ChangeGameState(string gameState)
        {
            CGameManagement.ChangeState(gameState);
        }

        public Embedded.XMLManager XMLReader(string file)
        {
            return new Embedded.XMLManager(file);
        }

        /*public System.Xml.XmlWriter XMLWriter(string file)
        {
            return System.Xml.XmlWriter.Create(file);
        }*/

        // Enums
        public void GetEnum(string enumType)
        {
            Type type = null;
            switch (enumType)
            {
                case "Keys":
                    type = typeof(Keys);
                    break;
                case "Buttons":
                    type = typeof(Buttons);
                    break;
            }

            if (type != null)
            {

                string[] names = Enum.GetNames(type);

                CLuaVM.VMHandler.NewTable(type.Name);

                int i = 0;
                foreach (int name in Enum.GetValues(type))
                {
                    string path = type.Name + "." + names[i++];
                    CLuaVM.VMHandler[path] = name;
                }
            }
            else
                CConsole.addMessage("Invalid enum: " + enumType);
        }

        // Settings
        public Settings.KeyMapping GetKeyMappings()
        {
            return Settings.CGameSettings._gameSettings.KeyMapping;
        }

        public Settings.Video GetVideoSettings()
        {
            return Settings.CGameSettings._gameSettings.Video;
        }

        public Vector2 GetScreenSize()
        {
            Vector2 size = new Vector2(
                Display2D.C2DEffect._graphicsDevice.PresentationParameters.BackBufferWidth,
                Display2D.C2DEffect._graphicsDevice.PresentationParameters.BackBufferHeight
                );
            return size;
        }

        // 2D Effects
        public void FadeScreen(int fadeOpacity, int timeMilliSecs, int sizeX, int sizeY, int posX, int posY, int red, int green, int blue, string callBack)
        {
            Display2D.C2DEffect.MethodDelegate testDelC = () => { Script.CLuaVM.CallFunction(callBack); };
            Display2D.C2DEffect.fadeEffect(fadeOpacity, timeMilliSecs, new Vector2(sizeX, sizeY), new Vector2(posX, posY), new Color(red, green, blue), testDelC);
        }

        public Embedded.C2DScriptRectangle GUIRectangle(Rectangle rect, Color color, bool active = true, int order = 1)
        {
            Embedded.C2DScriptRectangle elt = new Embedded.C2DScriptRectangle(rect, null, color, null, active, order);
            Display2D.C2DEffect.ScriptableRectangle.Add(elt);
            Display2D.C2DEffect.ScriptableRectangle = Display2D.C2DEffect.ScriptableRectangle.OrderBy(ord => ord.drawOrder).ToList();
            return elt;
        }

        public Embedded.C2DScriptRectangle GUIImage(Rectangle rect, Texture2D texture, Rectangle? sourceRect = null, Color? color = null, bool active = true, int order = 1)
        {
            if (sourceRect == Rectangle.Empty)
                sourceRect = null;

            Embedded.C2DScriptRectangle elt = new Embedded.C2DScriptRectangle(rect, sourceRect, color, texture, active, order);
            Display2D.C2DEffect.ScriptableRectangle.Add(elt);
            Display2D.C2DEffect.ScriptableRectangle = Display2D.C2DEffect.ScriptableRectangle.OrderBy(ord => ord.drawOrder).ToList();
            return elt;
        }

        public Texture2D GetTexture(string filename)
        {
            return Display2D.C2DEffect._content.Load<Texture2D>(filename);
        }

        // 2D Effects - Basic usage functions
        public Rectangle GetRectangle(int startX = 0, int startY = 0, int width = 0, int height = 0)
        {
            return new Rectangle(startX, startY, width, height);
        }

        public Color GetColor(int r, int g, int b, int a = 255)
        {
            return new Color(r, g, b, a);
        }

        public Color GetColorFromHex(string hexString)
        {
            if (hexString == null)
                return Color.White;
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);
            uint hex = uint.Parse(hexString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
            Color color = Color.White;
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

        public MouseState GetCursorInfo()
        {
            return Display2D.C2DEffect._mouseState;
        }

        public Vector3 Get3DTo2DPosition(float x, float y, float z)
        {
            if (CConsole._Camera != null)
                return Display2D.C2DEffect._graphicsDevice.Viewport.Project(new Vector3(x, y, z), CConsole._Camera._projection, CConsole._Camera._view, Matrix.Identity);
            else
                return Vector3.Zero;
        }

        public float GetDistanceBetweenPoints3D(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            return Vector3.Distance(new Vector3(x1, y1, z1), new Vector3(x2, y2, z2));
        }

        public float GetDistanceBetweenPoints2D(float x1, float y1, float x2, float y2)
        {
            return Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2));
        }

        public Vector3 GetCameraPosition()
        {
            if (CConsole._Camera != null)
                return CConsole._Camera._cameraPos;
            else
                return Vector3.Zero;
        }
    }
}
