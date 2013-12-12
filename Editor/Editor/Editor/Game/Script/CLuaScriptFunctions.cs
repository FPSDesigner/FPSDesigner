using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Timers;

using NLua;

namespace Editor.Game.Script
{
    class CLuaScriptFunctions
    {
        public Lua VMHandler;

        public void LoadFunctions(Lua handler)
        {
            VMHandler = handler;
        }

        public void SetTimer(string func, double ms)
        {
            System.Timers.Timer sttime = new System.Timers.Timer(ms);
            sttime.Start();
            sttime.Elapsed += delegate(object sender, ElapsedEventArgs e)
            {
                LuaFunction fnc = VMHandler.GetFunction(func);
                fnc.Call();
                sttime.Stop();
            };
        }
    }
}
