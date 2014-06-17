using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Engine.Game
{
    class CMultiplayer
    {
        private string userName;
        private UdpClient ClientHandler;
        private IPEndPoint EndPoint;
        public static string ConnectionKey = "LA45T6";


        public CMultiplayer(string username)
        {
            userName = username;
            ClientHandler = new UdpClient();
        }

        public void Connect(string host, int port)
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            ClientHandler.Connect(EndPoint);

            SendMessage("req|" + ConnectionKey + "|" + port + "|");
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
                string receivedData = GetString(ClientHandler.Receive(ref EndPoint));

                CConsole.addMessage("Receive data from " + EndPoint.ToString() + " : " + receivedData);
            }
        }



        public void SendMessage(string msg)
        {
            byte[] bytes = GetBytes(msg);
            ClientHandler.Send(bytes, bytes.Length);
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
