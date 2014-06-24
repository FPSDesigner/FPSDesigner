using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using FirstFloor.ModernUI.Windows.Controls;
using System.Text.RegularExpressions;
using Microsoft.Win32;

using ModelViewer;

namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for ModelManager.xaml
    /// </summary>
    public partial class ModelManager : UserControl
    {
        public event RoutedEventHandler ShouldClose;

        private string[] textureList = new string[4];

        public ModelManager()
        {
            InitializeComponent();

            selectButton0.Click += selectButton_Click;
            selectButton1.Click += selectButton_Click;
            selectButton2.Click += selectButton_Click;
            selectButton3.Click += selectButton_Click;

            selectModelUrl.Click += selectModelUrl_Click;

            ValidateButton.Click += ValidateButton_Click;
        }

        void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            if(modelUrl.Text == "")
                return;

            List<Engine.Game.LevelInfo.MapModels_Texture> textures = new List<Engine.Game.LevelInfo.MapModels_Texture>();

            for(int i = 0; i < 4; i++)
            {
                Image img = (Image)FindName("imgTexture" + i);
                if(img.Source.ToString() != "/Assets/placeholder.png" && textureList[i] != null)
                {
                    textures.Add(new Engine.Game.LevelInfo.MapModels_Texture {
                        Mesh = ((TextBox)FindName("meshName" + i)).Text,
                        Texture = textureList[i]
                    });
                }
            }
            

            Engine.Game.LevelInfo.MapModels_Model modelInfo =
                new Engine.Game.LevelInfo.MapModels_Model
                {
                    Position = new Engine.Game.LevelInfo.Coordinates(0, 0, 0),
                    Rotation = new Engine.Game.LevelInfo.Coordinates(0, 0, 0),
                    Alpha = 1,
                    ModelFile = "Models/" + modelUrl.Text,
                    Scale = new Engine.Game.LevelInfo.Coordinates(1, 1, 1),
                    SpecColor = 1,
                    Textures = new Engine.Game.LevelInfo.MapModels_Textures
                    {
                        Texture = textures
                    }

                };

            GlobalVars.embeddedGame.WPFHandler("addElement", new object[] { "model", modelInfo });
            GlobalVars.RaiseEvent("ReloadGameComponentsTreeView");

            ShouldClose("ModelManager", null);
        }

        void selectModelUrl_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the open file dialog box.
            OpenFileDialog modelDialog = new OpenFileDialog();

            // Set filter options and filter index.
            modelDialog.Filter = "Model file|*.fbx;*.xnb;";
            modelDialog.FilterIndex = 1;

            modelDialog.Multiselect = false;

            // Process input if the user clicked OK.
            if (modelDialog.ShowDialog() == true)
            {
                // Copy file to texture path
                string destFile = "./Content/Models/" + Path.GetFileName(modelDialog.FileName);
                try
                {
                    System.IO.File.Copy(modelDialog.FileName, destFile, true);
                }
                catch { }
                modelUrl.Text = Path.GetFileName(modelDialog.FileName);
            }
        }

        void selectButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the open file dialog box.
            OpenFileDialog imageDialog = new OpenFileDialog();

            // Set filter options and filter index.
            imageDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tga;*.xnb";
            imageDialog.FilterIndex = 1;

            imageDialog.Multiselect = false;

            // Process input if the user clicked OK.
            if (imageDialog.ShowDialog() == true)
            {
                // Copy file to texture path
                string destImage = "./Content/Textures/" + Path.GetFileName(imageDialog.FileName);
                System.IO.File.Copy(imageDialog.FileName, destImage, true);

                int textId = Int32.Parse(((Button)sender).Name.Replace("selectButton", ""));
                textureList[textId] = "Textures/" + Path.GetFileName(imageDialog.FileName);
                // Change image placeholder to this image
                if(Path.GetExtension(destImage) != ".xnb")
                    ((Image)((StackPanel)VisualTreeHelper.GetParent((UIElement)sender)).Children[2]).Source = new BitmapImage(new Uri(destImage,UriKind.Relative));
            }
        }



    }
}
