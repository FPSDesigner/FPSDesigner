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
    public enum gameStates
    {
        Starting,
        Menu,
        Loading,
        Pause,
        InGame
    };

    public class CGameState
    {

        private gameStates _actualState;

        public CGameState(gameStates actualState)
        {
            this._actualState = actualState;
        }

        public gameStates GetGameState()
        {
            return this._actualState;
        }

        public void ChangeState(gameStates newState)
        {
            this._actualState = newState;
        }

        public bool IsDevVersion()
        {
#if (DEBUG)
            return true;
#else
            return false;
#endif
        }
    }
}
