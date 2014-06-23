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
        private string ConnectionKey = "LA45T6";

        public List<CPlayer> playerList;

        public enum SentData
        {
            Int,
            Float,
            Vector3,
            Bool,
        };

        public CServer(int port)
        {
            Port = port;
            playerList = new List<CPlayer>();

            ServerHandler = new UdpClient(port);

            Run();

            GlobalVars.AddNewMessage("Connected on port [b]" + port + "[/b]");
            GlobalVars.AddNewMessage("* Hostname: [b]" + GlobalVars.serverInfo.Properties.HostName + "[/b]");
            GlobalVars.AddNewMessage("* Players: [b]0/" + GlobalVars.serverInfo.Properties.MaxPlayers + "[/b]");
            GlobalVars.AddNewMessage("Server started! Waiting for players...");
        }

        public void Run()
        {
            Thread ThreadListen = new Thread(new ThreadStart(Listen));
            ThreadListen.IsBackground = true;
            ThreadListen.Start();
        }

        private void Listen()
        {
            while (true)
            {

                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, Port);
                string data = GetString(ServerHandler.Receive(ref remoteEP));
                string AddressEP = remoteEP.ToString();
                string[] info = data.Split('|');
                CPlayer PlayerWriting = null;

                foreach (CPlayer pl in playerList)
                    if (AddressEP == pl.Address)
                        PlayerWriting = pl;

                if (PlayerWriting == null && info.Length > 3)
                {

                    if (info[0] == "req" && info[1] == ConnectionKey && info[2] == Port.ToString())
                    {
                        CPlayer newPlayer = new CPlayer(GetNewUniqueID(), info[3], remoteEP);
                        playerList.Add(newPlayer);
                        GlobalVars.AddNewMessage("New player (" + info[3] + ", ID: " + newPlayer.ID + ") successfuly connected!");

                        foreach (CPlayer pl in playerList)
                        {
                            if (pl != newPlayer)
                            {
                                SendMessage("JOIN|" + newPlayer.ID + "|" + info[3] + "|" + info[4], pl.endPoint);
                                SendMessage("JOIN|" + pl.ID + "|" + pl.userName + "|0/0/0", newPlayer.endPoint);
                            }
                        }
                    }
                    else
                        GlobalVars.AddNewMessage("Incorrect connection request: [b]" + data + "[/b]");
                }
                else if (PlayerWriting != null)
                {
                    PlayerWriting.lastPacket = DateTime.Now;

                    if (info.Length > 1 && info[0] == "INFO")
                    {
                        foreach (CPlayer pl in playerList)
                            if (pl != PlayerWriting)
                                SendMessage("SETINFO|" + PlayerWriting.ID + "|" + info[1] + "|" + info[2] + "|" + info[3] + "|" + info[4] + "|" + info[5] + "|" + info[6] + "|" + info[7] + "|" + info[8] + "|" + info[10] + "|" + info[11], pl.endPoint);
                    }
                    else if (data == "QUIT")
                    {
                        GlobalVars.AddNewMessage(PlayerWriting.userName + " disconnected.");
                        DisconnectPlayer(PlayerWriting);
                    }
                    else if (info.Length > 3 && info[0] == "HIT")
                    {
                        int id = (int)ExtractDataFromString(info[1], SentData.Int);
                        foreach (CPlayer pl in playerList)
                            if (pl.ID != id)
                            {
                                SendMessage("PLGOTHIT|" + id + "|" + info[2] + "|" + info[3], pl.endPoint);
                                return;
                            }
                            else
                            {
                                SendMessage("UGOTHIT|" + PlayerWriting.ID + "|" + info[2] + "|" + info[3], pl.endPoint);
                                return;
                            }
                    }
                }
            }
        }

        public void SendMessage(string msg, IPEndPoint remoteEp)
        {
            byte[] bytes = GetBytes(msg);
            ServerHandler.Send(bytes, bytes.Length, remoteEp);
        }

        public void HandleCommand(string command)
        {
            string[] cmd = command.Split(' ');

            if (cmd.Length > 1)
            {
                if (cmd[0] == "echo" || cmd[0] == "say")
                {
                    foreach (CPlayer pl in playerList)
                        SendMessage("ECHO|" + command.Replace("echo ", ""), pl.endPoint);
                }
            }
        }

        public void DisconnectPlayer(CPlayer player)
        {
            player.Disconnect();

            playerList.Remove(player);

            foreach (CPlayer pl in playerList)
                SendMessage("QUIT|" + player.ID, pl.endPoint);
        }

        private byte[] GetBytes(string str)
        {
            /*byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;*/
            return System.Text.Encoding.ASCII.GetBytes(str);
        }

        private string GetString(byte[] bytes)
        {
            /*char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);*/
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        private int GetNewUniqueID()
        {
            int id = -1;
            while (true)
            {
                id++;
                bool found = false;
                foreach (CPlayer pl in playerList)
                    if (pl.ID == id)
                    {
                        found = true;
                        break;
                    }

                if (!found)
                    break;
            }
            return id;
        }

        public string FormatDataToSend(object data)
        {
            if (data is float)
                return Math.Round((float)data, 3).ToString();
            else if (data is Vector3)
            {
                Vector3 ret = (Vector3)data;
                return FormatDataToSend(ret.X) + "/" + FormatDataToSend(ret.Y) + "/" + FormatDataToSend(ret.Z);
            }
            return data.ToString();
        }

        private object ExtractDataFromString(string msg, SentData type)
        {
            if (type == SentData.Vector3)
            {
                if (msg.Count(f => f == '/') == 2)
                {
                    string[] extracted = msg.Split('/');
                    return new Vector3((float)ExtractDataFromString(extracted[0], SentData.Float), (float)ExtractDataFromString(extracted[1], SentData.Float), (float)ExtractDataFromString(extracted[2], SentData.Float));
                }
            }
            else if (type == SentData.Float)
            {
                float ret;
                if (float.TryParse(msg.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out ret))
                    return ret;
                else
                    return 0f;
            }
            else if (type == SentData.Bool)
            {
                return (msg == "1" || msg == "true");
            }
            else if (type == SentData.Int)
            {
                int ret;
                if (Int32.TryParse(msg, out ret))
                    return ret;
                else
                    return 0;
            }
            return null;
        }

    }

    class Vector3
    {
        public float X, Y, Z;

        private void Initialize(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public Vector3(float val)
        {
            this.Initialize(val, val, val);
        }

        public Vector3(float X, float Y, float Z)
        {
            Initialize(X, Y, Z);
        }
    }
}
