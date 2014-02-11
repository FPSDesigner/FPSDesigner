using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NLua;

namespace Editor.Game.Script
{
    class CLuaVM : CScriptVM
    {
        public Lua VMHandler;
        private CLuaScriptFunctions scriptFunctions;

        public override void Initialize()
        {
            VMHandler = new Lua();
            scriptFunctions = new CLuaScriptFunctions();
            scriptFunctions.LuaVM = this;
            //VMHandler.LoadCLRPackage();

            // Initialize Events
            RegisterFunction("addEvent", this, this.GetType().GetMethod("internal_AddEvent"));

            base.Initialize();
        }

        public override void internal_AddEvent(string eventName, string functionVMName)
        {
            base.internal_AddEvent(eventName, functionVMName);
        }

        public override void RegisterFunction(string functionName, object target, System.Reflection.MethodBase function)
        {
            VMHandler.RegisterFunction(functionName, target, function);
        }

        public override void LoadScript(string scriptName)
        {
            try
            {
                VMHandler.DoFile(scriptName);
                CallEvent("scriptInit");
            }
            catch (Exception e)
            {
                Game.CConsole.addMessage("LUA Script Exception: " + e, true);
            }
        }

        public override void CallEvent(string eventName, object[] parameters = default(object[]))
        {
            if (EventsListVM.ContainsKey(eventName))
                CallFunction(EventsListVM[eventName], parameters);
        }

        public override void CallFunction(string functionName, object[] parameters = default(object[]))
        {
            LuaFunction func = (LuaFunction)VMHandler[functionName];
            if (func != null)
            {
                if (parameters == null)
                    func.Call();
                else
                    func.Call(parameters);
            }
        }

        public override void LoadDefaultFunctions()
        {
            // Script related
            RegisterFunction("print", scriptFunctions, scriptFunctions.GetType().GetMethod("Print"));
            RegisterFunction("log", scriptFunctions, scriptFunctions.GetType().GetMethod("Print"));
            RegisterFunction("setTimer", scriptFunctions, scriptFunctions.GetType().GetMethod("SetTimer"));
            RegisterFunction("getDate", scriptFunctions, scriptFunctions.GetType().GetMethod("GetDate"));
            RegisterFunction("getEnum", scriptFunctions, scriptFunctions.GetType().GetMethod("GetEnum"));
            //RegisterFunction("getScreenSize", scriptFunctions, scriptFunctions.GetType().GetMethod("GetScreenSize"));

            // Settings
            RegisterFunction("getKeyMappings", scriptFunctions, scriptFunctions.GetType().GetMethod("GetKeyMappings"));
            RegisterFunction("getVideoSettings", scriptFunctions, scriptFunctions.GetType().GetMethod("GetVideoSettings"));

            // GUI
            RegisterFunction("fadeScreen", scriptFunctions, scriptFunctions.GetType().GetMethod("FadeScreen"));
            RegisterFunction("guiRectangle", scriptFunctions, scriptFunctions.GetType().GetMethod("GUIRectangle"));

            // Basic GUI functions
            RegisterFunction("getRectangle", scriptFunctions, scriptFunctions.GetType().GetMethod("GetRectangle"));
            RegisterFunction("getColor", scriptFunctions, scriptFunctions.GetType().GetMethod("GetColor"));
        }
    }
}
