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

        private List<TreeViewItem> listTree_Trees, listTree_Models;

        private bool isMovingGyzmoAxis = false;

        public Home()
        {
            InitializeComponent();
            MainWindowInstance = MainWindow.Instance;

            m_game = new Engine.MainGameEngine(true);

            listTree_Trees = new List<TreeViewItem>();
            listTree_Models = new List<TreeViewItem>();

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

            GameComponentsList.SelectedItemChanged += GameComponentsList_SelectedItemChanged;



            GlobalVars.selectedToolButton = SelectButton;
            SelectButton.Foreground = new SolidColorBrush((Color)FindResource("AccentColor"));
            SelectButton.Click += ToolButton_Click;
            PositionButton.Click += ToolButton_Click;
            RotateButton.Click += ToolButton_Click;
            ScaleButton.Click += ToolButton_Click;

            LoadGameComponentsToTreeview();
        }

        void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalVars.selectedToolButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 209, 209, 209));
            GlobalVars.selectedToolButton = ((ModernButton)sender);
            GlobalVars.selectedToolButton.Foreground = new SolidColorBrush((Color)FindResource("AccentColor"));

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
                Console.WriteLine(Mouse.GetPosition(ShowXNAImage1));
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

                isMovingGyzmoAxis = false;
                Console.WriteLine("isMovingGyzmoAxis to FALSE");
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
                    }
                    else if (GlobalVars.selectedToolButton.Name == "PositionButton" && GlobalVars.selectedElt != null)
                    {
                        if ((string)selectElt[0] == "gizmo" && selectElt[1] is int)
                        {
                            m_game.WPFHandler("moveObject", new object[] { "pos", (int)selectElt[1], GlobalVars.selectedElt.eltType, GlobalVars.selectedElt.eltId });

                            isMovingGyzmoAxis = true;
                            Console.WriteLine("isMovingGyzmoAxis to TRUE");
                        }
                    }
                }
            }
        }


        void disTimer_Tick(object sender, EventArgs e)
        {
            m_game.ChangeEmbeddedViewport((int)ShowXNAImage1.RenderSize.Width, (int)ShowXNAImage1.RenderSize.Height);
            ShowXNAImage1.Source = m_game.em_WriteableBitmap;
            resizeTimer.Stop();
            m_game.shouldUpdateOnce = true;
        }

        private void ShowXNAImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void LoadGameComponentsToTreeview()
        {
            GameComponentsList.Items.Clear();
            listTree_Trees.Clear();
            listTree_Models.Clear();

            TreeViewItem Models = new TreeViewItem();
            TreeViewItem Trees = new TreeViewItem();
            TreeViewItem Water = new TreeViewItem();
            TreeViewItem Terrain = new TreeViewItem();

            Models.Header = "Models";
            Trees.Header = "Trees";
            Water.Header = "Water";
            Terrain.Header = "Terrain";

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

            // Add items to main tree view
            if (GlobalVars.gameInfo.MapModels != null && GlobalVars.gameInfo.MapModels.Models != null && GlobalVars.gameInfo.MapModels.Models.Count > 0)
                GameComponentsList.Items.Add(Models);

            if (Trees.Items.Count > 0)
                GameComponentsList.Items.Add(Trees);

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
                    m_game.WPFHandler("selectObject", new object[] { "model", i, GlobalVars.selectedToolButton.Name });
                    GlobalVars.selectedElt = new GlobalVars.SelectedElement("model", i);
                    m_game.shouldUpdateOnce = true;
                    return;
                }
            }

            // Trees
            for (int i = 0; i < listTree_Trees.Count; i++)
            {
                if (listTree_Trees[i].IsSelected)
                {
                    m_game.WPFHandler("selectObject", new object[] { "tree", i, GlobalVars.selectedToolButton.Name });
                    GlobalVars.selectedElt = new GlobalVars.SelectedElement("tree", i);
                    m_game.shouldUpdateOnce = true;
                    return;
                }
            }
            
        }

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
