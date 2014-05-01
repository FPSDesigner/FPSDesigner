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
            GameButton1.MouseMove += GameButton1_MouseMove;

            resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            resizeTimer.Tick += new EventHandler(disTimer_Tick);

            statusBarView1.Text = "Idle";

            GameComponentsList.SelectedItemChanged += GameComponentsList_SelectedItemChanged;

            GameButton1.GotFocus += ShowXNAImage1_GotFocus;
            GameButton1.LostFocus += ShowXNAImage1_LostFocus;
            GameButton1.Click += GameButton1_Click;

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

        void GameButton1_MouseMove(object sender, MouseEventArgs e)
        {
            /*Console.WriteLine("Normal:"+e.GetPosition(null));
            Console.WriteLine("Fixed:"+PointToScreen(e.GetPosition(null)));
            if (isMovingGame1)
                NativeMethods.SetCursorPos((int)initialMoveMousePosGame1.X, (int)initialMoveMousePosGame1.Y);*/
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

            //m_game.mouseX = (int)Mouse.GetPosition(ShowXNAImage1).X;
            //m_game.mouseY = (int)Mouse.GetPosition(ShowXNAImage1).Y;
        }

        void GameButton1_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalVars.selectedToolButton.Name == "SelectButton")
            {
                object value = m_game.WPFHandler("click", Mouse.GetPosition(ShowXNAImage1));
                if (value is object[])
                {
                    object[] selectElt = (object[])value;
                    if (selectElt.Length == 2)
                    {
                        if ((string)selectElt[0] == "tree" && selectElt[1] is int)
                        {
                            listTree_Trees[(int)selectElt[1]].IsSelected = true;
                            m_game.WPFHandler("selectObject", new object[] { "tree", (int)selectElt[1], GlobalVars.selectedToolButton.Name });
                        }
                        else if ((string)selectElt[0] == "model" && selectElt[1] is int)
                        {
                            listTree_Models[(int)selectElt[1]].IsSelected = true;
                            m_game.WPFHandler("selectObject", new object[] { "model", (int)selectElt[1], GlobalVars.selectedToolButton.Name });
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
            GlobalVars.selectedElt = new GlobalVars.SelectedElement((string)selectedItem.Header, (selectedItem.Parent is TreeViewItem) ? (string)((TreeViewItem)selectedItem.Parent).Header : "");

            // Models
            for (int i = 0; i < listTree_Models.Count; i++)
            {
                if (listTree_Models[i].IsSelected)
                {
                    m_game.WPFHandler("selectObject", new object[] { "model", i, GlobalVars.selectedToolButton.Name });
                    return;
                }
            }

            // Trees
            for (int i = 0; i < listTree_Trees.Count; i++)
            {
                if (listTree_Trees[i].IsSelected)
                {
                    m_game.WPFHandler("selectObject", new object[] { "tree", i, GlobalVars.selectedToolButton.Name });
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
