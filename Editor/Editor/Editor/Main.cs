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
        KeyboardState oldKeyboardState; //--| These 2 variables avec used to compare new state and old state (ex : Key pressed and released)
        MouseState oldMouseState;// --------|

        Game.CGameState gameState;
        Game.CConsole devConsole;
        Game.LevelInfo.CLevelInfo levelInfo;
        Game.Settings.CGameSettings gameSettings;
        Display2D.C2DEffect C2DEffect;
        Display2D.CRenderCapture renderCapture;
        Display2D.CPostProcessor postProcessor;

        Display3D.CModel model;
        Display3D.CCamera cam;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            gameState = new Game.CGameState(Game.gameStates.Starting);
            devConsole = new Game.CConsole(true, true);
            levelInfo = new Game.LevelInfo.CLevelInfo();
            gameSettings = Game.Settings.CGameSettings.getInstance();
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
            cam = new Display3D.CCamera(GraphicsDevice, new Vector3(100f, 250f, 2000f), new Vector3(-100.0f, -250.0f, -2000.0f), 0.1f, 10000.0f);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            renderCapture = new Display2D.CRenderCapture(GraphicsDevice);
            postProcessor = new Display2D.CPostProcessor(GraphicsDevice);

            devConsole.LoadContent(Content, GraphicsDevice, spriteBatch, cam);
            C2DEffect.LoadContent(Content, GraphicsDevice, spriteBatch, postProcessor);
            gameSettings.loadDatas(GraphicsDevice);

            devConsole.changeActivationKeys(gameSettings._gameSettings.KeyMapping.Console);

            model = new Display3D.CModel(Content.Load<Model>("3D//building"), new Vector3(0, 0, 0), new Vector3(0, -90f, 0), new Vector3(1.0f, 1.0f, 1.0f), GraphicsDevice);

            
        }


        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kbState = Keyboard.GetState(); 
            MouseState mouseState = Mouse.GetState();
            
            // Quit program when 'Escape' key is pressed
            if (kbState.IsKeyDown(Keys.Escape))
                this.Exit();

            // GetState Test
            if (kbState.IsKeyDown(Keys.Space))
                gameState.ChangeState(Game.gameStates.InGame);

            // Update Console
            devConsole.Update(kbState, gameTime);
            C2DEffect.Update(gameTime);

            cam.Update(gameTime, kbState, mouseState);

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

                
                model.Draw(cam._view, cam._projection);

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
