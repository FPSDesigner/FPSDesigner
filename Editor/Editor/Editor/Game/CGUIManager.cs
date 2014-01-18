using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using LearningMahesh.DynamicIOStream.Xml;

namespace Editor.Game
{
    class CGUIManager
    {
        private dynamic GUIXmlReader;
        private string[] GUIList;

        public CGUIManager()
        {
        }


        public void LoadGUIFile(string guiFile)
        {
            GUIXmlReader = DynamicXmlStream.Load(new FileStream(guiFile, FileMode.Open));

            // Load every GUI Names into GUIList array
            GUIList = new string[GUIXmlReader.GUIList.GUI.Count];
            if (GUIXmlReader.GUIList.GUI.Count > 0)
            {
                for (int i = 0; i < GUIXmlReader.GUIList.GUI.Count; i++)
                    GUIList[i] = GUIXmlReader.GUIList.GUI[i].Attribute(XName.Get("name")).Value;
            }
        }

        public bool DoesGuiExists(string guiName)
        {
            for (int i = 0; i < GUIList.Length; i++)
            {
                if (GUIList[i] == guiName)
                    return true;
            }
            return false;
        }

        public int GetGUIId(string guiName)
        {
            for (int i = 0; i < GUIList.Length; i++)
            {
                if (GUIList[i] == guiName)
                    return i;
            }
            return 0;
        }

        public void LoadGUI(int GuiId)
        {

        }

    }
}
