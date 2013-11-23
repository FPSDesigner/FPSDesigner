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
        Game.CGameStateManager _currentState;

        private GraphicsDeviceManager _graphicsManager;
        private GraphicsDevice _graphics;
        private MouseState _oldMouseState;

        /// <summary>
        /// Constructor, Initialize the class
        /// </summary>
        public void Initialize()
        {

        }

        /// <summary>
        /// Load all the contents
        /// </summary>
        /// <param name="content">ContentManager class</param>
        /// <param name="graphics">GraphicsDevice class</param>
        /// <param name="spriteBatch">SpriteBatch class</param>
        public void loadContent(ContentManager content, GraphicsDevice graphics, SpriteBatch spriteBatch,GraphicsDeviceManager graphicsDevice)
        {
            _graphicsManager = graphicsDevice;
            _graphics = graphics;

            //First state = Menu
           //Menu = new GameStates.CMenu(content.Load<Texture2D>(""));
            _currentState.ChangeState(Menu);

            //Initialize the current state : MENU
            _currentState._state.Initialize();            
        }

        public void unloadContent(ContentManager content)
        {
            //Unload the State content
            _currentState._state.unloadContent(content);
        }

        public void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState)
        {
            //Update the current state
            _currentState._state.Update(gameTime, mouseState);
        }

        public void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {
            _currentState._state.Draw(spritebatch, gameTime);
        }


    }
}
