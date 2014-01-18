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

        public virtual void loadContent(ContentManager content, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {

        }

        public virtual void unloadContent(ContentManager content)
        {

        }

        public virtual void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState, MouseState oldMouseState)
        {

        }

        public virtual void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {

        }

        public virtual void SendParam(object param)
        {

        }
    }
}
