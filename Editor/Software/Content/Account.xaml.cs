using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Software.Content
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class Account : UserControl
    {
        public Account()
        {
            InitializeComponent();

            
            languageList.Items.Add(new ComboBoxItem { Content = "Français", Name="FR" });

            languageList.SelectedIndex = 0;
            languageList.Text = (string)(((ComboBoxItem)languageList.Items[0]).Content);
        }
    }
}
