using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Editor.Display3D
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class CWater : GameComponent
    {
        #region Fields

        //vertex and index buffers for the water plane
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        private Vector3 _position;

        private Effect _effect;

        public TextureCube CubeMap;

        //scrolling normal maps that we will use as a
        //a normal for the water plane in the shader
        private Texture2D _waveMap0;
        private Texture2D _waveMap1;
        private RenderTarget2D _waveMapRT0;
        private RenderTarget2D _waveMapRT1;

        //user specified options to configure the water object
        public CWaterOptions Options;
        public float WaveHeight;

        private PostProcessor _postProcessor;

        private int _numVertices;
        private int _numTris;

        #endregion


        public CWater(Game game)
            : base(game)
        { }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            //build the water mesh
            _numVertices = Options.Width * Options.Height;
            _numTris = (Options.Width - 1) * (Options.Height - 1) * 2;
            var vertices = new VertexPositionTexture[_numVertices];

            Vector3[] verts;
            int[] indices;

            _position = Options.WaterPosition;

            //create the water vertex grid positions and indices
            GenTriGrid(Options.Height, Options.Width, Options.CellSpacing, Options.CellSpacing,
                       _position, out verts, out indices);

            //copy the verts into our PositionTextured array
            for (int i = 0; i < Options.Width; ++i)
            {
                for (int j = 0; j < Options.Height; ++j)
                {
                    int index = i * Options.Width + j;
                    vertices[index].Position = verts[index];
                    vertices[index].TextureCoordinate = new Vector2((float)j / Options.Width, (float)i / Options.Height);
                }
            }

            _vertexBuffer = new VertexBuffer(Game.GraphicsDevice,
                                          typeof(VertexPositionTexture), Options.Width * Options.Height,
                                          BufferUsage.WriteOnly);
            _vertexBuffer.SetData(vertices);

            _indexBuffer = new IndexBuffer(Game.GraphicsDevice, typeof(int), indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices);

            //load wave maps
            _waveMap0 = Game.Content.Load<Texture2D>("Textures/wave0");
            _waveMap1 = Game.Content.Load<Texture2D>("Textures/wave1");

            _postProcessor = new GaussianBlur(Game.GraphicsDevice, Game.Content, 50, _waveMap0,
                Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
            _waveMapRT0 = new RenderTarget2D(Game.GraphicsDevice, 512, 512, false, SurfaceFormat.Color, DepthFormat.None);
            _waveMapRT1 = new RenderTarget2D(Game.GraphicsDevice, 512, 512, false, SurfaceFormat.Color, DepthFormat.None);

            _waveMapRT0 = _postProcessor.Draw(_waveMap0);
            _waveMapRT1 = _postProcessor.Draw(_waveMap1);



            _effect = Game.Content.Load<Effect>("Shaders/WaterEffect");

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            var timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //update the wave map offsets so that they will scroll across the water
            Options.WaveMapOffset0 += Options.WaveMapVelocity0 * timeDelta;
            Options.WaveMapOffset1 += Options.WaveMapVelocity1 * timeDelta;

            if (Options.WaveMapOffset0.X >= 1.0f || Options.WaveMapOffset0.X <= -1.0f)
                Options.WaveMapOffset0.X = 0.0f;
            if (Options.WaveMapOffset1.X >= 1.0f || Options.WaveMapOffset1.X <= -1.0f)
                Options.WaveMapOffset1.X = 0.0f;
            if (Options.WaveMapOffset0.Y >= 1.0f || Options.WaveMapOffset0.Y <= -1.0f)
                Options.WaveMapOffset0.Y = 0.0f;
            if (Options.WaveMapOffset1.Y >= 1.0f || Options.WaveMapOffset1.Y <= -1.0f)
                Options.WaveMapOffset1.Y = 0.0f;

            if (Keyboard.GetState().IsKeyDown(Keys.Add))
                WaveHeight += 0.5f;
            if (Keyboard.GetState().IsKeyDown(Keys.Subtract))
                WaveHeight -= 0.5f;
            if (Keyboard.GetState().IsKeyDown(Keys.O))
                Options.WaveMapScale += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.L))
                Options.WaveMapScale -= 0.1f;

            WaveHeight = MathHelper.Clamp(WaveHeight, 0, 500);
            Options.WaveMapScale = MathHelper.Clamp(Options.WaveMapScale, 0, 100);

            base.Update(gameTime);
        }

        public void SetEffectParameter()
        {
            _effect.Parameters["WaveMapOffset0"].SetValue(Options.WaveMapOffset0);
            _effect.Parameters["WaveMapOffset1"].SetValue(Options.WaveMapOffset1);

            _effect.Parameters["WaterColor"].SetValue(Options.WaterColor);
            _effect.Parameters["CameraPosition"].SetValue(Camera.Position);
            _effect.Parameters["LightDirection"].SetValue(Game1.LightDirection);
            _effect.Parameters["LightColor"].SetValue(Game1.LightColor);

            _effect.Parameters["World"].SetValue(Matrix.Identity);
            _effect.Parameters["View"].SetValue(Camera.View);
            _effect.Parameters["Projection"].SetValue(Camera.Projection);

            _effect.Parameters["WaveMap0"].SetValue(_waveMap0);
            _effect.Parameters["WaveMap1"].SetValue(_waveMap1);
            _effect.Parameters["WaveMapRT0"].SetValue(_waveMapRT0);
            _effect.Parameters["WaveMapRT1"].SetValue(_waveMapRT1);

            _effect.Parameters["CubeMap"].SetValue(CubeMap);
            _effect.Parameters["TexScale"].SetValue(Options.WaveMapScale);
            _effect.Parameters["WaveHeight"].SetValue(WaveHeight);
            _effect.Parameters["IOR"].SetValue(1.14f);
        }

        public void Draw()
        {
            Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            Game.GraphicsDevice.Indices = _indexBuffer;
            Game.GraphicsDevice.SetVertexBuffer(_vertexBuffer);

            _effect.Techniques[0].Passes[0].Apply();
            Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _numVertices, 0,
                                                          _numTris);

            Game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }



        /// <summary>
        /// Generates a grid of vertices to use for the water plane.
        /// </summary>
        /// <param name="numVertRows">Number of rows. Must be 2^n + 1. Ex. 129, 257, 513.</param>
        /// <param name="numVertCols">Number of columns. Must be 2^n + 1. Ex. 129, 257, 513.</param>
        /// <param name="dx">Cell spacing in the x dimension.</param>
        /// <param name="dz">Cell spacing in the y dimension.</param>
        /// <param name="center">Center of the plane.</param>
        /// <param name="verts">Outputs the constructed vertices for the plane.</param>
        /// <param name="indices">Outpus the constructed triangle indices for the plane.</param>
        private void GenTriGrid(int numVertRows, int numVertCols, float dx, float dz,
                                Vector3 center, out Vector3[] verts, out int[] indices)
        {
            int numVertices = numVertRows * numVertCols;
            int numCellRows = numVertRows - 1;
            int numCellCols = numVertCols - 1;

            int mNumTris = numCellRows * numCellCols * 2;

            float width = numCellCols * dx;
            float depth = numCellRows * dz;

            //===========================================
            // Build vertices.

            // We first build the grid geometry centered about the origin and on
            // the xz-plane, row-by-row and in a top-down fashion.  We then translate
            // the grid vertices so that they are centered about the specified 
            // parameter 'center'.

            verts = new Vector3[numVertices];

            // Offsets to translate grid from quadrant 4 to center of 
            // coordinate system.
            float xOffset = -width * 0.5f;
            float zOffset = depth * 0.5f;

            int k = 0;
            for (float i = 0; i < numVertRows; ++i)
            {
                for (float j = 0; j < numVertCols; ++j)
                {
                    // Negate the depth coordinate to put in quadrant four.  
                    // Then offset to center about coordinate system.
                    verts[k] = new Vector3(0, 0, 0)
                    {
                        X = j * dx + xOffset,
                        Z = -i * dz + zOffset,
                        Y = 0.0f
                    };

                    Matrix translation = Matrix.CreateTranslation(center);
                    verts[k] = Vector3.Transform(verts[k], translation);

                    ++k; // Next vertex
                }
            }

            //===========================================
            // Build indices.

            indices = new int[mNumTris * 3];

            // Generate indices for each quad.
            k = 0;
            for (int i = 0; i < numCellRows; ++i)
            {
                for (int j = 0; j < numCellCols; ++j)
                {
                    indices[k] = i * numVertCols + j;
                    indices[k + 1] = i * numVertCols + j + 1;
                    indices[k + 2] = (i + 1) * numVertCols + j;

                    indices[k + 3] = (i + 1) * numVertCols + j;
                    indices[k + 4] = i * numVertCols + j + 1;
                    indices[k + 5] = (i + 1) * numVertCols + j + 1;

                    // next quad
                    k += 6;
                }
            }
        }
    }
}
