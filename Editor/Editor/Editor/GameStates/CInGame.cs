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
        private Display3D.CCamera cam;

        private Game.CCharacter _character; //Character : can shoot, etc..

        private KeyboardState _oldKeyState;

        private bool isPlayerUnderwater = false;
        private bool isSoftwareEmbedded = false;
        private Display3D.CGizmos Gizmos;

        Game.LevelInfo.CLevelInfo levelInfo;
        Game.LevelInfo.LevelData levelData;
        Display3D.CSkybox skybox;
        Display3D.CTerrain terrain;
        Display3D.CLensFlare lensFlare;
        List<Display3D.Particles.ParticleSystem> particlesList = new List<Display3D.Particles.ParticleSystem>();

        Game.CWeapon weapon;
        private GraphicsDevice _graphics;
        private ContentManager _content;

        private Dictionary<string, Model> modelsListWeapons = new Dictionary<string, Model>();
        private Dictionary<string, Texture2D> textureListWeapons = new Dictionary<string, Texture2D>();

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
            _content = content;

            Game.CSoundManager.LoadContent();

            Display3D.CModelManager.Initialize(content, graphics);
            Game.CEnemyManager.LoadContent(content);

            // We Add some useful sounds
            SoundEffect sound = content.Load<SoundEffect>("Sounds\\GRASSSTEP");
            Game.CSoundManager.AddSound("GRASSSTEP", sound, false, 10.0f);

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

                Dictionary<string, Texture2D> bumpTextures = new Dictionary<string, Texture2D>();

                if (modelInfo.BumpTextures != null)
                {
                    foreach (Game.LevelInfo.MapModels_Texture textureInfo in modelInfo.BumpTextures.Texture)
                        bumpTextures.Add(textureInfo.Mesh, content.Load<Texture2D>(textureInfo.Texture));
                }

                Display3D.CModelManager.addModel(_content,
                    new Display3D.CModel(
                    content.Load<Model>(modelInfo.ModelFile),
                    modelInfo.Position.Vector3,
                    modelInfo.Rotation.Vector3,
                    modelInfo.Scale.Vector3,
                    graphics,
                    modelTextures,
                    modelInfo.SpecColor,
                    modelInfo.Alpha,
                    bumpTextures,
                    (modelInfo.Explodable != null && modelInfo.Explodable)));
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
            if (levelData.Water != null && levelData.Water.Water != null && levelData.Water.Water.Count > 0)
            {
                foreach (Game.LevelInfo.Water water in levelData.Water.Water)
                {
                    Display3D.CWater waterInstance = new Display3D.CWater(
                        content, graphics, water.Coordinates.Vector3, water.DeepestPoint.Vector3,
                        new Vector2(water.SizeX, water.SizeY), water.Alpha
                    );

                    waterInstance.Objects.Add(skybox);

                    if (levelData.Terrain.UseTerrain)
                        waterInstance.Objects.Add(terrain);

                    Display3D.CWaterManager.AddWater(waterInstance);
                }

                terrain.waterHeight = Display3D.CWaterManager.listWater[0].waterPosition.Y;
            }

            /**** Weapons ****/
            List<object[]> objList = new List<object[]>();
            List<string[]> soundList = new List<string[]>();
            List<string[]> animList = new List<string[]>();
            List<float[]> animVelocityList = new List<float[]>();

            foreach (Game.LevelInfo.Weapon wep in levelData.Weapons.Weapon)
            {
                modelsListWeapons.Add(wep.Name, content.Load<Model>(wep.Model));
                textureListWeapons.Add(wep.Name, content.Load<Texture2D>(wep.Texture));
                objList.Add(
                    new object[] { wep.Type, wep.MaxClip, wep.MaxClip, 50, 1, wep.IsAutomatic, wep.ShotsPerSecs, wep.Range,
                        Matrix.CreateRotationX(wep.Rotation.X) * Matrix.CreateRotationY(wep.Rotation.Y) * Matrix.CreateRotationZ(wep.Rotation.Z),
                        wep.Offset.Vector3, wep.Scale, wep.Delay, wep.Name, wep.RecoilIntensity, wep.RecoilBackIntensity,wep.DamagesPerBullet }
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
            weapon.LoadContent(content, modelsListWeapons, textureListWeapons, objList, soundList, animList, animVelocityList);

            /**** Pickups ****/
            foreach (Game.LevelInfo.MapModels_Pickups pickup in levelData.MapModels.Pickups)
                Display3D.CPickUpManager.AddPickup(graphics, modelsListWeapons[pickup.WeaponName], textureListWeapons[pickup.WeaponName], pickup.Position.Vector3, pickup.Rotation.Vector3, pickup.Scale.Vector3, pickup.WeaponName, pickup.WeaponBullets);


            /**** Particles ****/
            //Display3D.Particles.ParticlesManager.AddNewParticle("fire", new Display3D.Particles.Elements.FireParticleSystem(content), true, new Vector3(-165.2928f, 179f, 80.45f));
            Display3D.Particles.ParticlesManager.AddNewParticle("gunshot_dirt", new Display3D.Particles.Elements.GSDirtParticleSystem(content), true, Vector3.Zero, null, false);
            Display3D.Particles.ParticlesManager.AddNewParticle("gun_smoke", new Display3D.Particles.Elements.GunSmokeParticleSystem(content), true, Vector3.Zero, null, false);
            Display3D.Particles.ParticlesManager.AddNewParticle("explosion", new Display3D.Particles.Elements.ExplosionParticleSystem(content), true, Vector3.Zero, null, false);
            Display3D.Particles.ParticlesManager.AddParticle("explosion", new Vector3(-115, 172, 80), 10);

            /**** Camera ****/
            Vector3 camPosition = new Vector3(levelData.SpawnInfo.SpawnPosition.X, levelData.SpawnInfo.SpawnPosition.Y, levelData.SpawnInfo.SpawnPosition.Z);
            Vector3 camRotation = new Vector3(levelData.SpawnInfo.SpawnRotation.X, levelData.SpawnInfo.SpawnRotation.Y, levelData.SpawnInfo.SpawnRotation.Z);
            float nearClip = levelData.SpawnInfo.NearClip;
            if (isSoftwareEmbedded)
                nearClip = 0.6f;
            cam = new Display3D.CCamera(graphics, camPosition, camRotation, nearClip, levelData.SpawnInfo.FarClip, isSoftwareEmbedded, isSoftwareEmbedded, (levelData.Terrain.UseTerrain) ? terrain : null, new bool[] { levelData.Terrain.UseTerrain, (levelData.Water != null && levelData.Water.Water != null && levelData.Water.Water.Count > 0) });

            /**** Lights ****/
            if (levelData.Lights.UseShadow != null && levelData.Lights.UseShadow)
                Display3D.CModelManager.ApplyRendererShadow(content, levelData.Lights.ShadowLightPos.Vector3, levelData.Lights.ShadowLightTarget.Vector3);
            else
                Display3D.CModelManager.ApplyLightEffect(content);
            Display3D.CLightsManager.LoadContent(content);
            if (levelData.Lights != null)
            {
                foreach (Game.LevelInfo.Light light in levelData.Lights.LightsList)
                    Display3D.CLightsManager.AddLight(light.Position.Vector3, light.Col, light.Attenuation);
                Display3D.CLightsManager.AddToRenderer();
            }
            Display3D.CModelManager.renderer.Camera = cam;
            /**** Trees ****/
            Display3D.TreeManager.LoadXMLTrees(cam, content, levelData.MapModels.Trees);

            /**** Gizmos ****/
            if (isSoftwareEmbedded)
                Gizmos = new Display3D.CGizmos(content, graphics, cam);

            // ******* All the console informations ******* //
            _character.LoadContent(content, graphics, weapon, cam, levelData.SpawnInfo.HandTexture);
            Display3D.CModelManager.AddPhysicsInformations(cam);

            Game.CConsole._Camera = cam;
            Game.CConsole._Weapon = weapon;

            List<float> totalWaterHeight = new List<float>();
            foreach (Display3D.CWater wat in Display3D.CWaterManager.listWater)
                totalWaterHeight.Add(wat.waterPosition.Y);

            if (levelData.Water != null && levelData.Water.Water != null && levelData.Water.Water.Count > 0)
                cam._physicsMap._waterHeight = totalWaterHeight;

            /**** Enemies ****/
            if (levelData.Bots != null && levelData.Bots.Bots.Count > 0)
                foreach (Game.LevelInfo.Bot bots in levelData.Bots.Bots)
                {
                    List<Texture2D> enemyTexture = new List<Texture2D>();

                    foreach (Game.LevelInfo.MapModels_Texture textureInfo in bots.ModelTexture.Texture)
                        enemyTexture.Add(LoadCorrectlyTexture(textureInfo.Texture));

                    Game.CEnemyManager.AddEnemy(content, cam, new Game.CEnemy(bots.ModelName, enemyTexture.ToArray(), bots.SpawnPosition.Vector3, bots.SpawnPosition.RotMatrix, bots.Life, bots.Velocity, bots.RangeOfAttack, bots.IsAggressive, bots.Name));
                }

            /*// ENEMY TEST
            Texture2D[] ennemyTexture = new Texture2D[1];
            ennemyTexture[0] = content.Load<Texture2D>("Textures\\StormTrooper");
            _enemy = new Game.CEnemy("StormTrooperAnimation", ennemyTexture, new Vector3(-142.7562f, 168.2f, 100.6888f)
                , Matrix.CreateRotationX(-1 * MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.Pi), 100.0f, 6f, 100.0f, false);*/

            // Initialize the Projectile Manager
            Display3D.CProjectileManager.Initialize();
        }

        public void UnloadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime, KeyboardState kbState, MouseState mouseState, MouseState oldMouseState)
        {
            // Update camera - _charac.Run is a functions allows player to run, look at the param
            cam.Update(gameTime, _character.SpeedModification(kbState, cam._physicsMap._fallingVelocity, weapon, cam), isPlayerUnderwater, Display3D.CWaterManager.GetWaterHeight(cam._cameraPos), kbState, mouseState, _oldKeyState);

            // Update all character actions
            if (!isSoftwareEmbedded)
            {
                _character.Update(mouseState, oldMouseState, kbState, _oldKeyState, weapon, gameTime, cam, (isPlayerUnderwater || cam._physicsMap._isOnWaterSurface));
                _oldKeyState = kbState;

                // ****** We get the weapon attribute to display it in console ****** //
                Game.CConsole._Weapon = weapon;
            }

            // Update projectile Manager
            Display3D.CProjectileManager.updateProjectileList(gameTime, terrain);

            if (!isSoftwareEmbedded)
                Display3D.TreeManager.Update(gameTime);

            // Check if player entered a pickup
            Display3D.CPickUpManager.Update(gameTime);
            Display3D.CPickUp EnteredPickup;

            if (!isSoftwareEmbedded && Display3D.CPickUpManager.CheckEnteredPickUp(new BoundingSphere(cam._cameraPos, 1.5f), out EnteredPickup))
            {
                int futurIndex = weapon._weaponPossessed.Count;

                weapon.GiveWeapon(EnteredPickup._weaponName, EnteredPickup._weaponBullets); // Give the weapon

                Game.CSoundManager.PlayInstance("PICKUPWEAPON", 1f); // Play the sound

                _character.ChangeWeapon(weapon, mouseState, true, futurIndex);

                Display3D.CPickUpManager.DelPickup(EnteredPickup); // Destroy the pickup
            }

            Game.CEnemyManager.Update(gameTime, cam);

            Game.CEnemyManager._enemyList[0].debugKey(kbState);

            Display3D.Particles.ParticlesManager.Update(gameTime);

            
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _graphics.BlendState = BlendState.Opaque;
            Display3D.CModelManager.renderer.Draw();

            Vector3 playerPos = cam._cameraPos;

            if (isPlayerUnderwater != Display3D.CWaterManager.IsPositionUnderwater(playerPos))
            {
                isPlayerUnderwater = !isPlayerUnderwater;
                Display3D.CWaterManager.isUnderWater = isPlayerUnderwater;
                terrain.isUnderWater = isPlayerUnderwater;
            }
            terrain.frustum = cam._Frustum;

            /*if (!isSoftwareEmbedded)
            {*/
                skybox.ColorIntensity = 0.8f;
                Display3D.CWaterManager.PreDraw(cam, gameTime);
                skybox.ColorIntensity = 1;
            /*}
            else
                _graphics.SetRenderTarget(Display2D.C2DEffect.renderTarget);*/

            skybox.Draw(cam._view, cam._projection, cam._cameraPos);

            terrain.Draw(cam._view, cam._projection, cam._cameraPos);

            Display3D.CWaterManager.Draw(cam._view, cam._projection, cam._cameraPos);

            // Draw the trees
            Display3D.TreeManager.Draw(cam, gameTime);
            _graphics.BlendState = BlendState.Opaque;
            _graphics.DepthStencilState = DepthStencilState.Default;

            // Draw all the models
            _graphics.SamplerStates[0] = SamplerState.LinearWrap;
            Display3D.CModelManager.Draw(cam, gameTime);

            // Draw enemies
            Game.CEnemyManager.Draw(gameTime, spriteBatch, cam);

            // Draw pickups
            Display3D.CPickUpManager.Draw(cam, gameTime);

            _graphics.BlendState = BlendState.Additive;
            lensFlare.UpdateOcclusion(cam._view, cam._nearProjection);
            _graphics.BlendState = BlendState.Opaque;

            Display3D.Particles.ParticlesManager.Draw(gameTime, cam._view, cam._projection);

            // Draw Projectiles
            Display3D.CProjectileManager.drawThrownProjectiles(gameTime, cam._view, cam._projection, cam);

            if (!isSoftwareEmbedded)
            {
                _graphics.Clear(ClearOptions.DepthBuffer, new Vector4(0), 65535, 0);
                if (cam.shouldDrawPlayer)
                    _character.Draw(spriteBatch, gameTime, cam._view, cam._nearProjection, cam._cameraPos, weapon);
                lensFlare.Draw(gameTime);
                Game.CEnemyManager.AddEnemyHud(spriteBatch, cam);
            }

            Display3D.CWaterManager.DrawDebug(spriteBatch);
            Display3D.CSimpleShapes.Draw(gameTime, cam._view, cam._projection);

            if (isSoftwareEmbedded)
            {
                _graphics.Clear(ClearOptions.DepthBuffer, new Vector4(0), 65535, 0);
                Gizmos.Draw(cam, gameTime);
                Display3D.CLightsManager.DrawSelect(spriteBatch, cam);
            }

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
                else if (action == "forwardVec")
                {
                    Matrix rotation = Matrix.CreateFromYawPitchRoll(cam._yaw, cam._pitch, cam._roll);
                    Vector3 _translation = Vector3.Transform(Vector3.Forward * (float)p[1], rotation);
                    //Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
                    return cam._cameraPos + _translation;
                }
                else if (action == "centerCamOnObject")
                {
                    Vector3 pos = Vector3.Zero;
                    object[] values = (object[])p[1];

                    string type = (string)values[0];
                    int eltId = (int)values[1];

                    if (type == "tree")
                        pos = Display3D.TreeManager._tTrees[eltId].Position;
                    else if (type == "model")
                        pos = Display3D.CModelManager.modelsList[eltId]._modelPosition;
                    else if (type == "pickup")
                        pos = Display3D.CPickUpManager._pickups[eltId]._Model._modelPosition;
                    else if (type == "water")
                        pos = Display3D.CWaterManager.listWater[eltId].waterPosition;
                    else if (type == "light")
                        pos = Display3D.CLightsManager.lights[eltId].Position;
                    else if (type == "bot")
                        pos = Game.CEnemyManager._enemyList[eltId]._model._position;

                    cam._cameraTarget = pos;

                    cam._yaw = (float)Math.Atan2(cam._cameraPos.X - pos.X, cam._cameraPos.Z - pos.Z);
                    cam._pitch = (float)Math.Atan2(pos.Y - cam._cameraPos.Y, Math.Sqrt(Math.Pow(cam._cameraPos.X - pos.X, 2) + Math.Pow(cam._cameraPos.Z - pos.Z, 2)));
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
                    System.Windows.Point cursorPos = (System.Windows.Point)p[1];

                    Vector3 nearSource = Display2D.C2DEffect.softwareViewport.Unproject(new Vector3((float)cursorPos.X, (float)cursorPos.Y, Display2D.C2DEffect.softwareViewport.MinDepth), cam._projection, cam._view, Matrix.Identity);
                    Vector3 farSource = Display2D.C2DEffect.softwareViewport.Unproject(new Vector3((float)cursorPos.X, (float)cursorPos.Y, Display2D.C2DEffect.softwareViewport.MaxDepth), cam._projection, cam._view, Matrix.Identity);
                    Vector3 direction = farSource - nearSource;

                    direction.Normalize();

                    Ray ray = new Ray(nearSource, direction);

                    // Gyzmo is the priority click
                    if (Gizmos.shouldDrawPos)
                    {
                        int? axisClicked = Gizmos.RayIntersectsAxis(ray, "pos");
                        if (axisClicked != null)
                            return new object[] { "gizmo", (int)axisClicked };
                    }
                    else if (Gizmos.shouldDrawRot)
                    {
                        int? axisClicked = Gizmos.RayIntersectsAxis(ray, "rot");
                        if (axisClicked != null)
                            return new object[] { "gizmo", (int)axisClicked };
                    }
                    else if (Gizmos.shouldDrawScale)
                    {
                        int? axisClicked = Gizmos.RayIntersectsAxis(ray, "scale");
                        if (axisClicked != null)
                            return new object[] { "gizmo", (int)axisClicked };
                    }

                    // Light check
                    int selectedLight;
                    if (Display3D.CLightsManager.PointClicksLight(new Vector2((float)cursorPos.X, (float)cursorPos.Y), cam, out selectedLight))
                        return new object[] { "light", selectedLight };

                    // Click distances
                    float? treeDistance = null, modelDistance = null, pickupDistance = null, botDistance = null;

                    // Tree check
                    int treeIdSelected;
                    treeDistance = Display3D.TreeManager.CheckRayCollision(ray, out treeIdSelected);

                    // Model check
                    int modelIdSelected;
                    modelDistance = Display3D.CModelManager.CheckRayIntersectsModel(ray, out modelIdSelected);

                    // Pickup check
                    int pickupIdSelected;
                    pickupDistance = Display3D.CPickUpManager.CheckRayIntersectsAnyPickup(ray, out pickupIdSelected);

                    // Bot check
                    int botIdSelected;
                    botDistance = Game.CEnemyManager.CheckRayIntersectsAnyBot(ray, out botIdSelected);

                    // Check which is the closest one
                    if (treeDistance != null && (treeDistance < modelDistance || modelDistance == null) && (treeDistance < pickupDistance || pickupDistance == null) && (treeDistance < botDistance || botDistance == null))
                        return new object[] { "tree", treeIdSelected };
                    else if (modelDistance != null && (modelDistance < pickupDistance || pickupDistance == null) && (modelDistance < botDistance || botDistance == null))
                        return new object[] { "model", modelIdSelected };
                    else if (pickupDistance != null && (modelDistance < botDistance || botDistance == null))
                        return new object[] { "pickup", pickupIdSelected };
                    else if (botDistance != null)
                        return new object[] { "bot", botIdSelected };
                }
                else if (action == "unselectObject")
                {
                    object[] values = (object[])p[1];
                    if ((string)values[0] == "tree")
                        Display3D.TreeManager.selectedTreeId = -1;
                    else if ((string)values[0] == "model")
                        Display3D.CModelManager.selectModelId = -1;
                    else if ((string)values[0] == "pickup")
                        Display3D.CPickUpManager.selectedPickupId = -1;
                    else if ((string)values[0] == "water")
                        Display3D.CWaterManager.selectedWater = -1;
                    else if ((string)values[0] == "light")
                        Display3D.CLightsManager.selectedLight = -1;
                    else if ((string)values[0] == "bot")
                        Game.CEnemyManager.selectedBot = -1;
                }
                else if (action == "selectObject")
                {
                    object[] values = (object[])p[1];

                    Vector3 newPos = Vector3.Zero;
                    if ((string)values[0] == "tree")
                    {
                        Display3D.TreeManager.selectedTreeId = (int)values[1];
                        newPos = Display3D.TreeManager._tTrees[(int)values[1]].Position;
                    }
                    else if ((string)values[0] == "model")
                    {
                        Display3D.CModelManager.selectModelId = (int)values[1];
                        newPos = Display3D.CModelManager.modelsList[(int)values[1]]._modelPosition;
                    }
                    else if ((string)values[0] == "pickup")
                    {
                        Display3D.CPickUpManager.selectedPickupId = (int)values[1];
                        newPos = Display3D.CPickUpManager._pickups[(int)values[1]]._Model._modelPosition;
                    }
                    else if ((string)values[0] == "water")
                    {
                        Display3D.CWaterManager.selectedWater = (int)values[1];
                        newPos = Display3D.CWaterManager.listWater[(int)values[1]].waterPosition;
                    }
                    else if ((string)values[0] == "light")
                    {
                        Display3D.CLightsManager.selectedLight = (int)values[1];
                        newPos = Display3D.CLightsManager.lights[(int)values[1]].Position;
                    }
                    else if ((string)values[0] == "bot")
                    {
                        Game.CEnemyManager.selectedBot = (int)values[1];
                        newPos = Game.CEnemyManager._enemyList[(int)values[1]]._model._position;
                    }
                    Gizmos.posGizmo._modelPosition = newPos;
                    Gizmos.rotGizmo._modelPosition = newPos;
                    Gizmos.scaleGizmo._modelPosition = newPos;
                    Gizmos.shouldDrawPos = false;
                    Gizmos.shouldDrawRot = false;
                    Gizmos.shouldDrawScale = false;
                    if ((string)values[2] == "PositionButton")
                        Gizmos.shouldDrawPos = true;
                    else if ((string)values[2] == "RotateButton")
                        Gizmos.shouldDrawRot = true;
                    else if ((string)values[2] == "ScaleButton")
                        Gizmos.shouldDrawScale = true;
                }
                else if (action == "changeTool")
                {
                    object[] values = (object[])p[1];
                    string newTool = (string)values[0];

                    Gizmos.shouldDrawPos = false;
                    Gizmos.shouldDrawRot = false;
                    Gizmos.shouldDrawScale = false;

                    if (newTool == "PositionButton")
                        Gizmos.shouldDrawPos = true;
                    else if (newTool == "RotateButton")
                        Gizmos.shouldDrawRot = true;
                    else if (newTool == "ScaleButton")
                        Gizmos.shouldDrawScale = true;
                }
                else if (action == "moveObject")
                {
                    object[] values = (object[])p[1];
                    string subaction = (string)values[0];

                    if (subaction == "start")
                    {
                        Gizmos.StartDrag((int)values[1], (string)values[2], (int)values[3], (System.Windows.Point)values[4], cam);
                    }
                    else if (subaction == "drag")
                    {
                        System.Windows.Point mousePoint = (System.Windows.Point)values[1];
                        Gizmos.Drag((int)mousePoint.X, (int)mousePoint.Y, cam);
                    }
                    else if (subaction == "stop")
                    {
                        Gizmos.StopDrag();
                    }
                }
                else if (action == "getElementInfo")
                {
                    object[] values = (object[])p[1];
                    string info = (string)values[0];
                    string eltType = (string)values[1];
                    int eltId = (int)values[2];
                    Vector3 pos = Vector3.Zero;

                    if (info == "pos")
                    {
                        if (eltType == "tree")
                            pos = Display3D.TreeManager._tTrees[eltId].Position;
                        else if (eltType == "model")
                            pos = Display3D.CModelManager.modelsList[eltId]._modelPosition;
                        else if (eltType == "pickup")
                            pos = Display3D.CPickUpManager._pickups[eltId]._Model._modelPosition;
                        else if (eltType == "water")
                            pos = Display3D.CWaterManager.listWater[eltId].waterPosition;
                        else if (eltType == "light")
                            pos = Display3D.CLightsManager.lights[eltId].Position;
                        else if (eltType == "bot")
                            pos = Game.CEnemyManager._enemyList[eltId]._model._position;
                    }
                    else if (info == "rot")
                    {
                        if (eltType == "tree")
                            pos = Display3D.TreeManager._tTrees[eltId].Rotation;
                        else if (eltType == "model")
                            pos = Display3D.CModelManager.modelsList[eltId]._modelRotation;
                        else if (eltType == "pickup")
                            pos = Display3D.CPickUpManager._pickups[eltId]._Model._modelRotation;
                        else if (eltType == "bot")
                            pos = new Vector3(Game.CEnemyManager._enemyList[eltId].rotationValue, 0, 0);
                    }
                    else if (info == "scale")
                    {
                        if (eltType == "tree")
                            pos = Display3D.TreeManager._tTrees[eltId].Scale;
                        else if (eltType == "model")
                            pos = Display3D.CModelManager.modelsList[eltId]._modelScale;
                        else if (eltType == "pickup")
                            pos = Display3D.CPickUpManager._pickups[eltId]._Model._modelScale;
                        else if (eltType == "water")
                            pos = Display3D.CWaterManager.listWater[eltId].waterMesh._modelScale;
                        else if (eltType == "bot")
                            pos = Game.CEnemyManager._enemyList[eltId]._scale;
                    }
                    else if (info == "treeseed")
                        return Display3D.TreeManager._tTrees[eltId]._seed;
                    else if (info == "treeprofile")
                        return Display3D.TreeManager._tTrees[eltId]._profile;
                    else if (info == "pickupname")
                        return Display3D.CPickUpManager._pickups[eltId]._weaponName;
                    else if (info == "pickupbullet")
                        return Display3D.CPickUpManager._pickups[eltId]._weaponBullets;
                    else if (info == "lightrange")
                        return Display3D.CLightsManager.lights[eltId].Attenuation;
                    else if (info == "explodable")
                        return Display3D.CModelManager.modelsList[eltId]._Explodable;
                    else if (info == "lightcolor")
                    {
                        Color col = Display3D.CLightsManager.lights[eltId].Color;
                        return col.R.ToString("X") + col.G.ToString("X") + col.B.ToString("X");
                    }
                    else if (info.Substring(0, 4) == "bot_")
                    {
                        if (info == "bot_name")
                            return Game.CEnemyManager._enemyList[eltId]._hudText;
                        else if (info == "bot_life")
                            return Game.CEnemyManager._enemyList[eltId]._life;
                        else if (info == "bot_speed")
                            return Game.CEnemyManager._enemyList[eltId]._runningVelocity;
                        else if (info == "bot_type")
                            return Game.CEnemyManager._enemyList[eltId]._type;
                        else if (info == "bot_rangeofattack")
                            return Game.CEnemyManager._enemyList[eltId]._rangeAttack;
                        else if (info == "bot_isaggressive")
                            return Game.CEnemyManager._enemyList[eltId]._isAgressive;
                    }
                    return new object[] { pos.X, pos.Y, pos.Z };
                }
                else if (action == "removeElement")
                {
                    object[] values = (object[])p[1];
                    string eltType = (string)values[0];
                    int eltId = (int)values[1];

                    if (eltType == "tree")
                    {
                        Display3D.TreeManager.selectedTreeId = -1;
                        Display3D.TreeManager._tTrees.RemoveAt(eltId);
                    }
                    else if (eltType == "pickup")
                    {
                        Display3D.CPickUpManager.selectedPickupId = -1;
                        Display3D.CPickUpManager._pickups.RemoveAt(eltId);
                    }
                    else if (eltType == "model")
                    {
                        Display3D.CModelManager.selectModelId = -1;
                        Display3D.CModelManager.modelsList.RemoveAt(eltId);
                    }
                    else if (eltType == "water")
                    {
                        Display3D.CWaterManager.selectedWater = -1;
                        Display3D.CWaterManager.listWater.RemoveAt(eltId);
                    }
                    else if (eltType == "light")
                    {
                        Display3D.CLightsManager.selectedLight = -1;
                        Display3D.CLightsManager.RemoveLight(eltId);
                    }
                    else if (eltType == "bot")
                    {
                        Game.CEnemyManager.selectedBot = -1;
                        Game.CEnemyManager.RemoveBot(eltId);
                    }
                    SaveXMLFile();
                }
                else if (action == "addElement")
                {
                    object[] values = (object[])p[1];
                    string eltType = (string)values[0];

                    if (eltType == "tree")
                    {
                        Game.LevelInfo.MapModels_Tree treeInfo = (Game.LevelInfo.MapModels_Tree)values[1];
                        levelData.MapModels.Trees.Add(treeInfo);
                        Display3D.TreeManager.AddTree(cam, _content, treeInfo);
                    }
                    else if (eltType == "model")
                    {
                        Game.LevelInfo.MapModels_Model modelInfo = (Game.LevelInfo.MapModels_Model)values[1];
                        levelData.MapModels.Models.Add(new Game.LevelInfo.MapModels_Model
                        {
                            Alpha = modelInfo.Alpha,
                            BumpTextures = modelInfo.BumpTextures,
                            ModelFile = modelInfo.ModelFile,
                            Position = modelInfo.Position,
                            Rotation = modelInfo.Rotation,
                            Textures = modelInfo.Textures,
                            Scale = modelInfo.Scale,
                            SpecColor = modelInfo.SpecColor,
                            Explodable = modelInfo.Explodable
                        });

                        Dictionary<string, Texture2D> modelTextures = new Dictionary<string, Texture2D>();

                        foreach (Game.LevelInfo.MapModels_Texture textureInfo in modelInfo.Textures.Texture)
                            modelTextures.Add(textureInfo.Mesh, _content.Load<Texture2D>(textureInfo.Texture));

                        Dictionary<string, Texture2D> bumpTextures = new Dictionary<string, Texture2D>();

                        if (modelInfo.BumpTextures != null)
                        {
                            foreach (Game.LevelInfo.MapModels_Texture textureInfo in modelInfo.BumpTextures.Texture)
                                bumpTextures.Add(textureInfo.Mesh, _content.Load<Texture2D>(textureInfo.Texture));
                        }

                        return Display3D.CModelManager.addModel(_content,
                            new Display3D.CModel(
                            _content.Load<Model>(modelInfo.ModelFile),
                            modelInfo.Position.Vector3,
                            modelInfo.Rotation.Vector3,
                            modelInfo.Scale.Vector3,
                            _graphics,
                            modelTextures,
                            modelInfo.SpecColor,
                            modelInfo.Alpha,
                            bumpTextures,
                            modelInfo.Explodable));

                    }
                    else if (eltType == "pickup")
                    {
                        Game.LevelInfo.MapModels_Pickups pickupVal = (Game.LevelInfo.MapModels_Pickups)values[1];
                        levelData.MapModels.Pickups.Add(pickupVal);
                        return Display3D.CPickUpManager.AddPickup(_graphics, modelsListWeapons[pickupVal.WeaponName], textureListWeapons[pickupVal.WeaponName], pickupVal.Position.Vector3, pickupVal.Rotation.Vector3, pickupVal.Scale.Vector3, pickupVal.WeaponName, pickupVal.WeaponBullets);
                    }
                    else if (eltType == "water")
                    {
                        Game.LevelInfo.Water waterVal = (Game.LevelInfo.Water)values[1];
                        levelData.Water.Water.Add(waterVal);
                        return Display3D.CWaterManager.AddWater(new Display3D.CWater(_content, _graphics, waterVal.Coordinates.Vector3, waterVal.DeepestPoint.Vector3, new Vector2(waterVal.SizeX, waterVal.SizeY), waterVal.Alpha));
                    }
                    else if (eltType == "light")
                    {
                        Game.LevelInfo.Light lightVal = (Game.LevelInfo.Light)values[1];
                        levelData.Lights.LightsList.Add(lightVal);
                        return Display3D.CLightsManager.AddLight(lightVal.Position.Vector3, lightVal.Col, lightVal.Attenuation);
                    }
                    else if (eltType == "bot")
                    {
                        Game.LevelInfo.Bot botVal = (Game.LevelInfo.Bot)values[1];
                        levelData.Bots.Bots.Add(botVal);

                        Texture2D[] textures = new Texture2D[1];

                        if (botVal.ModelTexture != null && botVal.ModelTexture.Texture != null)
                        {
                            textures = new Texture2D[botVal.ModelTexture.Texture.Count];
                            for (int i = 0; i < botVal.ModelTexture.Texture.Count; i++)
                                textures[i] = LoadCorrectlyTexture(botVal.ModelTexture.Texture[i].Texture);
                        }

                        Game.CEnemy enemy = new Game.CEnemy(botVal.ModelName, textures, botVal.SpawnPosition.Vector3, botVal.SpawnRotation.RotMatrix, botVal.Life, botVal.Velocity, botVal.RangeOfAttack, botVal.IsAggressive, botVal.Name, botVal.Type);

                        Game.CEnemyManager.AddEnemy(_content, cam, enemy);
                    }
                }

                else if (action == "setElementInfo")
                {
                    object[] values = (object[])p[1];
                    string info = (string)values[0];
                    object val = values[1];
                    string eltType = (values.Length > 2) ? (string)values[2] : "";
                    int eltId = (values.Length > 3) ? (int)values[3] : 0;

                    if (info == "handsTexture")
                    {
                        levelData.SpawnInfo.HandTexture = (string)val;
                    }

                    if (info == "pos" || info == "rot" || info == "scale")
                    {
                        if (val is object[])
                        {
                            object[] val2 = (object[])val;
                            Vector3 newPos = Vector3.Zero;
                            if (float.TryParse(val2[0].ToString(), out newPos.X) && float.TryParse(val2[1].ToString(), out newPos.Y) && float.TryParse(val2[2].ToString(), out newPos.Z))
                            {
                                if (info == "pos")
                                {
                                    if (eltType == "tree")
                                        Display3D.TreeManager._tTrees[eltId].Position = newPos;
                                    else if (eltType == "model")
                                        Display3D.CModelManager.modelsList[eltId]._modelPosition = newPos;
                                    else if (eltType == "pickup")
                                        Display3D.CPickUpManager._pickups[eltId]._Model._modelPosition = newPos;
                                    else if (eltType == "water")
                                        Display3D.CWaterManager.listWater[eltId].waterPosition = newPos;
                                    else if (eltType == "light")
                                        Display3D.CLightsManager.lights[eltId].Position = newPos;
                                }
                                else if (info == "rot")
                                {
                                    if (eltType == "tree")
                                        Display3D.TreeManager._tTrees[eltId].Rotation = newPos;
                                    else if (eltType == "model")
                                        Display3D.CModelManager.modelsList[eltId]._modelRotation = newPos;
                                    else if (eltType == "pickup")
                                        Display3D.CPickUpManager._pickups[eltId]._Model._modelRotation = newPos;
                                }
                                else if (info == "scale")
                                {
                                    if (eltType == "tree")
                                        Display3D.TreeManager._tTrees[eltId].Scale = newPos;
                                    else if (eltType == "model")
                                        Display3D.CModelManager.modelsList[eltId]._modelScale = newPos;
                                    else if (eltType == "pickup")
                                        Display3D.CPickUpManager._pickups[eltId]._Model._modelScale = newPos;
                                    else if (eltType == "water")
                                        Display3D.CWaterManager.listWater[eltId].waterSize = new Vector2(newPos.X, newPos.Z);
                                }
                            }
                        }
                    }
                    else if (info == "pickupbullets")
                    {
                        int newBullet;
                        if (Int32.TryParse(values[1].ToString(), out newBullet))
                        {
                            Display3D.CPickUpManager._pickups[eltId]._weaponBullets = newBullet;
                        }
                    }
                    else if (info == "pickupname")
                    {
                        string newName = val.ToString();
                        Display3D.CPickUpManager._pickups[eltId]._weaponName = newName;
                        foreach (Game.CWeapon.WeaponData wpd in weapon._weaponsArray)
                        {
                            if (wpd._name == newName)
                            {
                                Dictionary<string, Texture2D> textureList = new Dictionary<string, Texture2D>();
                                textureList.Add("ApplyAllMesh", wpd._weapTexture);
                                Display3D.CPickUpManager._pickups[eltId]._Model = new Display3D.CModel(wpd._wepModel, Display3D.CPickUpManager._pickups[eltId]._Model._modelPosition,
                                    Display3D.CPickUpManager._pickups[eltId]._Model._modelRotation, Display3D.CPickUpManager._pickups[eltId]._Model._modelScale,
                                    _graphics, textureList, 0, 1, null);
                                return true;
                            }
                        }
                    }
                    else if (info == "lightcolor")
                    {
                        string colorVal = values[1].ToString().Trim().Replace("#", "");
                        if ((colorVal.Length == 6 || colorVal.Length == 8) && System.Text.RegularExpressions.Regex.IsMatch(colorVal, @"\A\b[0-9a-fA-F]+\b\Z"))
                        {
                            Display3D.CLightsManager.lights[eltId].Color = Display3D.CLightsManager.GetColorFromHex(colorVal);
                            return true;
                        }
                    }
                    else if (info == "lightrange")
                    {
                        int attenuation;
                        if (Int32.TryParse(values[1].ToString(), out attenuation))
                            Display3D.CLightsManager.lights[eltId].Attenuation = attenuation;
                    }
                    else if (info == "explodable")
                    {
                        if (values[1] is bool)
                            Display3D.CModelManager.modelsList[eltId]._Explodable = (bool)values[1];
                    }
                    else if (info.Length > 4 && info.Substring(0, 4) == "bot_")
                    {
                        int intVal;
                        float floatVal;
                        if (info == "bot_name")
                            Game.CEnemyManager._enemyList[eltId].SetEnemyName(val.ToString());
                        else if (info == "bot_life")
                        {
                            if (Int32.TryParse(values[1].ToString(), out intVal))
                                Game.CEnemyManager._enemyList[eltId]._life = intVal;
                        }
                        else if (info == "bot_speed")
                        {
                            if (float.TryParse(values[1].ToString(), out floatVal))
                                Game.CEnemyManager._enemyList[eltId]._runningVelocity = floatVal;
                        }
                        else if (info == "bot_rangeofattack")
                        {
                            if (float.TryParse(values[1].ToString(), out floatVal))
                                Game.CEnemyManager._enemyList[eltId]._rangeAttack = floatVal;
                        }
                        else if (info == "bot_type")
                        {
                            if (Int32.TryParse(values[1].ToString(), out intVal))
                                Game.CEnemyManager._enemyList[eltId]._type = intVal;
                        }
                        else if (info == "bot_isaggressive")
                            Game.CEnemyManager._enemyList[eltId]._isAgressive = (bool)values[1];
                    }
                }
                else if (action == "getLevelData")
                {
                    SaveXMLFile();
                    return levelData;
                }
            }
            return false;
        }

        public void SaveXMLFile()
        {
            Display3D.CModelManager.UpdateGameLevel(ref levelData);
            Display3D.TreeManager.UpdateGameLevel(ref levelData);
            Display3D.CPickUpManager.UpdateGameLevel(ref levelData);
            Display3D.CWaterManager.UpdateGameLevel(ref levelData);
            Display3D.CLightsManager.UpdateGameLevel(ref levelData);
            Game.CEnemyManager.UpdateGameLevel(ref levelData);
        }

        public Texture2D LoadCorrectlyTexture(string file)
        {
            if (file.Contains(".") && !file.Contains(".xnb"))
                return Texture2D.FromStream(_graphics, new System.IO.FileStream(file, System.IO.FileMode.Open));
            else
                return _content.Load<Texture2D>(file.Replace(".xnb", ""));
        }
    }
}
