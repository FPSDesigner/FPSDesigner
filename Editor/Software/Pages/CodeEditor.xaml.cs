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
using System.IO;

using System.Windows.Threading;

namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl, IContent
    {
        public CodeEditor()
        {
            InitializeComponent();

            if (!Directory.Exists("Scripts"))
                Directory.CreateDirectory("Scripts");

            List<string> listFiles = Directory.GetFiles("Scripts").ToList<string>();
            listFiles.Add("New Script");

            foreach (string file in listFiles)
            {
                StackPanel sp = new StackPanel();
                Image icon = new Image();
                TextBlock tb = new TextBlock();

                // Image & TextBlock
                string imgSource = "/Assets/Script.png";
                if (file == "New Script")
                    imgSource = "/Assets/NewScript.png";

                icon.Source = new BitmapImage(new Uri(imgSource, UriKind.Relative));
                icon.Width = 16;
                icon.Height = 16;
                tb.Text = file.Replace("Scripts\\", "");
                tb.Margin = new Thickness(15, 0, 0, 3);

                // Add content to StackPanel
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(icon);
                sp.Children.Add(tb);

                ScriptsListView.Items.Add(sp);
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
