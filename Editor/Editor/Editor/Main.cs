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
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState oldKeyboardState;

        Game.CGameState gameState;
        Game.CConsole devConsole;
        Game.CLevelInfo levelInfo;
        Display2D.C2DEffect C2DEffect;
        Display2D.CRenderCapture renderCapture;
        Display2D.CPostProcessor postProcessor;
        Display3D.CModel model;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            gameState = new Game.CGameState(Game.gameStates.Starting);
            devConsole = new Game.CConsole(true, true);
            levelInfo = new Game.CLevelInfo();
            C2DEffect = Display2D.C2DEffect.getInstance();

            if (gameState.IsDevVersion())
            {
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 800;
                Window.AllowUserResizing = true;
            }
            else
            {
                graphics.IsFullScreen = true;
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
        }
        protected override void Initialize()
        {

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            renderCapture = new Display2D.CRenderCapture(GraphicsDevice);
            postProcessor = new Display2D.CPostProcessor(GraphicsDevice);

            devConsole.LoadContent(Content, GraphicsDevice, spriteBatch);
            C2DEffect.LoadContent(Content, GraphicsDevice, spriteBatch, postProcessor);


            model = new Display3D.CModel(Content.Load<Model>("3D//building"), new Vector3(0, 0, 0), new Vector3(0, -90f, 0), new Vector3(1.0f, 1.0f, 1.0f), GraphicsDevice);
        }


        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kbState = Keyboard.GetState();

            // Quit program when 'Escape' key is pressed
            if (kbState.IsKeyDown(Keys.Escape))
                this.Exit();

            // GetState Test
            if (kbState.IsKeyDown(Keys.Space))
                gameState.ChangeState(Game.gameStates.InGame);

            // Update Console
            devConsole.Update(kbState, gameTime);
            C2DEffect.Update(gameTime);

            oldKeyboardState = kbState;
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            if (gameState.GetGameState() != Game.gameStates.Starting)
            {
                // Capture the render
                renderCapture.Begin();

                GraphicsDevice.Clear(Color.CornflowerBlue);

                Matrix view = Matrix.CreateLookAt(new Vector3(0f, 500f, 2000f), new Vector3(0, 0, 0), Vector3.Up);
                Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), GraphicsDevice.Viewport.AspectRatio, 0.1f, 5000.0f);

                model.Draw(view, projection);

                // Draw the console
                devConsole.Draw(gameTime);
                C2DEffect.Draw(gameTime);


                // End capturing
                renderCapture.End();

                // Draw the render via post processing
                postProcessor.Input = renderCapture.GetTexture();
                postProcessor.Draw();


                base.Draw(gameTime);
            }
        }
    }
}
