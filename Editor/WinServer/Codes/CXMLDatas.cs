using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace WinServer.Codes
{
    public class ServerInfo
    {
        public Properties Properties { get; set; }
    }

    #region "Node - Properties"
    // Properties
    public class Properties
    {
        public string HostName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public int MaxPlayers { get; set; }
    }
    #endregion

}
