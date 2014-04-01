using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using FirstFloor.ModernUI.Presentation;

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


        #region "Windows Menu Helper"
        public static void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            //MainWindow MainWindowInstance = MainWindow.Instance;
            //if (e.Fragment == MainWindowInstance.MenuWindows.Links[0].DisplayName)
            //    return;

            //Console.WriteLine("Open Window: " + e.Fragment);
            //RoutedUICommand ruic = NavigationCommands.BrowseHome;
            //ruic.Execute(null, null);
        }

        public static void OnNavigatedTo(NavigationEventArgs e)
        {
            //MainWindow MainWindowInstance = MainWindow.Instance;
            //foreach (Link lc in MainWindowInstance.MenuWindows.Links)
            //    lc.Source = new Uri(e.Source.OriginalString + "#" + lc.DisplayName, UriKind.RelativeOrAbsolute);
        }

        public static void OnNavigatedFrom(NavigationEventArgs e)
        {
            //MainWindow MainWindowInstance = MainWindow.Instance;
            //foreach (Link lc in MainWindowInstance.MenuWindows.Links)
            //    lc.Source = new Uri(e.Source.OriginalString + "#" + lc.DisplayName, UriKind.RelativeOrAbsolute);
        }

        public static void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //MainWindow MainWindowInstance = MainWindow.Instance;
            //foreach (Link lc in MainWindowInstance.MenuWindows.Links)
            //    lc.Source = new Uri(e.Source.OriginalString + "#" + lc.DisplayName, UriKind.RelativeOrAbsolute);
        }
        #endregion
    }

    static public partial class NativeMethods
    {
        /// Return Type: BOOL->int  
        ///X: int  
        ///Y: int  
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);
    } 
}
