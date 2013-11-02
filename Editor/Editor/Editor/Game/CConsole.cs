using System;
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
    /// <summary>
    /// Basic console class to process several developer commands, display messages, etc.
    /// </summary>
    class CConsole
    {
        private bool _isConsoleActivated = false;
        private bool _isConsoleEnabled = false;
        private bool _drawGameStuff = true;
        private bool _drawFPS = true;
        private bool _drawGyzmo = false;

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

        private Display2D.C2DEffect C2DEffect = Display2D.C2DEffect.getInstance();


        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private KeyboardState _oldKeyBoardState;
        private ContentManager _Content;
        private CInput _inputManager = new CInput();
        private Display3D.CCamera _Camera;
        public Keys _activationKeys = Keys.OemTilde;


        private Texture2D _backgroundTexture;
        private Vector2 _backgroundPosition;
        private Vector2 _backgroundSize;
        private Color _backgroundColor;
        private SpriteFont _consoleFont;
        private Vector2 _inputLinePos;


        /// <summary>
        /// Process new commands
        /// </summary>
        /// <param name="command">The command typed</param>
        /// <param name="gameTime">GameTime snaphot</param>
        public void enterNewCommand(string command, GameTime gameTime)
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
                case "gyzmo":
                case "togglegyzmo":
                    _drawGyzmo = !_drawGyzmo;
                    addMessage("Gyzmo " + ((_drawGyzmo) ? "activated" : "disabled") + " !");
                    break;
                case "effect":
                    if (cmd.Length > 1)
                    {
                        string effectName = cmd[1].ToLower();
                        if (effectName == "gaussianblur" || effectName == "blur")
                        {
                            float blurAmount;
                            if (cmd.Length > 2 && float.TryParse(cmd[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out blurAmount))
                                C2DEffect.gaussianBlurEffect(blurAmount, true);
                            else
                                addMessage("USAGE: " + cmd[0] + " " + cmd[1] + " <blur intensity (float)>");
                        }
                        else if (effectName == "blackwhite" || effectName == "blackandwhite")
                        {
                            C2DEffect.BlackAndWhiteEffect();
                        }
                        else if (effectName == "fade")
                        {
                            if (cmd.Length > 1)
                            {
                                int fadeToOpacity;
                                int duration;
                                if (cmd.Length >= 4 && int.TryParse(cmd[2], out fadeToOpacity) && int.TryParse(cmd[3], out duration))
                                    C2DEffect.fadeEffect(fadeToOpacity, duration, gameTime, new Vector2(_graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight), new Vector2(0, 0), new Color(0, 0, 0), C2DEffect.nullFunction);
                                else
                                    addMessage("USAGE: " + cmd[0] + " " + cmd[1] + " <opacity (0-255)> <duration (ms)>");
                            }
                        }
                    }
                    else
                        addMessage("USAGE: " + cmd[0] + " <GaussianBlur/BlackWhite/Fade>");
                    break;
                case "help":
                    addMessage("Command List:");
                    addMessage("togglefps - effect");
                    break;
            }
        }

        /// <summary>
        /// Add a new message to the console
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="isGameMessage">Whether or not it's an internal message</param>
        public void addMessage(string msg, bool isGameMessage = false)
        {
            if (isGameMessage && !_drawGameStuff)
                return;
            _consoleLines.Add(msg);
        }

        /// <summary>
        /// Load content & initialize 2D drawable content
        /// </summary>
        /// <param name="contentManager">ContentManager class</param>
        /// <param name="graphicsDevice">GraphicsDevice class</param>
        /// <param name="spriteBatch">SpriteBatch class</param>
        /// <param name="Camera">Display3D.CCamera class</param>
        /// <param name="isConsoleActivated">True if we want the player to toggle the console</param>
        /// <param name="drawGameStuff">True if we want to display internal messages</param>
        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Display3D.CCamera Camera, bool isConsoleActivated = true, bool drawGameStuff = false)
        {
            // Instantiate classes
            this._graphicsDevice = graphicsDevice;
            this._spriteBatch = spriteBatch;
            this._Content = contentManager;
            this._Camera = Camera;
            this._isConsoleActivated = isConsoleActivated;
            this._drawGameStuff = drawGameStuff;

            Display3D.CSimpleShapes.Initialize(_graphicsDevice);

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

            // Load font & compute size, positions
            _consoleFont = _Content.Load<SpriteFont>("2D\\consoleFont");
            int fontSizeY = (int)_consoleFont.MeasureString("A").Y;
            _inputLinePos = new Vector2(10, WindowSize.Y / 4 - fontSizeY * 2);

            _maxLines = (int)(_inputLinePos.Y / fontSizeY) - 2;

        }

        /// <summary>
        /// Called at each frame to process code frame-per-frame
        /// </summary>
        /// <param name="keyboardState">KeyboardState class</param>
        /// <param name="gameTime">GameTime snapshot</param>
        public void Update(KeyboardState keyboardState, GameTime gameTime)
        {
            // Draw FPS
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

            // Draw Console
            if (!_isConsoleActivated)
                return;


            if (_oldKeyBoardState.IsKeyDown(_activationKeys) || keyboardState.IsKeyDown(_activationKeys))
            {
                if (keyboardState.IsKeyUp(_activationKeys))
                    _isConsoleEnabled = !_isConsoleEnabled;
                this._oldKeyBoardState = keyboardState;
                return;
            }

            if (_isConsoleEnabled)
            {

                // Enter
                if (_oldKeyBoardState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyDown(Keys.Enter))
                {
                    if (_inputLine_preLine.Length > 0 || _inputLine_postLine.Length > 0)
                    {
                        enterNewCommand(_inputLine_preLine + _inputLine_postLine, gameTime);
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

        /// <summary>
        /// Draw the console each frames
        /// </summary>
        /// <param name="gameTime">GameTime snapshot</param>
        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            if (_drawFPS)
            {
                _totalFrames++;

                _spriteBatch.DrawString(_consoleFont, string.Format("{0} FPS", _totalFPS),
                    new Vector2(10.0f, 20.0f), Color.White);
            }

            if (_drawGyzmo)
            {
                Display3D.CSimpleShapes.AddLine(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f), Color.Red);
                Display3D.CSimpleShapes.AddLine(new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), Color.Blue);
                Display3D.CSimpleShapes.AddLine(new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), Color.Green);

                Vector3 position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(_Camera._yaw, _Camera._pitch, 0));
                Matrix viewMatrix = Matrix.CreateLookAt(position, Vector3.Zero, Vector3.Up);
                Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _graphicsDevice.Viewport.AspectRatio, .1f, 100f);

                Display3D.CSimpleShapes.Draw(gameTime, viewMatrix, projection);
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

        /// <summary>
        /// Draw a text by using its formatted colors
        /// Example: "[color:#RRGGBBAA]test [color:#RRGGBBAA]test2"
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="pos">The position to draw the text</param>
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

        /// <summary>
        /// Transform a RGB color to a Color instance.
        /// </summary>
        /// <param name="hexString">The RGB string</param>
        /// <returns>A Color instance</returns>
        private Color RGBToColor(string hexString)
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

        /// <summary>
        /// Singleton Code
        /// </summary>
        private static CConsole instance = null;
        private static readonly object myLock = new object();

        private CConsole() { }
        public static CConsole getInstance()
        {
            lock (myLock)
            {
                if (instance == null) instance = new CConsole();
                return instance;
            }
        }

    }
}
