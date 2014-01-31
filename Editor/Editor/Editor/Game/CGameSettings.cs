using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Editor.Game.Settings
{
    public class CGameSettings
    {
        public GameSettingsInfos _gameSettings { get; set; }
        string _xmlFile;
        CXMLManager _xmlManager;
        GraphicsDevice _graphicsDevice;

        public GamePadState gamepadState;
        public GamePadState oldGamepadState;
        public bool useGamepad = false;


        // Singleton Code
        private static CGameSettings instance = null;
        private static readonly object myLock = new object();

        // Singelton Methods
        private CGameSettings() { }
        public static CGameSettings getInstance()
        {
            lock (myLock)
            {
                if (instance == null) instance = new CGameSettings();
                return instance;
            }
        }

        public GameSettingsInfos loadDatas(GraphicsDevice graphicsDevice, string xmlFile = "gameSettings.xml")
        {
            _xmlManager = new CXMLManager();
            _xmlFile = xmlFile;
            _graphicsDevice = graphicsDevice;

            if (File.Exists(xmlFile))
                _gameSettings = _xmlManager.deserializeClass<GameSettingsInfos>(_xmlFile);
            else
                generateDefaultSettings();

            gamepadState = GamePad.GetState(PlayerIndex.One);
            useGamepad = gamepadState.IsConnected;

            return _gameSettings;
        }

        public void reloadGamepadState()
        {
            oldGamepadState = gamepadState;

            gamepadState = GamePad.GetState(PlayerIndex.One);
            useGamepad = gamepadState.IsConnected;
        }


        public void generateDefaultSettings()
        {
            CInput CInput = new CInput();
            bool isKeyboardQwerty = (CInput.getKeyboardType() == "QWERTY"); 
            _gameSettings = new GameSettingsInfos
            {
                KeyMapping = new KeyMapping
                {
                    MForward = (isKeyboardQwerty) ? Keys.W : Keys.Z,
                    MLeft = (isKeyboardQwerty) ? Keys.A : Keys.Q,
                    MRight = Keys.D,
                    MBackward = Keys.S,
                    MJump = Keys.Space,
                    MCrouch = Keys.C,
                    MSprint = Keys.LeftShift,
                    Console = (isKeyboardQwerty) ? Keys.OemTilde : Keys.OemQuotes,
                    MouseSensibility = 0.001f,

                    GPSensibility = 0.05f,
                    GPJump = Buttons.A,
                    GPShot = Buttons.RightTrigger,
                    GPRun = Buttons.LeftShoulder,
                    GPCrouch = Buttons.B,
                },
                Video = new Video
                {
                    ResolutionX = _graphicsDevice.PresentationParameters.BackBufferWidth,
                    ResolutionY = _graphicsDevice.PresentationParameters.BackBufferHeight,
                }
            };

            saveDatas();
        }

        public void saveDatas()
        {
            XmlSerializer xs = new XmlSerializer(typeof(GameSettingsInfos));

            using (StreamWriter wr = new StreamWriter(_xmlFile))
            {
                xs.Serialize(wr, _gameSettings);
            }
        }
    }

    public class GameSettingsInfos
    {
        public KeyMapping KeyMapping { get; set; }
        public Video Video { get; set; }
    }

    #region "Node - Key Mapping"
    // Key Mapping node
    public class KeyMapping
    {
        public Keys MForward { get; set; }
        public Keys MRight { get; set; }
        public Keys MLeft { get; set; }
        public Keys MBackward { get; set; }
        public Keys MJump { get; set; }
        public Keys MCrouch { get; set; }
        public Keys MSprint { get; set; }
        public Keys Console { get; set; }
        public Buttons GPJump { get; set; }
        public Buttons GPShot { get; set; }
        public Buttons GPRun { get; set; }
        public Buttons GPCrouch { get; set; }
        public float MouseSensibility { get; set; }
        public float GPSensibility { get; set; }
    }
    #endregion

    #region "Node - Video"
    // Video node
    public class Video
    {
        public int ResolutionX { get; set; }
        public int ResolutionY { get; set; }
    }
    #endregion
}
