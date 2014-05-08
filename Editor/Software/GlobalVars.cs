using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
        #region Variables
        public static event RoutedEventHandler LaunchNewWindow;
        public static event RoutedEventHandler NewConsoleMessage;
        public static event RoutedEventHandler ReloadGameComponentsTreeView;
        public static event RoutedEventHandler SoftwareShouldForceClose;

        public static string[] extensionsProjectFile = new string[] { ".fpsd", ".fspdesigner" };
        public static List<string[]> LogList = new List<string[]>();

        public static string messageToDisplayInDialog = "";

        public static ModernButton selectedToolButton;
        public static string projectFile = "";
        public static string projectGameInfoFile = "";

        public static SelectedElement selectedElt;
        public static BitmapFrame SoftwareIcon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/Icon.ico", UriKind.RelativeOrAbsolute));
        public static Codes.ProjectData projectData;
        public static Engine.Game.LevelInfo.LevelData gameInfo;

        public static string defaultProjectInfoName = "projectInfo.fpsd";
        public static string defaultProjectGameName = "GameLevel.xml";
        public static string defaultGameName = "Editor.exe";
        public static string contentRootFolder = "Content/";
        public static string rootProjectFolder = "/Docs/";
        public static string[] requiredResourceFiles = new string[]
        {
            "Textures/uw_effect", 
            "2D/consoleFont",
            "2D/ErrorSmiley",
            "Sounds/GRASSSTEP",
            "Textures/LensFlare/glow",
            "Textures/LensFlare/flare1",
            "Textures/LensFlare/flare2",
            "Textures/LensFlare/flare3",
            "Textures/Clouds",
            "Textures/water_normal",
            "3D/Skysphere",
            "Effects/Skysphere",
            "Effects/Terrain",
            "Effects/PPLight",
            "Effects/NormalMapping",
            "Effects/WaterEffect",
            "3D/plane",
            "Sounds/Weapons/CHANGEWEAPON1",
            "Sounds/Weapons/CHANGEWEAPON2",
            "Sounds/Weapons/PICKUPWEAPON",
            "Particles/ParticleEffect",
            "Particles/smoke",
            "Trees/Textures/GrayBark",
            "Trees/Textures/BirchLeaf",
            "Trees/Textures/BirchBark",
            "Trees/LTreeShaders/Trunk",
            "Trees/LTreeShaders/Leaves",
            "Trees/Textures/BirchBark",
            "Trees/Textures/WillowLeaf",
            "Trees/Textures/PineBark",
            "Trees/Textures/PineLeaf",
            "Textures/Guizmo",
            "Models/Guizmo",
            "Textures/RotationGuizmo",
            "Models/RotationGuizmo",
            "Textures/ScalingGuizmo",
            "Models/ScalingGuizmo",
            "Models/Arm_Animation(Smoothed)",
            "Models/Plane",
            "Textures/PistolFlash001",
            "Textures/Lens",
            "Textures/StormTrooper",
            "Models/StormTrooperAnimation",
        };

        public static Engine.MainGameEngine embeddedGame;
        #endregion

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
            if (NewConsoleMessage != null)
                NewConsoleMessage(new string[] { msg, icon }, null);
        }

        public static void RaiseEvent(string eventName)
        {
            if (eventName == "ReloadGameComponentsTreeView")
                ReloadGameComponentsTreeView(null, null);
            else if (eventName == "SoftwareShouldForceClose")
                SoftwareShouldForceClose(null, null);
        }

        public static void SaveGameLevel()
        {
            try
            {
                Codes.CXMLManager.serializeClass(projectGameInfoFile, gameInfo);
                File.Copy(projectGameInfoFile, defaultProjectGameName, true);
            }
            catch (Exception e)
            {
                AddConsoleMsg("Couldn't save game level informations. Data: " + e.Message, "error");
            }
        }

        public static void InitializeProject()
        {
            if (File.Exists("./Editor.exe"))
                File.Copy("Editor.exe", GlobalVars.rootProjectFolder + GlobalVars.projectData.Properties.ExeName.Replace(".exe", "") + ".exe", true);

            if (!Directory.Exists(GlobalVars.rootProjectFolder + GlobalVars.rootProjectFolder))
                Directory.CreateDirectory(GlobalVars.rootProjectFolder + GlobalVars.contentRootFolder);

            CheckContentFilesExists();
        }

        public static void CheckContentFilesExists()
        {
            List<string> filesToCheck = new List<string>();

            foreach (string reqFile in requiredResourceFiles)
                filesToCheck.Add(reqFile);

            filesToCheck.Add(gameInfo.SpawnInfo.HandTexture);

            if (gameInfo.Terrain != null && gameInfo.Terrain.UseTerrain)
            {
                filesToCheck.Add(gameInfo.Terrain.TerrainTextures.BaseTexture);
                filesToCheck.Add(gameInfo.Terrain.TerrainTextures.BTexture);
                filesToCheck.Add(gameInfo.Terrain.TerrainTextures.GTexture);
                filesToCheck.Add(gameInfo.Terrain.TerrainTextures.RTexture);
                filesToCheck.Add(gameInfo.Terrain.TerrainTextures.HeightmapFile);
                filesToCheck.Add(gameInfo.Terrain.TerrainTextures.TextureFile);
            }

            if (gameInfo.MapModels != null)
            {
                if (gameInfo.MapModels.Models != null)
                {
                    foreach (Engine.Game.LevelInfo.MapModels_Model mdl in gameInfo.MapModels.Models)
                    {
                        filesToCheck.Add(mdl.ModelFile);
                        if (mdl.Textures != null && mdl.Textures.Texture != null)
                            foreach (Engine.Game.LevelInfo.MapModels_Texture texture in mdl.Textures.Texture)
                                filesToCheck.Add(texture.Texture);

                        if (mdl.BumpTextures != null && mdl.BumpTextures.Texture != null)
                            foreach (Engine.Game.LevelInfo.MapModels_Texture texture in mdl.BumpTextures.Texture)
                                filesToCheck.Add(texture.Texture);
                    }
                }
                if (gameInfo.MapModels.Trees != null)
                {
                    foreach (Engine.Game.LevelInfo.MapModels_Tree tree in gameInfo.MapModels.Trees)
                        filesToCheck.Add(tree.Profile);
                }
            }

            if (gameInfo.Weapons != null && gameInfo.Weapons.Weapon != null)
            {
                foreach (Engine.Game.LevelInfo.Weapon wep in gameInfo.Weapons.Weapon)
                {
                    filesToCheck.Add(wep.Model);
                    filesToCheck.Add(wep.Texture);
                    filesToCheck.Add(wep.WeaponSound.DryShot);
                    filesToCheck.Add(wep.WeaponSound.Reload);
                    filesToCheck.Add(wep.WeaponSound.Shot);
                }
            }

            // Format the list
            filesToCheck = filesToCheck.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList(); // Remove duplicats/empties/whitespaces
            filesToCheck[0] = "/" + filesToCheck[0];
            filesToCheck = filesToCheck.Select(f => f.TrimStart('/', '\\')).ToList(); // Remove first slashes

            List<string> errorFiles = new List<string>();

            string[] resourcesExt = new string[] { ".xnb", ".png", ".jpg", ".jpeg", ".gif", ".tga", ".raw", ".bmp", ".fbx", ".spritefont", ".fx", ".wav", ".ogg", ".mp3", ".aac", ".wma", ".dds", ".x" };
            foreach (string file in filesToCheck)
            {
                DirectoryInfo root = new DirectoryInfo(GlobalVars.rootProjectFolder + GlobalVars.contentRootFolder + Path.GetDirectoryName(file));
                if (!root.Exists || root.GetFiles(Path.GetFileName(file) + ".*").Length == 0)
                {
                    if (!File.Exists(GlobalVars.rootProjectFolder + GlobalVars.contentRootFolder + "/" + file))
                    {
                        bool found = false;
                        foreach (string ext in resourcesExt)
                        {
                            if (File.Exists("./Content/" + file + ext))
                            {
                                string directory = Path.GetDirectoryName(GlobalVars.rootProjectFolder + GlobalVars.contentRootFolder + "/" + file);
                                if (!Directory.Exists(directory))
                                    Directory.CreateDirectory(directory);

                                File.Copy("./Content/" + file + ext, GlobalVars.rootProjectFolder + GlobalVars.contentRootFolder + "/" + file + ext);
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            errorFiles.Add(file);
                    }
                }
            }

            if (errorFiles.Count > 0)
            {
                string totalErrorFiles = "";
                foreach (string err in errorFiles)
                    totalErrorFiles += err + "\n";

                messageToDisplayInDialog = "Multiple game files couldn't be loaded from your project content folder:\n\n" + totalErrorFiles;
            }

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
                SpawnInfo = new Engine.Game.LevelInfo.SpawnInfo
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
                    },
                    HandTexture = "Textures\\Uv_Hand",
                },
                Terrain = new Engine.Game.LevelInfo.Terrain
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
                Water = new Engine.Game.LevelInfo.Water
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
                MapModels = new Engine.Game.LevelInfo.MapModels { },
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
                            RecoilIntensity = 0,
                            RecoilBackIntensity = 0,
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
                            RecoilIntensity = 0.07f,
                            RecoilBackIntensity = 0.85f,
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
                            RecoilIntensity = 0.13f,
                            RecoilBackIntensity = 0.83f,
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
                            RecoilIntensity = 0.19f,
                            RecoilBackIntensity = 0.88f,
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
                            RecoilIntensity = 0.24f,
                            RecoilBackIntensity = 0.90f,
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

        #region Selected Element Class
        public class SelectedElement
        {
            public string eltType;
            public int eltId;

            public SelectedElement(string elt, int parent = -1)
            {
                eltType = elt;
                eltId = parent;
            }
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
