using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NLua;

namespace Editor.Game.Script
{
    abstract class CScriptVM
    {
        public Dictionary<string, string> EventsListVM = new Dictionary<string, string>();

        public abstract void LoadScript(string scriptName);
        public abstract void RegisterFunction(string functionName, object target, System.Reflection.MethodBase function);
        public abstract void CallEvent(string eventName, object[] parameters);
        public abstract void LoadDefaultFunctions();

        public virtual void Initialize()
        {
            LoadDefaultFunctions();
        }

        public virtual void internal_AddEvent(string eventName, string functionVMName)
        {
            if (!EventsListVM.ContainsKey(eventName))
            {
                EventsListVM.Add(eventName, functionVMName);
            }
        }

    }
}
