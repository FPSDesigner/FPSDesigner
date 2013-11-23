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
    public abstract class CGameState
    // EACH "PART" OF GAME IS A GAMESTATE (MENU, GAME, OPTIONS, ...)
    {
        public virtual void Initialize()
        {

        }

        public virtual void loadContent(ContentManager content)
        {

        }

        public virtual void unloadContent(ContentManager content)
        {

        }

        public virtual void Update(GameTime gametime, MouseState mouseState)
        {

        }

        public virtual void Draw(SpriteBatch spritebatch, GameTime gametime)
        {

        }
    }
}
