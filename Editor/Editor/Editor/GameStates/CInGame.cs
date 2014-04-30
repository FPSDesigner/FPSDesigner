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
using LTreesLibrary.Trees;

namespace Engine.GameStates
{
    class CInGame
    {
        private Display3D.CCamera cam; // (TEST) One camera instancied

        private Game.CCharacter _character; //Character : can shoot, etc..

        private KeyboardState _oldKeyState;

        private bool isPlayerUnderwater = false;
        private bool isSoftwareEmbedded = false;

        Game.LevelInfo.CLevelInfo levelInfo;
        Game.LevelInfo.LevelData levelData;
        Display3D.CSkybox skybox;
        Display3D.CTerrain terrain;
        Display3D.CLensFlare lensFlare;
        Display3D.CWater water;
        List<Display3D.Particles.ParticleSystem> particlesList = new List<Display3D.Particles.ParticleSystem>();

        Game.CWeapon weapon;
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
            isSoftwareEmbedded = Display2D.C2DEffect.isSoftwareEmbedded;
            levelData = levelInfo.loadLevelData("GameLevel.xml");
            _graphics = graphics;

            Game.CSoundManager.LoadContent();


            /**** Character ****/
            _character = new Game.CCharacter();
            _character.Initialize();

            _character._walkSpeed = levelData.SpawnInfo.WalkSpeed;
            _character._aimSpeed = levelData.SpawnInfo.AimSpeed;
            _character._sprintSpeed = levelData.SpawnInfo.SprintSpeed;

            Game.CConsole._Character = _character;

            /**** Models ****/

            foreach (Game.LevelInfo.MapModels_Model modelInfo in levelData.MapModels.Models)
            {
                Dictionary<string, Texture2D> modelTextures = new Dictionary<string, Texture2D>();

                foreach (Game.LevelInfo.MapModels_Texture textureInfo in modelInfo.Textures.Texture)
                    modelTextures.Add(textureInfo.Mesh, content.Load<Texture2D>(textureInfo.Texture));

                Display3D.CModelManager.addModel(new Display3D.CModel(
                    content.Load<Model>(modelInfo.ModelFile),
                    modelInfo.Position.Vector3,
                    modelInfo.Rotation.Vector3,
                    modelInfo.Scale.Vector3,
                    graphics,
                    modelTextures,
                    modelInfo.SpecColor,
                    modelInfo.Alpha));
            }

            lensFlare = new Display3D.CLensFlare();
            lensFlare.LoadContent(content, graphics, spriteBatch, new Vector3(0.8434627f, -0.4053462f, -0.4539611f));

            skybox = new Display3D.CSkybox(content, graphics, content.Load<TextureCube>("Textures/Clouds"));

            /**** Terrain ****/
            if (levelData.Terrain.UseTerrain)
            {
                terrain = new Display3D.CTerrain();
                terrain.InitializeTextures(levelData.Terrain.TerrainTextures, content);
                terrain.LoadContent(levelData.Terrain.CellSize, levelData.Terrain.Height, levelData.Terrain.TextureTiling, lensFlare.LightDirection, graphics, content);
                _character._terrain = terrain;
                Game.CConsole._Terrain = terrain;
            }

            /**** Water ****/
            if (levelData.Water.UseWater)
            {
                water = new Display3D.CWater(
                    content, graphics, new Vector3(levelData.Water.Coordinates.X, levelData.Water.Coordinates.Y, levelData.Water.Coordinates.Z),
                    new Vector2(levelData.Water.SizeX, levelData.Water.SizeY), levelData.Water.Alpha
                );

                water.Objects.Add(skybox);

                if (levelData.Terrain.UseTerrain)
                    water.Objects.Add(terrain);

                Game.CSoundManager.Water = water;
                Game.CConsole._Water = water;
                _character._water = water;
            }

            if (levelData.Terrain.UseTerrain && levelData.Water.UseWater)
                terrain.waterHeight = water.waterPosition.Y;

