using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Display3D
{
    /// <summary>
    /// Draws a LensFlare over a 3D Scene
    /// </summary>
    public class CLensFlare
    {
        const float glowSize = 200;

        const float querySize = 100;

        public Vector3 LightDirection = Vector3.Normalize(new Vector3(0.8434627f, -0.4053462f, -0.4539611f));

        Vector2 lightPosition;
        bool lightBehindCamera;


        Texture2D glowSprite;
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        VertexPositionColor[] queryVertices;
        GraphicsDevice graphicsDevice;


        static readonly BlendState ColorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };


        OcclusionQuery occlusionQuery;
        bool occlusionQueryActive;
        float occlusionAlpha;


        class Flare
        {
            public Flare(float position, float scale, Color color, string textureName)
            {
                Position = position;
                Scale = scale;
                Color = color;
                TextureName = textureName;
            }

            public float Position;
            public float Scale;
            public Color Color;
            public string TextureName;
            public Texture2D Texture;
        }


        /// <summary>
        /// Array contains informations of every lens flares.
        /// Position is relative to the center of the screen.
        /// </summary>
        Flare[] flares =
        {
            new Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "flare1"),
            new Flare( 0.3f, 0.4f, new Color(100, 255, 200), "flare1"),
            new Flare( 1.2f, 1.0f, new Color(100,  50,  50), "flare1"),
            new Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "flare1"),

            new Flare(-0.3f, 0.7f, new Color(200,  50,  50), "flare2"),
            new Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "flare2"),
            new Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "flare2"),

            new Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "flare3"),
            new Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "flare3"),
            new Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "flare3"),
        };



        /// <summary>
        /// Constructs a new lensflare component.
        /// </summary>
        public CLensFlare()
        {
        }


        /// <summary>
        /// Loads the content used by the lensflare component.
        /// </summary>
        public void LoadContent(ContentManager Content, GraphicsDevice _graphicsDevice, SpriteBatch _spriteBatch, Vector3 lightDirection)
        {
            spriteBatch = _spriteBatch;

            glowSprite = Content.Load<Texture2D>("Textures/LensFlare/glow");

            foreach (Flare flare in flares)
                flare.Texture = Content.Load<Texture2D>("Textures/LensFlare/" + flare.TextureName);

            basicEffect = new BasicEffect(_graphicsDevice);

            basicEffect.View = Matrix.Identity;
            basicEffect.VertexColorEnabled = true;

            queryVertices = new VertexPositionColor[4];

            queryVertices[0].Position = new Vector3(-querySize / 2, -querySize / 2, -1);
            queryVertices[1].Position = new Vector3(querySize / 2, -querySize / 2, -1);
            queryVertices[2].Position = new Vector3(-querySize / 2, querySize / 2, -1);
            queryVertices[3].Position = new Vector3(querySize / 2, querySize / 2, -1);

            graphicsDevice = _graphicsDevice;
            LightDirection = Vector3.Normalize(lightDirection);
            occlusionQuery = new OcclusionQuery(graphicsDevice);

        }



        /// <summary>
        /// Draws the lensflare component.
        /// </summary>
        public void Draw(GameTime gameTime)
        {
            DrawGlow();
            DrawFlares();
        }


        /// <summary>
        /// Measures how much the sun is visible using Occlusion Query.
        /// It draws a rectangle and checks how much of this rectangle is hidden behind an object
        /// </summary>
        public void UpdateOcclusion(Matrix View, Matrix Projection)
        {
            Matrix infiniteView = View;
            infiniteView.Translation = Vector3.Zero;

            Viewport viewport = graphicsDevice.Viewport;

            Vector3 projectedPosition = viewport.Project(-LightDirection, Projection, infiniteView, Matrix.Identity);

            if ((projectedPosition.Z < 0) || (projectedPosition.Z > 1))
            {
                lightBehindCamera = true;
                return;
            }

            lightPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
            lightBehindCamera = false;

            if (occlusionQueryActive)
            {
                if (!occlusionQuery.IsComplete)
                    return;

                const float queryArea = querySize * querySize;

                occlusionAlpha = Math.Min(occlusionQuery.PixelCount / queryArea, 1);
            }

            basicEffect.World = Matrix.CreateTranslation(lightPosition.X, lightPosition.Y, 0);
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            basicEffect.CurrentTechnique.Passes[0].Apply();

            occlusionQuery.Begin();
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, queryVertices, 0, 2);
            occlusionQuery.End();

            occlusionQueryActive = true;
        }


        /// <summary>
        /// Draws a large circular glow sprite, centered on the sun.
        /// </summary>
        public void DrawGlow()
        {
            if (lightBehindCamera || occlusionAlpha <= 0)
                return;

            Color color = Color.White * occlusionAlpha;
            Vector2 origin = new Vector2(glowSprite.Width, glowSprite.Height) / 2;
            float scale = glowSize * 2 / glowSprite.Width;

            spriteBatch.Begin(0, BlendState.AlphaBlend);
            spriteBatch.Draw(glowSprite, lightPosition, null, color, 0,
                             origin, scale, SpriteEffects.None, 0);
            spriteBatch.End();
        }


        /// <summary>
        /// Draws the lensflare sprites, computing the position
        /// of each one based on the current angle of the sun.
        /// </summary>
        public void DrawFlares()
        {
            if (lightBehindCamera || occlusionAlpha <= 0)
                return;

            Viewport viewport = graphicsDevice.Viewport;

            Vector2 screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;

            Vector2 flareVector = screenCenter - lightPosition;

            spriteBatch.Begin(0, BlendState.Additive);

            foreach (Flare flare in flares)
            {
                Vector2 flarePosition = lightPosition + flareVector * flare.Position;

                Vector4 flareColor = flare.Color.ToVector4();

                flareColor.W *= occlusionAlpha;

                Vector2 flareOrigin = new Vector2(flare.Texture.Width, flare.Texture.Height) / 2;

                spriteBatch.Draw(flare.Texture, flarePosition, null, new Color(flareColor), 1, flareOrigin, flare.Scale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }

    }
}
