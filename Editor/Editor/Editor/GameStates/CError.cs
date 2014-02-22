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

namespace Engine.GameStates
{
    class CError
    {
        // Background
        private Rectangle _backgroundRectangle;
        private Color _backgroundColor;
        private Texture2D _backgroundTexture;

        // Image
        private Texture2D _deadSmiley;
        private Vector2 _smileyPosition;

        // Text
        private bool _displayError = false;
        private SpriteFont _errorFont;
        private Vector2 _errorPos;
        private Color _errorColor;
        private string _errorText;

        // Other
        private SpriteBatch spriteBatch;
        private GraphicsDevice graphics;


        public void LoadContent(ContentManager content, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            this.spriteBatch = spriteBatch;
            this.graphics = graphics;

            _backgroundTexture = new Texture2D(graphics, 1, 1);
            _backgroundTexture.SetData(new Color[] { Color.White });

            _backgroundRectangle = new Rectangle(0, 0, graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight);
            _backgroundColor = new Color(51, 51, 51, 255);

            _deadSmiley = content.Load<Texture2D>("2D/ErrorSmiley");
            _smileyPosition = new Vector2(graphics.PresentationParameters.BackBufferWidth / 2 - 57, graphics.PresentationParameters.BackBufferHeight / 2 - 57);

            _errorFont = content.Load<SpriteFont>("2D/consoleFont");
            _errorPos = new Vector2(graphics.PresentationParameters.BackBufferWidth / 2 - _errorFont.MeasureString(_errorText).X / 2, graphics.PresentationParameters.BackBufferHeight / 2 + 57 + 20);
            _errorColor = new Color(164, 0, 4, 80);
        }

        public void UnloadContent(ContentManager content)
        {

        }

        public void SendParam(object error)
        {
            _errorText = (string)error;
            _displayError = true;
        }

        public void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState, MouseState oldMouseState)
        {
            if (graphics.PresentationParameters.BackBufferWidth != _backgroundRectangle.Width || graphics.PresentationParameters.BackBufferHeight != _backgroundRectangle.Height)
            {
                _backgroundRectangle = new Rectangle(0, 0, graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight);
                _smileyPosition = new Vector2(graphics.PresentationParameters.BackBufferWidth / 2 - 57, graphics.PresentationParameters.BackBufferHeight / 2 - 57);

                if (_displayError)
                    _errorPos = new Vector2(graphics.PresentationParameters.BackBufferWidth / 2 - _errorFont.MeasureString(_errorText).X / 2, graphics.PresentationParameters.BackBufferHeight / 2 + 57 + 20);
            }
        }

        public void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_backgroundTexture, _backgroundRectangle, _backgroundColor);
            spritebatch.Draw(_deadSmiley, _smileyPosition, Color.White);
            if (_displayError)
                spriteBatch.DrawString(_errorFont, _errorText, _errorPos, _errorColor, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.End();
        }
    }
}