            /**** Weapons ****/
            List<Model> modelsList = new List<Model>();
            List<Texture2D> textureList = new List<Texture2D>();
            List<object[]> objList = new List<object[]>();
            List<string[]> soundList = new List<string[]>();
            List<string[]> animList = new List<string[]>();
            List<float[]> animVelocityList = new List<float[]>();

            foreach (Game.LevelInfo.Weapon wep in levelData.Weapons.Weapon)
            {
                modelsList.Add(content.Load<Model>(wep.Model));
                textureList.Add(content.Load<Texture2D>(wep.Texture));
                objList.Add(
                    new object[] { wep.Type, 10, wep.MaxClip, 10, 10, wep.IsAutomatic, wep.ShotsPerSecs, wep.Range,
                        Matrix.CreateRotationX(wep.Rotation.X) * Matrix.CreateRotationY(wep.Rotation.Y) * Matrix.CreateRotationZ(wep.Rotation.Z),
                        wep.Offset.Vector3, wep.Scale, wep.Delay, wep.Name}
                    );
                soundList.Add(
                    new string[] { wep.WeaponSound.Shot, wep.WeaponSound.DryShot, wep.WeaponSound.Reload }
                    );
                animList.Add(
                    new string[] { wep.WeaponAnim.Walk, wep.WeaponAnim.Attack, wep.WeaponAnim.Idle, wep.WeaponAnim.Reload, wep.WeaponAnim.Switch, wep.WeaponAnim.Aim, wep.WeaponAnim.AimShot }
                    );
                animVelocityList.Add(
                    new float[] { wep.WeaponAnim.WalkSpeed, wep.WeaponAnim.AttackSpeed, wep.WeaponAnim.IdleSpeed, wep.WeaponAnim.ReloadSpeed, wep.WeaponAnim.SwitchSpeed, wep.WeaponAnim.AimSpeed, wep.WeaponAnim.AimShotSpeed }
                    );
            };

            weapon = new Game.CWeapon();
            weapon.LoadContent(content, modelsList, textureList, objList, soundList, animList, animVelocityList);

            /*// We create array containing all informations about weapons.
            weapon = new Game.CWeapon();
            Model[] testmodel = new Model[] { content.Load<Model>("Models//Machete"), content.Load<Model>("Models//M1911"), content.Load<Model>("Models//M24") };

            Texture2D[] weaponsTexture = new Texture2D[] { content.Load<Texture2D>("Textures//Uv_Machete"), content.Load<Texture2D>("Textures//M1911")
            ,content.Load<Texture2D>("Textures//M24")};

            object[][] testInfos = new object[][] {
                new object[] {2,1,1,1,1,false,2.0f,1,Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationZ(2.9f),new Vector3(0.21f, 0.15f, 0.04f), 1f, 0f},
                new object[] {0,10,10,20,1,false,2.0f,1,Matrix.CreateRotationZ(1.21f)*Matrix.CreateRotationY(-0.13f),new Vector3(.075f, 0.04f, 0.22f), 1f, 100f},
                new object[] {0,10,10,20,1,false,2.0f,1, Matrix.CreateRotationY(-0.13f)* Matrix.CreateRotationX(0.15f)
                ,new Vector3(0.42f,1.95f,0.16f), 0.8f, 100f}
            };

            string[][] testSounds = new string[][] {
                new string[] {
                    "Sounds\\Weapons\\MACHET_ATTACK"
                },
                new string[] {
                    "Sounds\\Weapons\\M1911_SHOT","Sounds\\Weapons\\DryFireSound", "Sounds\\Weapons\\M1911_RELOAD",
                },
                new string[] {
                    "Sounds\\Weapons\\M1911_SHOT","Sounds\\Weapons\\DryFireSound", "Sounds\\Weapons\\M1911_RELOAD",
                },
            };

            string[][] anims = new string[][] {
                new string[] {
                    "Machete_Walk", "Machete_Attack", "Machete_Wait","","Machete_Switch",""
                },
                new string[] {
                    "M1911_Walk", "M1911_Attack", "M1911_Wait", "M1911_Reloading","M1911_Switch","M1911_Aim"
                },
                new string[] {
                    "M24_Walk", "", "M24_Wait", "","",""
                },
            };

            float[][] animVelocity = new float[][] {
                new float[] {
                    1.4f, 3.0f, 0.7f, 0.0f, 3.8f
                },
                new float[] {
                    1.6f, 16.0f, 0.65f, 2.5f, 3.8f
                },
                new float[] {
                    1.4f, 10.0f, 0.65f, 2.5f, 3.8f
                },

            };

            weapon.LoadContent(content, testmodel, weaponsTexture, testInfos, testSounds, anims, animVelocity);
            */

