using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;

using Engine;

namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl, IContent
    {
        private MainGameEngine m_game;

        private DispatcherTimer resizeTimer = new DispatcherTimer();
        private MainWindow MainWindowInstance;

        private Point initialMoveMousePosGame1;

        private List<TreeViewItem> listTree_Trees, listTree_Models, listTree_Pickups;

        private bool isMovingGyzmoAxis = false;

        public Home()
        {
            InitializeComponent();
            MainWindowInstance = MainWindow.Instance;

            m_game = new Engine.MainGameEngine(true);
            GlobalVars.embeddedGame = m_game;

            listTree_Trees = new List<TreeViewItem>();
            listTree_Models = new List<TreeViewItem>();
            listTree_Pickups = new List<TreeViewItem>();

            ShowXNAImage1.Source = m_game.em_WriteableBitmap;

            GameButton1.SizeChanged += ShowXNAImage_SizeChanged;
            GameButton1.MouseWheel += GameButton1_MouseWheel;
            GameButton1.PreviewMouseMove += GameButton1_PreviewMouseMove;

            GameButton1.GotFocus += ShowXNAImage1_GotFocus;
            GameButton1.LostFocus += ShowXNAImage1_LostFocus;

            GameButton1.PreviewMouseLeftButtonDown += GameButton1_MouseDown;
            GameButton1.PreviewMouseLeftButtonUp += GameButton1_MouseUp;

            resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            resizeTimer.Tick += new EventHandler(disTimer_Tick);

            statusBarView1.Text = "Idle";

            GlobalVars.ReloadGameComponentsTreeView += (s, e) => { LoadGameComponentsToTreeview(); };

            GameComponentsList.SelectedItemChanged += GameComponentsList_SelectedItemChanged;

            GlobalVars.selectedToolButton = SelectButton;
            SelectButton.Foreground = new SolidColorBrush((Color)FindResource("AccentColor"));
            SelectButton.Click += ToolButton_Click;
            PositionButton.Click += ToolButton_Click;
            RotateButton.Click += ToolButton_Click;
            ScaleButton.Click += ToolButton_Click;

            PlayButton.Click += PreviewButton_Click;

            LoadGameComponentsToTreeview();
        }

        void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateGame();
        }

        private void GenerateGame()
        {
            m_game.shouldNotUpdate = true;
            m_game.em_dispatcherTimer.Stop();
            GlobalVars.gameInfo = (Engine.Game.LevelInfo.LevelData)m_game.WPFHandler("getLevelData", true);
            GlobalVars.SaveGameLevel();

            Process previewProcess = new Process();
            previewProcess.StartInfo.FileName = GlobalVars.defaultGameName;
            previewProcess.EnableRaisingEvents = true;

            previewProcess.Exited += (s, e) =>
            {
                Process proc = (Process)s;
                m_game.shouldNotUpdate = false;
                m_game.em_dispatcherTimer.Start();
                m_game.em_dispatcherTimer.Interval = TimeSpan.FromSeconds(1 / 60);
                m_game.em_dispatcherTimer.Tick += new EventHandler(m_game.GameLoop);
            };

            previewProcess.Start();
        }

        void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalVars.selectedToolButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 209, 209, 209));
            GlobalVars.selectedToolButton = ((ModernButton)sender);
            GlobalVars.selectedToolButton.Foreground = new SolidColorBrush((Color)FindResource("AccentColor"));
            m_game.WPFHandler("changeTool", new object[] { GlobalVars.selectedToolButton.Name });
            m_game.shouldUpdateOnce = true;

            GlobalVars.AddConsoleMsg("Selected tool " + GlobalVars.selectedToolButton.Name, "info");
        }

        void ShowXNAImage1_LostFocus(object sender, RoutedEventArgs e)
        {
            m_game.shouldNotUpdate = true;
            statusBarView1.Text = "Waiting...";
        }

        void ShowXNAImage1_GotFocus(object sender, RoutedEventArgs e)
        {
            m_game.shouldNotUpdate = false;
            statusBarView1.Text = "Idle";
        }

        void GameButton1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isMovingGyzmoAxis)
            {
                m_game.WPFHandler("moveObject", new object[] { "drag", Mouse.GetPosition(ShowXNAImage1) });
            }
        }

        void GameButton1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            float coef = 1;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                coef /= 4;

            m_game.WPFHandler("moveCameraForward", (float)e.Delta * coef);
        }

        private void GameButton1_MouseRightDown(object sender, MouseButtonEventArgs e)
        {
            m_game.shouldNotUpdate = false;
            statusBarView1.Text = "Moving...";
            m_game.WPFHandler("changeCamFreeze", false);

            initialMoveMousePosGame1 = PointToScreen(Mouse.GetPosition(null));
            Cursor = Cursors.ScrollAll;

            UIElement el = (UIElement)sender;
            el.CaptureMouse();
        }

        private void GameButton1_MouseRightUp(object sender, MouseButtonEventArgs e)
        {
            m_game.shouldNotUpdate = false;
            statusBarView1.Text = "Idle";

            m_game.WPFHandler("changeCamFreeze", true);

            Cursor = Cursors.Arrow;

            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();
        }

        void GameButton1_MouseUp(object sender, RoutedEventArgs e)
        {
            if (isMovingGyzmoAxis)
            {
                m_game.WPFHandler("moveObject", new object[] { "stop" });
                ApplyPropertiesWindow();
                isMovingGyzmoAxis = false;
            }
        }
        void GameButton1_MouseDown(object sender, RoutedEventArgs e)
        {
            object value = m_game.WPFHandler("click", Mouse.GetPosition(ShowXNAImage1));
            if (value is object[])
            {
                object[] selectElt = (object[])value;
                if (selectElt.Length == 2)
                {
                    if (GlobalVars.selectedToolButton.Name == "SelectButton")
                    {
                        if (GlobalVars.selectedElt != null)
                            m_game.WPFHandler("unselectObject", new object[] { GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });

                        if ((string)selectElt[0] == "tree" && selectElt[1] is int)
                        {
                            listTree_Trees[(int)selectElt[1]].IsSelected = true;
                            m_game.WPFHandler("selectObject", new object[] { "tree", (int)selectElt[1], GlobalVars.selectedToolButton.Name });
                            GlobalVars.selectedElt = new GlobalVars.SelectedElement("tree", (int)selectElt[1]);
                        }
                        else if ((string)selectElt[0] == "model" && selectElt[1] is int)
                        {
                            listTree_Models[(int)selectElt[1]].IsSelected = true;

                            m_game.WPFHandler("selectObject", new object[] { "model", (int)selectElt[1], GlobalVars.selectedToolButton.Name });
                            GlobalVars.selectedElt = new GlobalVars.SelectedElement("model", (int)selectElt[1]);
                        }
                        else if ((string)selectElt[0] == "pickup" && selectElt[1] is int)
                        {
                            listTree_Pickups[(int)selectElt[1]].IsSelected = true;

                            m_game.WPFHandler("selectObject", new object[] { "pickup", (int)selectElt[1], GlobalVars.selectedToolButton.Name });
                            GlobalVars.selectedElt = new GlobalVars.SelectedElement("pickup", (int)selectElt[1]);
                        }
                    }
                    else if (GlobalVars.selectedElt != null)
                    {
                        if ((string)selectElt[0] == "gizmo" && selectElt[1] is int)
                        {
                            m_game.WPFHandler("moveObject", new object[] { "start", (int)selectElt[1], GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId, Mouse.GetPosition(ShowXNAImage1) });

                            isMovingGyzmoAxis = true;
                        }
                    }
                }
            }
        }


        void disTimer_Tick(object sender, EventArgs e)
        {
            m_game.ChangeEmbeddedViewport((int)GameButton1.RenderSize.Width, (int)GameButton1.RenderSize.Height);
            ShowXNAImage1.Source = m_game.em_WriteableBitmap;
            resizeTimer.Stop();
            m_game.shouldUpdateOnce = true;
        }

        private void ShowXNAImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void LoadAvailableComponents()
        {
            AvailableComponentsList.Items.Clear();
        }
        
        private void LoadGameComponentsToTreeview()
        {
            GlobalVars.gameInfo = (Engine.Game.LevelInfo.LevelData)m_game.WPFHandler("getLevelData", true);
            GameComponentsList.Items.Clear();
            listTree_Trees.Clear();
            listTree_Models.Clear();
            listTree_Pickups.Clear();

            TreeViewItem Models = new TreeViewItem();
            TreeViewItem Trees = new TreeViewItem();
            TreeViewItem Water = new TreeViewItem();
            TreeViewItem Terrain = new TreeViewItem();
            TreeViewItem Pickups = new TreeViewItem();

            Models.Header = "Models";
            Trees.Header = "Trees";
            Water.Header = "Water";
            Terrain.Header = "Terrain";
            Pickups.Header = "Pick-Ups";

            // Trees
            if (GlobalVars.gameInfo.MapModels != null && GlobalVars.gameInfo.MapModels.Trees != null)
            {
                foreach (Engine.Game.LevelInfo.MapModels_Tree tree in GlobalVars.gameInfo.MapModels.Trees)
                {
                    TreeViewItem treeItem = new TreeViewItem();
                    treeItem.Header = System.IO.Path.GetFileName(tree.Profile);
                    if (treeItem.Header == null)
                        treeItem.Header = tree.Profile;

                    Trees.Items.Add(treeItem);
                    listTree_Trees.Add(treeItem);
                }
            }

            // Models
            if (GlobalVars.gameInfo.MapModels != null && GlobalVars.gameInfo.MapModels.Models != null)
            {
                foreach (Engine.Game.LevelInfo.MapModels_Model model in GlobalVars.gameInfo.MapModels.Models)
                {
                    TreeViewItem treeItem = new TreeViewItem();
                    treeItem.Header = System.IO.Path.GetFileName(model.ModelFile);
                    if (treeItem.Header == null)
                        treeItem.Header = model.ModelFile;

                    Models.Items.Add(treeItem);
                    listTree_Models.Add(treeItem);
                }
            }

            // Pick-Ups
            if (GlobalVars.gameInfo.MapModels != null && GlobalVars.gameInfo.MapModels.Pickups != null)
            {
                foreach (Engine.Game.LevelInfo.MapModels_Pickups pickup in GlobalVars.gameInfo.MapModels.Pickups)
                {
                    TreeViewItem treeItem = new TreeViewItem();
                    treeItem.Header = pickup.WeaponName;
                    if (treeItem.Header == null)
                        treeItem.Header = "Pick-up";

                    Pickups.Items.Add(treeItem);
                    listTree_Pickups.Add(treeItem);
                }
            }

            // Add items to main tree view
            if (Models.Items.Count > 0)
                GameComponentsList.Items.Add(Models);

            if (Trees.Items.Count > 0)
                GameComponentsList.Items.Add(Trees);

            if (Pickups.Items.Count > 0)
                GameComponentsList.Items.Add(Pickups);

            if (GlobalVars.gameInfo.Water != null && GlobalVars.gameInfo.Water.UseWater)
                GameComponentsList.Items.Add(Water);

            if (GlobalVars.gameInfo.Terrain != null && GlobalVars.gameInfo.Terrain.UseTerrain)
                GameComponentsList.Items.Add(Terrain);
        }

        void GameComponentsList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = (TreeViewItem)GameComponentsList.SelectedItem;

            // Models
            for (int i = 0; i < listTree_Models.Count; i++)
            {
                if (listTree_Models[i].IsSelected)
                {
                    if (GlobalVars.selectedElt != null)
                        m_game.WPFHandler("unselectObject", new object[] { GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });

                    m_game.WPFHandler("selectObject", new object[] { "model", i, GlobalVars.selectedToolButton.Name });
                    GlobalVars.selectedElt = new GlobalVars.SelectedElement("model", i);
                    ApplyPropertiesWindow();
                    m_game.shouldUpdateOnce = true;
                    return;
                }
            }

            // Trees
            for (int i = 0; i < listTree_Trees.Count; i++)
            {
                if (listTree_Trees[i].IsSelected)
                {
                    if (GlobalVars.selectedElt != null)
                        m_game.WPFHandler("unselectObject", new object[] { GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });

                    m_game.WPFHandler("selectObject", new object[] { "tree", i, GlobalVars.selectedToolButton.Name });
                    GlobalVars.selectedElt = new GlobalVars.SelectedElement("tree", i);
                    ApplyPropertiesWindow();
                    m_game.shouldUpdateOnce = true;
                    return;
                }
            }

            // Pickups
            for (int i = 0; i < listTree_Pickups.Count; i++)
            {
                if (listTree_Pickups[i].IsSelected)
                {
                    if (GlobalVars.selectedElt != null)
                        m_game.WPFHandler("unselectObject", new object[] { GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });

                    m_game.WPFHandler("selectObject", new object[] { "pickup", i, GlobalVars.selectedToolButton.Name });
                    GlobalVars.selectedElt = new GlobalVars.SelectedElement("pickup", i);
                    ApplyPropertiesWindow();
                    m_game.shouldUpdateOnce = true;
                    return;
                }
            }

        }

        #region ApplyProperties
        private void ApplyPropertiesWindow()
        {
            if (GlobalVars.selectedElt != null)
            {
                Properties.Children.Clear();
                Dictionary<string, StackPanel> spElements = new Dictionary<string, StackPanel>();

                // Position
                spElements["Position"] = new StackPanel();

                TextBox tbXPos = new TextBox();
                TextBox tbYPos = new TextBox();
                TextBox tbZPos = new TextBox();

                tbXPos.Width = 70;
                tbYPos.Width = 70;
                tbZPos.Width = 70;

                object[] pos = (object[])m_game.WPFHandler("getElementInfo", new object[] { "pos", GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });
                tbXPos.Text = pos[0].ToString();
                tbYPos.Text = pos[1].ToString();
                tbZPos.Text = pos[2].ToString();

                tbXPos.TextChanged += (s, e) => PropertyChanged(s, e, "pos");
                tbYPos.TextChanged += (s, e) => PropertyChanged(s, e, "pos");
                tbZPos.TextChanged += (s, e) => PropertyChanged(s, e, "pos");

                tbYPos.Margin = new Thickness(5, 0, 0, 0);
                tbZPos.Margin = new Thickness(5, 0, 0, 0);

                Label titlePos = new Label();
                titlePos.Content = "Position:";
                titlePos.Target = tbXPos;

                spElements["Position"].Children.Add(titlePos);
                spElements["Position"].Children.Add(tbXPos);
                spElements["Position"].Children.Add(tbYPos);
                spElements["Position"].Children.Add(tbZPos);

                // Rotation
                spElements["Rotation"] = new StackPanel();

                TextBox tbXRot = new TextBox();
                TextBox tbYRot = new TextBox();
                TextBox tbZRot = new TextBox();

                tbXRot.Width = 70;
                tbYRot.Width = 70;
                tbZRot.Width = 70;

                object[] rot = (object[])m_game.WPFHandler("getElementInfo", new object[] { "rot", GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });
                tbXRot.Text = rot[0].ToString();
                tbYRot.Text = rot[1].ToString();
                tbZRot.Text = rot[2].ToString();

                tbXRot.TextChanged += (s, e) => PropertyChanged(s, e, "rot");
                tbYRot.TextChanged += (s, e) => PropertyChanged(s, e, "rot");
                tbZRot.TextChanged += (s, e) => PropertyChanged(s, e, "rot");

                tbYRot.Margin = new Thickness(5, 0, 0, 0);
                tbZRot.Margin = new Thickness(5, 0, 0, 0);

                Label titleRot = new Label();
                titleRot.Content = "Rotation:";
                titleRot.Target = tbXRot;

                spElements["Rotation"].Children.Add(titleRot);
                spElements["Rotation"].Children.Add(tbXRot);
                spElements["Rotation"].Children.Add(tbYRot);
                spElements["Rotation"].Children.Add(tbZRot);

                // Scale
                spElements["Scale"] = new StackPanel();

                TextBox tbXScale = new TextBox();
                TextBox tbYScale = new TextBox();
                TextBox tbZScale = new TextBox();

                tbXScale.Width = 70;
                tbYScale.Width = 70;
                tbZScale.Width = 70;

                object[] scale = (object[])m_game.WPFHandler("getElementInfo", new object[] { "scale", GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });
                tbXScale.Text = scale[0].ToString();
                tbYScale.Text = scale[1].ToString();
                tbZScale.Text = scale[2].ToString();

                tbXScale.TextChanged += (s, e) => PropertyChanged(s, e, "scale");
                tbYScale.TextChanged += (s, e) => PropertyChanged(s, e, "scale");
                tbZScale.TextChanged += (s, e) => PropertyChanged(s, e, "scale");

                tbYScale.Margin = new Thickness(5, 0, 0, 0);
                tbZScale.Margin = new Thickness(5, 0, 0, 0);

                Label titleScale = new Label();
                titleScale.Content = "Scale:";
                titleScale.Target = tbXScale;

                spElements["Scale"].Children.Add(titleScale);
                spElements["Scale"].Children.Add(tbXScale);
                spElements["Scale"].Children.Add(tbYScale);
                spElements["Scale"].Children.Add(tbZScale);

                // Trees
                if (GlobalVars.selectedElt.eltType == "tree")
                {
                    // Profile
                    spElements["TreeProfile"] = new StackPanel();

                    TextBox tbProfile = new TextBox();

                    tbProfile.Width = 150;

                    string treeprofile = (string)m_game.WPFHandler("getElementInfo", new object[] { "treeprofile", GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });
                    tbProfile.Text = System.IO.Path.GetFileName(treeprofile);
                    tbProfile.Margin = new Thickness(5, 0, 0, 0);
                    tbProfile.IsEnabled = false;

                    Label titleTreeProfile = new Label();
                    titleTreeProfile.Content = "Tree Profile:";
                    titleTreeProfile.Target = tbProfile;

                    spElements["TreeProfile"].Children.Add(titleTreeProfile);
                    spElements["TreeProfile"].Children.Add(tbProfile);

                    // Seed
                    spElements["TreeSeed"] = new StackPanel();

                    TextBox tbSeed = new TextBox();

                    tbSeed.Width = 50;

                    object treeseed = m_game.WPFHandler("getElementInfo", new object[] { "treeseed", GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });
                    tbSeed.Text = treeseed.ToString();

                    tbSeed.Margin = new Thickness(5, 0, 0, 0);

                    Label titleTreeSeed = new Label();
                    titleTreeSeed.Content = "Tree Seed:";
                    titleTreeSeed.Target = tbSeed;

                    spElements["TreeSeed"].Children.Add(titleTreeSeed);
                    spElements["TreeSeed"].Children.Add(tbSeed);

                    // Duplicate Button
                    spElements["OptionsButtons"] = new StackPanel();

                    Button duplicateButton = new Button();
                    duplicateButton.Content = "Duplicate";
                    duplicateButton.Width = 150;

                    duplicateButton.Click += (s, e) =>
                    {
                        GlobalVars.gameInfo = (Engine.Game.LevelInfo.LevelData)m_game.WPFHandler("getLevelData", true);

                        GlobalVars.embeddedGame.WPFHandler("addElement", new object[] { "tree", GlobalVars.gameInfo.MapModels.Trees[GlobalVars.selectedElt.eltId] });
                        LoadGameComponentsToTreeview();
                    };

                    // Remove Button

                    Button removeButton = new Button();
                    removeButton.Content = "Remove";
                    removeButton.Width = 150;
                    removeButton.Margin = new Thickness(3, 0, 0, 0);

                    removeButton.Click += (s, e) =>
                    {
                        GlobalVars.gameInfo = (Engine.Game.LevelInfo.LevelData)m_game.WPFHandler("getLevelData", true);

                        GlobalVars.embeddedGame.WPFHandler("removeElement", new object[] { "tree", GlobalVars.selectedElt.eltId });
                        if (GlobalVars.selectedElt.eltId > 0)
                            GlobalVars.selectedElt.eltId--;
                        else
                            GlobalVars.selectedElt = null;
                        LoadGameComponentsToTreeview();
                        m_game.shouldUpdateOnce = true;
                    };
                    
                    spElements["OptionsButtons"].Children.Add(duplicateButton);
                    spElements["OptionsButtons"].Children.Add(removeButton);
                }
                else if (GlobalVars.selectedElt.eltType == "pickup")
                {
                    // WeaponName
                    spElements["WeaponName"] = new StackPanel();

                    TextBox tbWN = new TextBox();

                    tbWN.Width = 150;

                    object pickupname = m_game.WPFHandler("getElementInfo", new object[] { "pickupname", GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });
                    tbWN.Text = pickupname.ToString();

                    tbWN.Margin = new Thickness(5, 0, 0, 0);
                    tbWN.TextChanged += (s, e) => PropertyChanged(s, e, "pickupname");

                    Label titlePickupName = new Label();
                    titlePickupName.Content = "Weapon Name:";
                    titlePickupName.Target = tbWN;

                    spElements["WeaponName"].Children.Add(titlePickupName);
                    spElements["WeaponName"].Children.Add(tbWN);


                    // WeaponName
                    spElements["WeaponBullet"] = new StackPanel();

                    TextBox tbWB = new TextBox();
                    tbWB.Width = 50;

                    object pickupbullet = m_game.WPFHandler("getElementInfo", new object[] { "pickupbullet", GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });
                    tbWB.Text = pickupbullet.ToString();

                    tbWB.TextChanged += (s, e) => PropertyChanged(s, e, "pickupbullets");

                    tbWB.Margin = new Thickness(5, 0, 0, 0);

                    Label titlePickupBullet = new Label();
                    titlePickupBullet.Content = "Bullets :";
                    titlePickupBullet.Target = tbWB;

                    spElements["WeaponBullet"].Children.Add(titlePickupBullet);
                    spElements["WeaponBullet"].Children.Add(tbWB);

                    // Duplicate Button
                    spElements["OptionsButtons"] = new StackPanel();

                    Button duplicateButton = new Button();
                    duplicateButton.Content = "Duplicate";
                    duplicateButton.Width = 150;

                    duplicateButton.Click += (s, e) =>
                    {
                        GlobalVars.gameInfo = (Engine.Game.LevelInfo.LevelData)m_game.WPFHandler("getLevelData", true);

                        GlobalVars.embeddedGame.WPFHandler("addElement", new object[] { "pickup", GlobalVars.gameInfo.MapModels.Pickups[GlobalVars.selectedElt.eltId] });
                        LoadGameComponentsToTreeview();
                    };

                    // Remove Button
                    Button removeButton = new Button();
                    removeButton.Content = "Remove";
                    removeButton.Width = 150;
                    removeButton.Margin = new Thickness(3, 0, 0, 0);

                    removeButton.Click += (s, e) =>
                    {
                        GlobalVars.gameInfo = (Engine.Game.LevelInfo.LevelData)m_game.WPFHandler("getLevelData", true);

                        GlobalVars.embeddedGame.WPFHandler("removeElement", new object[] { "pickup", GlobalVars.selectedElt.eltId });
                        if (GlobalVars.selectedElt.eltId > 0)
                            GlobalVars.selectedElt.eltId--;
                        else
                            GlobalVars.selectedElt = null;
                        LoadGameComponentsToTreeview();
                        m_game.shouldUpdateOnce = true;
                    };

                    spElements["OptionsButtons"].Children.Add(duplicateButton);
                    spElements["OptionsButtons"].Children.Add(removeButton);
                }
                else if (GlobalVars.selectedElt.eltType == "model")
                {
                    // Duplicate Button
                    spElements["OptionsButtons"] = new StackPanel();

                    Button duplicateButton = new Button();
                    duplicateButton.Content = "Duplicate";
                    duplicateButton.Width = 150;

                    duplicateButton.Click += (s, e) =>
                        {
                            GlobalVars.gameInfo = (Engine.Game.LevelInfo.LevelData)m_game.WPFHandler("getLevelData", true);

                            GlobalVars.embeddedGame.WPFHandler("addElement", new object[] { "model", GlobalVars.gameInfo.MapModels.Models[GlobalVars.selectedElt.eltId] });
                            LoadGameComponentsToTreeview();
                        };


                    // Remove Button

                    Button removeButton = new Button();
                    removeButton.Content = "Remove";
                    removeButton.Width = 150;
                    removeButton.Margin = new Thickness(3, 0, 0, 0);

                    removeButton.Click += (s, e) =>
                    {
                        GlobalVars.embeddedGame.WPFHandler("removeElement", new object[] { "model", GlobalVars.selectedElt.eltId });
                        if (GlobalVars.selectedElt.eltId > 0)
                            GlobalVars.selectedElt.eltId--;
                        else
                            GlobalVars.selectedElt = null;

                        GlobalVars.gameInfo = (Engine.Game.LevelInfo.LevelData)m_game.WPFHandler("getLevelData", true);

                        LoadGameComponentsToTreeview();
                        m_game.shouldUpdateOnce = true;
                    };

                    spElements["OptionsButtons"].Children.Add(duplicateButton);
                    spElements["OptionsButtons"].Children.Add(removeButton);
                }

                // Add elements to the main StackPanel
                foreach (KeyValuePair<string, StackPanel> pair in spElements)
                {
                    spElements[pair.Key].Name = "ppt_" + pair.Key;
                    Properties.Children.Add(pair.Value);
                }
            }
        }

        private void PropertyChanged(object s, TextChangedEventArgs e, string propertyType)
        {
            if (propertyType == "pos" || propertyType == "rot" || propertyType == "scale")
            {
                List<string> vals = new List<string>();
                foreach (UIElement elt in (((FrameworkElement)s).Parent as StackPanel).Children)
                {
                    if (elt is TextBox)
                        vals.Add(((TextBox)elt).Text);
                }
                m_game.WPFHandler("setElementInfo", new object[] { propertyType, new object[] { vals[0], vals[1], vals[2] }, GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });
            }
            else if (propertyType == "pickupname")
            {
                if ((bool)m_game.WPFHandler("setElementInfo", new object[] { propertyType, ((TextBox)s).Text, GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId }))
                {
                    foreach (TreeViewItem parentpu in GameComponentsList.Items)
                        if ((string)parentpu.Header == "Pick-Ups")
                            ((TreeViewItem)parentpu.Items[GlobalVars.selectedElt.eltId]).Header = (string)((TextBox)s).Text;
                }
                
            }
            else if (propertyType == "pickupbullets")
                m_game.WPFHandler("setElementInfo", new object[] { propertyType, ((TextBox)s).Text, GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });

            m_game.shouldUpdateOnce = true;
        }
        #endregion

        #region "Windows Menu Helper"
        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            GlobalVars.OnFragmentNavigation(e);
        }

        public void OnNavigatedTo(NavigationEventArgs e)
        {
            GlobalVars.OnNavigatedTo(e);
        }

        public void OnNavigatedFrom(NavigationEventArgs e)
        {
            GlobalVars.OnNavigatedFrom(e);
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            GlobalVars.OnNavigatingFrom(e);
        }
        #endregion
    }
}
