using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Game
{
    class CGameStateManager
        // THIS CLASS IS USED TO MANAGE GAMESTATES, CHANGE IT, ETC ...
    {
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

        public Game.CGameState _state;

        public void ChangeState(Game.CGameState futurState)
        {
            _state = futurState;
            _state.Initialize();
        }
    }
}