            /**** Particles ****/
            //Display3D.Particles.ParticlesManager.AddNewParticle("fire", new Display3D.Particles.Elements.FireParticleSystem(content), true, new Vector3(-165.2928f, 179f, 80.45f));
            Display3D.Particles.ParticlesManager.AddNewParticle("gunshot_dirt", new Display3D.Particles.Elements.GSDirtParticleSystem(content), true, Vector3.Zero, null, false);
            Display3D.Particles.ParticlesManager.AddNewParticle("gun_smoke", new Display3D.Particles.Elements.GunSmokeParticleSystem(content), true, Vector3.Zero, null, false);

            /**** Camera ****/
            Vector3 camPosition = new Vector3(levelData.SpawnInfo.SpawnPosition.X, levelData.SpawnInfo.SpawnPosition.Y, levelData.SpawnInfo.SpawnPosition.Z);
            Vector3 camRotation = new Vector3(levelData.SpawnInfo.SpawnRotation.X, levelData.SpawnInfo.SpawnRotation.Y, levelData.SpawnInfo.SpawnRotation.Z);
            float nearClip = levelData.SpawnInfo.NearClip;
            if (isSoftwareEmbedded)
                nearClip = 0.6f;
            cam = new Display3D.CCamera(graphics, camPosition, camRotation, nearClip, levelData.SpawnInfo.FarClip, isSoftwareEmbedded, isSoftwareEmbedded, (levelData.Terrain.UseTerrain) ? terrain : null, new bool[] { levelData.Terrain.UseTerrain, levelData.Water.UseWater });

            /**** Trees ****/
            Display3D.TreeManager.LoadXMLTrees(cam, content, levelData.MapModels.Trees);

            // ******* All the console informations ******* //

            _character.LoadContent(content, graphics, weapon, cam);
            Display3D.CModelManager.AddPhysicsInformations(cam);

            Game.CConsole._Camera = cam;
            Game.CConsole._Weapon = weapon;

            if (levelData.Water.UseWater)
                cam._physicsMap._waterHeight = water.waterPosition.Y;

            x = levelData.Weapons.Weapon[1].Rotation.X;
            y = levelData.Weapons.Weapon[1].Rotation.Y;
            z = levelData.Weapons.Weapon[1].Rotation.Z;
        }

        float x, y, z;


