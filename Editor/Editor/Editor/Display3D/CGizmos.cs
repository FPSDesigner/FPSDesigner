using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Engine.Display3D
{
    class CGizmos
    {
        /* Axis IDs:
         * 0: X (Red)
         * 1: Y (Green)
         * 2: Z (Blue)
        */

        public CModel posGizmo, rotGizmo;

        public bool shouldDrawPos = true, shouldDrawRot = false;

        private BoundingBox[] boxesPos, spheresRot;
        private Vector3 gizmoSize;


        public CGizmos(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            Dictionary<string, Texture2D> guizmoTexture = new Dictionary<string, Texture2D>();
            guizmoTexture.Add("R", Content.Load<Texture2D>("Textures/Guizmo"));
            posGizmo = new Display3D.CModel(Content.Load<Model>("Models/Guizmo"), new Vector3(-125, 170, 85), Vector3.Zero, new Vector3(5, 5, 5), GraphicsDevice, guizmoTexture, 0, 1);
            
            GenerateBoundingBoxes();

            gizmoSize = new Vector3(0.15f);
        }

        public void Draw(CCamera cam, GameTime gameTime)
        {

            if (shouldDrawPos && cam.BoundingVolumeIsInView(posGizmo.BoundingSphere))
            {
                posGizmo._modelScale = Vector3.Distance(cam._cameraPos, posGizmo._modelPosition) * gizmoSize;
                posGizmo.Draw(cam._view, cam._projection, cam._cameraPos);
            }
        }

        public void GenerateBoundingBoxes()
        {
            boxesPos = new BoundingBox[3];
            spheresRot = new BoundingBox[3];

            foreach (ModelMesh mesh in posGizmo._model.Meshes)
            {
                int axisId = 0;
                if (mesh.Name == "G")
                    axisId = 1;
                else if (mesh.Name == "B")
                    axisId = 2;

                List<Vector3> listVectPosGyzmo = new List<Vector3>();
                foreach (Triangle tri in posGizmo._trianglesPositions)
                {
                    if (tri.TriName == mesh.Name)
                    {
                        listVectPosGyzmo.Add(tri.V0);
                        listVectPosGyzmo.Add(tri.V1);
                        listVectPosGyzmo.Add(tri.V2);
                    }
                }

                boxesPos[axisId] = BoundingBox.CreateFromPoints(listVectPosGyzmo);
            }
        }

        public int? RayIntersectsAxis(Ray ray)
        {
            for (int i = 0; i < 3; i++)
            {
                if (ray.Intersects(boxesPos[i]) != null)
                    return i;
            }
            return null;
        }
    }
}
