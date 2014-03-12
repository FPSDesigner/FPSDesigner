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

        public static void addModel(CModel model)
        {
            modelsList.Add(model);
        }

        public static void Draw(CCamera cam, GameTime gameTime)
        {
            foreach (CModel model in modelsList)
            {
                if (cam.BoundingVolumeIsInView(model.BoundingSphere))
                {
                    model.Draw(cam._view, cam._projection, cam._cameraPos);

                    if (DebugActivated)
                        foreach (Triangle tri in model._trianglesPositions)
                            CSimpleShapes.AddTriangle(tri.V0, tri.V1, tri.V2, Color.Black);
                }
            }
        }

        public static void AddPhysicsInformations(CCamera cam)
        {
            foreach (CModel model in modelsList)
            {
                cam._physicsMap._triangleList.AddRange(model._trianglesPositions);
                cam._physicsMap._triangleNormalsList.AddRange(model._trianglesNormal);
            }
        }

    }
}
