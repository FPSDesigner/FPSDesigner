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
         * 1: Z (Green)
         * 2: Y (Blue)
        */

        public CModel posGizmo, rotGizmo;

        public bool shouldDrawPos = true, shouldDrawRot = false;

        private BoundingBox[] boxesPos, spheresRot;
        private Vector3 gizmoSize;

        private bool isDragging = false;
        private int axisDragging;
        private string eltTypeDragging;
        private int eltIdDragging;



        public CGizmos(ContentManager Content, GraphicsDevice GraphicsDevice, CCamera cam)
        {
            Dictionary<string, Texture2D> guizmoTexture = new Dictionary<string, Texture2D>();
            guizmoTexture.Add("R", Content.Load<Texture2D>("Textures/Guizmo"));
            posGizmo = new Display3D.CModel(Content.Load<Model>("Models/Guizmo"), new Vector3(-125, 170, 85), Vector3.Zero, Vector3.One, GraphicsDevice, guizmoTexture, 0, 1);
            posGizmo.AddTrianglesToPhysics(cam, false);
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

            //posGizmo.AddTrianglesToPhysics();

            foreach (ModelMesh mesh in posGizmo._model.Meshes)
            {
                int axisId = 0;
                if (mesh.Name == "G")
                    axisId = 1;
                else if (mesh.Name == "B")
                    axisId = 2;

                List<Vector3> listVectPosGyzmo = new List<Vector3>();
                foreach (Triangle tri in posGizmo.GetRealTriangles())
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
            GenerateBoundingBoxes();
            for (int i = 0; i < 3; i++)
            {
                if (ray.Intersects(boxesPos[i]) != null)
                    return i;
            }
            return null;
        }

        public void StartDrag(int axis, string eltType, int eltId)
        {
            isDragging = true;
            axisDragging = axis;
            eltTypeDragging = eltType;
            eltIdDragging = eltId;
            posGizmo.shouldNotUpdateTriangles = true;
        }

        public void Drag(int posX, int posY, CCamera cam)
        {
            // We get the normal of the mouse
            Vector3 nearSourceCursor = Display2D.C2DEffect.softwareViewport.Unproject(new Vector3(posX, posY, Display2D.C2DEffect.softwareViewport.MinDepth), cam._projection, cam._view, Matrix.Identity);
            Vector3 farSourceCursor = Display2D.C2DEffect.softwareViewport.Unproject(new Vector3(posX, posY, Display2D.C2DEffect.softwareViewport.MaxDepth), cam._projection, cam._view, Matrix.Identity);
            Vector3 directionCursor = farSourceCursor - nearSourceCursor;
            directionCursor.Normalize();

            // First, we create a plane for the axis we're dragging
            Plane axisPlane;
            if (axisDragging == 0 || axisDragging == 1) // X/Z
                axisPlane = new Plane(posGizmo._modelPosition + Vector3.UnitX, posGizmo._modelPosition, posGizmo._modelPosition + Vector3.UnitZ);
            else
            {
                Matrix rotation = Matrix.CreateFromYawPitchRoll(cam._yaw, 0, 0);
                axisPlane = new Plane(posGizmo._modelPosition, posGizmo._modelPosition + Vector3.Up, posGizmo._modelPosition + Vector3.Transform(Vector3.Left, rotation));
            }
            
            Ray ray = new Ray(nearSourceCursor, directionCursor);

            float? distance = ray.Intersects(axisPlane);
            if(distance.HasValue)
            {
                Vector3 contactPoint = ray.Position + ray.Direction * distance.Value;

                if (eltTypeDragging == "model")
                {
                    if (axisDragging == 1)
                        CModelManager.modelsList[eltIdDragging]._modelPosition = new Vector3(contactPoint.X, CModelManager.modelsList[eltIdDragging]._modelPosition.Y, CModelManager.modelsList[eltIdDragging]._modelPosition.Z);
                    else if (axisDragging == 2)
                        CModelManager.modelsList[eltIdDragging]._modelPosition = new Vector3(CModelManager.modelsList[eltIdDragging]._modelPosition.X, contactPoint.Y, CModelManager.modelsList[eltIdDragging]._modelPosition.Z);
                    else if (axisDragging == 0)
                        CModelManager.modelsList[eltIdDragging]._modelPosition = new Vector3(CModelManager.modelsList[eltIdDragging]._modelPosition.X, CModelManager.modelsList[eltIdDragging]._modelPosition.Y, contactPoint.Z);
                    posGizmo._modelPosition = CModelManager.modelsList[eltIdDragging]._modelPosition;
                }
                else if (eltTypeDragging == "tree")
                {
                    
                    if (axisDragging == 1)
                        TreeManager._tTrees[eltIdDragging].Position = new Vector3(contactPoint.X, TreeManager._tTrees[eltIdDragging].Position.Y, TreeManager._tTrees[eltIdDragging].Position.Z);
                    else if (axisDragging == 2)
                        TreeManager._tTrees[eltIdDragging].Position = new Vector3(TreeManager._tTrees[eltIdDragging].Position.X, contactPoint.Y, TreeManager._tTrees[eltIdDragging].Position.Z);
                    else if (axisDragging == 0)
                        TreeManager._tTrees[eltIdDragging].Position = new Vector3(TreeManager._tTrees[eltIdDragging].Position.X, TreeManager._tTrees[eltIdDragging].Position.Y, contactPoint.Z);
                    posGizmo._modelPosition = TreeManager._tTrees[eltIdDragging].Position;
                }
            }
        }

        
        public void StopDrag()
        {
            isDragging = false;
            posGizmo.shouldNotUpdateTriangles = false;
            posGizmo.generateModelTriangles();
        }
    }
}
