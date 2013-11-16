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

namespace Editor.Game
{
    /// <summary>
    /// Used to manage the main class
    /// </summary>
    class CGameManagement
    {

        private Display3D.CModel model; // (TEST) One Model displayed
        private Display3D.CCamera cam; // (TEST) One camera instancied

        // All The States that exist
        private GameStates.CMenu Menu;
        private GameStates.CInGame InGame;

        // Current State
        Game.CGameStateManager _currentState;

        private Game.CConsole devConsole;
        private Game.Settings.CGameSettings gameSettings;
        private GraphicsDeviceManager _graphicsManager;
        private GraphicsDevice _graphics;
        private MouseState _oldMouseState;

        Display3D.CSkybox skybox;
        Display3D.CTerrain terrain;
        Display3D.CLensFlare lensFlare;
        Display3D.CWater water;

        /// <summary>
        /// Constructor, Initialize the class
        /// </summary>
        public void Initialize()
        {
            _currentState = Game.CGameStateManager.getInstance();

            devConsole = Game.CConsole.getInstance();
            gameSettings = Game.Settings.CGameSettings.getInstance();
        }

        /// <summary>
        /// Load all the contents
        /// </summary>
        /// <param name="content">ContentManager class</param>
        /// <param name="graphics">GraphicsDevice class</param>
        /// <param name="spriteBatch">SpriteBatch class</param>
        public void loadContent(ContentManager content, GraphicsDevice graphics, SpriteBatch spriteBatch, GraphicsDeviceManager graphicsDevice)
        {
            _graphicsManager = graphicsDevice;
            _graphics = graphics;

            model = new Display3D.CModel(content.Load<Model>("3D/building"), new Vector3(0, 45.5f, 0), new Vector3(0, -90f, 0), new Vector3(0.01f, 0.01f, 0.01f), graphics);
            cam = new Display3D.CCamera(graphics, new Vector3(0f, 50f, 5f), new Vector3(0f, 0f, 0f), 0.1f, 10000.0f, 0.1f);

            gameSettings.loadDatas(graphics);

            devConsole.LoadContent(content, graphics, spriteBatch, cam, true, false);
            devConsole._activationKeys = gameSettings._gameSettings.KeyMapping.Console;

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

        public void unloadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState)
        {
            cam.Update(gameTime, kbState, mouseState);

            devConsole.Update(kbState, gameTime);

            _oldMouseState = mouseState;
        }

        public void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {
            bool isPlayerUnderWater = water.isPositionUnderWater(cam._cameraPos);
            water.isUnderWater = isPlayerUnderWater;
            terrain.isUnderWater = isPlayerUnderWater;

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
