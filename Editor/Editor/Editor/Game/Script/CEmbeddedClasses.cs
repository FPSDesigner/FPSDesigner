using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Editor.Game.Script.Embedded
{
    // Convenction: C + type + Script + name. Ex: C2DScriptRectangle.

    // 2D Related
    class C2DScriptRectangle
    {
        public Rectangle Rectangle;
        public Rectangle? SourceRectangle;
        public Texture2D Texture;
        public Color Color;

        public bool isActive;

        private int _drawOrder;
        public int drawOrder
        {
            get
            {
                return _drawOrder;
            }
            set
            {
                _drawOrder = value;
                Display2D.C2DEffect.ScriptableRectangle = Display2D.C2DEffect.ScriptableRectangle.OrderBy(ord => ord.drawOrder).ToList();
            }
        }

        public C2DScriptRectangle(Rectangle rect, Rectangle? sourceRect = null, Color? color = null, Texture2D texture = null, bool active = true, int order = 1)
        {
            Rectangle = rect;
            SourceRectangle = sourceRect;
            Color = color ?? Color.White;
            isActive = active;
            drawOrder = order;

            if (texture != null)
                Texture = texture;
            else
            {
                Texture = new Texture2D(Display2D.C2DEffect._graphicsDevice, 1, 1, false, SurfaceFormat.Color);
                Texture.SetData<Color>(new Color[] { Color });
            }
        }
    }

    // XML Manager
    class XMLManager
    {
        public string fileName;

        private XDocument handler;


        public XMLManager(string file)
        {
            fileName = file;
            handler = XDocument.Load(file);
        }

        #region "Methods"
        public int Count(string elementName)
        {
            return handler.XPathSelectElements(elementName).Count();
        }

        public int Count(XElement parent, string elementName = "")
        {
            if (elementName == "")
                return parent.Elements().Count();
            else
                return parent.XPathSelectElements(elementName).Count();
        }

        public string GetAttribute(string elementName, string attribute, int elementId = 0)
        {
            XElement[] eltId = handler.XPathSelectElements(elementName).ToArray();
            if (elementId < eltId.Length)
                return eltId[elementId].Attribute(attribute).Value;
            else
                return "";
        }

        public XElement[] GetNodes(string elementName)
        {
             return handler.XPathSelectElements(elementName).ToArray();
        }

        public XElement GetNode(string child)
        {
            return handler.XPathSelectElement(child);
        }

        public XElement[] GetChildren(XElement parent)
        {
            return parent.Elements().ToArray();
        }

        public XElement GetChild(XElement parent, string child)
        {
            return parent.Element(child);
        }

        public bool HasNodeChilds(XElement node)
        {
            return node.HasElements;
        }

        public string GetElementValue(XElement element, string val = "Value")
        {
            if (val == "Value")
                return element.Value;
            else
                return element.Name.ToString();
        }
        #endregion
    }

}
