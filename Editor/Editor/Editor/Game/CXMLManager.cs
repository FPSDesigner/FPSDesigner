using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;


namespace Editor.Game
{
    class CXMLManager
    {

        public TypeClass deserializeClass<TypeClass>(string xmlFile) where TypeClass : class
        {
            TypeClass returnClass;

            XmlSerializer xs = new XmlSerializer(typeof(TypeClass));
            using (StreamReader rd = new StreamReader(xmlFile))
            {
                returnClass = xs.Deserialize(rd) as TypeClass;
            }
            return returnClass;
        }

        public void serializeClass(string xmlFile, Type classToSerialize)
        {
            XmlSerializer xs = new XmlSerializer(classToSerialize.GetType());

            using (StreamWriter wr = new StreamWriter(xmlFile))
            {
                xs.Serialize(wr, classToSerialize);
            }
        }

    }
}
