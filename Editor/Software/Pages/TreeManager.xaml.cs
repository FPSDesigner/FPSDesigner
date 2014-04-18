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

namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for TreeManager.xaml
    /// </summary>
    public partial class TreeManager : UserControl
    {
        private ModelViewer.ModelViewer modelViewer;
        private DispatcherTimer resizeTimer = new DispatcherTimer();

        public TreeManager()
        {
            InitializeComponent();

            modelViewer = new ModelViewer.ModelViewer(true);

            TreeViewImage.Source = modelViewer.em_WriteableBitmap;
            TreeViewImage.SizeChanged += TreeViewImage_SizeChanged;

            resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            resizeTimer.Tick += new EventHandler(disTimer_Tick);

            // Content initialization
            TreeProfile.SelectedIndex = 1;

            TreeSeedSlider.ValueChanged += (sender, args) => TreeSeedTB.Text = ((int)(args.NewValue * 100)).ToString();
            TreeSeedTB.TextChanged += (sender, args) => { 
                double newValue;
                if (Double.TryParse(TreeSeedTB.Text, out newValue))
                {
                    if (newValue > 1000)
                    {
                        newValue = 1000;
                        TreeSeedTB.Text = "1000";
                    }
                    TreeSeedSlider.Value = newValue / 100;
                }
            };

            // Allow only integers
            TreeSeedTB.PreviewTextInput += (sender, args) => args.Handled = !IsTextAllowed(args.Text);
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        void disTimer_Tick(object sender, EventArgs e)
        {
            modelViewer.ChangeEmbeddedViewport((int)TreeViewImage.RenderSize.Width, (int)TreeViewImage.RenderSize.Height);
            TreeViewImage.Source = modelViewer.em_WriteableBitmap;
            resizeTimer.Stop();
        }

        void TreeViewImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
        }
    }
}
