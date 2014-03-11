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

namespace Engine.GameStates
{
    class CInGame
    {
        private Display3D.CCamera cam; // (TEST) One camera instancied

        private Game.CCharacter _character; //Character : can shoot, etc..

        private KeyboardState _oldKeyState;

        private bool isPlayerUnderwater = false;

        Game.LevelInfo.CLevelInfo levelInfo;
        Game.LevelInfo.LevelData levelData;
        Display3D.CSkybox skybox;
        Display3D.CTerrain terrain;
        Display3D.CLensFlare lensFlare;
        Display3D.CWater water;
        List<Display3D.Particles.ParticleSystem> particlesList = new List<Display3D.Particles.ParticleSystem>();

        Game.CWeapon weapon;
        List<Display3D.CModel> models = new List<Display3D.CModel>();
        GraphicsDevice _graphics;

        public void Initialize()
        {
            levelInfo = new Game.LevelInfo.CLevelInfo();
        }

        public CInGame()
        {
        }

        public void LoadContent(ContentManager content, SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            /**** Variables Initialization ***/
            levelData = levelInfo.loadLevelData("GameLevel.xml");
            _graphics = graphics;


            /**** Character ****/
            _character = new Game.CCharacter();
            _character.Initialize();

            _character._initSpeed = levelData.SpawnInfo.MoveSpeed;


            /**** Models ****/

            foreach (Game.LevelInfo.MapModels_Model modelInfo in levelData.MapModels.Models)
            {
                Dictionary<string, Texture2D> modelTextures = new Dictionary<string,Texture2D>();

                foreach (Game.LevelInfo.MapModels_Texture textureInfo in modelInfo.Textures.Texture)
                    modelTextures.Add(textureInfo.Mesh, content.Load<Texture2D>(textureInfo.Texture));

                models.Add(new Display3D.CModel(
                    content.Load<Model>(modelInfo.ModelFile),
                    modelInfo.Position.Vector3,
                    modelInfo.Rotation.Vector3,
                    modelInfo.Scale.Vector3,
                    graphics,
                    modelTextures,
                    modelInfo.SpecColor,
                    modelInfo.Alpha));
            }

            Dictionary<string, Texture2D> treeTextures = new Dictionary<string, Texture2D>();
            Dictionary<string, Texture2D> testTree = new Dictionary<string, Texture2D>();

            // Local variable created to bypass the multiple content loading
            Model loadingModel;

            /*// Display 1 tree
            loadingModel = content.Load<Model>("Models//Tree001");
            treeTextures.Add("Tree001", content.Load<Texture2D>("Textures\\Model Textures\\Tree001"));
            _modelTree = new Display3D.CModel(loadingModel, new Vector3(-143.2928f, 169f, 85.45f), new Vector3(MathHelper.Pi, MathHelper.PiOver2, 0f),
                         new Vector3(2.2f), graphics, treeTextures);

            loadingModel = null;
            loadingModel = content.Load<Model>("Models//Tree002");
            testTree.Add("Tree002", content.Load<Texture2D>("Textures\\Model Textures\\test"));
            testTree.Add("leaf", content.Load<Texture2D>("Textures\\Model Textures\\Leaf002"));
            _testTree = new Display3D.CModel(loadingModel, new Vector3(-165.2928f, 169f, 80.45f), new Vector3(-MathHelper.PiOver2, 0f, 0f),
                        new Vector3(2f), graphics, testTree);*/

            
            /*// Create barrels
            loadingModel = null;
            loadingModel = content.Load<Model>("Models//Barrel");

            Dictionary<string, Texture2D> barrelTextures = new Dictionary<string, Texture2D>();
            barrelTextures.Add("RustyBarrel", content.Load<Texture2D>("Textures\\Model Textures\\RustyMetal"));
            Display3D.CModel modelBarrel = new Display3D.CModel(loadingModel, new Vector3(-128f, 169.2f, 82.45f), new Vector3(0f, 0f, 0f),
                                              new Vector3(0.75f), graphics, barrelTextures);
            Display3D.CModel modelBarrel2 = new Display3D.CModel(loadingModel, new Vector3(-120f, 169.2f, 88f), new Vector3(0f, 0f, 0f),///////////
                                                new Vector3(0.75f), graphics, barrelTextures);*/

            // Create Trash Bags
            loadingModel = null;
            loadingModel = content.Load<Model>("Models//TrashBag");

            Dictionary<string, Texture2D> trashbagTextures = new Dictionary<string, Texture2D>();
            trashbagTextures.Add("Garbage", content.Load<Texture2D>("Textures\\Model Textures\\garbageTexture"));
            Display3D.CModel modelTrashbag = new Display3D.CModel(loadingModel, new Vector3(-130f, 168.6f, 82.45f), new Vector3(0f, 0f, 0f),
                                              new Vector3(0.6f), graphics, trashbagTextures);
            Display3D.CModel modelTrashbag2 = new Display3D.CModel(loadingModel, new Vector3(-134f, 168.75f, 85.45f), new Vector3(0f, 0f, 0f),
                                              new Vector3(0.6f), graphics, trashbagTextures);

            // Create Traffic Light
            loadingModel = null;
            loadingModel = content.Load<Model>("Models//TrafficLight");
            Dictionary<string, Texture2D> trafficLightTextures = new Dictionary<string, Texture2D>();
            trafficLightTextures.Add("TrafficLight", content.Load<Texture2D>("Textures\\Model Textures\\TrafficLightTexture"));
            Display3D.CModel modelTrafficLight = new Display3D.CModel(loadingModel, new Vector3(-120f, 168.3f, 82.45f), new Vector3(0f, MathHelper.PiOver2, 0f),
                                              new Vector3(1.2f), graphics, trafficLightTextures);

            // We add all the models created to the model list
            //models.Add(_modelTree);
            //models.Add(_testTree);
            //models.Add(modelContainer);
            models.Add(modelBarrel);
            models.Add(modelBarrel2);
            models.Add(modelTrashbag);
            models.Add(modelTrashbag2);
            models.Add(modelTrafficLight);

            lensFlare = new Display3D.CLensFlare();
            lensFlare.LoadContent(content, graphics, spriteBatch, new Vector3(0.8434627f, -0.4053462f, -0.4539611f));

            skybox = new Display3D.CSkybox(content, graphics, content.Load<TextureCube>("Textures/Clouds"));

            /**** Terrain ****/
            if (levelData.Terrain.UseTerrain)
            {
                terrain = new Display3D.CTerrain();
                terrain.InitializeTextures(levelData.Terrain.TerrainTextures, content);
                terrain.LoadContent(levelData.Terrain.CellSize, levelData.Terrain.Height, levelData.Terrain.TextureTiling, lensFlare.LightDirection, graphics, content);
            }

            /**** Water ****/
            if (levelData.Water.UseWater)
            {
                water = new Display3D.CWater(
                    content, graphics, new Vector3(levelData.Water.Coordinates.X, levelData.Water.Coordinates.Y, levelData.Water.Coordinates.Z),
                    new Vector2(levelData.Water.SizeX, levelData.Water.SizeY), levelData.Water.Alpha, Display2D.C2DEffect._renderCapture.renderTarget
                );

                water.Objects.Add(skybox);

                if (levelData.Terrain.UseTerrain)
                    water.Objects.Add(terrain);
            }

            if (levelData.Terrain.UseTerrain && levelData.Water.UseWater)
                terrain.waterHeight = water.waterPosition.Y;

            /**** ****/

            Game.CSoundManager.LoadContent(water);

            // We create array containing all informations about weapons.

            weapon = new Game.CWeapon();

            Model[] testmodel = new Model[] { content.Load<Model>("Models//Machete"), content.Load<Model>("Models//M1911") };

            Texture2D[] weaponsTexture = new Texture2D[] { content.Load<Texture2D>("Textures//Uvw_Machete"), content.Load<Texture2D>("Textures//M1911") };

            object[][] testInfos = new object[][] {
                new object[] {2,1,1,1,1,false,2.0f,1,Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.Pi),new Vector3(0.2f, 0.2f, 0.1f), 1f, 0f},
                new object[] {0,10,10,20,1,false,2.0f,1,Matrix.CreateRotationZ(1.24f)*Matrix.CreateRotationY(-0.14f),new Vector3(.08f, 0.0f, 0.18f), 1f, 100f},
            };
            string[][] testSounds = new string[][] {
                new string[] {
                    "Sounds\\Weapons\\MACHET_ATTACK"
                },
                new string[] {
                    "Sounds\\Weapons\\M1911_SHOT","Sounds\\Weapons\\DryFireSound", "Sounds\\Weapons\\M1911_RELOAD",
                },
            };

