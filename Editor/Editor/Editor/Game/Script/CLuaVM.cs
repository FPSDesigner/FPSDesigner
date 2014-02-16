using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using NLua;

namespace Editor.Game.Script
{
    class CLuaVM
    {
        public static Lua VMHandler;
        private static CLuaScriptFunctions scriptFunctions;
        public static Dictionary<string, string> EventsListVM = new Dictionary<string, string>();

        public static bool _settingEnableHighFreqCalls = true;

        public static void Initialize()
        {
            VMHandler = new Lua();
            scriptFunctions = new CLuaScriptFunctions();
            //VMHandler.LoadCLRPackage();

            // Initialize Events
            Type type = typeof(CLuaVM);
            MethodInfo info = type.GetMethod("internal_AddEvent",  BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            RegisterFunction("addEvent", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, info);

            LoadDefaultFunctions();
        }

        public static void internal_AddEvent(string eventName, string functionVMName)
        {
            if (!EventsListVM.ContainsKey(eventName))
            {
                EventsListVM.Add(eventName, functionVMName);
            }
        }

        public static void RegisterFunction(string functionName, object target, System.Reflection.MethodBase function)
        {
            VMHandler.RegisterFunction(functionName, target, function);
        }

        public static void LoadScript(string scriptName)
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

        public static void CallEvent(string eventName, object[] parameters = default(object[]))
        {
            if (EventsListVM.ContainsKey(eventName))
                CallFunction(EventsListVM[eventName], parameters);
        }

        public static void CallFunction(string functionName, object[] parameters = default(object[]))
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

        public static void LoadDefaultFunctions()
        {
            // Script related
            RegisterFunction("print", scriptFunctions, scriptFunctions.GetType().GetMethod("Print"));
            RegisterFunction("log", scriptFunctions, scriptFunctions.GetType().GetMethod("Print"));
            RegisterFunction("setTimer", scriptFunctions, scriptFunctions.GetType().GetMethod("SetTimer"));
            RegisterFunction("getDate", scriptFunctions, scriptFunctions.GetType().GetMethod("GetDate"));
            RegisterFunction("getEnum", scriptFunctions, scriptFunctions.GetType().GetMethod("GetEnum"));
            //RegisterFunction("getScreenSize", scriptFunctions, scriptFunctions.GetType().GetMethod("GetScreenSize"));

            RegisterFunction("getMD5", scriptFunctions, scriptFunctions.GetType().GetMethod("GetMD5"));
            RegisterFunction("getFileMD5", scriptFunctions, scriptFunctions.GetType().GetMethod("GetFileMD5"));

            RegisterFunction("XMLReader", scriptFunctions, scriptFunctions.GetType().GetMethod("XMLReader"));
            //RegisterFunction("xmlWriter", scriptFunctions, scriptFunctions.GetType().GetMethod("XMLWriter"));

            // Settings
            RegisterFunction("getKeyMappings", scriptFunctions, scriptFunctions.GetType().GetMethod("GetKeyMappings"));
            RegisterFunction("getVideoSettings", scriptFunctions, scriptFunctions.GetType().GetMethod("GetVideoSettings"));

            // GUI
            RegisterFunction("fadeScreen", scriptFunctions, scriptFunctions.GetType().GetMethod("FadeScreen"));
            RegisterFunction("guiRectangle", scriptFunctions, scriptFunctions.GetType().GetMethod("GUIRectangle"));
            RegisterFunction("guiImage", scriptFunctions, scriptFunctions.GetType().GetMethod("GUIImage"));

            // Basic GUI functions
            RegisterFunction("getRectangle", scriptFunctions, scriptFunctions.GetType().GetMethod("GetRectangle"));
            RegisterFunction("getTexture", scriptFunctions, scriptFunctions.GetType().GetMethod("GetTexture"));
            RegisterFunction("getColor", scriptFunctions, scriptFunctions.GetType().GetMethod("GetColor"));
            RegisterFunction("getColorFromHex", scriptFunctions, scriptFunctions.GetType().GetMethod("GetColorFromHex"));
            RegisterFunction("getCursorPosition", scriptFunctions, scriptFunctions.GetType().GetMethod("GetCursorPosition"));
            RegisterFunction("get3DTo2DPosition", scriptFunctions, scriptFunctions.GetType().GetMethod("Get3DTo2DPosition"));

            // Area functions
            RegisterFunction("getDistanceBetweenPoints3D", scriptFunctions, scriptFunctions.GetType().GetMethod("GetDistanceBetweenPoints3D"));
            RegisterFunction("getDistanceBetweenPoints2D", scriptFunctions, scriptFunctions.GetType().GetMethod("GetDistanceBetweenPoints2D"));

            // Camera function
            RegisterFunction("getCameraPosition", scriptFunctions, scriptFunctions.GetType().GetMethod("GetCameraPosition"));

        }
    }
}
