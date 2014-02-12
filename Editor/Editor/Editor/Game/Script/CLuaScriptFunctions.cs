using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Timers;
using System.Security.Cryptography;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using NLua;

namespace Editor.Game.Script
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
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-","").ToLower();
                }
            }
        }

        public XmlReader XMLReader(string file)
        {
            return XmlReader.Create(file);
        }

        public XmlWriter XMLWriter(string file)
        {
            return XmlWriter.Create(file);
        }

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

        // 2D Effects
        public void FadeScreen(int fadeOpacity, int timeMilliSecs, int sizeX, int sizeY, int posX, int posY, int red, int green, int blue, string callBack)
        {
            Display2D.C2DEffect.MethodDelegate testDelC = () => { Script.CLuaVM.CallFunction(callBack); };
            Display2D.C2DEffect.fadeEffect(fadeOpacity, timeMilliSecs, new Vector2(sizeX, sizeY), new Vector2(posX, posY), new Color(red, green, blue), testDelC);
        }

        public Embedded.C2DScriptRectangle GUIRectangle(Rectangle rect, Color color, bool active = true, int order = 1)
        {
            Embedded.C2DScriptRectangle elt = new Embedded.C2DScriptRectangle(rect, color, active, order);
            Display2D.C2DEffect.ScriptableRectangle.Add(elt);
            Display2D.C2DEffect.ScriptableRectangle = Display2D.C2DEffect.ScriptableRectangle.OrderBy(ord => ord.drawOrder).ToList();
            return elt;
        }

        public Embedded.C2DScriptRectangle GUIImage(Rectangle rect, Color color, Texture2D texture, bool active = true, int order = 1)
        {
            Embedded.C2DScriptRectangle elt = new Embedded.C2DScriptRectangle(rect, color, active, order, texture);
            Display2D.C2DEffect.ScriptableRectangle.Add(elt);
            Display2D.C2DEffect.ScriptableRectangle = Display2D.C2DEffect.ScriptableRectangle.OrderBy(ord => ord.drawOrder).ToList();
            return elt;
        }

        public Texture2D GetTexture(string filename)
        {
            return Display2D.C2DEffect._content.Load<Texture2D>(filename);
        }

        // 2D Effects - Basic usage functions
        public Rectangle GetRectangle(int startX, int startY, int width, int height)
        {
            return new Rectangle(startX, startY, width, height);
        }

        public Color GetColor(int r, int g, int b, int a = 255)
        {
            return new Color(r, g, b, a);
        }
    }
}
