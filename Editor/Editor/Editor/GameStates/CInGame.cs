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

namespace Editor.GameStates
{
    class CInGame : Game.CGameState
    {
        #region "Singleton"

        // Singleton Code
        private static CInGame instance = null;
        private static readonly object myLock = new object();

        // Singelton Methods
        private CInGame() { }
        public static CInGame getInstance()
        {
            lock (myLock)
            {
                if (instance == null) instance = new CInGame();
                return instance;
            }
        }
        #endregion

        private Display3D.CModel model; // (TEST) One Model displayed
        private Display3D.CCamera cam; // (TEST) One camera instancied

        private Game.CConsole devConsole;
        private Game.Settings.CGameSettings gameSettings;

        private bool isPlayerUnderwater = false;

        Display3D.CSkybox skybox;
        Display3D.CTerrain terrain;
        Display3D.CLensFlare lensFlare;
        Display3D.CWater water;

        public override void Initialize()
        {
            devConsole = Game.CConsole.getInstance();
            gameSettings = Game.Settings.CGameSettings.getInstance();
            lensFlare = new Display3D.CLensFlare();

        }

        public override void loadContent(ContentManager content, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            model = new Display3D.CModel(content.Load<Model>("3D//building001"), new Vector3(0, 5.5f, 0), new Vector3(0, -90f, 0), new Vector3(0.01f, 0.01f, 0.01f), graphics);
            cam = new Display3D.CCamera(graphics, new Vector3(0f, 10f, 5f), new Vector3(0f, 0f, 0f), 0.1f, 10000.0f, 0.1f);

            gameSettings.loadDatas(graphics);

            devConsole.LoadContent(content, graphics, spriteBatch, cam, true, false);
            devConsole._activationKeys = gameSettings._gameSettings.KeyMapping.Console;

            skybox = new Display3D.CSkybox(content, graphics, content.Load<TextureCube>("Textures/Clouds"));
            terrain = new Display3D.CTerrain();
            terrain.LoadContent(content.Load<Texture2D>("Textures/Heightmap"), 0.9f, 50, content.Load<Texture2D>("Textures/terrain_grass"), 150, new Vector3(1, -1, 0), graphics, content);

            lensFlare = new Display3D.CLensFlare();
            lensFlare.LoadContent(content, graphics, spriteBatch, new Vector3(0.8434627f, -0.4053462f, -0.4539611f));

            skybox = new Display3D.CSkybox(content, graphics, content.Load<TextureCube>("Textures/Clouds"));

            terrain = new Display3D.CTerrain();
            terrain.LoadContent(content.Load<Texture2D>("Textures/Terrain/Heightmap"), 0.9f, 100, content.Load<Texture2D>("Textures/Terrain/terrain_grass"), 10, lensFlare.LightDirection, graphics, content);
            terrain.WeightMap = content.Load<Texture2D>("Textures/Terrain/weightMap");
            terrain.RTexture = content.Load<Texture2D>("Textures/Terrain/sand");
            terrain.GTexture = content.Load<Texture2D>("Textures/Terrain/rock");
            terrain.BTexture = content.Load<Texture2D>("Textures/Terrain/snow");
            terrain.DetailTexture = content.Load<Texture2D>("Textures/Terrain/noise_texture");

            model._lightDirection = lensFlare.LightDirection;

            water = new Display3D.CWater(content, graphics, new Vector3(0, 44.5f, 0), new Vector2(10 * 30));
            water.Objects.Add(skybox);
            water.Objects.Add(terrain);
            water.Objects.Add(model);
        }

        public override void unloadContent(ContentManager content)
        {

        }

        public override void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState, MouseState oldMouseState)
        {
            cam.Update(gameTime, kbState, mouseState);
             
             devConsole.Update(kbState, gameTime);
             
        }

        public override void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {
            if (isPlayerUnderwater != water.isPositionUnderWater(cam._cameraPos))
            {
                isPlayerUnderwater = !isPlayerUnderwater;
                water.isUnderWater = isPlayerUnderwater;
                terrain.isUnderWater = isPlayerUnderwater;
            }
            water.PreDraw(cam, gameTime);

            skybox.Draw(cam._view, cam._projection, cam._cameraPos);

            terrain.Draw(cam._view, cam._projection, cam._cameraPos);

            if (cam.BoundingVolumeIsInView(model.BoundingSphere))
                model.Draw(cam._view, cam._projection, cam._cameraPos);

            water.Draw(cam._view, cam._projection, cam._cameraPos);

            lensFlare.UpdateOcclusion(cam._view, cam._projection);
            lensFlare.Draw(gameTime);

            devConsole.Draw(gameTime);
        }

    }
}
