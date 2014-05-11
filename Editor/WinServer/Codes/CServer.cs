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
        private Socket socket;

        public List<CClient> clientList;

        enum SentData
        {
            Connection,
            Position
        }

        public CServer(int port)
        {
            clientList = new List<CClient>();

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            catch (Exception e)
            {
                throw e;
            }

            GlobalVars.AddNewMessage("Connected on port [b]" + port + "[/b]");
            GlobalVars.AddNewMessage("Waiting for players...");
        }

        public void Run()
        {
            Thread ThreadReceiveData = new Thread(new ThreadStart(ReceiveData));

            ThreadReceiveData.Start();
        }

        public void ReceiveData()
        {
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, GlobalVars.serverInfo.Properties.Port);
                EndPoint tmpRemote = (EndPoint)sender;

                // Client is not connected
                byte[] data = new byte[1024];
                int received = socket.ReceiveFrom(data, ref tmpRemote);

                foreach (CClient client in clientList)
                    if (client.Address == tmpRemote.ToString()) // Data from connected client
                    {
                        HandleDatas(data, received);
                        return;
                    }


                GlobalVars.AddNewMessage("Connection received from [b]" + tmpRemote.ToString() + "[/b]");
                if (data.Length > GlobalVars.ConnectionKey.Length + 2)
                {
                    int dataType = (int)data[0];
                    if (dataType == (int)SentData.Connection)
                    {
                        string key = Encoding.ASCII.GetString(data, 1, GlobalVars.ConnectionKey.Length);
                        if (key == GlobalVars.ConnectionKey)
                        {
                            string sentData = Encoding.ASCII.GetString(data, GlobalVars.ConnectionKey.Length + 1, received - GlobalVars.ConnectionKey.Length - 1);

                            clientList.Add(new CClient(sentData, tmpRemote));


                            GlobalVars.AddNewMessage("Connection accepted from [b]" + tmpRemote.ToString() + "[/b] ([i]" + sentData + "[/i])");
                            
                        }
                    }
                }
            }
        }

        public void HandleDatas(byte[] datas, int received)
        {
            if (datas.Length > 1)
            {
                SentData DataType = (SentData)datas[0];

                switch (DataType)
                {
                    case SentData.Position:
                        break;
                }
            }
        }
    }
}
