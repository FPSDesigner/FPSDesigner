using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPFLocalizeExtension.Extensions;
using WPFLocalizeExtension.Engine;

namespace Software
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Set the current user interface culture to the specific culture

            LocalizeDictionary.Instance.Culture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
            
            GlobalVars.AddConsoleMsg(GlobalVars.GetUIString("Logs_Initializing_Editor"));
        }
    }
}
