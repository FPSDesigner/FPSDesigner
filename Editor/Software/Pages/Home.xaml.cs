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

        private int count = 0;
        private DispatcherTimer resizeTimer = new DispatcherTimer();
        private MainWindow MainWindowInstance;

        public Home()
        {
            InitializeComponent();
            MainWindowInstance = MainWindow.Instance;

            m_game = new Engine.MainGameEngine(true);

            ShowXNAImage1.Source = m_game.em_WriteableBitmap;
            ShowXNAImage1.SizeChanged += ShowXNAImage_SizeChanged;
            GameButton1.MouseWheel += GameButton1_MouseWheel;

            resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            resizeTimer.Tick += new EventHandler(disTimer_Tick);

            LoadLoginPage();
        }

        private void LoadLoginPage()
        {
            var wnd = new ModernWindow
            {
                Style = (Style)App.Current.Resources["EmptyWindow"],
                Content = new Login
                {
                    Margin = new Thickness(32)
                },
                ResizeMode = System.Windows.ResizeMode.NoResize,
                MaxWidth = 850,
                MaxHeight = 320,
                Topmost = true,
            };

            wnd.Show();
        }

        void GameButton1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Console.WriteLine("MouseWheel: " + e.Delta);
            float coef = 1;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                coef /= 4;

            m_game.WPFHandler("moveCameraForward", (float)e.Delta * coef);
        }

        private void GameButton1_MouseRightDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Mouse down!");
            m_game.WPFHandler("changeCamFreeze", false);

            UIElement el = (UIElement)sender;
            el.CaptureMouse();
            Cursor = Cursors.IBeam;
        }

        private void GameButton1_MouseRightUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Mouse up!");
            m_game.WPFHandler("changeCamFreeze", true);

            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
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
