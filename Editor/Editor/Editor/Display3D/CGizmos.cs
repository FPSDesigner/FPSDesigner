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

        public bool shouldDrawPos = false, shouldDrawRot = false;

        private BoundingBox[] boxesPos, spheresRot;
        private Vector3 gizmoSize;
        private Vector2 initialMousePos;
        private Vector3 initialPosValue, initialRotValue, initialScaleValue;

        private int axisDragging;
        private string eltTypeDragging;
        private int eltIdDragging;
        private float rotateIntensity = 25; // The more the value is, the more sensitive it is



        public CGizmos(ContentManager Content, GraphicsDevice GraphicsDevice, CCamera cam)
        {
            Dictionary<string, Texture2D> guizmoTexture = new Dictionary<string, Texture2D>();
            guizmoTexture.Add("R", Content.Load<Texture2D>("Textures/Guizmo"));
            posGizmo = new Display3D.CModel(Content.Load<Model>("Models/Guizmo"), new Vector3(-125, 170, 85), Vector3.Zero, Vector3.One, GraphicsDevice, guizmoTexture, 0, 1);
            posGizmo.AddTrianglesToPhysics(cam, false);

            guizmoTexture.Clear();
            guizmoTexture.Add("R", Content.Load<Texture2D>("Textures/RotationGuizmo"));
            rotGizmo = new Display3D.CModel(Content.Load<Model>("Models/RotationGuizmo"), new Vector3(-125, 170, 85), Vector3.Zero, Vector3.One, GraphicsDevice, guizmoTexture, 0, 1);
            rotGizmo.AddTrianglesToPhysics(cam, false);

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
            else if (shouldDrawRot && cam.BoundingVolumeIsInView(rotGizmo.BoundingSphere))
            {
                rotGizmo._modelScale = Vector3.Distance(cam._cameraPos, rotGizmo._modelPosition) * gizmoSize;
                rotGizmo.Draw(cam._view, cam._projection, cam._cameraPos);
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

        public int? RayIntersectsAxis(Ray ray, string gizmo)
        {
            if (gizmo == "pos")
            {
                GenerateBoundingBoxes();
                for (int i = 0; i < 3; i++)
                {
                    if (ray.Intersects(boxesPos[i]) != null)
                        return i;
                }
            }
            else if (gizmo == "rot")
            {
                foreach (Triangle tri in rotGizmo.GetRealTriangles())
                {
                    Triangle testedTriangle = tri;
                    if (TriangleTest.Intersects(ref ray, ref testedTriangle) != null)
                    {
                        if (tri.TriName == "R")
                            return 0;
                        else if (tri.TriName == "G")
                            return 1;
                        else
                            return 2;
                    }
                }
            }
            return null;
        }

        public void StartDrag(int axis, string eltType, int eltId, System.Windows.Point mousePos)
        {
            axisDragging = axis;
            eltTypeDragging = eltType;
            eltIdDragging = eltId;
            posGizmo.shouldNotUpdateTriangles = true;
            rotGizmo.shouldNotUpdateTriangles = true;

            initialMousePos = new Vector2((float)mousePos.X, (float)mousePos.Y);

            if (eltType == "model")
            {
                initialPosValue = CModelManager.modelsList[eltIdDragging]._modelPosition;
                initialRotValue = CModelManager.modelsList[eltIdDragging]._modelRotation;
                initialScaleValue = CModelManager.modelsList[eltIdDragging]._modelScale;
            } else if (eltType == "tree")
            {
                initialPosValue = TreeManager._tTrees[eltIdDragging].Position;
                initialRotValue = TreeManager._tTrees[eltIdDragging].Rotation;
                initialScaleValue = TreeManager._tTrees[eltIdDragging].Scale;
            }
            else if (eltType == "pickup")
            {
                initialPosValue = CPickUpManager._pickups[eltIdDragging]._Model._modelPosition;
                initialRotValue = CPickUpManager._pickups[eltIdDragging]._Model._modelRotation;
                initialScaleValue = CPickUpManager._pickups[eltIdDragging]._Model._modelScale;
            }
        }

        public void Drag(int posX, int posY, CCamera cam)
        {
            Vector3 gizmoPosition = posGizmo._modelPosition;
            if (shouldDrawRot)
                gizmoPosition = rotGizmo._modelPosition;

            Ray ray;
            float? distance = null;
            Plane axisPlane;
            Vector3 contactPoint = Vector3.Zero;
            if (shouldDrawPos)
            {
                // We get the normal of the mouse
                Vector3 nearSourceCursor = Display2D.C2DEffect.softwareViewport.Unproject(new Vector3(posX, posY, Display2D.C2DEffect.softwareViewport.MinDepth), cam._projection, cam._view, Matrix.Identity);
                Vector3 farSourceCursor = Display2D.C2DEffect.softwareViewport.Unproject(new Vector3(posX, posY, Display2D.C2DEffect.softwareViewport.MaxDepth), cam._projection, cam._view, Matrix.Identity);
                Vector3 directionCursor = farSourceCursor - nearSourceCursor;
                directionCursor.Normalize();

                // First, we create a plane for the axis we're dragging

                if (axisDragging == 0 || axisDragging == 1) // X/Z
                    axisPlane = new Plane(gizmoPosition + Vector3.UnitX, gizmoPosition, gizmoPosition + Vector3.UnitZ);
                else
                {
                    Matrix rotation = Matrix.CreateFromYawPitchRoll(cam._yaw, 0, 0);
                    axisPlane = new Plane(gizmoPosition, gizmoPosition + Vector3.Up, gizmoPosition + Vector3.Transform(Vector3.Left, rotation));
                }
                ray = new Ray(nearSourceCursor, directionCursor);
                distance = ray.Intersects(axisPlane);
                if (distance.HasValue)
                    contactPoint = ray.Position + ray.Direction * distance.Value;
            }

            if (distance.HasValue || shouldDrawRot)
            {
                if (eltTypeDragging == "model")
                {
                    if (shouldDrawPos)
                    {
                        if (axisDragging == 1)
                            CModelManager.modelsList[eltIdDragging]._modelPosition = new Vector3(contactPoint.X, CModelManager.modelsList[eltIdDragging]._modelPosition.Y, CModelManager.modelsList[eltIdDragging]._modelPosition.Z);
                        else if (axisDragging == 2)
                            CModelManager.modelsList[eltIdDragging]._modelPosition = new Vector3(CModelManager.modelsList[eltIdDragging]._modelPosition.X, contactPoint.Y, CModelManager.modelsList[eltIdDragging]._modelPosition.Z);
                        else if (axisDragging == 0)
                            CModelManager.modelsList[eltIdDragging]._modelPosition = new Vector3(CModelManager.modelsList[eltIdDragging]._modelPosition.X, CModelManager.modelsList[eltIdDragging]._modelPosition.Y, contactPoint.Z);
                        posGizmo._modelPosition = CModelManager.modelsList[eltIdDragging]._modelPosition;
                        rotGizmo._modelPosition = CModelManager.modelsList[eltIdDragging]._modelPosition;
                    }
                    else if (shouldDrawRot)
                    {
                        Vector2 diff = new Vector2(posX, posY) - initialMousePos;
                        if (axisDragging == 1)
                            CModelManager.modelsList[eltIdDragging]._modelRotation = new Vector3(initialRotValue.X + diff.X / 20, CModelManager.modelsList[eltIdDragging]._modelRotation.Y, CModelManager.modelsList[eltIdDragging]._modelRotation.Z);
                        else if (axisDragging == 0)
                            CModelManager.modelsList[eltIdDragging]._modelRotation = new Vector3(CModelManager.modelsList[eltIdDragging]._modelRotation.X, initialRotValue.Y + diff.X / rotateIntensity, CModelManager.modelsList[eltIdDragging]._modelRotation.Z);
                        else if (axisDragging == 2)
                            CModelManager.modelsList[eltIdDragging]._modelRotation = new Vector3(CModelManager.modelsList[eltIdDragging]._modelRotation.X, CModelManager.modelsList[eltIdDragging]._modelRotation.Y, initialRotValue.Z + diff.Y / rotateIntensity);
                        //rotGizmo._modelRotation = CModelManager.modelsList[eltIdDragging]._modelRotation;
                    }
                }
                else if (eltTypeDragging == "tree")
                {
                    if (shouldDrawPos)
                    {
                        if (axisDragging == 1)
                            TreeManager._tTrees[eltIdDragging].Position = new Vector3(contactPoint.X, TreeManager._tTrees[eltIdDragging].Position.Y, TreeManager._tTrees[eltIdDragging].Position.Z);
                        else if (axisDragging == 2)
                            TreeManager._tTrees[eltIdDragging].Position = new Vector3(TreeManager._tTrees[eltIdDragging].Position.X, contactPoint.Y, TreeManager._tTrees[eltIdDragging].Position.Z);
                        else if (axisDragging == 0)
                            TreeManager._tTrees[eltIdDragging].Position = new Vector3(TreeManager._tTrees[eltIdDragging].Position.X, TreeManager._tTrees[eltIdDragging].Position.Y, contactPoint.Z);
                        posGizmo._modelPosition = TreeManager._tTrees[eltIdDragging].Position;
                        rotGizmo._modelPosition = TreeManager._tTrees[eltIdDragging].Position;
                    }
                    else if (shouldDrawRot)
                    {
                        Vector2 diff = new Vector2(posX, posY) - initialMousePos;
                        if (axisDragging == 1)
                            TreeManager._tTrees[eltIdDragging].Rotation = new Vector3(initialRotValue.X + diff.X / rotateIntensity, TreeManager._tTrees[eltIdDragging].Rotation.Y, TreeManager._tTrees[eltIdDragging].Rotation.Z);
                        else if (axisDragging == 0)
                            TreeManager._tTrees[eltIdDragging].Rotation = new Vector3(TreeManager._tTrees[eltIdDragging].Rotation.X, initialRotValue.Y + diff.X / rotateIntensity, TreeManager._tTrees[eltIdDragging].Rotation.Z);
                        else if (axisDragging == 2)
                            TreeManager._tTrees[eltIdDragging].Rotation = new Vector3(TreeManager._tTrees[eltIdDragging].Rotation.X, TreeManager._tTrees[eltIdDragging].Rotation.Y, initialRotValue.Z + diff.Y / rotateIntensity);
                    }
                }
                else if (eltTypeDragging == "pickup")
                {
                    if (shouldDrawPos)
                    {
                        Vector3 oldPos = CPickUpManager._pickups[eltIdDragging]._Model._modelPosition;
                        if (axisDragging == 1)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelPosition = new Vector3(contactPoint.X, oldPos.Y, oldPos.Z);
                        else if (axisDragging == 2)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelPosition = new Vector3(oldPos.X, contactPoint.Y, oldPos.Z);
                        else if (axisDragging == 0)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelPosition = new Vector3(oldPos.X, oldPos.Y, contactPoint.Z);
                        posGizmo._modelPosition = CPickUpManager._pickups[eltIdDragging]._Model._modelPosition;
                        rotGizmo._modelPosition = CPickUpManager._pickups[eltIdDragging]._Model._modelPosition;
                    }
                    else if (shouldDrawRot)
                    {
                        Vector3 oldPos = CPickUpManager._pickups[eltIdDragging]._Model._modelRotation;
                        Vector2 diff = new Vector2(posX, posY) - initialMousePos;
                        if (axisDragging == 1)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelRotation = new Vector3(initialRotValue.X + diff.X / rotateIntensity, oldPos.Y, oldPos.Z);
                        else if (axisDragging == 0)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelRotation = new Vector3(oldPos.X, initialRotValue.Y + diff.X / rotateIntensity, oldPos.Z);
                        else if (axisDragging == 2)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelRotation = new Vector3(oldPos.X, oldPos.Y, initialRotValue.Z + diff.Y / rotateIntensity);
                    }
                }
            }
        }


        public void StopDrag()
        {
            posGizmo.shouldNotUpdateTriangles = false;
            posGizmo.generateModelTriangles();
        }
    }
}
