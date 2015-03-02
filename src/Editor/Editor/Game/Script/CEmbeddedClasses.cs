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

namespace Engine.Game.Script.Embedded
{
    // Convenction: C + type + Script + name. Ex: C2DScriptRectangle.

    // 2D Related
    public class C2DScriptRectangle
    {
        
        public Rectangle? SourceRectangle;
        public Texture2D Texture;
        public Color Color;
        public float Rotation;
        public Vector2 Origin;

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

        private Rectangle _rectangle;
        public Rectangle Rectangle
        {
            get
            {
                return _rectangle;
            }
            set
            {
                _rectangle = value;
                if (_rectangle.Width == 0 || _rectangle.Height == 0)
                    _rectangle = new Rectangle(_rectangle.X, _rectangle.Y, Texture.Width, Texture.Height);
            }
        }


        public C2DScriptRectangle(Rectangle rect, Rectangle? sourceRect = null, Color? color = null, Texture2D texture = null, bool active = true, int order = 1, float rot = 0)
        {
            Color = color ?? Color.White;
            Rotation = rot;
            
            if (texture != null)
                Texture = texture;
            else
            {
                Texture = new Texture2D(Display2D.C2DEffect._graphicsDevice, 1, 1, false, SurfaceFormat.Color);
                Texture.SetData<Color>(new Color[] { Color });
            }

            isActive = active;
            drawOrder = order;
            Rectangle = rect;
            SourceRectangle = sourceRect;
            Origin = Vector2.Zero;
        }
    }

    public class C2DText
    {

        public string Text;
        public SpriteFont Font;
        public Vector2 Pos;
        public Color Color;
        public float Scale;
        public float Rotation;

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
                Display2D.C2DEffect.ScriptableText = Display2D.C2DEffect.ScriptableText.OrderBy(ord => ord.drawOrder).ToList();
            }
        }

        public C2DText(string text, string font, float x, float y, float scale, Color? color = null, float rot = 0f)
        {
            Font = Display2D.C2DEffect._content.Load<SpriteFont>(font);
            Text = text;
            Pos = new Vector2(x, y);
            Color = color ?? Color.White;
            Scale = scale;
            Rotation = rot;
            isActive = false;
        }
    }

    // XML Manager
    public class XMLManager
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
