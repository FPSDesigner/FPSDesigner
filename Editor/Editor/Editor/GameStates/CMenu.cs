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

namespace Editor.GameStates
{
    class CMenu: Game.CGameState
    {
        public Texture2D _background {get; private set; } // Background texture
        public Texture2D _buttonNewGame { get; private set; }//-|
        public Texture2D _buttonOptions { get; private set; }//-|All the texture "button" to lauchn the game ...
        public Texture2D _buttonExit { get; private set; }//----|
        public Texture2D _cursorTexture { get; private set; } // CursorSprite

        public Vector2 _mousePos { get; private set; }

        public SoundEffect _selectionSound { get; private set; } //Cursor sound

        public CMenu(Texture2D backgrnd, Texture2D buttonNGame, Texture2D ButtonOptns, Texture2D ButtonExt, Texture2D cursor, SoundEffect selectionSound)
        {
            this._background = backgrnd;
            this._buttonNewGame = buttonNGame;
            this._buttonOptions = _buttonOptions;
            this._buttonExit = ButtonExt;
            this._cursorTexture = cursor;
            this._selectionSound = selectionSound;
        }

        public override void Initialize()
        {
            _mousePos = new Vector2(0.0f);
        }

        public override void loadContent(ContentManager content)
        {

        }

        public override void unloadContent(ContentManager content)
        {

        }

        public override void Update(GameTime gametime)
        {

        }

        public override void Draw(SpriteBatch spritebatch, GameTime gametime)
        {

        }
    }
}
