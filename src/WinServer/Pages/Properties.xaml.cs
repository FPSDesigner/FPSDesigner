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

namespace WinServer.Pages
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class Properties : UserControl
    {
        public RoutedEventHandler PropertiesSet;

        public Properties()
        {
            InitializeComponent();

            tbHostname.Text = GlobalVars.serverInfo.Properties.HostName;
            tbPort.Text = GlobalVars.serverInfo.Properties.Port.ToString();
            tbPassword.Text = GlobalVars.serverInfo.Properties.Password;
            tbMaxplayer.Text = GlobalVars.serverInfo.Properties.MaxPlayers.ToString();

            btnValidate.Click += btnValidate_Click;
        }

        void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (tbHostname.Text != "" && tbPort.Text != "" && tbMaxplayer.Text != "")
            {
                try
                {
                    GlobalVars.serverInfo.Properties.HostName = tbHostname.Text;
                    GlobalVars.serverInfo.Properties.Port = Int32.Parse(tbPort.Text);
                    GlobalVars.serverInfo.Properties.Password = tbPassword.Text;
                    GlobalVars.serverInfo.Properties.MaxPlayers = Int32.Parse(tbMaxplayer.Text);

                    Codes.CXMLManager.serializeClass(GlobalVars.SettingsFile, GlobalVars.serverInfo);

                    PropertiesSet(this, null);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }
    }
}
