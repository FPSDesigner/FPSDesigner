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
using System.Windows.Media.Animation;

namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();

            userIdBox.Focus();
            loadingLogin.IsActive = false;
            textLoginIncorrect.Opacity = 0;
            imgLoginIncorrect.Opacity = 0;

            btnLogin.Click += btnLogin_Click;
        }

        void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Validate!");
            loadingLogin.IsActive = true;

            DoubleAnimation opacityAnim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
            textLoginIncorrect.BeginAnimation(OpacityProperty, opacityAnim);
            imgLoginIncorrect.BeginAnimation(OpacityProperty, opacityAnim);
        }
    }
}
