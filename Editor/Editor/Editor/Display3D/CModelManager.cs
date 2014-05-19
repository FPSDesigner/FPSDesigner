using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Engine.Display3D
{
    class CModelManager
    {
        public static List<CModel> modelsList = new List<CModel>();

        public static Effect normalMappingEffect;
        public static Effect shadowEffect;
        public static Effect lightEffect;
        public static Materials.PrelightingRenderer renderer;

        public static SoundEffect sound;

        public static bool DebugActivated = false;

        public static int selectModelId = -1;

        public static void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            renderer = new Materials.PrelightingRenderer(graphics, content);
            renderer.Models = new List<CModel>();

            normalMappingEffect = content.Load<Effect>("Effects\\NormalMapping");
            lightEffect = content.Load<Effect>("Effects/PPModel");

            sound = content.Load<SoundEffect>("Sounds\\Explosion");
            Game.CSoundManager.AddSound("Explosion", sound, false, 10.0f, new AudioListener(), new AudioEmitter());
        }

        public static void ApplyRendererShadow(ContentManager content, Vector3 pos, Vector3 target)
        {
            shadowEffect = content.Load<Effect>("Effects\\ShadowVSM");
            foreach (CModel model in modelsList)
            {
                model.SetModelEffect(shadowEffect, true);
            }

            renderer.ShadowLightPosition = pos;
            renderer.ShadowLightTarget = target;
            renderer.DoShadowMapping = true;
            renderer.ShadowMult = 0.3f;
        }

        public static void ApplyLightEffect(ContentManager content)
        {
            foreach (CModel model in modelsList)
            {
                model.SetModelEffect(CModelManager.lightEffect, true);
            }
        }

        public static void addModel(ContentManager content, CModel model)
        {
            modelsList.Add(model);
            renderer.Models.Add(model);
            model.LoadContent(content);
        }

        public static void Draw(CCamera cam, GameTime gameTime)
        {
            //renderer.ShadowLightPosition = CLightsManager.lights[0].Position;
            //renderer.ShadowLightTarget = new Vector3(renderer.ShadowLightTarget.X, renderer.ShadowLightTarget.Y - 0.1f, renderer.ShadowLightTarget.Z);
            foreach (CModel model in modelsList)
            {
                if (cam.BoundingVolumeIsInView(model.BoundingSphere))
                {
                    model.Draw(cam._view, cam._projection, cam._cameraPos);

                    if (selectModelId != -1 && modelsList[selectModelId] == model)
                        CSimpleShapes.AddBoundingBox(modelsList[selectModelId]._boundingBox, Color.Black);

                    if (DebugActivated)
                    {
                        Matrix worldMatrix = model.GetModelMatrix();
                        foreach (Triangle tri in model._trianglesPositions)
                        {
                            Triangle realTri = tri.NewByMatrix(worldMatrix);
                            CSimpleShapes.AddTriangle(realTri.V0, realTri.V1, realTri.V2, Color.Black);
                        }
                    }
                }
            }

            /*for (int i = 0; i < cam._physicsMap._triangleList.Count; i++) // Debug triangles collision
                CSimpleShapes.AddTriangle(cam._physicsMap._triangleList[i].V0, cam._physicsMap._triangleList[i].V1, cam._physicsMap._triangleList[i].V2, Color.Blue);
            for (int i = 0; i < cam._physicsMap._triangleNormalsList.Count; i++) // Debug triangles collision
                CSimpleShapes.AddLine(cam._physicsMap._triangleNormalsList[i], cam._physicsMap._triangleNormalsList[i] * 2, Color.Red);*/
            
            /*Matrix modelMatrix = modelsList[0].GetModelMatrix();
            for (int i = 0; i < modelsList[0]._trianglesPositions.Count; i++)
                CSimpleShapes.AddTriangle(Vector3.Transform(modelsList[0]._trianglesPositions[i].V0, modelMatrix),
                    Vector3.Transform(modelsList[0]._trianglesPositions[i].V1, modelMatrix),
                    Vector3.Transform(modelsList[0]._trianglesPositions[i].V2, modelMatrix), Color.Red);*/
        }

        public static void UpdateGameLevel(ref Game.LevelInfo.LevelData lvl)
        {
            for(int i = 0; i < modelsList.Count; i++)
            {
                CModel mdl = modelsList[i];

                lvl.MapModels.Models[i].Position = new Game.LevelInfo.Coordinates(mdl._modelPosition);
                lvl.MapModels.Models[i].Rotation = new Game.LevelInfo.Coordinates(mdl._modelRotation);
                lvl.MapModels.Models[i].Scale = new Game.LevelInfo.Coordinates(mdl._modelScale);
                lvl.MapModels.Models[i].Alpha = mdl.Alpha;
                lvl.MapModels.Models[i].Explodable = mdl._Explodable;
            }
            while(lvl.MapModels.Models.Count != modelsList.Count)
                lvl.MapModels.Models.RemoveAt(lvl.MapModels.Models.Count - 1);
        }

        public static void AddPhysicsInformations(CCamera cam)
        {
            foreach (CModel model in modelsList)
            {
                model.AddTrianglesToPhysics(cam);
            }
        }

        public static float? CheckRayIntersectsModel(Ray ray, out int modelId, float damage = -1)
        {
            Dictionary<int, float> modelsClicked = new Dictionary<int, float>();
            for(int i = 0; i < modelsList.Count; i++)
            {
                Matrix modelWorld = modelsList[i].GetModelMatrix();
                foreach (Triangle tri in modelsList[i]._trianglesPositions)
                {
                    Triangle triangle = tri.NewByMatrix(modelWorld);
                    float? distance = TriangleTest.Intersects(ref ray, ref triangle);
                    if (distance != null)
                    {
                        modelsClicked.Add(i, (float)distance);
                        break;
                    }
                }
            }

            if (modelsClicked.Count == 0)
            {
                modelId = -1;
                return null;
            }

            var closest = (from pair in modelsClicked
                        orderby pair.Value ascending
                        select pair).First();

            modelId = closest.Key;

            if (modelsList[modelId]._Explodable && damage > -1)
                modelsList[modelId].SetDamageToModel(damage);

            return closest.Value;
        }

        public static void ChangeModelsLightingEffect(LightingMode mode, int modelId = -1)
        {
            if (modelId == -1)
            {
                foreach (CModel mdl in modelsList)
                    mdl.lightMode = mode;
            }
            else
                modelsList[modelId].lightMode = mode;
        }

    }
}
