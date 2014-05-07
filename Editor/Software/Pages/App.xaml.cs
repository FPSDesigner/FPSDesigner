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
        private ModernWindow LoginPage, RegisterPage, SelectProjectPage;

        private Dictionary<string, ModernWindow> listExtWindows;

        private bool IsLogged = false;

        public App()
        {
            // Set the current user interface culture to the specific culture

            LocalizeDictionary.Instance.Culture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");

            GlobalVars.AddConsoleMsg(GlobalVars.GetUIString("Logs_Initializing_Editor"), "info");
            GlobalVars.LaunchNewWindow += GlobalVars_LaunchNewWindow;

            Startup += App_Startup;

            listExtWindows = new Dictionary<string, ModernWindow>();
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            // Load 'open-with' projects
            if (e.Args.Length > 0)
            {
                if (System.IO.File.Exists(e.Args[0]) && GlobalVars.extensionsProjectFile.Contains(System.IO.Path.GetExtension(e.Args[0])))
                {
                    // Check if the file is not locked.
                    try
                    {
                        using (System.IO.Stream stream = new System.IO.FileStream(e.Args[0], System.IO.FileMode.Open))
                        {
                            // Load the .fpsd
                            GlobalVars.projectData = Codes.CXMLManager.deserializeClass<Codes.ProjectData>(e.Args[0]);
                            GlobalVars.projectFile = e.Args[0];

                            // Load the gameinfo
                            GlobalVars.projectGameInfoFile = System.IO.Path.GetDirectoryName(e.Args[0]) + "\\" + GlobalVars.defaultProjectGameName;
                            GlobalVars.gameInfo = Codes.CXMLManager.deserializeClass<Engine.Game.LevelInfo.LevelData>(GlobalVars.projectGameInfoFile);
                        }
                    }
                    catch
                    {
                        GlobalVars.projectFile = "";
                        GlobalVars.projectGameInfoFile = "";
                    }
                }
            }

            /*IsLogged = true;
            Software.MainWindow.LoadMainWindow();
            SelectProject();
            //Software.MainWindow.Instance.Show();
            return;*/

            Software.MainWindow.LoadMainWindow();

            ShowLoginPage();
        }

        private void ShowLoginPage()
        {
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
                Icon = GlobalVars.SoftwareIcon
            };

            ((Pages.Login)LoginPage.Content).LoginSucceed += App_LoginSucceed;
            ((Pages.Login)LoginPage.Content).NeedRegister += App_NeedRegister;
            LoginPage.Closing += ModernWindow_Closed;

            LoginPage.Show();
        }

        void App_NeedRegister(object sender, RoutedEventArgs e)
        {
            RegisterPage = new ModernWindow
            {
                Style = (Style)App.Current.Resources["EmptyWindow"],
                Content = new Pages.Register
                {
                    Margin = new Thickness(32)
                },
                ResizeMode = System.Windows.ResizeMode.NoResize,
                MaxWidth = 650,
                Title = "FPSDesigner - Register",
                MaxHeight = 430,
                Icon = GlobalVars.SoftwareIcon
            };
            RegisterPage.Closing += ModernWindow_Closed;
            ((Pages.Register)RegisterPage.Content).RegisterSucceed += App_RegisterSucceed;
            RegisterPage.Show();

            LoginPage.Close();
        }

        void ModernWindow_Closed(object sender, EventArgs e)
        {
            ModernWindow windowClosed = (ModernWindow)sender;

            if (!IsLogged && windowClosed.Content is Pages.Login && (RegisterPage == null || !RegisterPage.IsActive))
                Application.Current.Shutdown();

            else if (!IsLogged && windowClosed.Content is Pages.Register && !LoginPage.IsActive)
            {
                // We reshow the login page
                ShowLoginPage();
            }
        }

        void App_LoginSucceed(object sender, RoutedEventArgs e)
        {
            IsLogged = true;
            SelectProject();
            LoginPage.Close();
        }

        void App_RegisterSucceed(object sender, RoutedEventArgs e)
        {
            IsLogged = true;
            SelectProject();
            RegisterPage.Close();
        }

        void SelectProject()
        {
            if (GlobalVars.projectFile == "")
            {
                SelectProjectPage = new ModernWindow
                {
                    Style = (Style)App.Current.Resources["EmptyWindow"],
                    Content = new Pages.SelectProject
                    {
                        Margin = new Thickness(32)
                    },
                    ResizeMode = System.Windows.ResizeMode.NoResize,
                    MaxWidth = 650,
                    Title = "FPSDesigner - Select a project",
                    MaxHeight = 200,
                    Icon = GlobalVars.SoftwareIcon
                };
                SelectProjectPage.Closing += (s, e) => { if (GlobalVars.projectFile == "") Application.Current.Shutdown(); };

                ((Pages.SelectProject)SelectProjectPage.Content).ProjectSelected += App_ProjectSelected;

                SelectProjectPage.Show();
            }
            else
                Software.MainWindow.Instance.Show();
        }

        void App_ProjectSelected(object sender, RoutedEventArgs e)
        {
            GlobalVars.projectFile = (string)sender;
            SelectProjectPage.Close();
            Software.MainWindow.Instance.Show();
        }

        void GlobalVars_LaunchNewWindow(object sender, RoutedEventArgs e)
        {
            string windowName = (string)sender;
            if (windowName == "Console")
            {
                if (!listExtWindows.ContainsKey(windowName))
                {
                    listExtWindows[windowName] = new ModernWindow
                    {
                        Style = (Style)App.Current.Resources["EmptyWindow"],
                        Content = new Pages.ConsoleLogs
                        {
                            Margin = new Thickness(32)
                        },
                        Title = "Console",
                        Width = 800,
                        Height = 400,
                        Icon = GlobalVars.SoftwareIcon
                    };

                    listExtWindows[windowName].Show();
                    listExtWindows[windowName].Closed += (send, args) => listExtWindows.Remove(windowName);
                    GlobalVars.NewConsoleMessage += (send, args) =>
                    {
                        string[] values = (string[])send;
                        ((Pages.ConsoleLogs)listExtWindows[windowName].Content).AddConsoleLog(values[0], values[1]);
                    };
                }
                else
                    listExtWindows[windowName].Activate();
            }
            else if (windowName == "TreeManager")
            {
                if (!listExtWindows.ContainsKey(windowName))
                {
                    listExtWindows[windowName] = new ModernWindow
                    {
                        Style = (Style)App.Current.Resources["EmptyWindow"],
                        Content = new Pages.TreeManager
                        {
                            Margin = new Thickness(32)
                        },
                        Title = "Tree Manager",
                        Width = 800,
                        Height = 600,
                        Icon = GlobalVars.SoftwareIcon
                    };

                    listExtWindows[windowName].Show();
                    listExtWindows[windowName].Closed += (send, args) => listExtWindows.Remove(windowName);
                    ((Pages.TreeManager)listExtWindows[windowName].Content).ShouldClose += App_ShouldClose;
                }
                else
                    listExtWindows[windowName].Activate();
            }
            else if (windowName == "TerrainManager")
            {
                if (!listExtWindows.ContainsKey(windowName))
                {
                    listExtWindows[windowName] = new ModernWindow
                    {
                        Style = (Style)App.Current.Resources["EmptyWindow"],
                        Content = new Pages.TerrainManager.TerrainManager
                        {
                            Margin = new Thickness(32)
                        },
                        Title = "Terrain Manager",
                        Width = 750,
                        Height = 500,
                        MinWidth = 750,
                        MinHeight = 500,
                        Icon = GlobalVars.SoftwareIcon
                    };

                    listExtWindows[windowName].Show();
                    listExtWindows[windowName].Closed += (send, args) => listExtWindows.Remove(windowName);
                    ((Pages.TerrainManager.TerrainManager)listExtWindows[windowName].Content).ShouldClose += App_ShouldClose;
                }
                else
                    listExtWindows[windowName].Activate();
            }
            else if (windowName == "ModelManager")
            {
                if (!listExtWindows.ContainsKey(windowName))
                {
                    listExtWindows[windowName] = new ModernWindow
                    {
                        Style = (Style)App.Current.Resources["EmptyWindow"],
                        Content = new Pages.ModelManager
                        {
                            Margin = new Thickness(32)
                        },
                        Title = "Model Manager",
                        Width = 750,
                        Height = 560,
                        MinWidth = 750,
                        MinHeight = 560,
                        Icon = GlobalVars.SoftwareIcon
                    };

                    listExtWindows[windowName].Show();
                    listExtWindows[windowName].Closed += (send, args) => listExtWindows.Remove(windowName);
                    ((Pages.ModelManager)listExtWindows[windowName].Content).ShouldClose += App_ShouldClose;
                }
                else
                    listExtWindows[windowName].Activate();
            }
        }

        void App_ShouldClose(object sender, RoutedEventArgs e)
        {
            listExtWindows[(string)sender].Close();
        }

    }
}