        public void UnloadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState, MouseState oldMouseState)
        {
            // Update camera - _charac.Run is a functions allows player to run, look at the param
            cam.Update(gameTime, _character.SpeedModification(kbState, cam._physicsMap._fallingVelocity, weapon, cam), isPlayerUnderwater, water.waterPosition.Y, kbState, mouseState, _oldKeyState);

            // Update all character actions
            if (!isSoftwareEmbedded)
            {
                _character.Update(mouseState, oldMouseState, kbState, _oldKeyState, weapon, gameTime, cam, (isPlayerUnderwater || cam._physicsMap._isOnWaterSurface));
                _oldKeyState = kbState;

                // ****** We get the weapon attribute to display it in console ****** //
                Game.CConsole._Weapon = weapon;
            }

            //if (kbState.IsKeyDown(Keys.Left))
            //    z -= 0.005f;
            //else if (kbState.IsKeyDown(Keys.Right))
            //    z += 0.005f;
            //else if (kbState.IsKeyDown(Keys.Up))
            //    x -= 0.005f;
            //else if (kbState.IsKeyDown(Keys.Down))
            //    x += 0.005f;

            //weapon._weaponsArray[weapon._selectedWeapon]._rotation = Matrix.CreateRotationX(x) * Matrix.CreateRotationY(y) * Matrix.CreateRotationZ(z);
            //Game.CConsole.addMessage(x + " " + y + " " + z);

            Display3D.Particles.ParticlesManager.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
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

            if (!isSoftwareEmbedded)
            {
                skybox.ColorIntensity = 0.8f;
                water.PreDraw(cam, gameTime);
                skybox.ColorIntensity = 1;
            }
            else
                _graphics.SetRenderTarget(Display2D.C2DEffect.renderTarget);

            skybox.Draw(cam._view, cam._projection, cam._cameraPos);

            terrain.Draw(cam._view, cam._projection, cam._cameraPos);

            water.Draw(cam._view, cam._projection, cam._cameraPos);

            // Draw the trees
            Display3D.TreeManager.Draw(cam, gameTime);
            _graphics.BlendState = BlendState.Opaque;
            _graphics.DepthStencilState = DepthStencilState.Default;

            // Draw all the models
            _graphics.SamplerStates[0] = SamplerState.LinearWrap;
            Display3D.CModelManager.Draw(cam, gameTime);

            _graphics.BlendState = BlendState.Additive;
            lensFlare.UpdateOcclusion(cam._view, cam._nearProjection);
            _graphics.BlendState = BlendState.Opaque;

            Display3D.Particles.ParticlesManager.Draw(gameTime, cam._view, cam._projection);

            if (!isSoftwareEmbedded)
            {
                _graphics.Clear(ClearOptions.DepthBuffer, new Vector4(0), 65535, 0);
                _character.Draw(spriteBatch, gameTime, cam._view, cam._nearProjection, cam._cameraPos, weapon);
                lensFlare.Draw(gameTime);

                water.DrawDebug(spriteBatch);
            }


            Display3D.CSimpleShapes.Draw(gameTime, cam._view, cam._projection);
            //renderer.DrawDebugBoxes(gameTime, cam._view, cam._projection);

        }

        public object SendParam(object param)
        {
            if (param.GetType().IsArray)
            {
                object[] p = (object[])param;

                string action = (string)p[0];

                if (action == "changeCamFreeze")
                {
                    cam.isCamFrozen = (bool)p[1];
                    cam.shouldUpdateMiddleScreen = true;
                }
                else if (action == "moveCameraForward")
                {
                    Matrix rotation = Matrix.CreateFromYawPitchRoll(cam._yaw, cam._pitch, cam._roll);
                    Vector3 _translation = Vector3.Transform(Vector3.Forward * (float)p[1] * 0.1f, rotation);
                    Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);

                    cam._cameraPos += _translation;
                    cam._cameraTarget = cam._cameraPos + forward;
                }
                else if (action == "click")
                {
                    // Check if user clicked a gamecomponent
                    Point cursorPos = (Point)p[1];
                    var nearPoint = _graphics.Viewport.Unproject(new Vector3(cursorPos.X, cursorPos.Y, 0), cam._projection, cam._view, Matrix.Identity);
                    var farPoint = _graphics.Viewport.Unproject(new Vector3(cursorPos.X, cursorPos.Y, 1), cam._projection, cam._view, Matrix.Identity);

                    var rayDirection = farPoint - nearPoint;
                    rayDirection.Normalize();

                    Ray ray = new Ray(nearPoint, rayDirection);

                    // Tree check
                    int treeIdSelected;
                    if (Display3D.TreeManager.CheckRayCollision(ray, out treeIdSelected))
                        return new object[] { "tree", treeIdSelected };
                }
            }
            return true;
        }

    }
}
