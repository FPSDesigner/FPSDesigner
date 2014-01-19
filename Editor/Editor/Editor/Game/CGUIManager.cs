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

        private CGameStateManager _gameStateManager;

        /* Elements to draw */
        private List<el_Texture2D>[] _Textures2D;
        private el_Cursor[] _Cursor;

        #region "Element Classes"
        // Texture 2D
        class el_Texture2D
        {
            public el_Texture2D(Texture2D texture, Vector2 pos, Rectangle? sourceRectangle, Color color, Vector2 origin, string hoverType, string clickAction, dynamic link, bool hasOver = false, float rotation = 0, float scale = 1f, SpriteEffects effects = SpriteEffects.None)
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
                ClickAction = clickAction;
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
            public string HoverType;
            public string ClickAction;
        }

        // Cursor
        class el_Cursor
        {
            public el_Cursor(Texture2D texture, Vector2 source, float scale = 0f)
            {
                Texture2D = texture;
                Source = source;
                Scale = scale;
                MousePos = Vector2.Zero;
            }

            public Texture2D Texture2D;
            public Vector2 Source;
            public float Scale;
            public Vector2 MousePos;
        }
        #endregion

        public CGUIManager(CGameStateManager gsManager)
        {
            _gameStateManager = gsManager;
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
            _Cursor = new el_Cursor[GUIXmlReader.GUIList.GUI.Count];
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

        public bool IsGUILoaded(string guiName)
        {
            return _Textures2D[GetGUIId(guiName)] != null;
        }

        public void LoadGUI(int GuiId, ContentManager Content, GraphicsDevice Graphics)
        {
            dynamic GUIData = GUIXmlReader.GUIList.GUI[GuiId];

            // Load Background

            _Textures2D[GuiId] = new List<el_Texture2D>();
            foreach (dynamic order in (GUIData.Elements.Element as DynamicXmlStream).AsDynamicEnumerable().OrderBy(ord => ord.DrawOrder.Value))
            {
                switch ((string)order.Type)
                {
                    case "Image":
                        // Origin
                        int posX = 0, posY = 0;
                        Vector2 origin = Vector2.Zero;
                        if (((XElement)order).Element("Origin") != null && Int32.TryParse((string)order.Origin.X, out posX) && Int32.TryParse((string)order.Origin.Y, out posY))
                            origin = new Vector2(posX, posY);

                        // Rectangle
                        Nullable<Rectangle> rect = null;
                        if (((XElement)order).Element("Source") != null && (string)order.Source != "null")
                            rect = new Rectangle(ParseInt(order.Source.X), ParseInt(order.Source.Y), ParseInt(order.Source.W), ParseInt(order.Source.H));

                        // Action
                        bool hasOver = (((XElement)order).Element("Hover") != null && ((XElement)order.Hover).Element("Action") != null);

                        string hoverAction = "";
                        if (hasOver)
                            hoverAction = order.Hover.Action.Attribute(XName.Get("type")).Value;

                        string clickAction = "";
                        if (((XElement)order).Element("Click") != null && ((XElement)order.Click).Element("Action") != null)
                            clickAction = order.Click.Action.Attribute(XName.Get("type")).Value;


                        _Textures2D[GuiId].Add(new el_Texture2D(Content.Load<Texture2D>((string)order.URI),
                            new Vector2(ParseInt(order.Position.X), ParseInt(order.Position.Y)),
                            rect,
                            (((XElement)order).Element("Color") != null) ? ParseColor((string)order.Color) : Color.White,
                            origin,
                            hoverAction,
                            clickAction,
                            order,
                            hasOver,
                            ((XElement)order).Element("Rotation") == null ? 0f : ParseFloat(order.Rotation),
                            ((XElement)order).Element("Scale") == null ? 1f : ParseFloat(order.Scale, 1f)));

                        break;
                    case "Cursor":
                        _Cursor[GuiId] = new el_Cursor(Content.Load<Texture2D>((string)order.URI),
                            new Vector2(ParseInt(order.Source.X), ParseInt(order.Source.Y)),
                            ((XElement)order).Element("Scale") != null ? ParseFloat(order.Scale) : 1f);
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
                _Cursor[_drawGuiId].MousePos = new Vector2(mouseState.X, mouseState.Y);
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
                                    case "source": // Change Source
                                        _Textures2D[_drawGuiId][i].SourceRectangle = new Rectangle(ParseInt(_Textures2D[_drawGuiId][i].XMLLink.Hover.Action.X), ParseInt(_Textures2D[_drawGuiId][i].XMLLink.Hover.Action.Y), ParseInt(_Textures2D[_drawGuiId][i].XMLLink.Hover.Action.W), ParseInt(_Textures2D[_drawGuiId][i].XMLLink.Hover.Action.H));
                                        break;
                                }
                            }
                            if (_Textures2D[_drawGuiId][i].ClickAction != "" && oldMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
                            {
                                // Can be clicked
                                switch (_Textures2D[_drawGuiId][i].ClickAction)
                                {
                                    case "changestate": // Change State
                                        Type typeInfo = Type.GetType("Editor.GameStates." + (string)_Textures2D[_drawGuiId][i].XMLLink.Click.Action);
                                        if (typeInfo != null)
                                        {
                                            System.Reflection.MethodInfo MethodInfo = typeInfo.GetMethod("getInstance");
                                            _gameStateManager.ChangeState((CGameState)MethodInfo.Invoke(null, null));
                                            _gameStateManager.Initialize();
                                            _gameStateManager.loadContent();
                                        }
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
                                case "source": // Change Source
                                    _Textures2D[_drawGuiId][i].SourceRectangle = new Rectangle(ParseInt(_Textures2D[_drawGuiId][i].XMLLink.Source.X), ParseInt(_Textures2D[_drawGuiId][i].XMLLink.Source.Y), ParseInt(_Textures2D[_drawGuiId][i].XMLLink.Source.W), ParseInt(_Textures2D[_drawGuiId][i].XMLLink.Source.H));
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
                spritebatch.Begin();
                for (int i = 0; i < _Textures2D[_drawGuiId].Count; i++)
                {
                    spritebatch.Draw(_Textures2D[_drawGuiId][i].Texture2D, _Textures2D[_drawGuiId][i].Position, _Textures2D[_drawGuiId][i].SourceRectangle, _Textures2D[_drawGuiId][i].Color,
                        _Textures2D[_drawGuiId][i].Rotation, _Textures2D[_drawGuiId][i].Origin, _Textures2D[_drawGuiId][i].Scale, _Textures2D[_drawGuiId][i].Effects, 1);
                }
                if (_Cursor[_drawGuiId] != null)
                {
                    spritebatch.Draw(_Cursor[_drawGuiId].Texture2D, _Cursor[_drawGuiId].MousePos - _Cursor[_drawGuiId].Source, null, Color.White, 0.0f, Vector2.Zero, _Cursor[_drawGuiId].Scale, SpriteEffects.None, 0);
                }
                spritebatch.End();
            }
        }

        #region "Conversion Methods"
        private int ParseInt(dynamic obj, int defaultValue = 0)
        {
            int Int;
            string str = (string)obj;
            if (str == "")
                return defaultValue;
            else if (!Int32.TryParse(str, out Int))
                return defaultValue;
            else
                return Int;
        }

        private float ParseFloat(dynamic obj, float defaultValue = 0f)
        {
            float Float;
            string str = (string)obj;
            if (str == "")
                return defaultValue;
            else if (!Single.TryParse(str, out Float))
                return defaultValue;
            else
                return Float;
        }

        private Color ParseColor(string hexString)
        {
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);
            uint hex = uint.Parse(hexString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
            Color color = Color.White;
            if (hexString.Length == 8)
            {
                color.A = (byte)(hex >> 24);
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            else if (hexString.Length == 6)
            {
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            return color;
        }
        #endregion

    }
}
