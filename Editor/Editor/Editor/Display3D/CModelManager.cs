using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Display3D
{
    class CModelManager
    {
        public static List<CModel> modelsList = new List<CModel>();

        public static bool DebugActivated = false;

        public static int selectModelId = -1;

        public static void addModel(CModel model)
        {
            modelsList.Add(model);
        }

        public static void Draw(CCamera cam, GameTime gameTime)
        {
            /*modelsList[0]._modelPosition = new Vector3(modelsList[0]._modelPosition.X, modelsList[0]._modelPosition.Y + 0.01f, modelsList[0]._modelPosition.Z);
            modelsList[0]._modelRotation = new Vector3(modelsList[0]._modelRotation.X, modelsList[0]._modelRotation.Y + 0.01f, modelsList[0]._modelRotation.Z);*/
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

        public static void AddPhysicsInformations(CCamera cam)
        {
            foreach (CModel model in modelsList)
            {
                model.AddTrianglesToPhysics(cam);
            }
        }

        public static float? CheckRayIntersectsModel(Ray ray, out int modelId)
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
            return closest.Value;
        }

    }
}
