﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Editor.Game
{
    class CConsole
    {
        /* *** Variables *** */
        private bool _isConsoleActivated = false;
        private bool _isConsoleEnabled = false;
        private bool _drawGameStuff = true;
        private bool _drawFPS = true;


        private float _elapsedTimeFPS = 0.0f;
        private int _totalFrames = 0;
        private int _totalFPS = 0;
        private int _maxLines = 15;

        private string _inputLinePre = "[color:#FF0000]> [/color]";
        private string _inputLine_preLine = "";
        private string _inputLine_postLine = "";
        private string _inputLineCursor = "_";

        private List<string> _consoleLines = new List<string>();
        private List<string> _commandsList = new List<string>();

        // Classes
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private KeyboardState _oldKeyBoardState;
        private ContentManager _Content;
        private CInput _inputManager = new CInput();
        private List<Keys> _activationKeys = new List<Keys> { Keys.OemTilde, Keys.OemQuotes };

        // 2D
        private Texture2D _backgroundTexture;
        private Vector2 _backgroundPosition;
        private Vector2 _backgroundSize;
        private Color _backgroundColor;
        private SpriteFont _consoleFont;
        private Vector2 _inputLinePos;


        /* *** Methods *** */

        // Constructor
        public CConsole(bool isConsoleActivated, bool drawGameStuff)
        {
            this._isConsoleActivated = isConsoleActivated;
            this._drawGameStuff = drawGameStuff;
        }


        // Commands List
        public void enterNewCommand(string command)
        {
            _commandsList.Add(command);
            addMessage(_inputLinePre + command);
            string[] cmd = command.Split(' ');
            switch (cmd[0])
            {
                case "exit":
                case "quit":
                    _isConsoleEnabled = false;
                    break;
                case "fps":
                case "togglefps":
                case "toggle_fps":
                    _drawFPS = !_drawFPS;
                    break;
            }
        }

        // Add a message to the console
        public void addMessage(string msg, bool isGameMessage = false)
        {
            if (isGameMessage && !_drawGameStuff)
                return;
            _consoleLines.Add(msg);
        }

        // Load all the contents
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            // Instantiate classes
            this._graphicsDevice = graphicsDevice;
            this._spriteBatch = spriteBatch;
            this._Content = contentManager;

            // Compute background position & size
            _backgroundPosition = Vector2.Zero;
            Vector2 WindowSize = new Vector2(_graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);
            _backgroundSize = new Vector2(WindowSize.X, WindowSize.Y / 4);

            // Define background texture
            _backgroundColor = new Color(0, 0, 0, 80); // Last parameter is opacity
            _backgroundTexture = new Texture2D(graphicsDevice, (int)_backgroundSize.X, (int)_backgroundSize.Y);

            Color[] data = new Color[(int)_backgroundSize.X * (int)_backgroundSize.Y];
            for (int i = 0; i < data.Length; ++i)
                data[i] = _backgroundColor;

            _backgroundTexture.SetData(data);

            // Load font & computations
            _consoleFont = _Content.Load<SpriteFont>("2D\\consoleFont");
            int fontSizeY = (int)_consoleFont.MeasureString("A").Y;
            _inputLinePos = new Vector2(10, WindowSize.Y / 4 - fontSizeY * 2);

            _maxLines = (int)(_inputLinePos.Y / fontSizeY) - 2;

        }

        // Update function: manage inputs, etc.
        public void Update(KeyboardState keyboardState, GameTime gameTime)
        {
            if (_drawFPS)
            {
                _elapsedTimeFPS += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                // 1 Second has passed
                if (_elapsedTimeFPS >= 1000.0f)
                {
                    _totalFPS = _totalFrames;
                    _totalFrames = 0;
                    _elapsedTimeFPS = 0;
                }
            }

            if (!_isConsoleActivated)
                return;

            if (!_isConsoleEnabled)
            {
                foreach (Keys keys in _activationKeys)
                {
                    if (_oldKeyBoardState.IsKeyDown(keys) && keyboardState.IsKeyUp(keys))
                    {
                        _isConsoleEnabled = !_isConsoleEnabled;
                        break;
                    }
                }
            }
            else
            {
                // Enter
                if (_oldKeyBoardState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyDown(Keys.Enter))
                {
                    if (_inputLine_preLine.Length > 0 || _inputLine_postLine.Length > 0)
                    {
                        enterNewCommand(_inputLine_preLine + _inputLine_postLine);
                        _inputLine_preLine = "";
                        _inputLine_postLine = "";
                    }
                    this._oldKeyBoardState = keyboardState;
                    return;
                }

                // Ctrl + <key>
                if (_oldKeyBoardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.LeftControl))
                {
                    if (_oldKeyBoardState.IsKeyDown(Keys.V) && keyboardState.IsKeyUp(Keys.V))
                    {
                        _inputLine_preLine += _inputManager.GetClipboardText();
                    }
                    this._oldKeyBoardState = keyboardState;
                    return;
                }

                // Left Arrow
                if (_oldKeyBoardState.IsKeyDown(Keys.Left) && keyboardState.IsKeyUp(Keys.Left))
                {
                    int preLength = _inputLine_preLine.Length;
                    if (preLength > 0)
                    {
                        _inputLine_postLine = _inputLine_preLine[preLength - 1] + _inputLine_postLine;
                        _inputLine_preLine = _inputLine_preLine.Substring(0, preLength - 1);
                    }
                    this._oldKeyBoardState = keyboardState;
                    return;
                }

                // Right Arrow
                if (_oldKeyBoardState.IsKeyDown(Keys.Right) && keyboardState.IsKeyUp(Keys.Right))
                {
                    int postLength = _inputLine_postLine.Length;
                    if (_inputLine_postLine.Length > 0)
                    {
                        _inputLine_preLine += _inputLine_postLine[0];
                        _inputLine_postLine = _inputLine_postLine.Substring(1);
                    }
                    this._oldKeyBoardState = keyboardState;
                    return;
                }

                // Other keys
                foreach (Keys key in keyboardState.GetPressedKeys())
                {
                    //addMessage(key.ToString(), false);
                    if (!_oldKeyBoardState.IsKeyDown(key))
                    {
                        // Are caps activated
                        bool caps = (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift) || System.Console.CapsLock);

                        // Back: remove 1 char from left
                        if (key == Keys.Back)
                        {
                            if (_inputLine_preLine.Length > 0)
                                _inputLine_preLine = _inputLine_preLine.Substring(0, _inputLine_preLine.Length - 1);
                        }
                        // Delete: remove 1 char from right
                        else if (key == Keys.Delete)
                        {
                            if (_inputLine_postLine.Length > 0)
                                _inputLine_postLine = _inputLine_postLine.Substring(1);
                        }
                        else
                            _inputLine_preLine += _inputManager.getRealTypedKey(key, caps);
                    }
                }
            }

            this._oldKeyBoardState = keyboardState;
        }

        // Draw the console
        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            if (_drawFPS)
            {
                _totalFrames++;

                _spriteBatch.DrawString(_consoleFont, string.Format("{0} FPS", _totalFPS),
                    new Vector2(10.0f, 20.0f), Color.White);
            }

            if (_isConsoleEnabled)
            {
                _spriteBatch.Draw(_backgroundTexture, _backgroundPosition, Color.Black);
                if (_consoleLines.Any())
                {
                    int listCount = _consoleLines.Count;

                    int topPosLine = _maxLines * 15 + 10;
                    for (int i = 1; i <= ((listCount > _maxLines) ? _maxLines : listCount); i++)
                        drawFormattedText(_consoleLines[listCount - i], new Vector2(10, topPosLine - i * 15));

                }
                drawFormattedText(_inputLinePre + _inputLine_preLine + _inputLineCursor + _inputLine_postLine, _inputLinePos);
            }

            _spriteBatch.End();
        }

        // Draw a text with different colors
        private void drawFormattedText(string text, Vector2 pos)
        {
            Color defaultColor = Color.LightGreen;

            if (text.Contains("[color:"))
            {
                int offset = 0;
                string[] splits = text.Split(new string[] { "[color:" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var str in splits)
                {
                    if (str.StartsWith("#"))
                    {
                        // We're using #RRGGBBAA here
                        string color = str.Substring(0, 7);

                        string[] msgs = str.Substring(8).Split(new string[] { "[/color]" }, StringSplitOptions.RemoveEmptyEntries);

                        _spriteBatch.DrawString(_consoleFont, msgs[0], pos + new Vector2(offset, 0), RGBToColor(color));
                        offset += (int)_consoleFont.MeasureString(msgs[0]).X;

                        // there should only ever be one other string or none
                        if (msgs.Length == 2)
                        {
                            _spriteBatch.DrawString(_consoleFont, msgs[1], pos + new Vector2(offset, 0), defaultColor);
                            offset += (int)_consoleFont.MeasureString(msgs[1]).X;
                        }
                    }
                    else
                    {
                        _spriteBatch.DrawString(_consoleFont, str, pos + new Vector2(offset, 0), defaultColor);
                        offset += (int)_consoleFont.MeasureString(str).X;
                    }
                }
            }
            else
            {
                _spriteBatch.DrawString(_consoleFont, text, pos, defaultColor);
            }
        }

        // #RRGGBBAA to Color
        static Color RGBToColor(string hexString)
        {
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);
            uint hex = uint.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
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
            else
            {
                throw new InvalidOperationException("Invalid hex representation of an ARGB or RGB color value.");
            }
            return color;
        }

        // Change the keys to open the console
        public void changeActivationKeys(List<Keys> newActivationKeys)
        {
            this._activationKeys = newActivationKeys;
        }

    }
}
