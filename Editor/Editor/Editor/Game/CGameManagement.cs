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
using System.Reflection;

namespace Editor.Game
{
    /// <summary>
    /// Used to manage the main class
    /// </summary>
    class CGameManagement
    {
        // Current State
        //public static Game.CGameStateManager _currentState;
        public static string currentState;

        private static GraphicsDeviceManager _graphicsManager;
        private static GraphicsDevice _graphics;
        private static SpriteBatch _spriteBatch;
        private static MouseState _oldMouseState;
        private static ContentManager _content;

        private static Dictionary<string, dynamic> gameStateList;
        

        /// <summary>
        /// Constructor, Initialize the class
        /// </summary>
        public static void Initialize()
        {
            //_currentState = Game.CGameStateManager.getInstance();
            //_GUIManager = new Game.CGUIManager(_currentState);
            //_GUIManager.LoadGUIFile("GUI.xml");

            // First state = Menu
            /*GameStates.CInGame instance = GameStates.CInGame.getInstance();
            _currentState.ChangeState(instance);
            _currentState.Initialize();*/

            gameStateList = new Dictionary<string, dynamic>();

            Type[] typelist = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "Editor.GameStates");
            for (int i = 0; i < typelist.Length; i++)
            {
                gameStateList.Add(typelist[i].Name, (dynamic)Activator.CreateInstance(typelist[i]));
            }

        }

        /// <summary>
        /// Load all the contents
        /// </summary>
        /// <param name="content">ContentManager class</param>
        /// <param name="graphics">GraphicsDevice class</param>
        /// <param name="spriteBatch">SpriteBatch class</param>
        public static void LoadContent(ContentManager content, GraphicsDevice graphics, SpriteBatch spriteBatch, GraphicsDeviceManager graphicsDevice)
        {
            _graphicsManager = graphicsDevice;
            _graphics = graphics;
            _content = content;
            _spriteBatch = spriteBatch;

            gameStateList[currentState].Initialize();
            gameStateList[currentState].LoadContent(content, spriteBatch, graphics);
        }

        public static void UnloadContent(ContentManager content)
        {
            gameStateList[currentState].UnloadContent(content);
        }

        public static void ChangeState(string newState)
        {
            if (IsValidGameState(newState))
            {
                Game.Script.CLuaVM.CallEvent("changeState", new object[] { currentState, newState });

                UnloadContent(_content);
                currentState = newState;
                gameStateList[currentState].Initialize();
                gameStateList[currentState].LoadContent(_content, _spriteBatch, _graphicsManager);
            }
        }

        public static bool IsValidGameState(string gameState)
        {
            return gameStateList.ContainsKey(gameState);
        }

        public static void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState)
        {
            gameStateList[currentState].Update(gameTime, kbState, mouseState, _oldMouseState);

            _oldMouseState = mouseState;
        }

        public static void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {
            gameStateList[currentState].Draw(spritebatch, gameTime);
        }

        private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

    }
}
