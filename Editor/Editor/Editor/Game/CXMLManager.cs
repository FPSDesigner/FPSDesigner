using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
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
                    CConsole.WriteLogs("Error encoutered while scanning " + xmlFile + " (" + e.Message + ")");
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

    [Serializable()]
    public class SerializableVector3
    {

        // Private state 

        public string Scale;


        public SerializableVector3()
        {
            //this.name = name;
        }
        public SerializableVector3(object name)
        {
            this.Scale = (string)name;
        }


        

        /*public float X;
        public float Y;
        public float Z;

        public Vector3 Vector3
        {
            get
            {
                return new Vector3(X, Y, Z);
            }
        }

        public SerializableVector3() { }
        public SerializableVector3(Vector3 vector)
        {
            float val;
            X = float.TryParse(vector.X.ToString(), out val) ? val : 0.0f;
            Y = float.TryParse(vector.Y.ToString(), out val) ? val : 0.0f;
            Z = float.TryParse(vector.Z.ToString(), out val) ? val : 0.0f;
        }*/
    }
}
