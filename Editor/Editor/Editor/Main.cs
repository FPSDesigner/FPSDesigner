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

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using System.Diagnostics;

namespace Engine
{
    /// <summary>
    /// The main file, core of the program
    /// </summary>
    public class MainGameEngine : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState oldKeyboardState; //--| These 2 variables avec used to compare new state and old state (ex : Key pressed and released)
        MouseState oldMouseState;

        Display2D.CRenderCapture renderCapture;
        Display2D.CPostProcessor postProcessor;

        public bool isSoftwareEmbedded = false;

        // WPF
        // Objet qui sera transmis au WPF
        public WriteableBitmap WriteableBitmap { get; set; }

        // A de multiple endroit nous avons besoin de savoir la taille de l'affichage XNA
        private Point m_sizeViewport;
        // Objet qui contiendra notre scène après le rendu
        private RenderTarget2D m_renderTarget2D;
        // Servira pour faire la conversion du RenderTarget vers le WritableBitmap
        private byte[] m_bytes;
        // Les fonctionnements interne de XNA étant bypassés, il faut un timer
        private DispatcherTimer m_dispatcherTimer;

        private GameTime m_GameTime;
        private Stopwatch stopwatch_gt;

        public MainGameEngine(bool launchedFromSoftware = false)
        {
            isSoftwareEmbedded = launchedFromSoftware;

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 700;
            graphics.IsFullScreen = false;
            Window.AllowUserResizing = false;

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;


            // Icon
            if (System.IO.File.Exists("Icon.ico"))
                ((System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle)).Icon = new System.Drawing.Icon("Icon.ico");

            // WPF
            if (isSoftwareEmbedded)
            {
                // On prepare la resolution du rendu, l'image de sorti, l'image en entré, les bytes pour la conversion
                m_sizeViewport = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                WriteableBitmap = new WriteableBitmap(m_sizeViewport.X, m_sizeViewport.Y, 96, 96, PixelFormats.Bgr565, null);
                m_bytes = new byte[m_sizeViewport.X * m_sizeViewport.Y * 2];

                m_GameTime = new GameTime();
                m_dispatcherTimer = new DispatcherTimer();
                m_dispatcherTimer.Interval = TimeSpan.FromSeconds(1 / 60);
                m_dispatcherTimer.Tick += new EventHandler(GameLoop);

                this.Initialize();
                this.LoadContent();
                m_dispatcherTimer.Start();
                stopwatch_gt = new Stopwatch();
                stopwatch_gt.Start();
            }
        }

        protected override void Initialize()
        {
            if (isSoftwareEmbedded)
            {
                // Create the graphics device
                IGraphicsDeviceManager graphicsDeviceManager = this.Services.GetService(typeof(IGraphicsDeviceManager)) as IGraphicsDeviceManager;
                if (graphicsDeviceManager != null)
                    graphicsDeviceManager.CreateDevice();
                else
                    throw new Exception("Unable to retrieve GraphicsDeviceManager");

                // Width must a multiple of 2
                m_renderTarget2D = new RenderTarget2D(GraphicsDevice, m_sizeViewport.X, m_sizeViewport.Y, true, SurfaceFormat.Bgr565, DepthFormat.Depth16);
            }

            Game.CGameManagement.currentState = "CInGame";
            Game.CGameManagement.Initialize();

            SamplerState sState = new SamplerState();
            sState.Filter = TextureFilter.Linear;
            graphics.GraphicsDevice.SamplerStates[0] = sState;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            renderCapture = new Display2D.CRenderCapture(GraphicsDevice);
            postProcessor = new Display2D.CPostProcessor(GraphicsDevice);

            Display2D.C2DEffect.LoadContent(Content, GraphicsDevice, spriteBatch, postProcessor, renderCapture);
            Display2D.C2DEffect.isSoftwareEmbedded = isSoftwareEmbedded;
            Display2D.C2DEffect.renderTarget = (isSoftwareEmbedded) ? m_renderTarget2D : renderCapture.renderTarget;

            Display3D.Particles.ParticlesManager.LoadContent(GraphicsDevice);
            Game.Settings.CGameSettings.LoadDatas(GraphicsDevice);
            Game.CConsole.LoadContent(Content, GraphicsDevice, spriteBatch, true, true/*false*/);
            Game.CConsole._activationKeys = Game.Settings.CGameSettings._gameSettings.KeyMapping.Console;

            try
            {
                Game.CGameManagement.LoadContent(Content, GraphicsDevice, spriteBatch, graphics);
            }
            catch (Exception e)
            {
                Game.CGameManagement.ChangeState("CError");
                Game.CGameManagement.SendParam("Error encountered\n\nCheck logs for more information");
                Game.CConsole.WriteLogs(e.ToString());
            }

            /*Game.Script.CLuaVM.Initialize();
            Game.Script.CLuaVM.LoadScript("GuiManager.lua");*/
        }


        protected override void UnloadContent()
        {
            Game.CGameManagement.UnloadContent(Content);
        }

        TimeSpan lastTime = new TimeSpan();
        protected override void Update(GameTime gameTime)
        {
            if (isSoftwareEmbedded || base.IsActive)
            {
                if (isSoftwareEmbedded)
                {
                    TimeSpan currentTime = stopwatch_gt.Elapsed;
                    m_GameTime = new GameTime(currentTime, currentTime - lastTime);
                    lastTime = currentTime;
                }
                KeyboardState kbState = Keyboard.GetState();
                MouseState mouseState = Mouse.GetState();

                if (Game.Script.CLuaVM._settingEnableHighFreqCalls)
                    Game.Script.CLuaVM.CallEvent("framePulse");

                // Quit program when 'Escape' key is pressed
                if (kbState.IsKeyDown(Keys.Escape))
                    this.Exit();

                Game.CGameManagement.Update(gameTime, kbState, mouseState);

                Display2D.C2DEffect.Update(gameTime, kbState, mouseState);
                Game.CConsole.Update(kbState, gameTime);

                oldKeyboardState = kbState;
                oldMouseState = mouseState;
                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // WPF
            if (isSoftwareEmbedded)
                GraphicsDevice.SetRenderTarget(m_renderTarget2D);

            // Capture the render
            renderCapture.Begin();
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            // Draw "All" the State
            Game.CGameManagement.Draw(spriteBatch, gameTime);

            // Draw the Console effect
            Display2D.C2DEffect.Draw(gameTime);
            Game.CConsole.Draw(gameTime);

            // End capturing
            renderCapture.End();

            // Draw the render via post processing
            postProcessor.Input = renderCapture.GetTexture();
            postProcessor.Draw(gameTime);

            base.Draw(gameTime);

            // WPF
            if (isSoftwareEmbedded)
            {
                GraphicsDevice.SetRenderTarget(null);

                m_renderTarget2D.GetData(m_bytes);

                WriteableBitmap.Lock();
                System.Runtime.InteropServices.Marshal.Copy(m_bytes, 0, WriteableBitmap.BackBuffer, m_bytes.Length);
                WriteableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, m_sizeViewport.X, m_sizeViewport.Y));
                WriteableBitmap.Unlock();
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            this.Update(m_GameTime);
            this.Draw(m_GameTime);
        }

    }
}
