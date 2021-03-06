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

using System.Windows.Media.Imaging;
using System.Windows.Threading;

using System.Diagnostics;

using LTreesLibrary.Trees;


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

        private bool useTree = false;
        public SimpleTree tree;
        private LTreesLibrary.Trees.Wind.WindStrengthSin treeWind;
        private LTreesLibrary.Trees.Wind.TreeWindAnimator treeAnimator;
        private Matrix treeMatrix;
        private Matrix[] treeMatrices;
        private float TreeRotation = 0f;
        public float treeScale = 0f;  
        private bool treeUseWind = true;
        private bool treeUseBranches = true;
        

        private CModel _currentModel;

        // Scale
        private Vector2 _maxResolution = new Vector2(1920f, 1080f);
        private Vector3 _scalingFactor = new Vector3(1f, 1f, 1f);

        // Informations about the model
        private SpriteFont _modelFontName;

        public ModelViewer(bool launchedFromSoftware = false)
        {
            isSoftwareEmbedded = launchedFromSoftware;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.Window.Title = "Model Viewer";

            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 700;
            graphics.IsFullScreen = false;
            Window.AllowUserResizing = true;

            // Icon
            if (System.IO.File.Exists("Icon.ico"))
                ((System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle)).Icon = new System.Drawing.Icon("Icon.ico");

            // WPF
            if (launchedFromSoftware)
            {
                Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
                em_sizeViewport = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                em_WriteableBitmap = new WriteableBitmap(em_sizeViewport.X, em_sizeViewport.Y, 96, 96, System.Windows.Media.PixelFormats.Bgr565, null);
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
            graphics.ApplyChanges(); // Seem to be crashing
            if (isSoftwareEmbedded)
            {
                //camera.ChangeProjectionMatrix(width, height, graphics.GraphicsDevice);
                em_sizeViewport = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                em_WriteableBitmap = new WriteableBitmap(em_sizeViewport.X, em_sizeViewport.Y, 96, 96, System.Windows.Media.PixelFormats.Bgr565, null);
                em_bytes = new byte[em_sizeViewport.X * em_sizeViewport.Y * 2];
                em_renderTarget2D = new RenderTarget2D(GraphicsDevice, em_sizeViewport.X, em_sizeViewport.Y, true, SurfaceFormat.Bgr565, DepthFormat.Depth16);
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
                em_renderTarget2D = new RenderTarget2D(GraphicsDevice, em_sizeViewport.X, em_sizeViewport.Y, true, SurfaceFormat.Bgr565, DepthFormat.Depth16);
            }

            camera = new CCamera(GraphicsDevice, Vector3.Up + new Vector3(50f), Vector3.Zero, 0.5f, 10000f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            /*Dictionary<string, Texture2D> text = new Dictionary<string, Texture2D>();
            text.Add("Barrel", Content.Load<Texture2D>("11"));

            LoadNewModel("B", text, new Vector3(0), 1.0f);

            _modelFontName = Content.Load<SpriteFont>("Font1");*/
        }

        protected override void UnloadContent()
        {

        }

        public void LoadNewTree(string profileName, int seed = -1, bool displayBranch = true, bool wind = true)
        {
            TreeProfile profile = Content.Load<TreeProfile>(profileName);

            if (seed == -1)
                tree = profile.GenerateSimpleTree();
            else
            {
                Random rand = new Random(seed);
                tree = profile.GenerateSimpleTree(rand);
            }

            useTree = true;

            treeUseBranches = displayBranch;
            treeUseWind = wind;

            // Wind
            treeWind = new LTreesLibrary.Trees.Wind.WindStrengthSin();
            treeAnimator = new LTreesLibrary.Trees.Wind.TreeWindAnimator(treeWind);

            // Scale
            ChangeCameraZoom(0);
            
            float BoundingSphereRadius = (float)(new Ray(Vector3.Zero, Vector3.Up)).Intersects(camera._frustum.Top);

            treeScale = BoundingSphereRadius / tree.TrunkMesh.BoundingSphere.Radius;
            treeMatrices = new Matrix[2];
            treeMatrices[0] = Matrix.CreateScale(treeScale);
            treeMatrices[1] = Matrix.CreateTranslation(new Vector3(0, -BoundingSphereRadius / 2, 0));
        }

        public void LoadNewModel(string modelUri, Dictionary<String, Texture2D> textures, Vector3 modelRotation, float alpha = 1)
        {
            _currentModel = new CModel(
                Content.Load<Model>(modelUri),
                modelRotation,
                new Vector3(1),
                GraphicsDevice,
                textures,
                0,
                alpha);

            if (camera._frustum.Contains(_currentModel.BoundingSphere) == ContainmentType.Contains)
            {
                while (camera._frustum.Contains(_currentModel.BoundingSphere) == ContainmentType.Contains)
                {
                    _currentModel._modelScale += new Vector3(1f);
                    _currentModel.buildBoundingSphere();
                }
            }
            else
            {
                while (camera._frustum.Contains(_currentModel.BoundingSphere) != ContainmentType.Contains)
                {
                    _currentModel._modelScale -= new Vector3(0.1f);
                    _currentModel.buildBoundingSphere();
                }
            }
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
                camera.Update(gameTime);

                if (_currentModel != null)
                {
                    _currentModel.Update(gameTime);
                }

                if (useTree)
                {
                    TreeRotation += 0.005f;
                    treeMatrix = treeMatrices[0] * Matrix.CreateRotationY(TreeRotation) * treeMatrices[1];
                    if (treeUseWind)
                    {
                        treeWind.Update(gameTime);
                        treeAnimator.Animate(tree.Skeleton, tree.AnimationState, gameTime);
                    }
                }

                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // WPF
            if (isSoftwareEmbedded)
                GraphicsDevice.SetRenderTarget(em_renderTarget2D);

            GraphicsDevice.Clear(new Microsoft.Xna.Framework.Color(78, 77, 71));

            if (_currentModel != null)
            {
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

                //_currentModel.Draw(camera._view, camera._projection);
            }

            if (useTree)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.BlendState = BlendState.AlphaBlend;

                tree.DrawTrunk(treeMatrix, camera._view, camera._projection);
                if (treeUseBranches)
                    tree.DrawLeaves(treeMatrix, camera._view, camera._projection);
            }


            /*
            // Draw the text info

            int xPos = (40 * this.GraphicsDevice.Viewport.Width) / 1920;
            int yPos = (40 * this.GraphicsDevice.Viewport.Height) / 1080;

            _scalingFactor.X = (this.GraphicsDevice.PresentationParameters.BackBufferWidth) / _maxResolution.X;
            _scalingFactor.Y = (this.GraphicsDevice.PresentationParameters.BackBufferHeight) / _maxResolution.Y;

            Microsoft.Xna.Framework.Matrix globalTransformation = Microsoft.Xna.Framework.Matrix.CreateScale(_scalingFactor);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, globalTransformation);
            spriteBatch.DrawString(_modelFontName, "Model : Barrel \nPoly Count : ", new Vector2(xPos, yPos), Microsoft.Xna.Framework.Color.White);
            spriteBatch.End();
            */


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
        public int GetTreeData(int data)
        {
            switch (data)
            {
                case 0:
                    return tree.TrunkMesh.NumberOfTriangles;
                case 1:
                    return tree.TrunkMesh.NumberOfVertices;
                case 2:
                    return tree.Skeleton.Leaves.Count;
                case 3:
                    return tree.Skeleton.Bones.Count;
            }
            return 0;
        }

        /// <summary>
        /// Change the camera zoom
        /// </summary>
        /// <param name="zoom">Negative value to zoom in, positive to zoom out.</param>
        public void ChangeCameraZoom(float zoom)
        {
            if (zoom == 0 || zoom < 0 && camera._cameraPos.Y <= 1.0f)
                return;

            camera._cameraPos = Vector3.Up + new Vector3(zoom);
            camera.ReloadFrustum();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            this.Update(em_GameTime);
            this.Draw(em_GameTime);
        }

    }
}
