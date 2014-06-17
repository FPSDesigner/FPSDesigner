using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace WinServer.Codes
{
    class CServer
    {
        private UdpClient ServerHandler;
        private int Port;

        public List<CPlayer> clientList;

        public CServer(int port)
        {
            Port = port;
            clientList = new List<CPlayer>();

            ServerHandler = new UdpClient(port);

            Run();

            GlobalVars.AddNewMessage("Connected on port [b]" + port + "[/b]");
            GlobalVars.AddNewMessage("Waiting for players...");
        }

        public void Run()
        {
            Thread ThreadListen = new Thread(new ThreadStart(Listen));
            ThreadListen.IsBackground = true;
            ThreadListen.Start();
        }

        public void Listen()
        {
            while (true)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port);
                string data = GetString(ServerHandler.Receive(ref remoteEP));

                GlobalVars.AddNewMessage("Received data from [b]" + remoteEP.ToString() + "[/b] ...");
                GlobalVars.AddNewMessage("Message: [b]" + data + "[/b]");

                SendMessage("Welcome|My Server", remoteEP);
            }
        }



        public void SendMessage(string msg, IPEndPoint remoteEp)
        {
            byte[] bytes = GetBytes(msg);
            ServerHandler.Send(bytes, bytes.Length, remoteEp);
        }

        public byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

    }
}
