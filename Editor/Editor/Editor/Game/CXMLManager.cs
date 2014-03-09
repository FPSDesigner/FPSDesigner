using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;


namespace Engine.Game
{
    /// <summary>
    /// XML Serializer & Desierializer class
    /// </summary>
    class CXMLManager
    {
        /// <summary>
        /// Desierialize a file into a class
        /// </summary>
        /// <typeparam name="TypeClass">The type of the class into we want to desierialize the file</typeparam>
        /// <param name="xmlFile">The XML file</param>
        /// <returns>The instance of the class</returns>
        public TypeClass deserializeClass<TypeClass>(string xmlFile) where TypeClass : class
        {
            TypeClass returnClass;

            XmlSerializer xs = new XmlSerializer(typeof(TypeClass));
            using (StreamReader rd = new StreamReader(xmlFile))
            {
                try
                {
                    returnClass = xs.Deserialize(rd) as TypeClass;
                }
                catch (Exception e)
                {
                    CConsole.WriteLogs("Error encoutered while scanning "+xmlFile+" ("+e.Message+")");
                    throw e;
                }
            }
            return returnClass;
        }

        /// <summary>
        /// Serialize a class into a XML file
        /// </summary>
        /// <param name="xmlFile">The XML file</param>
        /// <param name="classToSerialize">The class we want to serialize</param>
        public void serializeClass(string xmlFile, Object classToSerialize)
        {
            XmlSerializer xs = new XmlSerializer(classToSerialize.GetType());

            using (StreamWriter wr = new StreamWriter(xmlFile))
            {
                xs.Serialize(wr, classToSerialize);
            }
        }

    }
}
