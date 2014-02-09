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

        public void Print(string msg)
        {
            CConsole.addMessage(msg);
        }
    }
}
