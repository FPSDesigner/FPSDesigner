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
        private Display3D.CModel _modelTree; // (TEST) One Model displayed
        private Display3D.CModel _testTree; // (TEST) One Model displayed

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
            Dictionary<string, Texture2D> treeTextures = new Dictionary<string, Texture2D>();
            Dictionary<string, Texture2D> testTree = new Dictionary<string, Texture2D>();

            // Display 1 tree
            treeTextures.Add("Tree001", content.Load<Texture2D>("Textures\\Model Textures\\Tree001"));
            _modelTree = new Display3D.CModel(content.Load<Model>("Models//Tree001"), new Vector3(-185.2928f, 169.4f, 80.45f), new Vector3(0f, MathHelper.PiOver2, 0f),
                         new Vector3(1.5f), graphics, treeTextures, 0.4f);

            testTree.Add("Tree002", content.Load<Texture2D>("Textures\\Model Textures\\test"));
            testTree.Add("leaf", content.Load<Texture2D>("Textures\\Model Textures\\Leaf002"));
            _testTree = new Display3D.CModel(content.Load<Model>("Models//Tree002"), new Vector3(-165.2928f, 169f, 80.45f), new Vector3(-MathHelper.PiOver2, 0f, 0f),
                        new Vector3(2f), graphics, testTree);

            // Create one container
            Dictionary<string, Texture2D> containerTextures = new Dictionary<string, Texture2D>();
            containerTextures.Add("RustyContainer", content.Load<Texture2D>("Textures\\Model Textures\\RustyContainer"));
            Display3D.CModel modelContainer = new Display3D.CModel(content.Load<Model>("Models//Container"), new Vector3(-135.2928f, 168f, 95.45f), new Vector3(0f, 0f, -MathHelper.PiOver2),
                                              new Vector3(2f), graphics, containerTextures);

            // Create barrels
            Dictionary<string, Texture2D> barrelTextures = new Dictionary<string, Texture2D>();
            barrelTextures.Add("RustyBarrel", content.Load<Texture2D>("Textures\\Model Textures\\RustyMetal"));
            Display3D.CModel modelBarrel = new Display3D.CModel(content.Load<Model>("Models//Barrel"), new Vector3(-128f, 169f, 82.45f), new Vector3(0f, 0f, 0f),
                                              new Vector3(0.75f), graphics, barrelTextures);
            Display3D.CModel modelBarrel2 = new Display3D.CModel(content.Load<Model>("Models//Barrel"), new Vector3(-120f, 169f, 88f), new Vector3(0f, 0f, 0f),
                                  new Vector3(0.75f), graphics, barrelTextures);

            // Create Trash Bags
            Dictionary<string, Texture2D> trashbagTextures = new Dictionary<string, Texture2D>();
            trashbagTextures.Add("Garbage", content.Load<Texture2D>("Textures\\Model Textures\\garbageTexture"));
            Display3D.CModel modelTrashbag = new Display3D.CModel(content.Load<Model>("Models//TrashBag"), new Vector3(-130f, 172f, 82.45f), new Vector3(0f, 0f, 0f),
                                              new Vector3(1.0f), graphics, trashbagTextures);

            // We add all the models created to the model list
            models.Add(_modelTree);
            models.Add(_testTree);
            models.Add(modelContainer);
            models.Add(modelBarrel);
            models.Add(modelBarrel2);
            models.Add(modelTrashbag);

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
                    content, graphics, new Vector3(levelData.Water.Coordinates.PosX, levelData.Water.Coordinates.PosY, levelData.Water.Coordinates.PosZ),
                    new Vector2(levelData.Water.SizeX, levelData.Water.SizeY), levelData.Water.Alpha, Display2D.C2DEffect._renderCapture.renderTarget
                );

                water.Objects.Add(skybox);

                if (levelData.Terrain.UseTerrain)
                    water.Objects.Add(terrain);
            }

            if (levelData.Terrain.UseTerrain && levelData.Water.UseWater)
                terrain.waterHeight = water.waterPosition.Y;

            /**** Camera ****/
            Game.LevelInfo.Coordinates camPosSp = levelData.SpawnInfo.SpawnCoordinates;
            cam = new Display3D.CCamera(graphics, new Vector3(camPosSp.PosX, camPosSp.PosY, camPosSp.PosZ), new Vector3(camPosSp.RotX, camPosSp.RotY, camPosSp.RotZ), levelData.SpawnInfo.NearClip, levelData.SpawnInfo.FarClip, false, (levelData.Terrain.UseTerrain) ? terrain : null, new bool[] { levelData.Terrain.UseTerrain, levelData.Water.UseWater });
            Game.CConsole._Camera = cam;

            for (int i = 0; i < models.Count; i++)
            {
                cam._physicsMap._triangleList.AddRange(models[i]._trianglesPositions);
                cam._physicsMap._triangleNormalsList.AddRange(models[i]._trianglesNormal);
            }

            if (levelData.Water.UseWater)
                cam._physicsMap._waterHeight = water.waterPosition.Y;

            /**** ****/

            Game.CSoundManager.LoadContent(water);

            // We create array containing all informations about weapons.

            weapon = new Game.CWeapon();

            Model[] testmodel = new Model[] { content.Load<Model>("Models//Machete"), content.Load<Model>("Models//M1911") };

            Texture2D[] weaponsTexture = new Texture2D[] { content.Load<Texture2D>("Textures//Uvw_Machete"), content.Load<Texture2D>("Textures//M1911") };

            //Offset M1911 null
            //rotation Matrix.CreateRotationZ(1.25f)
            //scale 1.3

            object[][] testInfos = new object[][] {
                new object[] {2,1,1,1,1,false,2.0f,1,Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.Pi),new Vector3(0.2f, 0.2f, 0.1f), 1f},
                new object[] {0,10,10,20,1,false,2.0f,1,Matrix.CreateRotationZ(1.2f)*Matrix.CreateRotationY(-0.14f),new Vector3(.05f, 0.0f, 0.18f), 1f},
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

            //Load content for Chara class
            _character.LoadContent(content, graphics, weapon);

            particlesList.Add(new Display3D.Particles.Elements.FireParticleSystem(content));
            particlesList.Add(new Display3D.Particles.Elements.GSDirtParticleSystem(content));

            for (int i = 0; i < particlesList.Count; i++)
            {
                particlesList[i].Initialize();
                particlesList[i].LoadContent(graphics);
            }
        }

        public void UnloadContent(ContentManager content)
        {

        }

        //int u = 0;
        public void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState, MouseState oldMouseState)
        {
            // Update camera - _charac.Run is a functions allows player to run, look at the param
            cam.Update(gameTime, _character.SpeedModification(kbState, cam._physicsMap._fallingVelocity, weapon, cam), isPlayerUnderwater, water.waterPosition.Y, kbState, mouseState, _oldKeyState);

            // Update all character actions
            _character.Update(mouseState, oldMouseState, kbState, _oldKeyState, weapon, gameTime, cam, (isPlayerUnderwater || cam._physicsMap._isOnWaterSurface));
            _oldKeyState = kbState;

           /* if ((u++) % 20 == 0)
            {
                particlesList[0].AddParticle(new Vector3(-165.2928f, 169f, 80.45f), Vector3.Zero);
                particlesList[1].AddParticle(new Vector3(-185.2928f, 172f, 80.45f), Vector3.Zero);
            }

            for (int i = 0; i < particlesList.Count; i++)
            {
                particlesList[i].Update(gameTime);
            }*/
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

            for (int i = 0; i < particlesList.Count; i++)
                particlesList[i].Draw(gameTime, cam._view, cam._projection);

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
