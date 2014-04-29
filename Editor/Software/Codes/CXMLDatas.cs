using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace Software.Codes
{
    public class ProjectData
    {
        public Properties Properties { get; set; }
    }

    #region "Node - Properties"
    // Properties
    public class Properties
    {
        public string Author { get; set; }
        public string LastEditionDate { get; set; }
        public string GameName { get; set; }
    }
    #endregion

}
