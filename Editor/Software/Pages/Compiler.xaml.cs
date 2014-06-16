/*using System;
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
using System.Windows.Media.Animation;
using FirstFloor.ModernUI.Windows.Controls;
using System.ComponentModel;
using System.IO;


namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for Compiler.xaml
    /// </summary>
    public partial class Compiler : UserControl
    {
        private Codes.ContentBuilder contentBuilder;
        private Codes.ComboItem selectedItem;

        public Compiler()
        {
            InitializeComponent();
        }

        public void Compile(string from, string to)
        {
            Compil1.Visibility = System.Windows.Visibility.Visible;
            Compil2.Visibility = System.Windows.Visibility.Hidden;

            selectedItem = new Codes.ComboItem(System.IO.Path.GetFileName(Input.Text), Input.Text);

            // Compilation
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
                delegate(object o, DoWorkEventArgs args)
                {
                    this.contentBuilder = new Codes.ContentBuilder(true);
                    this.contentBuilder.Clear();

                    this.contentBuilder.Add(selectedItem);

                    string error = this.contentBuilder.Build();

                    if (!String.IsNullOrEmpty(error))
                    {
                        //MessageBox.Show(string.Format("{0}: {1}", item.Value, error));
                        return;
                    }

                    string tempPath = this.contentBuilder.OutputDirectory;
                    string[] files = Directory.GetFiles(tempPath, "*.xnb");

                    foreach (string file in files)
                    {
                        string output = Path.GetDirectoryName(file)+"/";
                        if (File.Exists(Path.Combine(output, Path.GetFileName(file))))
                            File.Delete(Path.Combine(output, Path.GetFileName(file)));

                        System.IO.File.Move(file, Path.Combine(output, Path.GetFileName(file)));
                    }

                    Compil1.Visibility = System.Windows.Visibility.Hidden;
                    Compil2.Visibility = System.Windows.Visibility.Visible;
                });

            bw.RunWorkerAsync();


            
        }
    }
}
*/