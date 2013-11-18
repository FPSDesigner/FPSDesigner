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
        VertexPositionNormalTexture[] vertices;
        VertexBuffer vertexBuffer;

        // Indexes
        int[] indices;
        IndexBuffer indexBuffer;

        // Array of all vertexes heights
        float[,] heights;

        // Maximum height of terrain
        float height;

        // Distance between vertices on x and z axes
        float cellSize;

        // How many times we need to draw the texture
        float textureTiling;

        // Number of vertices on x and z axes
        int width, length;

        // Number of vertices and indices
        int nVertices, nIndices;

        public bool isUnderWater = false;

        // Classes
        Effect effect;
        GraphicsDevice GraphicsDevice;
        public Vector3 lightDirection;
        Texture2D heightMap;
        Texture2D baseTexture;

        // World matrix contains scale, position, rotation...
        Matrix World;

        public Texture2D RTexture, BTexture, GTexture, WeightMap;
        public Texture2D DetailTexture;
        public float DetailDistance = 2500;
        public float DetailTextureTiling = 100;


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
            this.cellSize = CellSize;
            this.height = Height;
            this.World = Matrix.CreateTranslation(new Vector3(0, 0, 0));

            this.GraphicsDevice = GraphicsDevice;

            effect = Content.Load<Effect>("Effects/Terrain");

            // 1 vertex per pixel
            nVertices = width * length;

            // (Width-1) * (Length-1) cells, 2 triangles per cell, 3 indices per triangle
            nIndices = (width - 1) * (length - 1) * 6;

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture),
                nVertices, BufferUsage.WriteOnly);

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits,
                nIndices, BufferUsage.WriteOnly);

            getHeights();
            createVertices();
            createIndices();
            genNormals();

            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
            indexBuffer.SetData<int>(indices);
        }

        /// <summary>
        /// Translate every heigtmap's pixels to height
        /// </summary>
        private void getHeights()
        {
            // Extract pixel data
            Color[] heightMapData = new Color[width * length];
            heightMap.GetData<Color>(heightMapData);

            // Create heights[,] array
            heights = new float[width, length];

            // For each pixel
            for (int y = 0; y < length; y++)
                for (int x = 0; x < width; x++)
                {
                    // Get color value (0 - 255)
                    float amt = heightMapData[y * width + x].R;

                    // Scale to (0 - 1)
                    amt /= 255.0f;

                    // Multiply by max height to get final height
                    heights[x, y] = amt * height;
                }
        }

        /// <summary>
        /// Create the vertices
        /// </summary>
        private void createVertices()
        {
            vertices = new VertexPositionNormalTexture[nVertices];

            // Calculate the position offset that will center the terrain at (0, 0, 0)
            Vector3 offsetToCenter = -new Vector3(((float)width / 2.0f) * cellSize, 0, ((float)length / 2.0f) * cellSize);

            // For each pixel in the image
            for (int z = 0; z < length; z++)
                for (int x = 0; x < width; x++)
                {
                    // Find position based on grid coordinates and height in heightmap
                    Vector3 position = new Vector3(x * cellSize,
                        heights[x, z], z * cellSize) + offsetToCenter;

                    // UV coordinates range from (0, 0) at grid location (0, 0) to 
                    // (1, 1) at grid location (width, length)
                    Vector2 uv = new Vector2((float)x / width, (float)z / length);

                    // Create the vertex
                    vertices[z * width + x] = new VertexPositionNormalTexture(position, Vector3.Zero, uv);
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
            // For each triangle
            for (int i = 0; i < nIndices; i += 3)
            {
                // Find the position of each corner of the triangle
                Vector3 v1 = vertices[indices[i]].Position;
                Vector3 v2 = vertices[indices[i + 1]].Position;
                Vector3 v3 = vertices[indices[i + 2]].Position;

                // Cross the vectors between the corners to get the normal
                Vector3 normal = Vector3.Cross(v1 - v2, v1 - v3);
                normal.Normalize();

                // Add the influence of the normal to each vertex in the
                // triangle
                vertices[indices[i]].Normal += normal;
                vertices[indices[i + 1]].Normal += normal;
                vertices[indices[i + 2]].Normal += normal;
            }

            // Average the influences of the triangles touching each vertex
            for (int i = 0; i < nVertices; i++)
                vertices[i].Normal.Normalize();
        }

        /// <summary>
        /// Draw the terrain
        /// </summary>
        /// <param name="View">The camera View matrix</param>
        /// <param name="Projection">The camera Projection matrix</param>
        /// <param name="cameraPos">The camera position</param>
        public void Draw(Matrix View, Matrix Projection, Vector3 cameraPos)
        {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            effect.Parameters["View"].SetValue(View);
            effect.Parameters["Projection"].SetValue(Projection);
            effect.Parameters["BaseTexture"].SetValue(baseTexture);
            effect.Parameters["TextureTiling"].SetValue(textureTiling);
            effect.Parameters["LightDirection"].SetValue(lightDirection);

            effect.Parameters["RTexture"].SetValue(RTexture);
            effect.Parameters["GTexture"].SetValue(GTexture);
            effect.Parameters["BTexture"].SetValue(BTexture);
            effect.Parameters["WeightMap"].SetValue(WeightMap);

            effect.Parameters["DetailTexture"].SetValue(DetailTexture);
            effect.Parameters["DetailDistance"].SetValue(DetailDistance);
            effect.Parameters["DetailTextureTiling"].SetValue(DetailTextureTiling);

            if (isUnderWater)
            {
                effect.Parameters["LightIntensity"].SetValue(0.1f);
            }
            else
                effect.Parameters["LightIntensity"].SetValue(1.0f);

            effect.Techniques[0].Passes[0].Apply();

            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                nVertices, 0, nIndices / 3);
        }

        /// <summary>
        /// Sets the clip plane for water reflection rendereing
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
            Point terrainMiddle = new Point((width - 1) / 2, (length - 1) / 2);

            float t = 0f;

            while (true)
            {
                t += 0.0001f;

                float XPos = nearSource.X + direction.X * t;
                int IndWidth = terrainMiddle.X + (int)XPos;


                float ZPos = nearSource.Z + direction.Z * t;
                int IndLength = terrainMiddle.Y + (int)ZPos;

                if (IndWidth >= width || IndWidth < 0 || IndLength >= length || IndLength < 0)
                    break;

                float YPos = nearSource.Y + direction.Y * t;
                float IndHeight = heights[IndWidth, IndLength];


                if (IndHeight >= YPos)
                    return new Vector3(XPos, YPos, ZPos);
            }

            return Vector3.Zero;
        }

        /// <summary>
        /// Get the terrain height at a certain position
        /// </summary>
        /// <param name="X">Position X</param>
        /// <param name="Z">Position Y</param>
        /// <param name="Steepness">The steepness at position X Z</param>
        /// <returns>The height & steepness at position X Y</returns>
        public float GetHeightAtPosition(float X, float Z, out float Steepness)
        {
            // Clamp coordinates to locations on terrain
            X = MathHelper.Clamp(X, (-width / 2) * cellSize,
                (width / 2) * cellSize);
            Z = MathHelper.Clamp(Z, (-length / 2) * cellSize,
                (length / 2) * cellSize);

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

            // Get the heights at the two corners of the cell
            float h1 = heights[x1, z1];
            float h2 = heights[x2, z2];

            // Determine steepness (angle between higher and lower vertex of cell)
            Steepness = (float)Math.Atan(Math.Abs((h1 - h2)) /
                (cellSize * Math.Sqrt(2)));

            // Find the average of the amounts lost from coordinates during 
            // truncation above
            float leftOver = ((X - x1) + (Z - z1)) / 2f;

            // Interpolate between the corner vertices' heights
            return MathHelper.Lerp(h1, h2, leftOver);
        }
    }
}
