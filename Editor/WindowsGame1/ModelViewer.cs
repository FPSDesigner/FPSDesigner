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


namespace ModelViewer
{
    public class ModelViewer : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        CCamera camera;

        
        // WPF
        // Used to emulate XNA when embedded in WPF

        public WriteableBitmap em_WriteableBitmap { get; set; }
        private Point em_sizeViewport;
        private RenderTarget2D em_renderTarget2D;
        private byte[] em_bytes;
        private DispatcherTimer em_dispatcherTimer;
        private GameTime em_GameTime;
        private Stopwatch em_StopWatch;
        private TimeSpan em_LastTime;
        public bool isSoftwareEmbedded = false;

        private CModel _currentModel; 

        public ModelViewer(bool launchedFromSoftware = false)
        {
            isSoftwareEmbedded = launchedFromSoftware;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 700;
            graphics.IsFullScreen = false;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            

            // Icon
            if (System.IO.File.Exists("Icon.ico"))
                ((System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle)).Icon = new System.Drawing.Icon("Icon.ico");

            // WPF
            if (launchedFromSoftware)
            {
                em_sizeViewport = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                em_WriteableBitmap = new WriteableBitmap(em_sizeViewport.X, em_sizeViewport.Y, 96, 96, PixelFormats.Bgr565, null);
                em_bytes = new byte[em_sizeViewport.X * em_sizeViewport.Y * 2];

                em_LastTime = new TimeSpan();
                em_GameTime = new GameTime();
                em_dispatcherTimer = new DispatcherTimer();
                em_dispatcherTimer.Interval = TimeSpan.FromSeconds(1 / 60);
                em_dispatcherTimer.Tick += new EventHandler(GameLoop);

                this.Initialize();
                this.LoadContent();
                em_dispatcherTimer.Start();
                em_StopWatch = new Stopwatch();
                em_StopWatch.Start();
            }
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            ChangeEmbeddedViewport(Window.ClientBounds.Width, Window.ClientBounds.Height);
        }

        public void ChangeEmbeddedViewport(int width, int height)
        {
            if (width % 2 != 0)
                width++;

            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;

            if (isSoftwareEmbedded)
            {
                em_sizeViewport = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                em_WriteableBitmap = new WriteableBitmap(em_sizeViewport.X, em_sizeViewport.Y, 96, 96, PixelFormats.Bgr565, null);
                em_bytes = new byte[em_sizeViewport.X * em_sizeViewport.Y * 2];
                em_renderTarget2D = new RenderTarget2D(GraphicsDevice, em_sizeViewport.X, em_sizeViewport.Y, true, SurfaceFormat.Bgr565, DepthFormat.Depth16);
            }

            //graphics.ApplyChanges(); // Seem to be crashing
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
                em_renderTarget2D = new RenderTarget2D(GraphicsDevice, em_sizeViewport.X, em_sizeViewport.Y, true, SurfaceFormat.Bgr565, DepthFormat.Depth16);
            }

            camera = new CCamera(GraphicsDevice, Vector3.Up + Vector3.UnitX, Vector3.Zero, 0.5f, 10000f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {

        }

        public void LoadNewModel(string modelUri, Dictionary<String, Texture2D> textures,Vector3 modelRotation,float alpha = 1)
        {
            _currentModel = new CModel(
                Content.Load<Model>(modelUri),
                modelRotation,
                new Vector3(1),
                GraphicsDevice,
                textures,
                0,
                alpha);
        }

        protected override void Update(GameTime gameTime)
        {
            if (isSoftwareEmbedded || base.IsActive)
            {
                if (isSoftwareEmbedded)
                {
                    TimeSpan currentTime = em_StopWatch.Elapsed;
                    em_GameTime = new GameTime(currentTime, currentTime - em_LastTime);
                    em_LastTime = currentTime;
                }

                KeyboardState kbState = Keyboard.GetState();
                MouseState mouseState = Mouse.GetState();

                if (_currentModel != null)
                {
                    _currentModel.Update(gameTime);
                }

                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // WPF
            if (isSoftwareEmbedded)
                GraphicsDevice.SetRenderTarget(em_renderTarget2D);

            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            if (_currentModel != null)
            {
                _currentModel.Draw(camera._view, camera._projection);
            }

            base.Draw(gameTime);

            // WPF
            if (isSoftwareEmbedded)
            {
                GraphicsDevice.SetRenderTarget(null);

                em_renderTarget2D.GetData(em_bytes);
                em_WriteableBitmap.Lock();
                System.Runtime.InteropServices.Marshal.Copy(em_bytes, 0, em_WriteableBitmap.BackBuffer, em_bytes.Length);
                em_WriteableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, em_sizeViewport.X, em_sizeViewport.Y));
                em_WriteableBitmap.Unlock();
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            this.Update(em_GameTime);
            this.Draw(em_GameTime);
        }

    }
}
