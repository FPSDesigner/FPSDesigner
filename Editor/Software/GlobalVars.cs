using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Automation.Peers;

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
        public static event RoutedEventHandler LaunchNewWindow;
        

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
            MainWindow MainWindowInstance = MainWindow.Instance;
            if (e.Fragment == MainWindowInstance.MenuActions.Links[0].DisplayName)
                return;

            Console.WriteLine("FN");
            Console.WriteLine("Open Window: " + e.Fragment);

            /*NavigationCommands.GoToPage.Execute("/Pages/Home.xaml", null);

            BBCodeBlock bs = new BBCodeBlock();

            bs.LinkNavigator.Navigate(new Uri("/Pages/Home.xaml", UriKind.Relative), (FrameworkElement)this);
    

            List<Link> l = new List<Link>();
            l.AddRange(MainWindowInstance.MenuActions.Links);
            MainWindowInstance.MenuActions.Links.Clear();
            foreach (Link elt in l)
                MainWindowInstance.MenuActions.Links.Add(elt);

            //LaunchNewWindow(e.Fragment, null);*/
        }

        public static void OnNavigatedTo(NavigationEventArgs e)
        {
            MainWindow MainWindowInstance = MainWindow.Instance;
            foreach (Link lc in MainWindowInstance.MenuActions.Links)
                lc.Source = new Uri(e.Source.OriginalString + "#" + lc.DisplayName, UriKind.RelativeOrAbsolute);

            Console.WriteLine("TO");
        }

        public static void OnNavigatedFrom(NavigationEventArgs e)
        {
            MainWindow MainWindowInstance = MainWindow.Instance;
            foreach (Link lc in MainWindowInstance.MenuActions.Links)
                lc.Source = new Uri(e.Source.OriginalString + "#" + lc.DisplayName, UriKind.RelativeOrAbsolute);
        }

        public static void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            MainWindow MainWindowInstance = MainWindow.Instance;
            foreach (Link lc in MainWindowInstance.MenuActions.Links)
            lc.Source = new Uri(e.Source.OriginalString + "#" + lc.DisplayName, UriKind.RelativeOrAbsolute);

            Console.WriteLine("FROM: " + e.Source);
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
