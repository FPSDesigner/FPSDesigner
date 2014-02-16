using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Used to manage the main class
    /// </summary>
    class CGameManagement
    {
        // All The States that exist
        private GameStates.CMenu Menu;
        private GameStates.CInGame InGame;

        // Current State
        public Game.CGameStateManager _currentState;
        Game.CGUIManager _GUIManager;

        private GraphicsDeviceManager _graphicsManager;
        private GraphicsDevice _graphics;
        private MouseState _oldMouseState;

        /// <summary>
        /// Constructor, Initialize the class
        /// </summary>
        public void Initialize()
        {
            _currentState = Game.CGameStateManager.getInstance();
            _GUIManager = new Game.CGUIManager(_currentState);
            _GUIManager.LoadGUIFile("GUI.xml");

            // Example Menu
            /*Game.LevelInfo.GameMenu data = new Game.LevelInfo.GameMenu
            {
                Type = "Image",
                BackgroundMusic = "Sounds/Menu/MENU_SoundSelction",
                SelectionSound = "Sounds/Menu/MENU_SoundSelction",
                BGImageFile = "2D/Menu/MENU_Bckground",
                CursorFile = "2D/Menu/MENU_Sight",
                CursorClickX = 17,
                CursorClickY = 17,
                ButtonsInfo = new Game.LevelInfo.ButtonsInfo
                {
                    ButtonsImages = "2D/Menu/MENU_Buttons",
                    MenuButton = new Game.LevelInfo.MenuButton[]
                    {
                        new Game.LevelInfo.MenuButton {Action = 1, PosX = 50, PosY = 20, Height = 100, Width = 700, ImgPosX = 0, ImgPosY = 0 },
                        new Game.LevelInfo.MenuButton {Action = 1, PosX = 50, PosY = 200, Height = 100, Width = 700, ImgPosX = 0, ImgPosY = 120 },
                    }
                }
            };*/

            // First state = Menu
            _currentState.ChangeState(new GameStates.CMenu(_GUIManager));
            _currentState.Initialize();
        }

        /// <summary>
        /// Load all the contents
        /// </summary>
        /// <param name="content">ContentManager class</param>
        /// <param name="graphics">GraphicsDevice class</param>
        /// <param name="spriteBatch">SpriteBatch class</param>
        public void loadContent(ContentManager content, GraphicsDevice graphics, SpriteBatch spriteBatch, GraphicsDeviceManager graphicsDevice)
        {
            _graphicsManager = graphicsDevice;
            _graphics = graphics;

            _currentState.content = content;
            _currentState.graphics = graphics;
            _currentState.spriteBatch = spriteBatch;
            _currentState.graphicsDevice = graphicsDevice;

            _currentState.loadContent();
      
        }

        public void unloadContent(ContentManager content)
        {
            //Unload the State content
            _currentState.actualState.UnloadContent(content);
        }

        public void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState)
        {
            //Update the current state
            _currentState.actualState.Update(gameTime, kbState, mouseState, _oldMouseState);

            _oldMouseState = mouseState;
        }

        public void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {
            _currentState.actualState.Draw(spritebatch, gameTime);
        }

        public void SendParam(object param)
        {
            _currentState.actualState.SendParam(param);
        }


    }
}
