using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Basic console class to process several developer commands, display messages, etc.
    /// </summary>
    class CConsole
    {
        private static bool _isConsoleActivated = false;
        public static bool _isConsoleEnabled = false;
        private static bool _isConsoleFileLoaded = false;
        private static bool _drawGameStuff = true;
        private static bool _drawFPS = true;
        private static bool _drawGyzmo = false;

        private static float _elapsedTimeFPS = 0.0f;
        private static int _totalFrames = 0;
        private static int _totalFPS = 0;
        private static int _maxLines = 15;

        private static string _inputLinePre = "[color:#FF0000]> [/color]";
        private static string _inputLine_preLine = "";
        private static string _inputLine_postLine = "";
        private static string _inputLineCursor = "_";

        private static List<string> _consoleLines = new List<string>();
        private static List<string> _commandsList = new List<string>();

        private static StreamWriter _logsFile;
        private static GraphicsDevice _graphicsDevice;
        private static SpriteBatch _spriteBatch;
        private static KeyboardState _oldKeyBoardState;
        private static ContentManager _Content;
        private static CInput _inputManager = new CInput();

        private static Texture2D _backgroundTexture;
        private static Vector2 _backgroundPosition;
        private static Vector2 _backgroundSize;
        private static Color _backgroundColor;
        private static SpriteFont _consoleFont;
        private static Vector2 _inputLinePos;

        public static Keys _activationKeys = Keys.OemTilde;
        public static Display3D.CCamera _Camera;
        public static Game.CCharacter _Character;
        public static Game.CEnemyManager _EnemyManager;
        public static Game.CWeapon _Weapon;
        public static Display3D.CTerrain _Terrain;
        public static Display3D.CWater _Water;

        /// <summary>
        /// Process new commands
        /// </summary>
        /// <param name="command">The command typed</param>
        /// <param name="gameTime">GameTime snaphot</param>
        public static void enterNewCommand(string command, GameTime gameTime)
        {
            _commandsList.Add(command);
            addMessage(_inputLinePre + command, false, command);
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
                                Display2D.C2DEffect.gaussianBlurEffect(blurAmount, true);
                            else
                                addMessage("USAGE: " + cmd[0] + " " + cmd[1] + " <blur intensity (float)>");
                        }
                        else if (effectName == "blackwhite" || effectName == "blackandwhite")
                        {
                            Display2D.C2DEffect.BlackAndWhiteEffect();
                        }
                        else if (effectName == "fade")
                        {
                            if (cmd.Length > 1)
                            {
                                int fadeToOpacity;
                                int duration;
                                if (cmd.Length >= 4 && int.TryParse(cmd[2], out fadeToOpacity) && int.TryParse(cmd[3], out duration))
                                    Display2D.C2DEffect.fadeEffect(fadeToOpacity, duration, new Vector2(_graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight), new Vector2(0, 0), new Color(0, 0, 0), Display2D.C2DEffect.nullFunction);
                                else
                                    addMessage("USAGE: " + cmd[0] + " " + cmd[1] + " <opacity (0-255)> <duration (ms)>");
                            }
                        }
                    }
                    else
                        addMessage("USAGE: " + cmd[0] + " <GaussianBlur/BlackWhite/Fade>");
                    break;
                case "getposition":
                    addMessage(_Camera._cameraPos.ToString());
                    break;
                case "debug":
                    int debugType;
                    if (cmd.Length > 1 && Int32.TryParse(cmd[1], out debugType))
                    {
                        if (debugType == 0)
                        {
                            _Terrain.debugActivated = false;
                            _Water.debugActivated = false;
                            Display3D.CModelManager.DebugActivated = false;
                            _Terrain.terrainDrawPrimitive = PrimitiveType.TriangleList;
                        }
                        if (debugType == 1)
                        {
                            _Terrain.debugActivated = true;
                            _Water.debugActivated = true;
                            Display3D.CModelManager.DebugActivated = true;
                            _Terrain.terrainDrawPrimitive = PrimitiveType.TriangleList;
                        }
                        if (debugType == 2)
                        {
                            _Terrain.debugActivated = true;
                            _Water.debugActivated = true;
                            Display3D.CModelManager.DebugActivated = true;
                            _Terrain.terrainDrawPrimitive = PrimitiveType.LineList;
                        }
                        addMessage("Debug mode " + debugType + " activated.");
                    }
                    else
                        addMessage("USAGE: " + cmd[0] + " <0-2>");
                    break;
                case "setrunspeed":
                    float runSpeed;
                    if (cmd.Length > 1 && float.TryParse(cmd[1], out runSpeed))
                    {
                        _Character._sprintSpeed = runSpeed;
                        addMessage("Run speed set to " + runSpeed);
                    }
                    else
                        addMessage("USAGE: " + cmd[0] + " <Speed>");
                    break;
                case "help":
                    addMessage("Command List:");
                    addMessage("togglefps - effect - getposition - setrunspeed - weapon_info - debug - teleport");
                    break;
                case "weapon_info":
                    addMessage("Magazine : " + _Weapon._weaponsArray[_Weapon._selectedWeapon]._actualClip + " | Bullets available : " + _Weapon._weaponsArray[_Weapon._selectedWeapon]._bulletsAvailable);
                    break;
                case "teleport":
                    if (cmd.Length > 3)
                    {
                        float x, y, z;
                        if(float.TryParse(cmd[1], out x) && float.TryParse(cmd[2], out y) && float.TryParse(cmd[3], out z))
                        {
                            _Camera._cameraPos = new Vector3(x,y,z);
                        }
                        else
                            addMessage("USAGE: " + cmd[0] + " <X : float> <Y : float> <Z : float>");
                    }
                    else
                        addMessage("USAGE: " + cmd[0] + " <X> <Y> <Z>");
                    break;
                case "freezeEnemy":
                    if (cmd.Length > 0)
                    {
                        CEnemyManager._enemyList[0]._isFrozen = !CEnemyManager._enemyList[0]._isFrozen;
                    }
                    break;
                case "enemyInfo":
                    addMessage("Is Agressive : " + CEnemyManager._enemyList[0]._isAgressive + "\n Life : "+
                        CEnemyManager._enemyList[0]._life + " %\n");
                    break;
                case "lightModels":
                    int lightEffect = 0;
                    if (Int32.TryParse(cmd[1], out lightEffect))
                        Display3D.CModelManager.ChangeModelsLightingEffect((Display3D.LightingMode)lightEffect);
                    break;
            }
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
        public static void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, bool isConsoleActivated = true, bool drawGameStuff = false)
        {
            // Instantiate classes
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _Content = contentManager;
            _isConsoleActivated = isConsoleActivated;
            _drawGameStuff = drawGameStuff;

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

            _maxLines = (int)(_inputLinePos.Y / fontSizeY);

            try
            {
                FileInfo fi = new FileInfo(@"ConsoleLog.log");
                _logsFile = new StreamWriter(fi.Open(FileMode.Create));
                _isConsoleFileLoaded = true;

                WriteLogs("======================");
                WriteLogs("Loaded " + fi.Name + " - Console logs");
                WriteLogs("======================");
            }
            catch (Exception e)
            {
                //addMessage("Cannot open console logs: " + e.Message, true);
                _isConsoleFileLoaded = false;
            }

        }

        /// <summary>
        /// Add a new message to the console
        /// </summary>
        /// <param name="msg">The message to display</param>
        /// <param name="isGameMessage">Whether or not it's an internal message</param>
        public static void addMessage(string msg, bool isGameMessage = false, string msgNoFormat = "")
        {
            if (isGameMessage && !_drawGameStuff)
                return;

            _consoleLines.Add(msg);

            if (msgNoFormat != "")
                WriteLogs(msgNoFormat);
            else
                WriteLogs(msg);
        }

        /// <summary>
        /// Writes a message at the end of the console log file.
        /// </summary>
        /// <param name="message">The message to write</param>
        public static void WriteLogs(string message, bool addDate = true, bool flush = true)
        {
            addDate = false;
            if (_isConsoleFileLoaded)
            {
                _logsFile.WriteLine(((addDate) ? "[" + DateTime.Now.ToString() + "] " : "") + message);
                if (flush)
                    _logsFile.Flush();
            }
        }

        /// <summary>
        /// Called at each frame to process code frame-per-frame
        /// </summary>
        /// <param name="keyboardState">KeyboardState class</param>
        /// <param name="gameTime">GameTime snapshot</param>
        public static void Update(KeyboardState keyboardState, GameTime gameTime)
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
                _oldKeyBoardState = keyboardState;
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
                    _oldKeyBoardState = keyboardState;
                    return;
                }

                // Ctrl + <key>
                if (_oldKeyBoardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.LeftControl))
                {
                    if (_oldKeyBoardState.IsKeyDown(Keys.V) && keyboardState.IsKeyUp(Keys.V))
                    {
                        _inputLine_preLine += _inputManager.GetClipboardText();
                    }
                    _oldKeyBoardState = keyboardState;
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
                    _oldKeyBoardState = keyboardState;
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
                    _oldKeyBoardState = keyboardState;
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

            _oldKeyBoardState = keyboardState;
        }

        /// <summary>
        /// Draw the console each frames
        /// </summary>
        /// <param name="gameTime">GameTime snapshot</param>
        public static void Draw(GameTime gameTime)
        {
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

                Vector3 position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(_Camera._yaw, _Camera._pitch, _Camera._roll));
                Matrix viewMatrix = Matrix.CreateLookAt(position, Vector3.Zero, Vector3.Up);
                Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _graphicsDevice.Viewport.AspectRatio, .1f, 100f);

                Display3D.CSimpleShapes.Draw(gameTime, viewMatrix, projection);
            }

            if (_isConsoleEnabled)
            {
                _spriteBatch.Draw(_backgroundTexture, _backgroundPosition, Color.Red);
                if (_consoleLines.Any())
                {
                    int listCount = _consoleLines.Count;

                    int topPosLine = _maxLines * 15 + 10;
                    for (int i = 1; i <= ((listCount > _maxLines) ? _maxLines : listCount); i++)
                        drawFormattedText(_consoleLines[listCount - i], new Vector2(10, topPosLine - i * 15));

                }
                drawFormattedText(_inputLinePre + _inputLine_preLine + _inputLineCursor + _inputLine_postLine, _inputLinePos);
            }

        }

        /// <summary>
        /// Draw a text by using its formatted colors
        /// Example: "[color:#RRGGBBAA]test [color:#RRGGBBAA]test2"
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="pos">The position to draw the text</param>
        private static void drawFormattedText(string text, Vector2 pos)
        {
            Color defaultColor = Color.LightGreen;

            if (text == null)
                return;

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
        private static Color RGBToColor(string hexString)
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
    }
}
