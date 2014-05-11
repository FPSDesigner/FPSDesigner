using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace WinServer.Codes
{
    class CClient
    {
        public string userName { get; private set; }

        public EndPoint endPoint;
        public string Address;

        public CClient(string name, EndPoint ep)
        {
            this.userName = name;
            this.endPoint = ep;
            this.Address = ep.ToString();
        }
    }
}
