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
using System.Security;
using System.Security.Cryptography;
using FirstFloor.ModernUI.Windows.Controls;
using System.ComponentModel;
using Microsoft.Win32;

namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for SelectProject.xaml
    /// </summary>
    public partial class SelectProject : UserControl
    {
        public event RoutedEventHandler ProjectSelected;

        public SelectProject()
        {
            InitializeComponent();

            buttonLoadProject.Click += buttonLoadProject_Click;
            buttonNewProject.Click += buttonNewProject_Click;

            textboxSelectNewFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\FPS Designer Project";
            buttonSelectNewFolder.Click += (s, e) => OpenFindNewDirectoryDialog();

            btnValidateNew.Click += btnValidateNew_Click;
        }

        void buttonLoadProject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog projectDialog = new OpenFileDialog();

            // Set filter options and filter index.
            projectDialog.Filter = "FPSDesigner File|*.fpsd;*.fspdesigner";
            projectDialog.FilterIndex = 1;

            projectDialog.Title = "Select a project file";
            projectDialog.Multiselect = false;

            // Process input if the user clicked OK.
            if (projectDialog.ShowDialog() == true)
            {
                if (System.IO.File.Exists(projectDialog.FileName))
                {
                    try
                    {
                        GlobalVars.projectData = Codes.CXMLManager.deserializeClass<Codes.ProjectData>(projectDialog.FileName);
                        ProjectSelected(projectDialog.FileName, null);
                    }
                    catch
                    {
                        loadErrorText.Visibility = System.Windows.Visibility.Visible;
                    }

                }
            }
        }

        void buttonNewProject_Click(object sender, RoutedEventArgs e)
        {
            collapsableGridNew.Visibility = System.Windows.Visibility.Visible;
            collapsableGridNew.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500))));
        }

        private void OpenFindNewDirectoryDialog()
        {
            System.Windows.Forms.FolderBrowserDialog projectDialog = new System.Windows.Forms.FolderBrowserDialog();

            // Set filter options and filter index.
            projectDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Process input if the user clicked OK.
            if (projectDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textboxSelectNewFolder.Text = projectDialog.SelectedPath;
            }
        }

        void btnValidateNew_Click(object sender, RoutedEventArgs e)
        {
            if (textboxSelectName.Text.Length > 0)
            {
                try
                {
                    if (!System.IO.Directory.Exists(textboxSelectNewFolder.Text))
                        System.IO.Directory.CreateDirectory(textboxSelectNewFolder.Text);

                    GlobalVars.projectData = new Codes.ProjectData
                    {
                        Properties = new Codes.Properties
                        {
                            Author = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                            LastEditionDate = DateTime.Now.ToString("M/d/yyyy"),
                            GameName = textboxSelectName.Text,
                        }
                    };

                    Codes.CXMLManager.serializeClass(textboxSelectNewFolder.Text + '\\' + GlobalVars.defaultProjectInfoName, GlobalVars.projectData);

                    ProjectSelected(textboxSelectNewFolder.Text, null);
                }
                catch
                {
                    errorMsg.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500))));
                }
            }
        }
    }
}
