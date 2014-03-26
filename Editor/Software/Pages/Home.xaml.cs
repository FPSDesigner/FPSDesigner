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

            ShowXNAImage.Source = m_game.em_WriteableBitmap;
            ShowXNAImage.SizeChanged += ShowXNAImage_SizeChanged;

            resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            resizeTimer.Tick += new EventHandler(disTimer_Tick);
        }

        void disTimer_Tick(object sender, EventArgs e)
        {
            m_game.ChangeEmbeddedViewport((int)ShowXNAImage.RenderSize.Width, (int)ShowXNAImage.RenderSize.Height);
            ShowXNAImage.Source = m_game.em_WriteableBitmap;
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
