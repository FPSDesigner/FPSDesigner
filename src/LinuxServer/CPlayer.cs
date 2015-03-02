using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace LinuxServer
{
    class CPlayer
    {
        public string userName;

        public IPEndPoint endPoint;
        public string Address;
        public int ID;
        public DateTime lastPacket;
        private Timer timerSendInfo;

        public CPlayer(int plId, string name, IPEndPoint ep)
        {
            this.ID = plId;
            this.userName = name;
            this.endPoint = ep;
            this.Address = ep.ToString();
            this.lastPacket = DateTime.Now;

            GlobalVars.serverInstance.SendMessage("OK|" + GlobalVars.serverInfo.Properties.HostName, endPoint);

            timerSendInfo = new Timer(new TimerCallback(CheckTimeout), null, 5000, 5000);
        }

        public void CheckTimeout(Object param)
        {
            if (lastPacket.AddSeconds(10) <= DateTime.Now) // Timeout 15 secs
                GlobalVars.serverInstance.DisconnectPlayer(this);
        }

        public void Disconnect()
        {
            timerSendInfo.Dispose();
        }
    }
}
