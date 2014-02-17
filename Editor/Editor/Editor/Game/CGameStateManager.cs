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
        public static Game.CGameState actualState;

        public static string gameStateName;

        public static ContentManager content;
        public static GraphicsDevice graphics;
        public static SpriteBatch spriteBatch;
        public static GraphicsDeviceManager graphicsDevice;

        public static void ChangeState(Game.CGameState newState)
        {
            gameStateName = newState.GetType().Name;
            Game.Script.CLuaVM.CallEvent("changeState", new object[] { (actualState == null) ? "null" : actualState.GetType().Name, (newState == null) ? "null" : gameStateName });
            actualState = newState;
        }

        public static void Initialize()
        {
            actualState.Initialize();
        }

        public static void loadContent()
        {
            actualState.LoadContent(content, spriteBatch, graphics);
        }

        public static void SendParam(object param)
        {
            actualState.SendParam(param);
        }
    }
}
