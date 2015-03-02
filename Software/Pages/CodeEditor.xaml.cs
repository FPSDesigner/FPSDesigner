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
using System.Windows.Media.Animation;
using System.Timers;
using System.Windows.Threading;
using System.Reflection;

using Engine;

namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl, IContent
    {
        public static RoutedCommand Shortcuts = new RoutedCommand();

        private Dictionary<string, string> codeFiles = new Dictionary<string, string>();
        private Button selectedButton;
        private ModernWindow textboxDialog;
        private string newScriptName = "New Script";

        public CodeEditor()
        {
            InitializeComponent();

            if (!Directory.Exists("Scripts"))
                Directory.CreateDirectory("Scripts");

            ReloadFilesToTreeView();

            saveFileButton.Click += saveFileButton_Click;

            selectedButton = (Button)listButtonsTab.Children[0];
            codeFiles.Add(newScriptName, textEditor.Text);

            selectedButton.Click += tabFileButton_Click;

            RoutedCommand firstSettings = new RoutedCommand();
            firstSettings.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(firstSettings, ShortcutSave));

            // Methods list generation
            MethodInfo[] mi = (typeof(Engine.Game.Script.CLuaScriptFunctions)).GetMethods();
            Array.Sort(mi,
                delegate(MethodInfo m1, MethodInfo m2)
                {
                    return m1.Name.CompareTo(m2.Name);
                }
            );
            foreach (var method in mi)
            {
                if (method.Name == "Equals" || method.Name == "ToString" || method.Name == "GetType")
                    continue;

                var parameters = method.GetParameters();
                var parameterDescriptions = string.Join(", ", method.GetParameters()
                                 .Select(x => x.ParameterType + " " + x.Name)
                                 .ToArray());

                StackPanel sp = new StackPanel();
                Image icon = new Image();
                TextBlock tb = new TextBlock();

                // Image & TextBlock
                icon.Source = new BitmapImage(new Uri("/Assets/Icons/AddMethod.png", UriKind.Relative));
                icon.Width = 16;
                icon.Height = 16;
                tb.Text = method.Name;
                tb.Margin = new Thickness(15, 0, 0, 3);

                // Add content to StackPanel
                sp.Orientation = Orientation.Horizontal;

                sp.ToolTip = method.Name + "(" + parameterDescriptions + ")";
                sp.Children.Add(icon);
                sp.Children.Add(tb);
                sp.MouseDown += sp_MouseDownMethods;

                FunctionsList.Items.Add(sp);
            }
        }

        private void ShortcutSave(object sender, ExecutedRoutedEventArgs e)
        {
            saveCurrentFile();
        }

        private void sp_MouseDownMethods(object sender, MouseButtonEventArgs e)
        {
            textEditor.Text = textEditor.Text.Insert(textEditor.CaretOffset, ((TextBlock)((StackPanel)sender).Children[1]).Text + "()");
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
                    CreateNewFileTab(blockid.Text);
                }
            }
        }

        void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            saveCurrentFile();
        }

        private void saveCurrentFile()
        {
            if ((string)selectedButton.Content != newScriptName)
            {
                if (File.Exists("Scripts/" + selectedButton.Content))
                {
                    System.IO.File.WriteAllText("Scripts/" + selectedButton.Content, textEditor.Text);

                    DoubleAnimation opacityAnim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
                    checkValidSave.BeginAnimation(OpacityProperty, opacityAnim);

                    Timer timerResetCheck = new Timer(2000);
                    timerResetCheck.Elapsed += timerResetCheck_Elapsed;
                    timerResetCheck.Enabled = true;
                }
            }
            else
            {
                textboxDialog = new ModernWindow
                {
                    Style = (Style)App.Current.Resources["EmptyWindow"],
                    Content = new TextboxDialog
                    {
                        Margin = new Thickness(32),
                    },
                    MaxHeight = 100,
                    MaxWidth = 635,
                    ResizeMode = ResizeMode.NoResize
                };

                textboxDialog.Show();

                ((Pages.TextboxDialog)textboxDialog.Content).EnteredText += CodeEditor_EnteredText;
            }
        }

        void CodeEditor_EnteredText(object sender, RoutedEventArgs e)
        {
            string text = ((TextBox)sender).Text;
            textboxDialog.Close();

            System.IO.File.WriteAllText("Scripts/" + text, textEditor.Text);

            DoubleAnimation opacityAnim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
            checkValidSave.BeginAnimation(OpacityProperty, opacityAnim);

            Timer timerResetCheck = new Timer(2000);
            timerResetCheck.Elapsed += timerResetCheck_Elapsed;
            timerResetCheck.Enabled = true;

            CreateNewFileTab(text);
            ReloadFilesToTreeView();
        }

        void timerResetCheck_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Stop();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DoubleAnimation opacityAnim2 = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(500)));
                checkValidSave.BeginAnimation(OpacityProperty, opacityAnim2);
            }));
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

        private void CreateNewFileTab(string fileName)
        {
            Button newBut = new Button();

            newBut.Content = fileName;
            newBut.SetResourceReference(Grid.BackgroundProperty, "SelectedTabButton");
            newBut.Click += tabFileButton_Click;

            listButtonsTab.Children.Add(newBut);

            selectedButton.SetResourceReference(Grid.BackgroundProperty, "ButtonBackground");
            selectedButton = newBut;

            string txtFile = System.IO.File.ReadAllText("Scripts/" + fileName);

            textEditor.Text = txtFile;
            codeFiles.Add(fileName, txtFile);
        }

        private void ReloadFilesToTreeView()
        {
            // Removing items from the listview
            if (ScriptsListView.Items.Count > 0)
            {
                foreach (UIElement elt in ScriptsListView.Items)
                {
                    StackPanel element = (StackPanel)elt;
                    element.Children.Clear();
                }
                ScriptsListView.Items.Clear();
            }

            List<string> listFiles = Directory.GetFiles("Scripts").ToList<string>();
            listFiles.Add(newScriptName);

            foreach (string file in listFiles)
            {
                StackPanel sp = new StackPanel();
                Image icon = new Image();
                TextBlock tb = new TextBlock();

                // Image & TextBlock
                string imgSource = "/Assets/Icons/Script.png";
                if (file == newScriptName)
                    imgSource = "/Assets/Icons/NewScript.png";

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
