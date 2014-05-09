using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using FirstFloor.ModernUI.Presentation;

namespace WinServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ModernWindow PropertiesPage;

        private bool PropertiesSet = false;

        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Try to deserialize server settings
            try
            {
                GlobalVars.serverInfo = Codes.CXMLManager.deserializeClass<Codes.ServerInfo>(GlobalVars.SettingsFile);
            }
            catch
            {
                GlobalVars.serverInfo = GlobalVars.defaultServerInfo();
            }

            PropertiesPage = new ModernWindow
            {
                Style = (Style)App.Current.Resources["EmptyWindow"],
                Content = new Pages.Properties
                {
                    Margin = new Thickness(32)
                },
                ResizeMode = System.Windows.ResizeMode.NoResize,
                MaxWidth = 500,
                Title = "Server Properties",
                MaxHeight = 320,
                Icon = GlobalVars.SoftwareIcon
            };

            ((Pages.Properties)PropertiesPage.Content).PropertiesSet += App_PropertiesSet;
            PropertiesPage.Closing += ModernWindow_Closed;

            PropertiesPage.Show();

            WinServer.MainWindow.LoadMainWindow();
        }

        void App_PropertiesSet(object sender, RoutedEventArgs e)
        {
            PropertiesSet = true;
            PropertiesPage.Close();
            WinServer.MainWindow.Instance.Show();
        }

        void ModernWindow_Closed(object sender, EventArgs e)
        {
            ModernWindow windowClosed = (ModernWindow)sender;

            if (windowClosed.Content is Pages.Properties && PropertiesSet == false)
                Application.Current.Shutdown();
        }
    }

}
