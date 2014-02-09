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
            base.Initialize();

            VMHandler = new Lua();
            scriptFunctions = new CLuaScriptFunctions();
            scriptFunctions.LuaVM = this;
            //VMHandler.LoadCLRPackage();

            // Initialize Events
            RegisterFunction("addEvent", this, this.GetType().GetMethod("internal_AddEvent"));
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
            }
            catch (Exception e)
            {
                Game.CConsole.addMessage("Script loading exception: " + e, true);
            }
        }

        public override void CallEvent(string eventName, object[] parameters = default(object[]))
        {
            if (EventsListVM.ContainsKey(eventName))
            {
                LuaFunction eventFunc = (LuaFunction)VMHandler[EventsListVM[eventName]];
                if(eventFunc != null)
                    eventFunc.Call(parameters);
            }
        }

        public override void LoadDefaultFunctions()
        {
            RegisterFunction("setTimer", scriptFunctions, scriptFunctions.GetType().GetMethod("SetTimer"));
        }
    }
}
