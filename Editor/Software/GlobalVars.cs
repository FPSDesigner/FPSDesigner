using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WPFLocalizeExtension.Extensions;

namespace Software
{
    static class GlobalVars
    {
        public static List<string> LogList = new List<string>();
        public static string selectedTool = "Select";

        

        public static string GetUIString(string key)
        {
            string uiString;
            LocExtension locExtension = new LocExtension("Software:Strings:"+key);
            locExtension.ResolveLocalizedValue(out uiString);
            return uiString;
        }

        public static void AddConsoleMsg(string key)
        {
            LogList.Add(key);
        }
    }
}
