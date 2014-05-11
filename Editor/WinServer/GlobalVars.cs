using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Automation.Peers;
using System.Windows.Media.Imaging;

using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using FirstFloor.ModernUI.Presentation;

namespace WinServer
{
    static class GlobalVars
    {
        public static List<string> consoleListMessage = new List<string>();
        public static BBCodeBlock consoleElement;
        public static string SettingsFile = "serverSettings.xml";
        public static string ConnectionKey = "LA45T6";
        public static Codes.ServerInfo serverInfo;
        public static BitmapFrame SoftwareIcon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/Icon.ico", UriKind.RelativeOrAbsolute));

        public static void AddNewMessage(string msg)
        {
            consoleListMessage.Add(msg);

            if (consoleElement != null)
            {
                if (consoleElement.Dispatcher.CheckAccess())
                    consoleElement.BBCode += "> " + msg.Replace("\n", "") + "\n";
                else
                    consoleElement.Dispatcher.Invoke((Action)(() => consoleElement.BBCode += "> " + msg.Replace("\n", "") + "\n"));
            }
        }

        public static Codes.ServerInfo defaultServerInfo()
        {
            return new Codes.ServerInfo
            {
                Properties = new Codes.Properties
                {
                    HostName = "New Server",
                    MaxPlayers = 64,
                    Port = 7777,
                }
            };
        }

    }
}
