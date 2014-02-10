using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Timers;

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
        public CLuaVM LuaVM;

        public void SetTimer(string func, double ms)
        {
            if (LuaVM.VMHandler.GetFunction(func) == null)
            {
                LuaVM.CallEvent("errorEncountered", new object[] { "SetTimer: Function " + func + " is not defined." });
                return;
            }

            System.Timers.Timer sttime = new System.Timers.Timer(ms);
            sttime.Start();
            sttime.Elapsed += delegate(object sender, ElapsedEventArgs e)
            {
                LuaFunction fnc = LuaVM.VMHandler.GetFunction(func);
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

                LuaVM.VMHandler.NewTable(type.Name);

                int i = 0;
                foreach (int name in Enum.GetValues(type))
                {
                    string path = type.Name + "." + names[i++];
                    LuaVM.VMHandler[path] = name;
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
            Display2D.C2DEffect.MethodDelegate testDelC = () => { LuaVM.CallFunction(callBack); };
            Display2D.C2DEffect.fadeEffect(fadeOpacity, timeMilliSecs, new Vector2(sizeX, sizeY), new Vector2(posX, posY), new Color(red, green, blue), testDelC);
        }
    }
}
