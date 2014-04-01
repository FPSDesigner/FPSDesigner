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

namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public event RoutedEventHandler LoginSucceed;

        public Login()
        {
            InitializeComponent();

            userIdBox.Focus();
            loadingLogin.IsActive = false;
            textLoginIncorrect.Opacity = 0;
            imgLoginIncorrect.Opacity = 0;

            btnLogin.Click += btnLogin_Click;
        }

        async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            loadingLogin.IsActive = true;

            await Task.Delay(1000);

            string username = userIdBox.Text;
            string password = passwordBox.Password;

            bool loginSuccess = false;

            if (username.Length > 0 && password.Length > 0)
            {
                password = GetSHA256Hash(new SHA256Managed(), GetMd5Hash(MD5.Create(), password));

                // Check authentification
                using (var wb = new WebClient())
                {
                    var data = new System.Collections.Specialized.NameValueCollection();
                    data["user"] = username;
                    data["pass"] = password;

                    AuthAnswer answer = null;
                    try
                    {
                        string response = System.Text.Encoding.UTF8.GetString(wb.UploadValues("http://www.fpsdesigner.com/sft/login.php", "POST", data));
                        answer = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<AuthAnswer>(response);

                        if (answer.ans == "OK")
                            loginSuccess = true;
                    }
                    catch (Exception exc)
                    {
                        ModernDialog.ShowMessage("An unexpected error occured.\nPlease check your internet connection to log in.", "Can't log in", MessageBoxButton.OK);
                        Console.WriteLine("Login error occured: "+exc.GetType());
                    }
                }
            }

            if (!loginSuccess)
            {
                DoubleAnimation opacityAnim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
                textLoginIncorrect.BeginAnimation(OpacityProperty, opacityAnim);
                imgLoginIncorrect.BeginAnimation(OpacityProperty, opacityAnim);
                loadingLogin.IsActive = false;
            }
            else
            {
                // Login succeed
                LoginSucceed(this, null);
            }
        }

        #region Cryptography
        string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }

        string GetSHA256Hash(SHA256Managed crypt, string password)
        {
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(password), 0, Encoding.ASCII.GetByteCount(password));

            foreach (byte bit in crypto)
                hash += bit.ToString("x2");

            return hash;
        }
        #endregion
    }

    /// <summary>
    /// Json answer for authentification
    /// </summary>
    public class AuthAnswer
    {
        public string ans { get; set; }
    }
}
