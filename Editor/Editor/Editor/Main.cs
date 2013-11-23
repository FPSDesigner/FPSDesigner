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

namespace Editor
{
    /// <summary>
    /// The main file, core of the program
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState oldKeyboardState; //--| These 2 variables avec used to compare new state and old state (ex : Key pressed and released)
        MouseState oldMouseState;

        Game.CGameManagement GameManagement; // Class Created to relieving main's Class

        Game.LevelInfo.CLevelInfo levelInfo;

        Display2D.C2DEffect C2DEffect;
        Display2D.CRenderCapture renderCapture;
        Display2D.CPostProcessor postProcessor;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;
            Window.AllowUserResizing = true;

        }
        protected override void Initialize()
        {
            GameManagement = new Game.CGameManagement();
            GameManagement.Initialize();

            levelInfo = new Game.LevelInfo.CLevelInfo();

            C2DEffect = Display2D.C2DEffect.getInstance();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            GameManagement.loadContent(Content, GraphicsDevice, spriteBatch, graphics);

            renderCapture = new Display2D.CRenderCapture(GraphicsDevice);
            postProcessor = new Display2D.CPostProcessor(GraphicsDevice);

            C2DEffect.LoadContent(Content, GraphicsDevice, spriteBatch, postProcessor);
        }


        protected override void UnloadContent()
        {
            GameManagement.unloadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (base.IsActive)
            {
                KeyboardState kbState = Keyboard.GetState();
                MouseState mouseState = Mouse.GetState();

                // Quit program when 'Escape' key is pressed
                if (kbState.IsKeyDown(Keys.Escape))
                    this.Exit();

                GameManagement.Update(gameTime, kbState, mouseState);

                C2DEffect.Update(gameTime);

                oldKeyboardState = kbState;
                oldMouseState = mouseState;
                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // Capture the render
            //renderCapture.Begin();

            GraphicsDevice.Clear(Color.Black);

            // Draw "All" the State
            spriteBatch.Begin();
            GameManagement.Draw(spriteBatch, gameTime);
            spriteBatch.End();

            // Draw the Console effect
            C2DEffect.Draw(gameTime);

            // End capturing
            /*renderCapture.End();

            // Draw the render via post processing
            postProcessor.Input = renderCapture.GetTexture();
            postProcessor.Draw();*/

            base.Draw(gameTime);
        }
    }
}
