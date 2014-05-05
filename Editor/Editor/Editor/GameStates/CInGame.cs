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
        Display3D.CWater water;
        List<Display3D.Particles.ParticleSystem> particlesList = new List<Display3D.Particles.ParticleSystem>();

        Game.CWeapon weapon;
        private GraphicsDevice _graphics;
        private ContentManager _content;

        // ENEMY TEST
        private Game.CEnemy _enemy;

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

            /**** Character ****/
            _character = new Game.CCharacter();
            _character.Initialize();

            _character._walkSpeed = levelData.SpawnInfo.WalkSpeed;
            _character._aimSpeed = levelData.SpawnInfo.AimSpeed;
            _character._sprintSpeed = levelData.SpawnInfo.SprintSpeed;

            Game.CConsole._Character = _character;

            /**** Models ****/

            Display3D.CModelManager.LoadContent(content);

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

                Display3D.CModelManager.addModel(new Display3D.CModel(
                    content.Load<Model>(modelInfo.ModelFile),
                    modelInfo.Position.Vector3,
                    modelInfo.Rotation.Vector3,
                    modelInfo.Scale.Vector3,
                    graphics,
                    modelTextures,
                    modelInfo.SpecColor,
                    modelInfo.Alpha,
                    bumpTextures,
                    Display3D.CModelManager.normalMappingEffect));
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
            Dictionary<string, Model> modelsList = new Dictionary<string, Model>();
            Dictionary<string, Texture2D> textureList = new Dictionary<string, Texture2D>();
            List<object[]> objList = new List<object[]>();
            List<string[]> soundList = new List<string[]>();
            List<string[]> animList = new List<string[]>();
            List<float[]> animVelocityList = new List<float[]>();

            foreach (Game.LevelInfo.Weapon wep in levelData.Weapons.Weapon)
            {
                modelsList.Add(wep.Name, content.Load<Model>(wep.Model));
                textureList.Add(wep.Name, content.Load<Texture2D>(wep.Texture));
                objList.Add(
                    new object[] { wep.Type, wep.MaxClip, wep.MaxClip, 50, 1, wep.IsAutomatic, wep.ShotsPerSecs, wep.Range,
                        Matrix.CreateRotationX(wep.Rotation.X) * Matrix.CreateRotationY(wep.Rotation.Y) * Matrix.CreateRotationZ(wep.Rotation.Z),
                        wep.Offset.Vector3, wep.Scale, wep.Delay, wep.Name, wep.RecoilIntensity, wep.RecoilBackIntensity }
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

            /**** Pickups ****/
            foreach (Game.LevelInfo.MapModels_Pickups pickup in levelData.MapModels.Pickups)
                Display3D.CPickUpManager.AddPickup(graphics, modelsList[pickup.WeaponName], textureList[pickup.WeaponName], pickup.Position.Vector3, pickup.Rotation.Vector3, pickup.Scale.Vector3, pickup.WeaponName, pickup.WeaponBullets);


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

            /**** Gizmos ****/
            if (isSoftwareEmbedded)
                Gizmos = new Display3D.CGizmos(content, graphics, cam);

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

            // ENEMY TEST
            Texture2D[] ennemyTexture = new Texture2D[1];
            ennemyTexture[0] = content.Load<Texture2D>("Textures\\StormTrooper");
            _enemy = new Game.CEnemy("StormTrooperAnimation", ennemyTexture, new Vector3(-125f, 168.95f, 88)
                , Matrix.CreateRotationX(-1 * MathHelper.PiOver2));

            _enemy.LoadContent(content, cam);
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

            if (!isSoftwareEmbedded)
                Display3D.TreeManager.Update(gameTime);

            // Check if player entered a pickup
            Display3D.CPickUpManager.Update(gameTime);
            Display3D.CPickUp EnteredPickup;
            if (Display3D.CPickUpManager.CheckEnteredPickUp(cam._cameraPos, out EnteredPickup))
            {
                // Add weapon etc.
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

            // ENEMY TEST
            _enemy.MoveTo(cam._cameraPos);
            _enemy.Update(gameTime);

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

            // ENEMY TEST
            _enemy.Draw(gameTime, spriteBatch, cam._view, cam._projection);

            // Draw pickups
            Display3D.CPickUpManager.Draw(cam, gameTime);

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

            if (isSoftwareEmbedded)
            {
                _graphics.Clear(ClearOptions.DepthBuffer, new Vector4(0), 65535, 0);
                Gizmos.Draw(cam, gameTime);
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

                    // Click distances
                    float? treeDistance = null, modelDistance = null, pickupDistance = null;

                    // Tree check
                    int treeIdSelected;
                    treeDistance = Display3D.TreeManager.CheckRayCollision(ray, out treeIdSelected);

                    // Model check
                    int modelIdSelected;
                    modelDistance = Display3D.CModelManager.CheckRayIntersectsModel(ray, out modelIdSelected);

                    // Pickup check
                    int pickupIdSelected;
                    pickupDistance = Display3D.CPickUpManager.CheckRayIntersectsAnyPickup(ray, out pickupIdSelected);

                    // Check which is the closest one
                    if (treeDistance != null && (treeDistance < modelDistance || modelDistance == null) && (treeDistance < pickupDistance || pickupDistance == null))
                        return new object[] { "tree", treeIdSelected };
                    else if (modelDistance != null && (modelDistance < pickupDistance || pickupDistance == null))
                        return new object[] { "model", modelIdSelected };
                    else if (pickupDistance != null)
                        return new object[] { "pickup", pickupIdSelected };
                }
                else if (action == "unselectObject")
                {
                    object[] values = (object[])p[1];
                    if ((string)values[0] == "tree")
                    {
                        Display3D.TreeManager.selectedTreeId = -1;
                    }
                    else if ((string)values[0] == "model")
                    {
                        Display3D.CModelManager.selectModelId = -1;
                    }
                    else if ((string)values[0] == "pickup")
                    {
                        Display3D.CPickUpManager.selectedPickupId = -1;
                    }
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
                    }
                    else if (info == "rot")
                    {
                        if (eltType == "tree")
                            pos = Display3D.TreeManager._tTrees[eltId].Rotation;
                        else if (eltType == "model")
                            pos = Display3D.CModelManager.modelsList[eltId]._modelRotation;
                        else if (eltType == "pickup")
                            pos = Display3D.CPickUpManager._pickups[eltId]._Model._modelRotation;
                    }
                    else if (info == "scale")
                    {
                        if (eltType == "tree")
                            pos = Display3D.TreeManager._tTrees[eltId].Scale;
                        else if (eltType == "model")
                            pos = Display3D.CModelManager.modelsList[eltId]._modelScale;
                        else if (eltType == "pickup")
                            pos = Display3D.CPickUpManager._pickups[eltId]._Model._modelScale;
                    }
                    else if (info == "treeseed")
                        return Display3D.TreeManager._tTrees[eltId]._seed;
                    else if (info == "treeprofile")
                        return Display3D.TreeManager._tTrees[eltId]._profile;
                    else if (info == "pickupname")
                        return Display3D.CPickUpManager._pickups[eltId]._weaponName;
                    else if (info == "pickupbullet")
                        return Display3D.CPickUpManager._pickups[eltId]._weaponBullets;
                    return new object[] { pos.X, pos.Y, pos.Z };
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
                        levelData.MapModels.Models.Add(modelInfo);

                        Dictionary<string, Texture2D> modelTextures = new Dictionary<string, Texture2D>();

                        foreach (Game.LevelInfo.MapModels_Texture textureInfo in modelInfo.Textures.Texture)
                            modelTextures.Add(textureInfo.Mesh, _content.Load<Texture2D>(textureInfo.Texture));

                        Dictionary<string, Texture2D> bumpTextures = new Dictionary<string, Texture2D>();

                if (modelInfo.BumpTextures != null)
                {
                    foreach (Game.LevelInfo.MapModels_Texture textureInfo in modelInfo.BumpTextures.Texture)
                        bumpTextures.Add(textureInfo.Mesh, _content.Load<Texture2D>(textureInfo.Texture));
                }

                        Display3D.CModelManager.addModel(new Display3D.CModel(
                            _content.Load<Model>(modelInfo.ModelFile),
                            modelInfo.Position.Vector3,
                            modelInfo.Rotation.Vector3,
                            modelInfo.Scale.Vector3,
                            _graphics,
                            modelTextures,
                            modelInfo.SpecColor,
                            modelInfo.Alpha,
                            bumpTextures,
                            Display3D.CModelManager.normalMappingEffect));

                    }
                }
                else if (action == "setElementInfo")
                {
                    object[] values = (object[])p[1];
                    string info = (string)values[0];
                    object val = values[1];
                    string eltType = (string)values[2];
                    int eltId = (int)values[3];

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
                                    _graphics, textureList, 0, 1,null, Display3D.CModelManager.normalMappingEffect);
                                return true;
                            }
                        }
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
        }
    }
}
