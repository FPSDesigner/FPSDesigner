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
        Dictionary<string, string> codeFiles = new Dictionary<string, string>();
        Button selectedButton;
        string newScriptName = "New Script";

        public CodeEditor()
        {
            InitializeComponent();

            if (!Directory.Exists("Scripts"))
                Directory.CreateDirectory("Scripts");

            List<string> listFiles = Directory.GetFiles("Scripts").ToList<string>();
            listFiles.Add(newScriptName);

            foreach (string file in listFiles)
            {
                StackPanel sp = new StackPanel();
                Image icon = new Image();
                TextBlock tb = new TextBlock();

                // Image & TextBlock
                string imgSource = "/Assets/Script.png";
                if (file == newScriptName)
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
                sp.MouseDown += sp_MouseDown;

                ScriptsListView.Items.Add(sp);
            }

            saveFileButton.Click += saveFileButton_Click;

            selectedButton = (Button)listButtonsTab.Children[0];
            codeFiles.Add(newScriptName, textEditor.Text);

            selectedButton.Click += tabFileButton_Click;
        }

        void sp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock blockid = (TextBlock)((StackPanel)sender).Children[1];

            if (blockid.Text == (string)selectedButton.Content)
                return;

            bool isFileAlreadyOpened = false;
            foreach (UIElement button in listButtonsTab.Children)
            {
                Button but = (Button)button;
                if ((string)but.Content == blockid.Text)
                {
                    isFileAlreadyOpened = true;

                    codeFiles[(string)selectedButton.Content] = textEditor.Text;
                    textEditor.Text = codeFiles[(string)but.Content];

                    but.SetResourceReference(Grid.BackgroundProperty, "SelectedTabButton");
                    selectedButton.SetResourceReference(Grid.BackgroundProperty, "ButtonBackground");
                    selectedButton = but;
                    return;
                }
            }

            if (!isFileAlreadyOpened)
            {
                if (File.Exists("Scripts/" + blockid.Text))
                {
                    Button newBut = new Button();

                    newBut.Content = blockid.Text;
                    newBut.SetResourceReference(Grid.BackgroundProperty, "SelectedTabButton");
                    newBut.Click += tabFileButton_Click;

                    listButtonsTab.Children.Add(newBut);

                    selectedButton.SetResourceReference(Grid.BackgroundProperty, "ButtonBackground");
                    selectedButton = newBut;

                    string txtFile = System.IO.File.ReadAllText("Scripts/" + blockid.Text);

                    textEditor.Text = txtFile;
                    codeFiles.Add(blockid.Text, txtFile);
                }
            }
        }

        void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            /*if (File.Exists("Scripts/" + TabNameFile.Content))
            {
                System.IO.File.WriteAllText("Scripts/" + TabNameFile.Content, textEditor.Text);
            }*/
        }

        void tabFileButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Button)sender != selectedButton)
            {
                Button clickedButton = (Button)sender;

                codeFiles[(string)selectedButton.Content] = textEditor.Text;
                textEditor.Text = codeFiles[(string)clickedButton.Content];

                clickedButton.SetResourceReference(Grid.BackgroundProperty, "SelectedTabButton");
                selectedButton.SetResourceReference(Grid.BackgroundProperty, "ButtonBackground");
                selectedButton = clickedButton;
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
