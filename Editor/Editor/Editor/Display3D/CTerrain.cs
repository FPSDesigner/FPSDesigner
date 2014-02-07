using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Editor.Display3D
{
    class CTerrain : IRenderable
    {
        /// <summary>
        /// Variables
        /// </summary>

        // Vertexes
        public VertexPositionNormalTexture[][] vertices;
        VertexBuffer[] vertexBuffer;

        // Indexes
        int[] indices;
        IndexBuffer indexBuffer;

        // BoundingBox
        BoundingBox[] boundingBoxChunks;

        // Array of all vertexes heights
        float[][,] heights;

        // Maximum height of terrain
        float height;

        // Distance between vertices on x and z axes
        public float cellSize;

        // How many times we need to draw the texture
        float textureTiling;

        // Number of vertices on x and z axes
        public int width, length, oWidth, oLength;

        // Middle of the terrain
        public Point terrainMiddle;

        // Number of vertices and indices
        int nVertices, nIndices;

        public bool _isUnderWater = false;
        public bool _enableUnderWaterFog = true;

        public bool isUnderWater
        {
            set
            {
                effect.Parameters["IsUnderWater"].SetValue(value);
                effect.Parameters["LightIntensity"].SetValue((value) ? 0.1f : 1.0f);
                _isUnderWater = value;
            }
            get
            {
                return _isUnderWater;
            }
        }

        // Classes
        public Effect effect;
        public Effect effectLight;
        GraphicsDevice GraphicsDevice;
        public Vector3 lightDirection;
        Texture2D heightMap;
        Texture2D baseTexture;
        public CCamera camera;

        // World matrix contains scale, position, rotation...
        Matrix World;

        private Texture2D _WeightMap;
        public Texture2D WeightMap
        {
            set
            {
                _WeightMap = value;
                WeightMapChunks = genWeightmaps(_WeightMap, chunksSize, chunksSize);
            }
            get
            {
                return _WeightMap;
            }
        }

        public Texture2D RTexture, BTexture, GTexture;
        public Texture2D DetailTexture;
        public Texture2D[] WeightMapChunks;
        public float DetailDistance = 2500;
        public float DetailTextureTiling = 100;

        private bool areDefaultParamsLoaded = false;
        private int usedTechniqueIndex = 3;
        private int chunksSize = 256 / 4;
        private int chunkAmounts;
        private int chunkAmountsSide;


        /// <summary>
        /// Constructor
        /// </summary>
        public CTerrain()
        {
        }

        /// <summary>
        /// Initialize the Terrain Class
        /// </summary>
        /// <param name="HeightMap">The Heightmap texture</param>
        /// <param name="CellSize">The distance between vertices</param>
        /// <param name="Height">The maximum height of the terrain</param>
        /// <param name="BaseTexture">The texture to draw</param>
        /// <param name="TextureTiling">Number of texture we draw</param>
        /// <param name="LightDirection">Light Direction</param>
        /// <param name="GraphicsDevice">The graphics Device</param>
        /// <param name="Content">The ContentManager</param>
        public void LoadContent(Texture2D HeightMap, float CellSize, float Height, Texture2D BaseTexture, float TextureTiling, Vector3 LightDirection, GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            this.baseTexture = BaseTexture;
            this.textureTiling = TextureTiling;
            this.lightDirection = LightDirection;
            this.heightMap = HeightMap;
            this.width = HeightMap.Width;
            this.length = HeightMap.Height;
            this.oWidth = width;
            this.oLength = length;
            this.cellSize = CellSize;
            this.height = Height;
            this.World = Matrix.CreateTranslation(new Vector3(0, 0, 0));

            this.GraphicsDevice = GraphicsDevice;

            effect = Content.Load<Effect>("Effects/Terrain");
            effectLight = Content.Load<Effect>("Effects/PPLight");

            chunkAmountsSide = (width / chunksSize);
            chunkAmounts = chunkAmountsSide * chunkAmountsSide;
            width = chunksSize + 1;
            length = chunksSize + 1;

            // 1 vertex per pixel
            nVertices = width * length;

            // (Width-1) * (Length-1) cells, 2 triangles per cell, 3 indices per triangle
            nIndices = (width - 1) * (length - 1) * 6;

            vertexBuffer = new VertexBuffer[chunkAmounts];
            boundingBoxChunks = new BoundingBox[chunkAmounts];
            WeightMapChunks = new Texture2D[chunkAmounts];

            for (int i = 0; i < chunkAmounts; i++)
                vertexBuffer[i] = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture),
                    nVertices, BufferUsage.WriteOnly);

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits,
                    nIndices, BufferUsage.WriteOnly);

            getHeights();
            createVertices();
            createIndices();
            genNormals();


            for (int i = 0; i < chunkAmounts; i++)
                vertexBuffer[i].SetData<VertexPositionNormalTexture>(vertices[i]);

            indexBuffer.SetData<int>(indices);

            terrainMiddle = new Point((oWidth - 1) / 2, (oLength - 1) / 2);
        }

        /// <summary>
        /// Translate every heigtmap's pixels to height
        /// </summary>
        private void getHeights()
        {
            int amountSide = (int)Math.Sqrt(chunkAmounts);

            // Extract pixel data
            Color[] heightMapData = new Color[oWidth * oLength];
            heightMap.GetData<Color>(heightMapData);

            // Create heights[,] array
            heights = new float[chunkAmounts][,];
            for (int i = 0; i < chunkAmounts; i++)
                heights[i] = new float[width, length];

            // For each pixel
            for (int chunk = 0; chunk < chunkAmounts; chunk++)
                for (int y = 0; y < length; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offsetX = (chunksSize) * (chunk % chunkAmountsSide);
                        int offsetY = (chunksSize) * (chunk / chunkAmountsSide);

                        // Get color value (0 - 255)
                        float amt = heightMapData[Math.Min(y + offsetX, oLength - 1) * oWidth + Math.Min(x + offsetY, oWidth - 1)].R;

                        // Scale to (0 - 1)
                        amt /= 255.0f;

                        // Multiply by max height to get final height
                        heights[chunk][x, y] = amt * height;
                    }
                }
        }

        /// <summary>
        /// Create the vertices
        /// </summary>
        private void createVertices()
        {
            vertices = new VertexPositionNormalTexture[chunkAmounts][];
            for (int chunk = 0; chunk < chunkAmounts; chunk++)
            {
                List<Vector3> BBPoints = new List<Vector3>();
                vertices[chunk] = new VertexPositionNormalTexture[nVertices];

                int offsetY = chunksSize * (chunk % (chunkAmounts / 2));
                int offsetX = chunksSize * (chunk / 2);

                Vector3 offsetToCenter = new Vector3((-(chunkAmountsSide / 2.0f) + (chunk / chunkAmountsSide)) * chunksSize * cellSize, 0, (-(chunkAmountsSide / 2.0f) + (chunk % chunkAmountsSide)) * chunksSize * cellSize);

                Vector3 lowestHeight = Vector3.Zero;
                Vector3 highestHeight = Vector3.Zero;

                // For each pixel in the image
                for (int z = 0; z < length; z++)
                    for (int x = 0; x < width; x++)
                    {
                        // Find position based on grid coordinates and height in heightmap
                        Vector3 position = new Vector3(x * cellSize, heights[chunk][x, z], z * cellSize) + offsetToCenter;

                        if ((z == 0 || z == length - 1) && (x == 0 || x == width - 1))
                            BBPoints.Add(position);

                        if (highestHeight == Vector3.Zero || highestHeight.Y < position.Y)
                            highestHeight = position;
                        if (lowestHeight == Vector3.Zero || lowestHeight.Y > position.Y)
                            lowestHeight = position;

                        // UV coordinates range from (0, 0) at grid location (0, 0) to 
                        // (1, 1) at grid location (width, length)
                        Vector2 uv = new Vector2((float)x / width, (float)z / length);

                        // Create the vertex
                        vertices[chunk][z * width + x] = new VertexPositionNormalTexture(position, Vector3.Zero, uv);
                    }

                BBPoints.Add(lowestHeight);
                BBPoints.Add(highestHeight);
                boundingBoxChunks[chunk] = BoundingBox.CreateFromPoints(BBPoints);
            }
        }

        /// <summary>
        /// Create the indices
        /// </summary>
        private void createIndices()
        {
            indices = new int[nIndices];

            int i = 0;

            // For each cell
            for (int x = 0; x < width - 1; x++)
                for (int z = 0; z < length - 1; z++)
                {
                    // Find the indices of the corners
                    int upperLeft = z * width + x;
                    int upperRight = upperLeft + 1;
                    int lowerLeft = upperLeft + width;
                    int lowerRight = lowerLeft + 1;

                    // Specify upper triangle
                    indices[i++] = upperLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerLeft;

                    // Specify lower triangle
                    indices[i++] = lowerLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerRight;
                }
        }

        /// <summary>
        /// Calculate normals for each vertex
        /// </summary>
        private void genNormals()
        {
            for (int chunk = 0; chunk < chunkAmounts; chunk++)
            {
                // For each triangle
                for (int i = 0; i < nIndices; i += 3)
                {
                    // Find the position of each corner of the triangle
                    Vector3 v1 = vertices[chunk][indices[i]].Position;
                    Vector3 v2 = vertices[chunk][indices[i + 1]].Position;
                    Vector3 v3 = vertices[chunk][indices[i + 2]].Position;

                    // Cross the vectors between the corners to get the normal
                    Vector3 normal = Vector3.Cross(v1 - v2, v1 - v3);
                    normal.Normalize();

                    // Add the influence of the normal to each vertex in the
                    // triangle
                    vertices[chunk][indices[i]].Normal += normal;
                    vertices[chunk][indices[i + 1]].Normal += normal;
                    vertices[chunk][indices[i + 2]].Normal += normal;
                }

                // Average the influences of the triangles touching each vertex
                for (int i = 0; i < nVertices; i++)
                    vertices[chunk][i].Normal.Normalize();
            }
        }

        /// <summary>
        /// Create the weightmaps portions for the chunks
        /// </summary>
        public Texture2D[] genWeightmaps(Texture2D original, int partWidth, int partHeight)
        {
            int yCount = original.Width / partWidth;//The number of textures in each horizontal row
            int xCount = original.Height / partHeight;//The number of textures in each vertical column
            Texture2D[] r = new Texture2D[xCount * yCount];//Number of parts = (area of original) / (area of each part).
            int dataPerPart = partWidth * partHeight;//Number of pixels in each of the split parts

            //Get the pixel data from the original texture:
            Color[] originalData = new Color[original.Width * original.Height];
            original.GetData<Color>(originalData);

            int index = 0;
            for (int y = 0; y < yCount * partHeight; y += partHeight)
                for (int x = 0; x < xCount * partWidth; x += partWidth)
                {
                    //The texture at coordinate {x, y} from the top-left of the original texture
                    Texture2D part = new Texture2D(original.GraphicsDevice, partWidth, partHeight);
                    //The data for part
                    Color[] partData = new Color[dataPerPart];

                    //Fill the part data with colors from the original texture
                    for (int py = 0; py < partHeight; py++)
                        for (int px = 0; px < partWidth; px++)
                        {
                            int partIndex = px + py * partWidth;
                            //If a part goes outside of the source texture, then fill the overlapping part with Color.Transparent
                            if (y + py >= original.Height || x + px >= original.Width)
                                partData[partIndex] = Color.Transparent;
                            else
                                partData[partIndex] = originalData[(x + px) + (y + py) * original.Width];
                        }

                    //Fill the part with the extracted data
                    part.SetData<Color>(partData);
                    //Stick the part in the return array:                    
                    r[index++] = part;
                }
            //Return the array of parts.
            return r;
        }

        /// <summary>
        /// Draw the terrain
        /// </summary>
        /// <param name="View">The camera View matrix</param>
        /// <param name="Projection">The camera Projection matrix</param>
        /// <param name="cameraPos">The camera position</param>
        /// 
        int u2 = 0;
        int f = 0;
        public void Draw(Matrix View, Matrix Projection, Vector3 cameraPos)
        {
            GraphicsDevice.Indices = indexBuffer;

            if (!areDefaultParamsLoaded)
            {
                areDefaultParamsLoaded = true;
                SendEffectDefaultParameters();
            }

            int i = 0;

            u2++;
            if (u2 > 100)
            {
                f++;
                u2 = 0;
            }
            CSimpleShapes.AddBoundingBox(boundingBoxChunks[f], Color.Red);
            CSimpleShapes.AddBoundingSphere(new BoundingSphere(new Vector3(500, 0, 500), 30.0f), Color.Green);
            CSimpleShapes.AddBoundingSphere(new BoundingSphere(Vector3.Zero, 40.0f), Color.Blue);
            for (int chunk = 0; chunk < chunkAmounts; chunk++)
            {

                if (camera.BoundingVolumeIsInView(boundingBoxChunks[chunk]))
                {
                    i++;
                    GraphicsDevice.SetVertexBuffer(vertexBuffer[chunk]);

                    effect.Parameters["View"].SetValue(View);
                    effect.Parameters["Projection"].SetValue(Projection);
                    effect.Parameters["WeightMap"].SetValue(WeightMapChunks[8 * (chunk % 8) + chunk / 8]);


                    /* Techniques:
                     * 0: !fog && !underwater (T1)
                     * 1: fog && !underwater (T2)
                     * 2: !fog && underwater (T3)
                     * 3: fog && underwater (T4)
                    */

                    int index = 3;
                    if (!_enableUnderWaterFog)
                        index = (_isUnderWater) ? 2 : 0;
                    else
                        if (!_isUnderWater)
                            index = 1;


                    if (usedTechniqueIndex != index)
                    {
                        effect.CurrentTechnique = effect.Techniques[index];
                        usedTechniqueIndex = index;
                    }

                    effect.Techniques[index].Passes[0].Apply();


                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, nVertices, 0, nIndices / 3);


                }
            }
            Console.WriteLine(i + " chunksdrawn");
        }

        public void SendEffectDefaultParameters()
        {

            effect.Parameters["BaseTexture"].SetValue(baseTexture);
            effect.Parameters["TextureTiling"].SetValue(textureTiling);
            effect.Parameters["LightDirection"].SetValue(lightDirection);

            effect.Parameters["RTexture"].SetValue(RTexture);
            effect.Parameters["GTexture"].SetValue(GTexture);
            effect.Parameters["BTexture"].SetValue(BTexture);

            effect.Parameters["DetailTexture"].SetValue(DetailTexture);
            effect.Parameters["DetailDistance"].SetValue(DetailDistance);
            effect.Parameters["DetailTextureTiling"].SetValue(DetailTextureTiling);

        }

        /// <summary>
        /// Sets the clip plane for water reflection rendering
        /// </summary>
        public void SetClipPlane(Vector4? Plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

            if (Plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(Plane.Value);
        }

        /// <summary>
        /// Get the position on the terrain from a screen position
        /// </summary>
        /// <param name="device">Graphics Device</param>
        /// <param name="view">View Matrix</param>
        /// <param name="projection">Projection Matrix</param>
        /// <param name="X">The X screen position</param>
        /// <param name="Y">The Y screen position</param>
        /// <returns>The position on the terrain</returns>
        public Vector3 Pick(GraphicsDevice device, Matrix view, Matrix projection, int X, int Y)
        {
            Vector3 NearestPoint = Vector3.Zero;

            Vector3 nearSource = device.Viewport.Unproject(new Vector3(X, Y, device.Viewport.MinDepth), projection, view, World);
            Vector3 farSource = device.Viewport.Unproject(new Vector3(X, Y, device.Viewport.MaxDepth), projection, view, World);
            Vector3 direction = farSource - nearSource;

            float t = 0f;

            while (true)
            {
                t += 0.0001f;

                float XPos = nearSource.X + direction.X * t;
                int IndWidth = terrainMiddle.X + (int)XPos;


                float ZPos = nearSource.Z + direction.Z * t;
                int IndLength = terrainMiddle.Y + (int)ZPos;

                if (IndWidth >= oWidth || IndWidth < 0 || IndLength >= oLength || IndLength < 0)
                    break;

                float YPos = nearSource.Y + direction.Y * t;
                float IndHeight = heights[GetChunkFromIndices(IndWidth, IndLength)][IndWidth % width, IndLength % length];


                if (IndHeight >= YPos)
                    return new Vector3(XPos, YPos, ZPos);
            }

            return Vector3.Zero;
        }

        public int GetChunkFromIndices(int x, int z)
        {
            Console.WriteLine(x + " -- " + z);
            return 0;
        }

        /// <summary>
        /// Get the terrain height at a certain position
        /// </summary>
        /// <param name="X">Position X</param>
        /// <param name="Z">Position Y</param>
        /// <returns>The height & steepness at position X Y</returns>
        public float GetHeightAtPosition(float X, float Z, out float Steepness)
        {
            // Clamp coordinates to locations on terrain
            X = MathHelper.Clamp(X, (-oWidth / 2) * cellSize,
                (oWidth / 2) * cellSize);
            Z = MathHelper.Clamp(Z, (-oLength / 2) * cellSize,
                (oLength / 2) * cellSize);

            // Map from (-Width/2->Width/2,-Length/2->Length/2) 
            // to (0->Width, 0->Length)
            X += (oWidth / 2f) * cellSize;
            Z += (oLength / 2f) * cellSize;

            // Map to cell coordinates
            X /= cellSize;
            Z /= cellSize;
            X = 0;
            Z = 0;
            int x = (int)X;
            int z = (int)Z;
            float fTX = X - x;
            float fTY = Z - z;

            float fSampleH1 = heights[GetChunkFromIndices(x, z)][x, z];
            float fSampleH2 = heights[GetChunkFromIndices(x + 1, z)][x + 1, z];
            float fSampleH3 = heights[GetChunkFromIndices(x, z + 1)][x, z + 1];
            float fSampleH4 = heights[GetChunkFromIndices(x + 1, z + 1)][x + 1, z + 1];

            Steepness = (float)Math.Atan(Math.Abs((fSampleH1 - fSampleH4)) / (cellSize * Math.Sqrt(2)));

            return (fSampleH1 * (1.0f - fTX) + fSampleH2 * fTX) * (1.0f - fTY) + (fSampleH3 * (1.0f - fTX) + fSampleH4 * fTX) * (fTY) + 50f;


            //return MathHelper.Lerp(h1, h2, leftOver);
        }


        /// <summary>
        /// Transform a world position to a terrain position
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Z"></param>
        /// <returns></returns>
        public Vector2 positionToTerrain(float X, float Z)
        {
            // Clamp coordinates to locations on terrain
            X = MathHelper.Clamp(X, (-oWidth / 2) * cellSize,
                (oWidth / 2) * cellSize);
            Z = MathHelper.Clamp(Z, (-oLength / 2) * cellSize,
                (oLength / 2) * cellSize);

            // Map from (-Width/2->Width/2,-Length/2->Length/2) 
            // to (0->Width, 0->Length)
            X += (width / 2f) * cellSize;
            Z += (length / 2f) * cellSize;

            // Map to cell coordinates
            X /= cellSize;
            Z /= cellSize;

            // Truncate coordinates to get coordinates of top left cell vertex
            int x1 = (int)X;
            int z1 = (int)Z;

            // Try to get coordinates of bottom right cell vertex
            int x2 = x1 + 1 == width ? x1 : x1 + 1;
            int z2 = z1 + 1 == length ? z1 : z1 + 1;

            return new Vector2(x2, z2);
        }

        /// <summary>
        /// Get the normal at a certain point of the world
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Z"></param>
        /// <returns>The normal vector</returns>
        public Vector3 getNormalAtPoint(float X, float Z)
        {
            Vector2 translatedPosition = positionToTerrain(X, Z);
            int verticeIndex = (int)(translatedPosition.Y * 512 + translatedPosition.X);

            return vertices[GetChunkFromIndices((int)translatedPosition.X, (int)translatedPosition.Y)][verticeIndex].Normal;
        }



    }
}