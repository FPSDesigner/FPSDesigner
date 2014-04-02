using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPFLocalizeExtension.Extensions;
using WPFLocalizeExtension.Engine;
using FirstFloor.ModernUI.Windows.Controls;

namespace Software
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ModernWindow LoginPage;

        private bool IsLogged = false;

        public App()
        {
            // Set the current user interface culture to the specific culture

            LocalizeDictionary.Instance.Culture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
            
            GlobalVars.AddConsoleMsg(GlobalVars.GetUIString("Logs_Initializing_Editor"));

            Startup += App_Startup;
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            Software.MainWindow.LoadMainWindow();

            LoginPage = new ModernWindow
            {
                Style = (Style)App.Current.Resources["EmptyWindow"],
                Content = new Pages.Login
                {
                    Margin = new Thickness(32)
                },
                ResizeMode = System.Windows.ResizeMode.NoResize,
                MaxWidth = 850,
                Title = "FPSDesigner - Login",
                MaxHeight = 320,
            };

            ((Pages.Login)LoginPage.Content).LoginSucceed += App_LoginSucceed;
            LoginPage.Closing += LoginPage_Closed;

            LoginPage.Show();
        }

        void LoginPage_Closed(object sender, EventArgs e)
        {
            if (!IsLogged)
                Application.Current.Shutdown();
        }

        void App_LoginSucceed(object sender, RoutedEventArgs e)
        {
            IsLogged = true;
            Software.MainWindow.Instance.Show();
            LoginPage.Close();
        }
    }
}
