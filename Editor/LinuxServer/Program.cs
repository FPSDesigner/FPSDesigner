using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinuxServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GlobalVars.serverInfo = defaultServerInfo();

            Console.WriteLine("Welcome on FPSDesigner server!");
            Console.WriteLine("Hostname:");
            GlobalVars.serverInfo.Properties.HostName = Console.ReadLine();

            Console.WriteLine("Password:");
            GlobalVars.serverInfo.Properties.Password = Console.ReadLine();

            int Port, MaxPlayers;

            Console.WriteLine("Port:");
            while (!Int32.TryParse(Console.ReadLine(), out Port)) ;
            GlobalVars.serverInfo.Properties.Port = Port;

            Console.WriteLine("Maxplayers:");
            while (!Int32.TryParse(Console.ReadLine(), out MaxPlayers)) ;
            GlobalVars.serverInfo.Properties.MaxPlayers = MaxPlayers;

            GlobalVars.serverInstance = new CServer(GlobalVars.serverInfo.Properties.Port);

            string command = "";
            while ((command = Console.ReadLine()) != "quit")
            {
                Console.WriteLine("> " + command);
                GlobalVars.serverInstance.HandleCommand(command);
            }
        }

        public static ServerInfo defaultServerInfo()
        {
            return new ServerInfo
            {
                Properties = new Properties
                {
                    HostName = "New Server",
                    MaxPlayers = 64,
                    Port = 7777,
                    Password = "",
                }
            };
        }
    }

    static class GlobalVars
    {
        public static CServer serverInstance;
        public static ServerInfo serverInfo;

        public static void AddNewMessage(string msg)
        {
            string[] BBCode = new string[] { "[b]", "[/b]" };

            for (int i = 0; i < BBCode.Length; i++)
                msg = msg.Replace(BBCode[i], "");

            Console.WriteLine(msg);
        }
    }
}
