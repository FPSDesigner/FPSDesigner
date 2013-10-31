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

        private Display3D.CModel model; // (TEST) One Model displayed
        private Display3D.CCamera cam; // (TEST) One camera instancied

        // All The States that exist
        private GameStates.CMenu Menu;
        private GameStates.CInGame InGame;

        // Current State
        Game.CGameStateManager _currentState;

        private Game.CConsole devConsole;

        private Game.Settings.CGameSettings gameSettings;
        
        /// <summary>
        /// Constructor, Initialize the class
        /// </summary>
        public void Initialize()
        {
            _currentState = Game.CGameStateManager.getInstance();

            devConsole = new Game.CConsole(true, true);
            gameSettings = Game.Settings.CGameSettings.getInstance(); // Singleton Initialization
        }

        /// <summary>
        /// Load all the contents
        /// </summary>
        /// <param name="content">ContentManager class</param>
        /// <param name="graphics">GraphicsDevice class</param>
        /// <param name="spriteBatch">SpriteBatch class</param>
        public void loadContent(ContentManager content, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            model = new Display3D.CModel(content.Load<Model>("3D//building"), new Vector3(0, 0, 0), new Vector3(0, -90f, 0), new Vector3(1.0f, 1.0f, 1.0f), graphics);
            cam = new Display3D.CCamera(graphics, new Vector3(100f, 250f, 2000f), new Vector3(-100.0f, -250.0f, -2000.0f), 0.1f, 10000.0f, 1.0f);

            gameSettings.loadDatas(graphics);

            devConsole.LoadContent(content, graphics, spriteBatch, cam);
            devConsole._activationKeys = gameSettings._gameSettings.KeyMapping.Console;
        }

        public void unloadContent(ContentManager content)
        {

        }


        public void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState)
        {
            cam.Update(gameTime, kbState, mouseState);

            devConsole.Update(kbState, gameTime);
        }

        public void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {
            model.Draw(cam._view, cam._projection);

            devConsole.Draw(gameTime);
        }
    }
}
