using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using FirstFloor.ModernUI.Windows.Controls;
using System.Windows.Threading;
using System.Text.RegularExpressions;

using ModelViewer;

namespace Software.Pages.TerrainManager
{
    /// <summary>
    /// Interaction logic for TerrainManager.xaml
    /// </summary>
    public partial class TerrainManager : UserControl
    {
        public event RoutedEventHandler ShouldClose;

        public TerrainManager()
        {
            InitializeComponent();

            ValidateButton.Click += ValidateButton_Click;
        }

        void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            ShouldClose("TerrainManager", null);
        }

    }
}
