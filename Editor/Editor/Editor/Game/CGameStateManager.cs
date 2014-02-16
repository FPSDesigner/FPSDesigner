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
    class CGameStateManager
    {
        #region "Singleton"
        // Singleton Code
        private static CGameStateManager instance = null;
        private static readonly object myLock = new object();

        // Singelton Methods
        private CGameStateManager() { }
        public static CGameStateManager getInstance()
        {
            lock (myLock)
            {
                if (instance == null) instance = new CGameStateManager();
                return instance;
            }
        }
        #endregion

        public Game.CGameState actualState;

        public ContentManager content;
        public GraphicsDevice graphics;
        public SpriteBatch spriteBatch;
        public GraphicsDeviceManager graphicsDevice;

        public void ChangeState(Game.CGameState newState)
        {
            Game.Script.CLuaVM.CallEvent("changeState", new object[] { (actualState == null) ? "null" : actualState.GetType().Name, (newState == null) ? "null" : newState.GetType().Name });
            actualState = newState;
        }

        public void Initialize()
        {
            actualState.Initialize();
        }

        public void loadContent()
        {
            actualState.LoadContent(content, spriteBatch, graphics);
        }

        public void SendParam(object param)
        {
            actualState.SendParam(param);
        }
    }
}
