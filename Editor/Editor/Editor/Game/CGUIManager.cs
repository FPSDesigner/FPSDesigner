using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using LearningMahesh.DynamicIOStream.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Editor.Game
{
    class CGUIManager
    {
        private dynamic GUIXmlReader;
        private string[] GUIList;

        private bool _drawGui;
        private int _drawGuiId;

        /* Elements to draw */
        private List<el_Texture2D>[] _Textures2D;

        #region "Element Classes"
        class el_Texture2D
        {
            public el_Texture2D(Texture2D texture, Vector2 pos, Rectangle? sourceRectangle, Color color, Vector2 origin, int hoverType, dynamic link, bool hasOver = false, float rotation = 0, float scale = 1f, SpriteEffects effects = SpriteEffects.None)
            {
                Texture2D = texture;
                Position = pos;
                SourceRectangle = sourceRectangle;
                Color = color;
                Origin = origin;
                Rotation = rotation;
                Scale = scale;
                Effects = effects;
                HasOverAction = hasOver;
                XMLLink = link;
                IsHovered = false;
                HoverType = hoverType;
            }

            public Texture2D Texture2D;
            public Vector2 Position;
            public Rectangle? SourceRectangle;
            public Color Color;
            public Vector2 Origin;
            public float Rotation;
            public float Scale;
            public SpriteEffects Effects;
            public bool HasOverAction;
            public dynamic XMLLink;
            public bool IsHovered;
            public int HoverType;
        }
        #endregion

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

            _Textures2D = new List<el_Texture2D>[GUIXmlReader.GUIList.GUI.Count];
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

        public void LoadGUI(int GuiId, ContentManager Content, GraphicsDevice Graphics)
        {
            dynamic GUIData = GUIXmlReader.GUIList.GUI[GuiId];

            // Load Background

            foreach (dynamic order in (GUIData.Elements.Element as DynamicXmlStream).AsDynamicEnumerable().OrderBy(ord => ord.DrawOrder.Value))
            {
                switch ((string)order.Type)
                {
                    case "Image":
                        // Origin
                        int posX = 0, posY = 0;
                        Vector2 origin = Vector2.Zero;
                        if (((XElement)order).Element("Origin") != null && Int32.TryParse(order.Origin.X, out posX) && Int32.TryParse(order.Origin.Y, out posY))
                            origin = new Vector2(posX, posY);

                        // Rectangle
                        Nullable<Rectangle> rect = null;
                        if (((XElement)order).Element("Source") != null && (string)order.Source != "null")
                            rect = new Rectangle((int)order.Source.X, (int)order.Source.Y, (int)order.Source.W, (int)order.Source.H);

                        // Action
                        bool hasOver = (((XElement)order).Element("Hover") != null && ((XElement)order.Hover).Element("Action") != null);

                        int action = 0;
                        if(hasOver)
                        {
                            if (((XElement)order.Hover.Action).Element("Source") != null)
                                action = 1;
                        }
                        _Textures2D[GuiId].Add(new el_Texture2D(Content.Load<Texture2D>(order.URI),
                            new Vector2((int)order.Position.X, (int)order.Position.Y),
                            rect,
                            (((XElement)order).Element("Color") != null) ? new Color((int)order.Color.R, (int)order.Color.G, (int)order.Color.b) : Color.White,
                            origin,
                            action,
                            order,
                            hasOver,
                            ((XElement)order).Element("Rotation") == null ? 0f : (float)order.Rotation,
                            ((XElement)order).Element("Scale") == null ? 0f : (float)order.Scale));

                        break;
                }
            }

            _drawGui = true;
            _drawGuiId = GuiId;

        }

        public void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState, MouseState oldMouseState)
        {
            if (_drawGui)
            {
                for (int i = 0; i < _Textures2D[_drawGuiId].Count; i++)
                {
                    if (_Textures2D[_drawGuiId][i].HasOverAction)
                    {
                        if (mouseState.X >= _Textures2D[_drawGuiId][i].Position.X && mouseState.X <= (_Textures2D[_drawGuiId][i].Position.X + ((Rectangle)_Textures2D[_drawGuiId][i].SourceRectangle).Width) &&
                            mouseState.Y >= _Textures2D[_drawGuiId][i].Position.Y && mouseState.Y <= (_Textures2D[_drawGuiId][i].Position.Y + ((Rectangle)_Textures2D[_drawGuiId][i].SourceRectangle).Height))
                        {
                            // Is hover
                            if (!_Textures2D[_drawGuiId][i].IsHovered)
                            {
                                _Textures2D[_drawGuiId][i].IsHovered = true;
                                switch (_Textures2D[_drawGuiId][i].HoverType)
                                {
                                    case 1: // Change Source
                                        _Textures2D[_drawGuiId][i].SourceRectangle = new Rectangle((int)_Textures2D[_drawGuiId][i].XMLLink.Hover.Action.Source.X, (int)_Textures2D[_drawGuiId][i].XMLLink.Hover.Action.Source.Y, (int)_Textures2D[_drawGuiId][i].XMLLink.Hover.Action.Source.W, (int)_Textures2D[_drawGuiId][i].XMLLink.Hover.Action.Source.H);
                                        break;
                                }
                            }
                        }
                        else if (_Textures2D[_drawGuiId][i].IsHovered)
                        {
                            // Changed from hovered to not
                            _Textures2D[_drawGuiId][i].IsHovered = false;
                            switch (_Textures2D[_drawGuiId][i].HoverType)
                            {
                                case 1: // Change Source
                                    _Textures2D[_drawGuiId][i].SourceRectangle = new Rectangle((int)_Textures2D[_drawGuiId][i].XMLLink.Source.X, (int)_Textures2D[_drawGuiId][i].XMLLink.Source.Y, (int)_Textures2D[_drawGuiId][i].XMLLink.Source.W, (int)_Textures2D[_drawGuiId][i].XMLLink.Source.H);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {
            if (_drawGui)
            {
                for (int i = 0; i < _Textures2D[_drawGuiId].Count; i++)
                {
                    spritebatch.Draw(_Textures2D[_drawGuiId][i].Texture2D, _Textures2D[_drawGuiId][i].Position, _Textures2D[_drawGuiId][i].SourceRectangle, _Textures2D[_drawGuiId][i].Color,
                        _Textures2D[_drawGuiId][i].Rotation, _Textures2D[_drawGuiId][i].Origin, _Textures2D[_drawGuiId][i].Scale, _Textures2D[_drawGuiId][i].Effects, 1);
                }
            }
        }

    }
}
