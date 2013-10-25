using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Display3D
{
    /// <summary>
    /// A simple-shapes renderer
    /// </summary>
    /// <remarks>
    /// 
    /// CSimpleShapes allows rendering simple shapes such as lines, triangles and boudings elements.
    /// We can call theses methods via the Draw() method or use the lifetime parameter to specify
    /// the duration of rendering the element.
    /// </remarks>
    public static class CSimpleShapes
    {
        // A single shape in our renderer
        class SimpleShape
        {
            /// <summary>
            /// The array of vertices the shape can use.
            /// </summary>
            public VertexPositionColor[] Vertices;

            /// <summary>
            /// The number of lines to draw for this shape.
            /// </summary>
            public int LineCount;

            /// <summary>
            /// The length of time to keep this shape visible.
            /// </summary>
            public float Lifetime;
        }

        // We use a cache system to reuse our SimpleShape instances to avoid creating garbage
        private static readonly List<SimpleShape> cachedShapes = new List<SimpleShape>();
        private static readonly List<SimpleShape> activeShapes = new List<SimpleShape>();

        // Allocate an array to hold our vertices; this will grow as needed by our renderer
        private static VertexPositionColor[] verts = new VertexPositionColor[64];

        // Our graphics device and the effect we use to render the shapes
        private static GraphicsDevice graphics;
        private static BasicEffect effect;

        // An array we use to get corners from frustums and bounding boxes
        private static Vector3[] corners = new Vector3[8];

        // This holds the vertices for our unit sphere that we will use when drawing bounding spheres
        private const int sphereResolution = 30;
        private const int sphereLineCount = (sphereResolution + 1) * 3;
        private static Vector3[] unitSphere;

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to use for rendering.</param>
        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            if (graphics != null)
                return;
            // Save the graphics device
            graphics = graphicsDevice;

            // Create and initialize our effect
            effect = new BasicEffect(graphicsDevice);
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;
            effect.DiffuseColor = Vector3.One;
            effect.World = Matrix.Identity;

            // Create our unit sphere vertices
            InitializeSphere();
        }

        /// <summary>
        /// Adds a line to be rendered for just one frame.
        /// </summary>
        /// <param name="a">The first point of the line.</param>
        /// <param name="b">The second point of the line.</param>
        /// <param name="color">The color in which to draw the line.</param>
        public static void AddLine(Vector3 a, Vector3 b, Color color)
        {
            AddLine(a, b, color, 0f);
        }

        /// <summary>
        /// Adds a line to be rendered for a set amount of time.
        /// </summary>
        /// <param name="a">The first point of the line.</param>
        /// <param name="b">The second point of the line.</param>
        /// <param name="color">The color in which to draw the line.</param>
        /// <param name="life">The amount of time, in seconds, to keep rendering the line.</param>
        public static void AddLine(Vector3 a, Vector3 b, Color color, float life)
        {
            // Get a SimpleShape we can use to draw the line
            SimpleShape shape = GetShapeForLines(1, life);

            // Add the two vertices to the shape
            shape.Vertices[0] = new VertexPositionColor(a, color);
            shape.Vertices[1] = new VertexPositionColor(b, color);
        }

        /// <summary>
        /// Adds a triangle to be rendered for just one frame.
        /// </summary>
        /// <param name="a">The first vertex of the triangle.</param>
        /// <param name="b">The second vertex of the triangle.</param>
        /// <param name="c">The third vertex of the triangle.</param>
        /// <param name="color">The color in which to draw the triangle.</param>
        public static void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            AddTriangle(a, b, c, color, 0f);
        }

        /// <summary>
        /// Adds a triangle to be rendered for a set amount of time.
        /// </summary>
        /// <param name="a">The first vertex of the triangle.</param>
        /// <param name="b">The second vertex of the triangle.</param>
        /// <param name="c">The third vertex of the triangle.</param>
        /// <param name="color">The color in which to draw the triangle.</param>
        /// <param name="life">The amount of time, in seconds, to keep rendering the triangle.</param>
        public static void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color color, float life)
        {
            // Get a SimpleShape we can use to draw the triangle
            SimpleShape shape = GetShapeForLines(3, life);

            // Add the vertices to the shape
            shape.Vertices[0] = new VertexPositionColor(a, color);
            shape.Vertices[1] = new VertexPositionColor(b, color);
            shape.Vertices[2] = new VertexPositionColor(b, color);
            shape.Vertices[3] = new VertexPositionColor(c, color);
            shape.Vertices[4] = new VertexPositionColor(c, color);
            shape.Vertices[5] = new VertexPositionColor(a, color);
        }

        /// <summary>
        /// Adds a frustum to be rendered for just one frame.
        /// </summary>
        /// <param name="frustum">The frustum to render.</param>
        /// <param name="color">The color in which to draw the frustum.</param>
        public static void AddBoundingFrustum(BoundingFrustum frustum, Color color)
        {
            AddBoundingFrustum(frustum, color, 0f);
        }

        /// <summary>
        /// Adds a frustum to be rendered for a set amount of time.
        /// </summary>
        /// <param name="frustum">The frustum to render.</param>
        /// <param name="color">The color in which to draw the frustum.</param>
        /// <param name="life">The amount of time, in seconds, to keep rendering the frustum.</param>
        public static void AddBoundingFrustum(BoundingFrustum frustum, Color color, float life)
        {
            // Get a SimpleShape we can use to draw the frustum
            SimpleShape shape = GetShapeForLines(12, life);

            // Get the corners of the frustum
            frustum.GetCorners(corners);

            // Fill in the vertices for the bottom of the frustum
            shape.Vertices[0] = new VertexPositionColor(corners[0], color);
            shape.Vertices[1] = new VertexPositionColor(corners[1], color);
            shape.Vertices[2] = new VertexPositionColor(corners[1], color);
            shape.Vertices[3] = new VertexPositionColor(corners[2], color);
            shape.Vertices[4] = new VertexPositionColor(corners[2], color);
            shape.Vertices[5] = new VertexPositionColor(corners[3], color);
            shape.Vertices[6] = new VertexPositionColor(corners[3], color);
            shape.Vertices[7] = new VertexPositionColor(corners[0], color);

            // Fill in the vertices for the top of the frustum
            shape.Vertices[8] = new VertexPositionColor(corners[4], color);
            shape.Vertices[9] = new VertexPositionColor(corners[5], color);
            shape.Vertices[10] = new VertexPositionColor(corners[5], color);
            shape.Vertices[11] = new VertexPositionColor(corners[6], color);
            shape.Vertices[12] = new VertexPositionColor(corners[6], color);
            shape.Vertices[13] = new VertexPositionColor(corners[7], color);
            shape.Vertices[14] = new VertexPositionColor(corners[7], color);
            shape.Vertices[15] = new VertexPositionColor(corners[4], color);

            // Fill in the vertices for the vertical sides of the frustum
            shape.Vertices[16] = new VertexPositionColor(corners[0], color);
            shape.Vertices[17] = new VertexPositionColor(corners[4], color);
            shape.Vertices[18] = new VertexPositionColor(corners[1], color);
            shape.Vertices[19] = new VertexPositionColor(corners[5], color);
            shape.Vertices[20] = new VertexPositionColor(corners[2], color);
            shape.Vertices[21] = new VertexPositionColor(corners[6], color);
            shape.Vertices[22] = new VertexPositionColor(corners[3], color);
            shape.Vertices[23] = new VertexPositionColor(corners[7], color);
        }

        /// <summary>
        /// Adds a bounding box to be rendered for just one frame.
        /// </summary>
        /// <param name="box">The bounding box to render.</param>
        /// <param name="color">The color in which to draw the bounding box.</param>
        public static void AddBoundingBox(BoundingBox box, Color color)
        {
            AddBoundingBox(box, color, 0f);
        }

        /// <summary>
        /// Adds a bounding box to be rendered for a set amount of time.
        /// </summary>
        /// <param name="box">The bounding box to render.</param>
        /// <param name="color">The color in which to draw the bounding box.</param>
        /// <param name="life">The amount of time, in seconds, to keep rendering the bounding box.</param>
        public static void AddBoundingBox(BoundingBox box, Color color, float life)
        {
            // Get a SimpleShape we can use to draw the box
            SimpleShape shape = GetShapeForLines(12, life);

            // Get the corners of the box
            box.GetCorners(corners);

            // Fill in the vertices for the bottom of the box
            shape.Vertices[0] = new VertexPositionColor(corners[0], color);
            shape.Vertices[1] = new VertexPositionColor(corners[1], color);
            shape.Vertices[2] = new VertexPositionColor(corners[1], color);
            shape.Vertices[3] = new VertexPositionColor(corners[2], color);
            shape.Vertices[4] = new VertexPositionColor(corners[2], color);
            shape.Vertices[5] = new VertexPositionColor(corners[3], color);
            shape.Vertices[6] = new VertexPositionColor(corners[3], color);
            shape.Vertices[7] = new VertexPositionColor(corners[0], color);

            // Fill in the vertices for the top of the box
            shape.Vertices[8] = new VertexPositionColor(corners[4], color);
            shape.Vertices[9] = new VertexPositionColor(corners[5], color);
            shape.Vertices[10] = new VertexPositionColor(corners[5], color);
            shape.Vertices[11] = new VertexPositionColor(corners[6], color);
            shape.Vertices[12] = new VertexPositionColor(corners[6], color);
            shape.Vertices[13] = new VertexPositionColor(corners[7], color);
            shape.Vertices[14] = new VertexPositionColor(corners[7], color);
            shape.Vertices[15] = new VertexPositionColor(corners[4], color);

            // Fill in the vertices for the vertical sides of the box
            shape.Vertices[16] = new VertexPositionColor(corners[0], color);
            shape.Vertices[17] = new VertexPositionColor(corners[4], color);
            shape.Vertices[18] = new VertexPositionColor(corners[1], color);
            shape.Vertices[19] = new VertexPositionColor(corners[5], color);
            shape.Vertices[20] = new VertexPositionColor(corners[2], color);
            shape.Vertices[21] = new VertexPositionColor(corners[6], color);
            shape.Vertices[22] = new VertexPositionColor(corners[3], color);
            shape.Vertices[23] = new VertexPositionColor(corners[7], color);
        }

        /// <summary>
        /// Adds a bounding sphere to be rendered for just one frame.
        /// </summary>
        /// <param name="sphere">The bounding sphere to render.</param>
        /// <param name="color">The color in which to draw the bounding sphere.</param>
        public static void AddBoundingSphere(BoundingSphere sphere, Color color)
        {
            AddBoundingSphere(sphere, color, 0f);
        }

        /// <summary>
        /// Adds a bounding sphere to be rendered for a set amount of time.
        /// </summary>
        /// <param name="sphere">The bounding sphere to render.</param>
        /// <param name="color">The color in which to draw the bounding sphere.</param>
        /// <param name="life">The amount of time, in seconds, to keep rendering the bounding sphere.</param>
        public static void AddBoundingSphere(BoundingSphere sphere, Color color, float life)
        {
            // Get a SimpleShape we can use to draw the sphere
            SimpleShape shape = GetShapeForLines(sphereLineCount, life);

            // Iterate our unit sphere vertices
            for (int i = 0; i < unitSphere.Length; i++)
            {
                // Compute the vertex position by transforming the point by the radius and center of the sphere
                Vector3 vertPos = unitSphere[i] * sphere.Radius + sphere.Center;

                // Add the vertex to the shape
                shape.Vertices[i] = new VertexPositionColor(vertPos, color);
            }
        }

        /// <summary>
        /// Draws the shapes that were added to the renderer and are still alive.
        /// </summary>
        /// <param name="gameTime">The current game timestamp.</param>
        /// <param name="view">The view matrix to use when rendering the shapes.</param>
        /// <param name="projection">The projection matrix to use when rendering the shapes.</param>
        public static void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            // Update our effect with the matrices.
            effect.View = view;
            effect.Projection = projection;

            // Calculate the total number of vertices we're going to be rendering.
            int vertexCount = 0;
            foreach (var shape in activeShapes)
                vertexCount += shape.LineCount * 2;

            // If we have some vertices to draw
            if (vertexCount > 0)
            {
                // Make sure our array is large enough
                if (verts.Length < vertexCount)
                {
                    // If we have to resize, we make our array twice as large as necessary so
                    // we hopefully won't have to resize it for a while.
                    verts = new VertexPositionColor[vertexCount * 2];
                }

                // Now go through the shapes again to move the vertices to our array and
                // add up the number of lines to draw.
                int lineCount = 0;
                int vertIndex = 0;
                foreach (SimpleShape shape in activeShapes)
                {
                    lineCount += shape.LineCount;
                    int shapeVerts = shape.LineCount * 2;
                    for (int i = 0; i < shapeVerts; i++)
                        verts[vertIndex++] = shape.Vertices[i];
                }

                // Start our effect to begin rendering.
                effect.CurrentTechnique.Passes[0].Apply();

                // We draw in a loop because the Reach profile only supports 65,535 primitives. While it's
                // not incredibly likely, if a game tries to render more than 65,535 lines we don't want to
                // crash. We handle this by doing a loop and drawing as many lines as we can at a time, capped
                // at our limit. We then move ahead in our vertex array and draw the next set of lines.
                int vertexOffset = 0;
                while (lineCount > 0)
                {
                    // Figure out how many lines we're going to draw
                    int linesToDraw = Math.Min(lineCount, 65535);

                    // Draw the lines
                    graphics.DrawUserPrimitives(PrimitiveType.LineList, verts, vertexOffset, linesToDraw);

                    // Move our vertex offset ahead based on the lines we drew
                    vertexOffset += linesToDraw * 2;

                    // Remove these lines from our total line count
                    lineCount -= linesToDraw;
                }
            }

            // Go through our active shapes and retire any shapes that have expired to the
            // cache list. 
            bool resort = false;
            for (int i = activeShapes.Count - 1; i >= 0; i--)
            {
                SimpleShape s = activeShapes[i];
                s.Lifetime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (s.Lifetime <= 0)
                {
                    cachedShapes.Add(s);
                    activeShapes.RemoveAt(i);
                    resort = true;
                }
            }

            // If we move any shapes around, we need to resort the cached list
            // to ensure that the smallest shapes are first in the list.
            if (resort)
                cachedShapes.Sort(CachedShapesSort);
        }

        /// <summary>
        /// Creates the unitSphere array of vertices.
        /// </summary>
        private static void InitializeSphere()
        {
            // We need two vertices per line, so we can allocate our vertices
            unitSphere = new Vector3[sphereLineCount * 2];

            // Compute our step around each circle
            float step = MathHelper.TwoPi / sphereResolution;

            // Used to track the index into our vertex array
            int index = 0;

            // Create the loop on the XY plane first
            for (float a = 0f; a < MathHelper.TwoPi; a += step)
            {
                unitSphere[index++] = new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0f);
                unitSphere[index++] = new Vector3((float)Math.Cos(a + step), (float)Math.Sin(a + step), 0f);
            }

            // Next on the XZ plane
            for (float a = 0f; a < MathHelper.TwoPi; a += step)
            {
                unitSphere[index++] = new Vector3((float)Math.Cos(a), 0f, (float)Math.Sin(a));
                unitSphere[index++] = new Vector3((float)Math.Cos(a + step), 0f, (float)Math.Sin(a + step));
            }

            // Finally on the YZ plane
            for (float a = 0f; a < MathHelper.TwoPi; a += step)
            {
                unitSphere[index++] = new Vector3(0f, (float)Math.Cos(a), (float)Math.Sin(a));
                unitSphere[index++] = new Vector3(0f, (float)Math.Cos(a + step), (float)Math.Sin(a + step));
            }
        }

        /// <summary>
        /// A method used for sorting our cached shapes based on the size of their vertex arrays.
        /// </summary>
        private static int CachedShapesSort(SimpleShape s1, SimpleShape s2)
        {
            return s1.Vertices.Length.CompareTo(s2.Vertices.Length);
        }

        /// <summary>
        /// Gets a SimpleShape instance for a given line counta and lifespan.
        /// </summary>
        private static SimpleShape GetShapeForLines(int lineCount, float life)
        {
            SimpleShape shape = null;

            // We go through our cached list trying to find a shape that contains
            // a large enough array to hold our desired line count. If we find such
            // a shape, we move it from our cached list to our active list and break
            // out of the loop.
            int vertCount = lineCount * 2;
            for (int i = 0; i < cachedShapes.Count; i++)
            {
                if (cachedShapes[i].Vertices.Length >= vertCount)
                {
                    shape = cachedShapes[i];
                    cachedShapes.RemoveAt(i);
                    activeShapes.Add(shape);
                    break;
                }
            }

            // If we didn't find a shape in our cache, we create a new shape and add it
            // to the active list.
            if (shape == null)
            {
                shape = new SimpleShape { Vertices = new VertexPositionColor[vertCount] };
                activeShapes.Add(shape);
            }

            // Set the line count and lifetime of the shape based on our parameters.
            shape.LineCount = lineCount;
            shape.Lifetime = life;

            return shape;
        }
    }
}