            string[][] anims = new string[][] {
                new string[] {
                    "Machete_Walk", "Machete_Attack", "Machete_Wait","","Machete_Switch"
                },
                new string[] {
                    "M1911_Walk", "M1911_Attack", "M1911_Wait", "M1911_Reloading","M1911_Switch"
                },
            };

            float[][] animVelocity = new float[][] {
                new float[] {
                    1.6f, 4.0f, 0.7f,0.0f,2.5f
                },
                new float[] {
                    1.6f, 16.0f, 0.8f,2.0f,2.5f
                },

            };

            weapon.LoadContent(content, testmodel, weaponsTexture, testInfos, testSounds, anims, animVelocity);

            // Load content for Chara class
            _character.LoadContent(content, graphics, weapon);

            /**** Particles ****/
            Display3D.Particles.ParticlesManager.AddNewParticle("fire", new Display3D.Particles.Elements.FireParticleSystem(content), true, new Vector3(-165.2928f, 179f, 80.45f));
            Display3D.Particles.ParticlesManager.AddNewParticle("dirt", new Display3D.Particles.Elements.GSDirtParticleSystem(content), true, new Vector3(-185.2928f, 172f, 80.45f), null, false);

            /**** Camera ****/
            Vector3 camPosition = new Vector3(levelData.SpawnInfo.SpawnPosition.X, levelData.SpawnInfo.SpawnPosition.Y, levelData.SpawnInfo.SpawnPosition.Z);
            Vector3 camRotation = new Vector3(levelData.SpawnInfo.SpawnRotation.X, levelData.SpawnInfo.SpawnRotation.Y, levelData.SpawnInfo.SpawnRotation.Z);
            cam = new Display3D.CCamera(graphics, camPosition, camRotation, levelData.SpawnInfo.NearClip, levelData.SpawnInfo.FarClip, false, (levelData.Terrain.UseTerrain) ? terrain : null, new bool[] { levelData.Terrain.UseTerrain, levelData.Water.UseWater });
            Game.CConsole._Camera = cam;

