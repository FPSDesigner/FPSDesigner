using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Engine.Game
{
    class CMultiplayer
    {
        private string userName;
        private UdpClient ClientHandler;
        private IPEndPoint EndPoint;
        private string ConnectionKey = "LA45T6";
        private Thread ThreadListen;
        private Timer timerSendInfo;

        private bool isConnected = false;
        private int tickRate = 128;

        private Dictionary<int, CPlayer> listPlayers;

        private enum SentData
        {
            Int,
            Float,
            Vector3,
        };


        public CMultiplayer(string username)
        {
            userName = username;
            tickRate = 1000 / tickRate;
            ClientHandler = new UdpClient();
            listPlayers = new Dictionary<int, CPlayer>();

            /*ClientHandler.Client.SendTimeout = 5000;
            ClientHandler.Client.ReceiveTimeout = 5000;*/
        }

        public bool Connect(string host, int port)
        {
            IPAddress IP;
            if (IPAddress.TryParse(host, out IP))
            {
                EndPoint = new IPEndPoint(IP, port);
                ClientHandler.Connect(EndPoint);

                SendMessage("req|" + ConnectionKey + "|" + port + "|" + userName + "|" + FormatDataToSend(CConsole._Camera._cameraPos));

                CConsole.addMessage("Connecting to server...");
                isConnected = true;
                return true;
            }
            return false;
        }

        public void Run()
        {
            ThreadListen = new Thread(new ThreadStart(Listen));
            ThreadListen.IsBackground = true;
            ThreadListen.Start();
        }

        public void Listen()
        {
            while (true)
            {
                string receivedData = GetString(ClientHandler.Receive(ref EndPoint));
                string[] datas = receivedData.Split('|');

                if (!isConnected)
                    return;

                if (receivedData.StartsWith("OK|")) // Connection success
                {
                    CConsole.addMessage("Connection succeed to " + EndPoint.ToString() + " (" + receivedData.Replace("OK|", "") + ")!");
                    timerSendInfo = new Timer(new TimerCallback(SendServerInformations), null, 1000, tickRate);
                }
                else if (receivedData.StartsWith("JOIN|")) // New player
                {
                    int id = Int32.Parse(datas[1]);

                    if (listPlayers.ContainsKey(id))
                        PlayerDisconnected(id);

                    CPlayer newPlayer = new CPlayer(id, datas[2], (Vector3)ExtractDataFromString(datas[3], SentData.Vector3));
                    listPlayers.Add(id, newPlayer);
                }
                else if (receivedData.StartsWith("SETINFO|")) // New player
                {
                    int ID;
                    if(Int32.TryParse(datas[1], out ID) && listPlayers.ContainsKey(ID))
                    {
                        listPlayers[ID].SetNewPos((Vector3)ExtractDataFromString(datas[2], SentData.Vector3), (Vector3)ExtractDataFromString(datas[3], SentData.Vector3));
                    }
                }
                else if (receivedData.StartsWith("QUIT|")) // Server message
                {
                    int ID;
                    if (Int32.TryParse(datas[1], out ID))
                        PlayerDisconnected(ID);
                }
                else if (receivedData.StartsWith("ECHO|")) // Server message
                {
                    CConsole.addMessage("Server: "+receivedData.Replace("ECHO|", ""));
                }


            }
        }

        private void SendServerInformations(Object param)
        {
            string pitch = "0";
            if (false) // Is aiming here, we send pitch
                pitch = FormatDataToSend(CConsole._Camera._pitch);

            SendMessage("INFO|"+FormatDataToSend(CConsole._Camera._cameraPos) + "|" + FormatDataToSend(CConsole._Camera._yaw + MathHelper.Pi) + "/" + pitch + "/0|");
        }

        public void PlayerDisconnected(int id)
        {
            if (listPlayers.ContainsKey(id))
            {
                listPlayers[id].Disconnect();
                listPlayers.Remove(id);
            }
        }


        public void SendMessage(string msg)
        {
            byte[] bytes = GetBytes(msg);
            ClientHandler.Send(bytes, bytes.Length);
        }

        public byte[] GetBytes(string str)
        {
            /*byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;*/
            return System.Text.Encoding.ASCII.GetBytes(str);
        }

        public string GetString(byte[] bytes)
        {
            /*char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);*/
            return System.Text.Encoding.ASCII.GetString(bytes);
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
            return null;
        }

        public void Disconnect()
        {
            isConnected = false;

            SendMessage("QUIT");

            foreach (KeyValuePair<int, CPlayer> pl in listPlayers)
                pl.Value.Disconnect();

            timerSendInfo.Dispose();

            try
            {
                ThreadListen.Abort();
            }
            catch (Exception e) { }
        }
    }
}
