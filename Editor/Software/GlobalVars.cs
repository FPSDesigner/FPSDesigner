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
        public static event RoutedEventHandler LaunchNewWindow;
        public static string[] extensionsProjectFile = new string[] { ".fpsd", ".fspdesigner" };

        public static string selectedTool = "Select";
        public static string projectFile = "";
        public static string projectGameInfoFile = "";

        public static BitmapFrame SoftwareIcon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/Icon.ico", UriKind.RelativeOrAbsolute));
        public static Codes.ProjectData projectData;
        public static Engine.Game.LevelInfo.LevelData gameInfo;

        public static string defaultProjectInfoName = "projectInfo.fpsd";
        public static string defaultProjectGameName = "GameLevel.xml";

        public static string GetUIString(string key)
        {
            string uiString;
            LocExtension locExtension = new LocExtension("Software:Strings:" + key);
            locExtension.ResolveLocalizedValue(out uiString);
            return uiString;
        }

        public static void AddConsoleMsg(string msg, string icon)
        {
            LogList.Add(new string[] { msg, icon });
        }

        public static void SaveGameLevel()
        {
            Codes.CXMLManager.serializeClass(projectGameInfoFile, gameInfo);
        }


        #region Windows Menu Helper
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

        #region Default GameData Info
        public static void CreateDefaultGameLevel()
        {
            gameInfo = new Engine.Game.LevelInfo.LevelData
            {
                SpawnInfo =
                {
                    NearClip = 0.05f,
                    FarClip = 10000f,
                    SprintSpeed = 18,
                    WalkSpeed = 9,
                    AimSpeed = 3,
                    SpawnPosition = new Engine.Game.LevelInfo.Coordinates
                    {
                        X = 0,
                        Y = 10,
                        Z = 0,
                    },
                    SpawnRotation = new Engine.Game.LevelInfo.Coordinates
                    {
                        X = 0,
                        Y = 0,
                        Z = 0,
                    }
                },
                Terrain =
                {
                    UseTerrain = false,
                    CellSize = 10,
                    Height = 300,
                    TextureTiling = 1000,
                    TerrainTextures = new Engine.Game.LevelInfo.TerrainTextures
                    {
                        HeightmapFile = "",
                        TextureFile = "",
                        RTexture = "",
                        GTexture = "",
                        BTexture = "",
                        BaseTexture = "",
                    }
                },
                Water =
                {
                    UseWater = false,
                    SizeX = 1000,
                    SizeY = 1000,
                    Alpha = 0,
                    Coordinates = new Engine.Game.LevelInfo.Coordinates
                    {
                        X = 0,
                        Y = 0,
                        Z = 0,
                    }
                },
                MapModels = { },
                Weapons = new Engine.Game.LevelInfo.Weapons
                {
                    Weapon = new List<Engine.Game.LevelInfo.Weapon>
                    {
                        new Engine.Game.LevelInfo.Weapon
                        {
                            Model = "Models//Machete",
                            Texture = "Textures//Uv_Machete",
                            Type = 2,
                            MaxClip = 1,
                            IsAutomatic = false,
                            ShotsPerSecs = 2,
                            Range = 1,
                            Rotation = new Engine.Game.LevelInfo.Coordinates
                            {
                                X = 1.645987f,
                                Y = 0,
                                Z = 2.942064f,
                            },
                            Offset = new Engine.Game.LevelInfo.Coordinates
                            {
                                X = 0.21f,
                                Y = 0.15f,
                                Z = 0.04f,
                            },
                            Scale = 1,
                            Delay = 0,
                            Name = "Machete",
                            WeaponSound = new Engine.Game.LevelInfo.WeaponSound
                            {
                                Shot = "Sounds\\Weapons\\MACHET_ATTACK",
                                DryShot = "",
                                Reload = "",
                            },
                            WeaponAnim = new Engine.Game.LevelInfo.WeaponAnim
                            {
                                Walk = "Machete_Walk",
                                Attack = "Machete_Attack",
                                Idle = "Machete_Wait",
                                Reload = "",
                                Switch = "Machete_Switch",
                                Aim = "",

                                WalkSpeed = 1.4f,
                                AttackSpeed = 3,
                                IdleSpeed = 0.7f,
                                ReloadSpeed = 0,
                                SwitchSpeed = 3.8f,
                                AimSpeed = 1,
                            }
                        },
                        new Engine.Game.LevelInfo.Weapon
                        {
                            Model = "Models//M1911",
                            Texture = "Textures//M1911",
                            Type = 0,
                            MaxClip = 10,
                            IsAutomatic = false,
                            ShotsPerSecs = 2,
                            Range = 1,
                            Rotation = new Engine.Game.LevelInfo.Coordinates
                            {
                                X = -0.121999f,
                                Y = 0,
                                Z = 1.21203f,
                            },
                            Offset = new Engine.Game.LevelInfo.Coordinates
                            {
                                X = 0.042f,
                                Y = 0.04f,
                                Z = 0.22f,
                            },
                            Scale = 1,
                            Delay = 100,
                            Name = "M1911",
                            WeaponSound = new Engine.Game.LevelInfo.WeaponSound
                            {
                                Shot = "Sounds\\Weapons\\M1911_SHOT",
                                DryShot = "Sounds\\Weapons\\DryFireSound",
                                Reload = "Sounds\\Weapons\\M1911_RELOAD",
                            },
                            WeaponAnim = new Engine.Game.LevelInfo.WeaponAnim
                            {
                                Walk = "M1911_Walk",
                                Attack = "M1911_Attack",
                                Idle = "M1911_Wait",
                                Reload = "M1911_Reloading",
                                Switch = "M1911_Switch",
                                Aim = "M1911_Aim",
                                AimShot = "M1911_AimShot",

                                WalkSpeed = 1.4f,
                                AttackSpeed = 3,
                                IdleSpeed = 0.7f,
                                ReloadSpeed = 0,
                                SwitchSpeed = 3.8f,
                                AimSpeed = 1,
                                AimShotSpeed = 16.2f
                            }
                        },
                        new Engine.Game.LevelInfo.Weapon
                        {
                            Model = "Models//Ak",
                            Texture = "Textures//Uv_Ak",
                            Type = 0,
                            MaxClip = 46,
                            IsAutomatic = true,
                            ShotsPerSecs = 10,
                            Range = 1,
                            Rotation = new Engine.Game.LevelInfo.Coordinates
                            {
                                X = -0.11f,
                                Y = -0.12f,
                                Z = 0.67f,
                            },
                            Offset = new Engine.Game.LevelInfo.Coordinates
                            {
                                X = 0.44f,
                                Y = 1.3f,
                                Z = 0.08f,
                            },
                            Scale = 1,
                            Delay = 425,
                            Name = "AK47",
                            WeaponSound = new Engine.Game.LevelInfo.WeaponSound
                            {
                                Shot = "Sounds\\Weapons\\AK47_SHOT",
                                DryShot = "Sounds\\Weapons\\DryFireSound",
                                Reload = "Sounds\\Weapons\\M1911_RELOAD",
                            },
                            WeaponAnim = new Engine.Game.LevelInfo.WeaponAnim
                            {
                                Walk = "Ak_Walk",
                                Attack = "Ak_Attack",
                                Idle = "Ak_Wait",
                                Reload = "Ak_Reload",
                                Switch = "Ak_Switch",
                                Aim = "Ak_Aim",
                                AimShot = "Ak_AimShot",

                                WalkSpeed = 2.0f,
                                AttackSpeed = 17,
                                IdleSpeed = 1,
                                ReloadSpeed = 1.8f,
                                SwitchSpeed = 3.8f,
                                AimSpeed = 1,
                                AimShotSpeed = 18
                            }
                        },
                        new Engine.Game.LevelInfo.Weapon
                        {
                            Model = "Models//Deagle",
                            Texture = "Textures//Deagle",
                            Type = 0,
                            MaxClip = 46,
                            IsAutomatic = false,
                            ShotsPerSecs = 1,
                            Range = 1,
                            Rotation = new Engine.Game.LevelInfo.Coordinates
                            {
                                X = 1.525f,
                                Y = 0,
                                Z = 1.33f,
                            },
                            Offset = new Engine.Game.LevelInfo.Coordinates
                            {
                                X = 0.01f,
                                Y = 0.35f,
                                Z = 0,
                            },
                            Scale = 1,
                            Delay = 0,
                            Name = "Desert Eagle",
                            WeaponSound = new Engine.Game.LevelInfo.WeaponSound
                            {
                                Shot = "Sounds\\Weapons\\M1911_SHOT",
                                DryShot = "Sounds\\Weapons\\DryFireSound",
                                Reload = "Sounds\\Weapons\\M1911_RELOAD",
                            },
                            WeaponAnim = new Engine.Game.LevelInfo.WeaponAnim
                            {
                                Walk = "Deagle_Walk",
                                Attack = "Deagle_Attack",
                                Idle = "Deagle_Wait",
                                Reload = "Deagle_Reload",
                                Switch = "Deagle_Switch",
                                Aim = "Deagle_Aim",
                                AimShot = "Deagle_AimShot",

                                WalkSpeed = 2.0f,
                                AttackSpeed = 8,
                                IdleSpeed = 1,
                                ReloadSpeed = 1.7f,
                                SwitchSpeed = 3.8f,
                                AimSpeed = 1,
                                AimShotSpeed = 7
                            }
                        },
                        new Engine.Game.LevelInfo.Weapon
                        {
                            Model = "Models//M40A5",
                            Texture = "Textures//M40A5",
                            Type = 0,
                            MaxClip = 46,
                            IsAutomatic = false,
                            ShotsPerSecs = 1,
                            Range = 1,
                            Rotation = new Engine.Game.LevelInfo.Coordinates
                            {
                                X = 1.42f,
                                Y = 0.15f,
                                Z = 2.35f,
                            },
                            Offset = new Engine.Game.LevelInfo.Coordinates
                            {
                                X = -1,
                                Y = 0.35f,
                                Z = 0.02f,
                            },
                            Scale = 1,
                            Delay = 0,
                            Name = "M40A5",
                            WeaponSound = new Engine.Game.LevelInfo.WeaponSound
                            {
                                Shot = "Sounds\\Weapons\\M1911_SHOT",
                                DryShot = "Sounds\\Weapons\\DryFireSound",
                                Reload = "Sounds\\Weapons\\M1911_RELOAD",
                            },
                            WeaponAnim = new Engine.Game.LevelInfo.WeaponAnim
                            {
                                Walk = "M40A5_Walk",
                                Attack = "M40A5_Attack",
                                Idle = "M40A5_Wait",
                                Reload = "M40A5_Reload",
                                Switch = "M40A5_Switch",
                                Aim = "M40A5_Aim",
                                AimShot = "None",

                                WalkSpeed = 1.8f,
                                AttackSpeed = 8,
                                IdleSpeed = 0.5f,
                                ReloadSpeed = 1.7f,
                                SwitchSpeed = 3.8f,
                                AimSpeed = 2.9f,
                                AimShotSpeed = 7
                            }
                        }
                    },
                }
            };
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
