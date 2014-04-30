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
        public event RoutedEventHandler ShouldClose;
        private float initialZoom = 20;

        public TreeManager()
        {
            InitializeComponent();

            modelViewer = new ModelViewer.ModelViewer(true);

            TreeViewImage.Source = modelViewer.em_WriteableBitmap;
            TreeViewImage.SizeChanged += TreeViewImage_SizeChanged;

            ButtonGame.SizeChanged += ButtonGame_SizeChanged;

            resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            resizeTimer.Tick += new EventHandler(disTimer_Tick);

            // Content initialization
            TreeProfile.SelectedIndex = 1;

            TreeSeedSlider.ValueChanged += (sender, args) => TreeSeedTB.Text = ((int)(args.NewValue * 100)).ToString();
            TreeSeedTB.TextChanged += (sender, args) =>
            {
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
            TreeSeedTB.Text = TreeSeedSlider.Value.ToString();

            PreviewButton.Click += PreviewButton_Click;
            GenerateButton.Click += GenerateButton_Click;

            ZoomSlider.ValueChanged += ZoomSlider_ValueChanged;
            modelViewer.ChangeCameraZoom((float)ZoomSlider.Value * 2);
            initialZoom = (float)ZoomSlider.Value;
        }

        void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if(GlobalVars.gameInfo.MapModels == null)
                GlobalVars.gameInfo.MapModels = new Engine.Game.LevelInfo.MapModels { };
            if (GlobalVars.gameInfo.MapModels.Trees == null)
                GlobalVars.gameInfo.MapModels.Trees = new List<Engine.Game.LevelInfo.MapModels_Tree>();

            int seed = 1;

            GlobalVars.gameInfo.MapModels.Trees.Add(
                new Engine.Game.LevelInfo.MapModels_Tree
                {
                    Profile = TreeProfile.Text,
                    Position = new Engine.Game.LevelInfo.Coordinates
                    {
                        X = 0,
                        Y = 0,
                        Z = 0,
                    },
                    Rotation = new Engine.Game.LevelInfo.Coordinates
                    {
                        X = 0,
                        Y = 0,
                        Z = 0,
                    },
                    Scale = new Engine.Game.LevelInfo.Coordinates
                    {
                        X = modelViewer.treeScale,
                        Y = modelViewer.treeScale,
                        Z = modelViewer.treeScale,
                    },
                    Seed = (Int32.TryParse(TreeSeedTB.Text, out seed) ? seed : 1),
                    Wind = (bool)WindButton.IsChecked,
                    Branches = (bool)BranchesButton.IsChecked,
                }
            );
            GlobalVars.SaveGameLevel();
            ShouldClose("TreeManager", null);
        }

        void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            modelViewer.ChangeCameraZoom((float)e.NewValue * 2);
        }

        void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            int seed = -1;
            if (TreeSeedTB.Text != "")
                seed = int.Parse(TreeSeedTB.Text);

            ZoomSlider.Value = initialZoom;
            modelViewer.LoadNewTree("Trees/Trees/" + (string)((ComboBoxItem)TreeProfile.SelectedValue).Content, seed, (bool)BranchesButton.IsChecked, (bool)WindButton.IsChecked);

            STTrunkVertices.Text = modelViewer.GetTreeData(0) + " Trunk Vertices";
            STTrunkTriangles.Text = modelViewer.GetTreeData(1) + " Trunk Triangles";
            STBones.Text = modelViewer.GetTreeData(2) + " Bones";
            STLeaves.Text = STBones.Text = modelViewer.GetTreeData(3) + " Leaves";
        }

        void ButtonGame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        void disTimer_Tick(object sender, EventArgs e)
        {
            modelViewer.ChangeEmbeddedViewport((int)ButtonGame.RenderSize.Width, (int)ButtonGame.RenderSize.Height);
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
