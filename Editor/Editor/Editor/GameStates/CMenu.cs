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
        //Define all the attributes : Sprites, Sounds, etc
        #region "Node - Attributes"
        public GraphicsDevice _graphics { get; private set; }
        // Bckgrnd Sprite 
        public Texture2D _background {get; private set; }
        // Title Sprite
        public Texture2D _title { get; private set; }
        //-|All the buttons in ONE texture (sprite)
        public Texture2D _buttons { get; private set; }
        // CursorSprite
        public Texture2D _cursorTexture { get; private set; } 
        
        public Vector2 _mousePos { get;  private set; }
        public Vector2 _buttonsPos {  get; private set; }
        public Vector2 _titlePos { get; private set; }

        //Cursor sound
        public SoundEffect _selectionSound { get; private set; }

        //Sprite Width(Menu)
        public int _widthButtons { get; private set; }
        //Sprite Height (Menu)
        public int _heightButtons { get; private set; }

        //Button Number (help us to use the menu)
        public int _buttonNumber { get; private set; }

        //coefficient used to draw sprites according to resolution
        public float[] _scaleCoefficient { get; private set; }

        //This class contain all buttons and an image
        private Game.LevelInfo.ButtonsInfo _buttonClass;
        private Game.LevelInfo.MenuButton[] _buttonList;

        #endregion

        public CMenu(Texture2D backgrnd, Texture2D buttons,Vector2 buttonPos, int heightButtons,int widthButtons,int buttonsNumber,Texture2D title, 
            Vector2 titlePos,Texture2D cursor, SoundEffect selectionSound,Game.LevelInfo.ButtonsInfo buttonClass,GraphicsDevice graphics)
        {
            #region "Node - Attributes Initialisation"
            this._background = backgrnd;
            this._buttons = buttons;
            this._title = title;
            this._cursorTexture = cursor;
            this._selectionSound = selectionSound;

            this._graphics = graphics;

            this._buttonsPos = buttonPos;
            this._titlePos = titlePos;

            this._heightButtons = heightButtons;
            this._widthButtons = widthButtons;
            this._buttonNumber = buttonsNumber;

            this._buttonClass = buttonClass;

            #endregion
        }

        public override void Initialize()
        {
            _mousePos = new Vector2(0.0f);

            _scaleCoefficient = new float [2];
            _scaleCoefficient[0] = _graphics.Viewport.Width / 1920f; //Scale in x
            _scaleCoefficient[1] = _graphics.Viewport.Height / 1080f; //Scale in y

            // We get the button's List
            _buttonList = _buttonClass.MenuButton;
        }

        public override void loadContent(ContentManager content)
        {
            
        }

        public override void unloadContent(ContentManager content)
        {

        }

        public override void Update(GameTime gametime, MouseState mouseState, MouseState oldMouseState, GameStates.CInGame inGame)
        {
            _mousePos = new Vector2(mouseState.X, mouseState.Y);

            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
            {
                Game.CGameStateManager.getInstance().ChangeState(inGame);
            }
        }

        public override void Draw(SpriteBatch spritebatch, GameTime gametime, MouseState mouseState, MouseState oldMouseState, GameStates.CInGame inGame)
        {
            //Draw Background
            spritebatch.Draw(_background, Vector2.Zero, null, Color.White, 0.0f, new Vector2(0.0f), new Vector2(_scaleCoefficient[0], _scaleCoefficient[1]), SpriteEffects.None, 0);
            //Draw The Title
            spritebatch.Draw(_title, new Vector2(_titlePos.X * _scaleCoefficient[0], _titlePos.Y * _scaleCoefficient[1]), null, Color.White, 0.0f, new Vector2(0.0f), new Vector2(_scaleCoefficient[0], _scaleCoefficient[1]), SpriteEffects.None, 0);
            //Draw The Menu
            SelectionMenu(spritebatch, mouseState, oldMouseState, inGame);
            //Draw The Cursor
            spritebatch.Draw(_cursorTexture, _mousePos, null, Color.White, 0.0f, 
                new Vector2(_cursorTexture.Width/2,_cursorTexture.Height/2), new Vector2(_scaleCoefficient[0], _scaleCoefficient[1]), SpriteEffects.None, 0);
            
        }

        //Function used to animate the menu
        private void SelectionMenu(SpriteBatch spritebatch, MouseState mouseState, MouseState oldMouseState, GameStates.CInGame inGame)
        {
            //Just to get swtitched on button
            int buttonNbr = (_buttonList.Length / 2);

            for (int i = 0; i != _buttonList.Length / 2; i++){
                spritebatch.Draw(_buttons,new Vector2(_buttonList[i]._posX * _scaleCoefficient[0], _buttonList[i]._posY * _scaleCoefficient[1]),
                    new Rectangle(_buttonList[i]._imgPosX,_buttonList[i]._imgPosY,_buttonList[i]._width,_buttonList[i]._height),
                    Color.White, 0.0f, Vector2.Zero, new Vector2(_scaleCoefficient[0], _scaleCoefficient[1]), SpriteEffects.None, 0);

                if (_mousePos.X >= (_buttonList[i]._posX * _scaleCoefficient[0]) && _mousePos.X <= (_buttonList[i]._posX + _buttonList[i]._width) * _scaleCoefficient[0])
                {
                    if (_mousePos.Y >= (_buttonList[i]._posY * _scaleCoefficient[1]) && _mousePos.Y <= (_buttonList[i]._posY + _buttonList[i]._height) * _scaleCoefficient[1])
                    {
                        //Just draw the switched on button
                        spritebatch.Draw(_buttons, new Vector2(_buttonList[i]._posX * _scaleCoefficient[0], _buttonList[i]._posY * _scaleCoefficient[1]),
                        new Rectangle(_buttonList[buttonNbr]._imgPosX, _buttonList[buttonNbr]._imgPosY, _buttonList[buttonNbr]._width, _buttonList[buttonNbr]._height),
                        Color.White, 0.0f, Vector2.Zero, new Vector2(_scaleCoefficient[0], _scaleCoefficient[1]), SpriteEffects.None, 0);

                        //If he clicks
                        if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                        {
                            //All the possible
                            switch (_buttonList[i]._action)
                            {
                                case 1:
                                    Game.CGameStateManager.getInstance().ChangeState(inGame);
                                    break;
                            }
                        }
                    }

                }
            }
        }
    }
}