            for (int i = 0; i < models.Count; i++)
            {
                cam._physicsMap._triangleList.AddRange(models[i]._trianglesPositions);
                cam._physicsMap._triangleNormalsList.AddRange(models[i]._trianglesNormal);
            }

            if (levelData.Water.UseWater)
                cam._physicsMap._waterHeight = water.waterPosition.Y;
        }

        public void UnloadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState, MouseState oldMouseState)
        {
            // Update camera - _charac.Run is a functions allows player to run, look at the param
            cam.Update(gameTime, _character.SpeedModification(kbState, cam._physicsMap._fallingVelocity, weapon, cam), isPlayerUnderwater, water.waterPosition.Y, kbState, mouseState, _oldKeyState);

            // Update all character actions
            _character.Update(mouseState, oldMouseState, kbState, _oldKeyState, weapon, gameTime, cam, (isPlayerUnderwater || cam._physicsMap._isOnWaterSurface));
            _oldKeyState = kbState;

            Display3D.Particles.ParticlesManager.Update(gameTime);
        }

        public void Draw(SpriteBatch spritebatch, GameTime gameTime)
        {

            //renderer.Draw();
            Vector3 playerPos = cam._cameraPos;
            //playerPos.Y -= cam._playerHeight;

            if (isPlayerUnderwater != water.isPositionUnderWater(playerPos))
            {
                isPlayerUnderwater = !isPlayerUnderwater;
                water.isUnderWater = isPlayerUnderwater;
                terrain.isUnderWater = isPlayerUnderwater;
            }
            terrain.frustum = cam._Frustum;

            skybox.ColorIntensity = 0.8f;
            water.PreDraw(cam, gameTime);
            skybox.ColorIntensity = 1;

            skybox.Draw(cam._view, cam._projection, cam._cameraPos);

            terrain.Draw(cam._view, cam._projection, cam._cameraPos);

            water.Draw(cam._view, cam._projection, cam._cameraPos);

            // Draw all the models
            _graphics.SamplerStates[0] = SamplerState.LinearWrap;
            for (int i = 0; i < models.Count; i++)
                if (cam.BoundingVolumeIsInView(models[i].BoundingSphere))
                    models[i].Draw(cam._view, cam._projection, cam._cameraPos);


            lensFlare.UpdateOcclusion(cam._view, cam._nearProjection);

            Display3D.Particles.ParticlesManager.Draw(gameTime, cam._view, cam._projection);

            BlendState defaultBS = _graphics.BlendState;
            _graphics.Clear(ClearOptions.DepthBuffer, new Vector4(0), 65535, 0);
            _graphics.BlendState = BlendState.AlphaBlend;
            _character.Draw(spritebatch, gameTime, cam._view, cam._nearProjection, cam._cameraPos, weapon);
            _graphics.BlendState = defaultBS;

            lensFlare.Draw(gameTime);

            Display3D.CSimpleShapes.Draw(gameTime, cam._view, cam._projection);
            //renderer.DrawDebugBoxes(gameTime, cam._view, cam._projection);
        }

    }
}
