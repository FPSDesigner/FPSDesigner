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

        private bool isMovingGame1 = false;
        private Point initialMoveMousePosGame1;

        public Home()
        {
            InitializeComponent();
            MainWindowInstance = MainWindow.Instance;

            m_game = new Engine.MainGameEngine(true);

            ShowXNAImage1.Source = m_game.em_WriteableBitmap;
            ShowXNAImage1.SizeChanged += ShowXNAImage_SizeChanged;
            GameButton1.MouseWheel += GameButton1_MouseWheel;
            GameButton1.MouseMove += GameButton1_MouseMove;

            resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            resizeTimer.Tick += new EventHandler(disTimer_Tick);

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
            statusBarView1.Text = "Moving...";
            m_game.WPFHandler("changeCamFreeze", false);

            isMovingGame1 = true;
            initialMoveMousePosGame1 = PointToScreen(Mouse.GetPosition(null));
            Cursor = Cursors.ScrollAll;

            UIElement el = (UIElement)sender;
            el.CaptureMouse();
        }

        private void GameButton1_MouseRightUp(object sender, MouseButtonEventArgs e)
        {
            statusBarView1.Text = "Idle";

            m_game.WPFHandler("changeCamFreeze", true);

            isMovingGame1 = false;
            Cursor = Cursors.Arrow;

            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();
        }

        void disTimer_Tick(object sender, EventArgs e)
        {
            m_game.ChangeEmbeddedViewport((int)ShowXNAImage1.RenderSize.Width, (int)ShowXNAImage1.RenderSize.Height);
            ShowXNAImage1.Source = m_game.em_WriteableBitmap;
            resizeTimer.Stop();
        }

        private void ShowXNAImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
            //XNAStatus.Width = e.NewSize.Width;
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
