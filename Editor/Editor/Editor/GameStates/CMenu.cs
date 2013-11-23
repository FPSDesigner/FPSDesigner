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
    class CMenu : Game.CGameState
    {
        #region "Singleton"
        // Singleton Code
        private static CMenu instance = null;
        private static readonly object myLock = new object();

        // Singelton Methods
        private CMenu() { }
        public static CMenu getInstance()
        {
            lock (myLock)
            {
                if (instance == null) instance = new CMenu();
                return instance;
            }
        }
        #endregion

        #region "Internal Datas Class"
        class menuDatas
        {
            #region "Constructor"
            public menuDatas(Game.LevelInfo.MenuButton[] menuButtons, Texture2D buttonsImage, Texture2D cursorImage,
                Texture2D backgroundImage, SoundEffect selectionSound, SoundEffect backgroundMusic)
            {
                this.menuButtons = menuButtons;

                this.buttonsImage = buttonsImage;
                this.cursorImage = cursorImage;
                this.backgroundImage = backgroundImage;

                this.selectionSound = selectionSound;
                this.backgroundMusic = backgroundMusic;
            }
            #endregion

            public Game.LevelInfo.MenuButton[] menuButtons;

            public int[,] buttonsPositions;
            public int buttonsCount = 0;
            public int selectedButton = -1;

            public Vector2 backgroundImageSize;
            public Vector2 cursorSize;
            public Vector2 cursorClickPosition;
            public Vector2 scaleCoefficient;
            public Vector2[] buttonsSize;

            public Texture2D buttonsImage;
            public Texture2D cursorImage;
            public Texture2D backgroundImage;

            public SoundEffect selectionSound;
            public SoundEffect backgroundMusic;

        }
        #endregion

        private menuDatas _menuData;
        private Game.LevelInfo.GameMenu _gameMenu;
        private Vector2 _mousePos;

        public CMenu(Game.LevelInfo.GameMenu GameMenu)
        {
            _gameMenu = GameMenu;
        }

        public override void loadContent(ContentManager content, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            _menuData = new menuDatas(_gameMenu.ButtonsInfo.MenuButton, content.Load<Texture2D>(_gameMenu.ButtonsInfo.ButtonsImages), content.Load<Texture2D>(_gameMenu.CursorFile),
                content.Load<Texture2D>(_gameMenu.BGImageFile), content.Load<SoundEffect>(_gameMenu.SelectionSound), content.Load<SoundEffect>(_gameMenu.BackgroundMusic));

            _menuData.buttonsCount = _gameMenu.ButtonsInfo.MenuButton.Count();

            _menuData.buttonsPositions = new int[_menuData.buttonsCount, 2];
            _menuData.buttonsSize = new Vector2[_menuData.buttonsCount];

            _menuData.backgroundImageSize = new Vector2(graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight);
            _menuData.scaleCoefficient = new Vector2(
                graphics.PresentationParameters.BackBufferWidth / _menuData.backgroundImage.Width,
                graphics.PresentationParameters.BackBufferHeight / _menuData.backgroundImage.Height);
            _menuData.cursorClickPosition = new Vector2(_gameMenu.CursorClickX * _menuData.scaleCoefficient.X, _gameMenu.CursorClickY * _menuData.scaleCoefficient.Y);
            _menuData.cursorSize = new Vector2(_menuData.cursorImage.Width * _menuData.scaleCoefficient.X, _menuData.cursorImage.Height * _menuData.scaleCoefficient.Y);

            for (int i = 0; i < _menuData.buttonsCount; i++)
            {
                _menuData.buttonsPositions[i, 0] = _gameMenu.ButtonsInfo.MenuButton[i].PosX * graphics.PresentationParameters.BackBufferWidth;
                _menuData.buttonsPositions[i, 1] = _gameMenu.ButtonsInfo.MenuButton[i].PosY * graphics.PresentationParameters.BackBufferHeight;

                _menuData.buttonsSize[i] = new Vector2(_gameMenu.ButtonsInfo.MenuButton[i].Width * _menuData.scaleCoefficient.X, _gameMenu.ButtonsInfo.MenuButton[i].Height * _menuData.scaleCoefficient.Y);
            }
        }

        public override void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState, MouseState oldMouseState)
        {
            _mousePos = new Vector2(mouseState.X, mouseState.Y);
            bool isOverAnyButton = false;

            for (int i = 0; i < _menuData.buttonsCount; i++)
            {
                if (_mousePos.X >= _menuData.buttonsPositions[i, 0] && _mousePos.X <= (_menuData.buttonsPositions[i, 0] + _menuData.menuButtons[i].Width))
                {
                    if (_mousePos.Y >= _menuData.buttonsPositions[i, 1] && _mousePos.Y <= (_menuData.buttonsPositions[i, 1] + _menuData.menuButtons[i].Height))
                    {
                        _menuData.selectedButton = i;
                        isOverAnyButton = true;
                    }
                }
            }

            if (!isOverAnyButton)
                _menuData.selectedButton = -1;

            // Clicks management
            if (isOverAnyButton)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                {
                    switch (_menuData.menuButtons[_menuData.selectedButton].Action)
                    {
                        case 1:
                            // Start InGame;
                            break;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {
            // Draw Background
            spritebatch.Draw(_menuData.backgroundImage, Vector2.Zero, null, Color.White, 0.0f, new Vector2(0.0f), _menuData.backgroundImageSize, SpriteEffects.None, 0);

            // Draw Buttons
            for (int i = 0; i < _menuData.buttonsCount; i++)
            {
                int displacementSelection = (_menuData.selectedButton == i) ? _menuData.backgroundImage.Height / 2 : 0;
                spritebatch.Draw(_menuData.buttonsImage, _menuData.buttonsSize[i],
                    new Rectangle(_menuData.menuButtons[i].ImgPosX, _menuData.menuButtons[i].ImgPosY + displacementSelection, (int)_menuData.buttonsSize[i].X, (int)_menuData.buttonsSize[i].Y),
                    Color.White, 0.0f, Vector2.Zero, new Vector2(), SpriteEffects.None, 0);
            }

            // Draw Cursor
            spritebatch.Draw(_menuData.cursorImage, _mousePos, null, Color.White, 0.0f, _menuData.cursorClickPosition, _menuData.cursorSize, SpriteEffects.None, 0);
        }

    }
}