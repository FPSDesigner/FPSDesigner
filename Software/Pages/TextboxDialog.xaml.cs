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

namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for TextboxDialog.xaml
    /// </summary>
    public partial class TextboxDialog : UserControl
    {
        public event RoutedEventHandler EnteredText;

        public TextboxDialog()
        {
            InitializeComponent();

            saveButton.Click += saveButton_Click;
        }

        void saveButton_Click(object sender, RoutedEventArgs e)
        {
            string text = fileNameBox.Text;
            if (text != "" && !System.IO.File.Exists("Scripts/" + text))
            {
                EnteredText(fileNameBox, null);
            }
        }
    }
}
