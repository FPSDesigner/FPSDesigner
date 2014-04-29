using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Media.Imaging;

using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using FirstFloor.ModernUI.Presentation;

using WPFLocalizeExtension.Extensions;

namespace Software
{
    static class GlobalVars
    {
        public static List<string[]> LogList = new List<string[]>();
        public static string selectedTool = "Select";
        public static event RoutedEventHandler LaunchNewWindow;
        public static string[] extensionsProjectFile = new string[] { ".fpsd", ".fspdesigner" };
        public static string projectFile = "";
        public static BitmapFrame SoftwareIcon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/Icon.ico", UriKind.RelativeOrAbsolute));
        public static Codes.ProjectData projectData;
        public static string defaultProjectInfoName = "projectInfo.fpsd";

        public static string GetUIString(string key)
        {
            string uiString;
            LocExtension locExtension = new LocExtension("Software:Strings:"+key);
            locExtension.ResolveLocalizedValue(out uiString);
            return uiString;
        }

        public static void AddConsoleMsg(string msg, string icon)
        {
            LogList.Add(new string[] { msg, icon });
        }


        #region "Windows Menu Helper"
        public static void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            MainWindow MainWindowInstance = MainWindow.Instance;

            //Console.WriteLine("Open Window: " + e.Fragment);

            foreach (Link elt in MainWindowInstance.HomeGroupAction.Links)
            {
                string[] splittedFrag = elt.Source.OriginalString.Split('#');
                if (splittedFrag.Length > 1 && splittedFrag[1] == e.Fragment)
                {
                    MainWindowInstance.ContentSource = new Uri("/Pages/Home.xaml", UriKind.Relative);
                    LaunchNewWindow(e.Fragment, null);
                }
            }
        }

        public static void OnNavigatedTo(NavigationEventArgs e)
        {
            //MainWindow MainWindowInstance = MainWindow.Instance;
            //foreach (Link lc in MainWindowInstance.MenuActions.Links)
            //    lc.Source = new Uri(e.Source.OriginalString + "#" + lc.DisplayName, UriKind.RelativeOrAbsolute);
        }

        public static void OnNavigatedFrom(NavigationEventArgs e)
        {
            //MainWindow MainWindowInstance = MainWindow.Instance;
            //foreach (Link lc in MainWindowInstance.MenuActions.Links)
            //    lc.Source = new Uri(e.Source.OriginalString + "#" + lc.DisplayName, UriKind.RelativeOrAbsolute);
        }

        public static void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //MainWindow MainWindowInstance = MainWindow.Instance;
            //foreach (Link lc in MainWindowInstance.MenuActions.Links)
            //lc.Source = new Uri(e.Source.OriginalString + "#" + lc.DisplayName, UriKind.RelativeOrAbsolute);
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
