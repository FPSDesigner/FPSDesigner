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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();

            GlobalVars.consoleElement = MainLogs;
            GlobalVars.consoleElementScroll = scrollConsole;

            GlobalVars.serverInstance = new Codes.CServer(GlobalVars.serverInfo.Properties.Port);

            Send.Click += Send_Click;
            Send.IsDefault = true;
        }

        void Send_Click(object sender, RoutedEventArgs e)
        {
            if (CommandLine.Text.Length > 0)
            {
                GlobalVars.AddNewMessage(CommandLine.Text);
                GlobalVars.serverInstance.HandleCommand(CommandLine.Text);
            }

            CommandLine.Text = "";
        }
    }
}
