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
         * 3: Uniform (Yellow)
        */

        public CModel posGizmo, rotGizmo, scaleGizmo;

        public bool shouldDrawPos = false, shouldDrawRot = false, shouldDrawScale = false;

        private BoundingBox[] boxesPos, spheresRot;
        private Vector3 gizmoSize;
        private Vector2 initialMousePos;
        private Vector3 initialPosValue, initialRotValue, initialScaleValue, initial3DPoint;

        private int axisDragging;
        private string eltTypeDragging;
        private int eltIdDragging;
        private float rotateIntensity = 50; // The more the value is, the more sensitive it is



        public CGizmos(ContentManager Content, GraphicsDevice GraphicsDevice, CCamera cam)
        {
            Dictionary<string, Texture2D> guizmoTexture = new Dictionary<string, Texture2D>();
            guizmoTexture.Add("R", Content.Load<Texture2D>("Textures/Guizmo"));
            posGizmo = new Display3D.CModel(Content.Load<Model>("Models/Guizmo"), Vector3.Zero, new Vector3(-MathHelper.PiOver2, MathHelper.PiOver2, 0), Vector3.One, GraphicsDevice, guizmoTexture, 0, 1);
            posGizmo.AddTrianglesToPhysics(cam, false);

            Dictionary<string, Texture2D> guizmoTexture2 = new Dictionary<string, Texture2D>();
            guizmoTexture2.Add("R", Content.Load<Texture2D>("Textures/RotationGuizmo"));
            rotGizmo = new Display3D.CModel(Content.Load<Model>("Models/RotationGuizmo"), Vector3.Zero, Vector3.Zero, Vector3.One, GraphicsDevice, guizmoTexture2, 0, 1);
            rotGizmo.AddTrianglesToPhysics(cam, false);

            Dictionary<string, Texture2D> guizmoTexture3 = new Dictionary<string, Texture2D>();
            guizmoTexture3.Add("R", Content.Load<Texture2D>("Textures/ScalingGuizmo"));
            scaleGizmo = new Display3D.CModel(Content.Load<Model>("Models/ScalingGuizmo"), Vector3.Zero, new Vector3(-MathHelper.PiOver2, MathHelper.PiOver2, 0), Vector3.One, GraphicsDevice, guizmoTexture3, 0, 1);
            scaleGizmo.AddTrianglesToPhysics(cam, false);

            GenerateBoundingBoxes();

            gizmoSize = new Vector3(0.12f);
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
            else if (shouldDrawScale && cam.BoundingVolumeIsInView(scaleGizmo.BoundingSphere))
            {
                scaleGizmo._modelScale = Vector3.Distance(cam._cameraPos, rotGizmo._modelPosition) * gizmoSize;
                scaleGizmo.Draw(cam._view, cam._projection, cam._cameraPos);
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
            else if (gizmo == "scale")
            {
                foreach (Triangle tri in scaleGizmo.GetRealTriangles())
                {
                    Triangle testedTriangle = tri;
                    if (TriangleTest.Intersects(ref ray, ref testedTriangle) != null)
                    {
                        if (tri.TriName == "R")
                            return 0;
                        else if (tri.TriName == "G")
                            return 1;
                        else if (tri.TriName == "B")
                            return 2;
                        else
                            return 3;
                    }
                }
            }
            return null;
        }

        public void StartDrag(int axis, string eltType, int eltId, System.Windows.Point mousePos, CCamera cam)
        {
            axisDragging = axis;
            eltTypeDragging = eltType;
            eltIdDragging = eltId;
            posGizmo.shouldNotUpdateTriangles = true;
            rotGizmo.shouldNotUpdateTriangles = true;
            scaleGizmo.shouldNotUpdateTriangles = true;

            initialMousePos = new Vector2((float)mousePos.X, (float)mousePos.Y);

            Vector3 nearSourceCursor = Display2D.C2DEffect.softwareViewport.Unproject(new Vector3(initialMousePos.X, initialMousePos.Y, Display2D.C2DEffect.softwareViewport.MinDepth), cam._projection, cam._view, Matrix.Identity);
            Vector3 farSourceCursor = Display2D.C2DEffect.softwareViewport.Unproject(new Vector3(initialMousePos.X, initialMousePos.Y, Display2D.C2DEffect.softwareViewport.MaxDepth), cam._projection, cam._view, Matrix.Identity);
            Vector3 directionCursor = farSourceCursor - nearSourceCursor;
            directionCursor.Normalize();

            Plane axisPlane;
            if (axisDragging == 0 || axisDragging == 1) // X/Z
                axisPlane = new Plane(posGizmo._modelPosition + Vector3.UnitX, posGizmo._modelPosition, posGizmo._modelPosition + Vector3.UnitZ);
            else
            {
                Matrix rotation = Matrix.CreateFromYawPitchRoll(cam._yaw, 0, 0);
                axisPlane = new Plane(posGizmo._modelPosition, posGizmo._modelPosition + Vector3.Up, posGizmo._modelPosition + Vector3.Transform(Vector3.Left, rotation));
            }
            Ray initialRay = new Ray(nearSourceCursor, directionCursor);

            float? distance = initialRay.Intersects(axisPlane);
            if (distance.HasValue)
                initial3DPoint = initialRay.Position + initialRay.Direction * distance.Value;

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
            else if (eltType == "pickup")
            {
                initialPosValue = CWaterManager.listWater[eltIdDragging].waterPosition;
                initialRotValue = Vector3.Zero;
                initialScaleValue = new Vector3(CWaterManager.listWater[eltIdDragging].waterSize.X, 1, CWaterManager.listWater[eltIdDragging].waterSize.Y);
            }
        }

        public void Drag(int posX, int posY, CCamera cam)
        {
            Vector3 gizmoPosition = posGizmo._modelPosition;
            if (shouldDrawRot)
                gizmoPosition = rotGizmo._modelPosition;
            else if (shouldDrawScale)
                gizmoPosition = scaleGizmo._modelPosition;

            Ray ray;
            float? distance = null;
            Plane axisPlane;
            Vector3 contactPoint = Vector3.Zero;
            if (shouldDrawPos || shouldDrawScale)
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
                        Vector3 diff = contactPoint - initial3DPoint;
                        if (axisDragging == 1)
                            CModelManager.modelsList[eltIdDragging]._modelPosition = new Vector3(initialPosValue.X + diff.X, CModelManager.modelsList[eltIdDragging]._modelPosition.Y, CModelManager.modelsList[eltIdDragging]._modelPosition.Z);
                        else if (axisDragging == 2)
                            CModelManager.modelsList[eltIdDragging]._modelPosition = new Vector3(CModelManager.modelsList[eltIdDragging]._modelPosition.X, initialPosValue.Y + diff.Y, CModelManager.modelsList[eltIdDragging]._modelPosition.Z);
                        else if (axisDragging == 0)
                            CModelManager.modelsList[eltIdDragging]._modelPosition = new Vector3(CModelManager.modelsList[eltIdDragging]._modelPosition.X, CModelManager.modelsList[eltIdDragging]._modelPosition.Y, initialPosValue.Z + diff.Z);
                        posGizmo._modelPosition = CModelManager.modelsList[eltIdDragging]._modelPosition;
                        rotGizmo._modelPosition = CModelManager.modelsList[eltIdDragging]._modelPosition;
                        scaleGizmo._modelPosition = CModelManager.modelsList[eltIdDragging]._modelPosition;
                    }
                    else if (shouldDrawRot)
                    {
                        Vector2 diff = new Vector2(posX, posY) - initialMousePos;
                        if (axisDragging == 2)
                            CModelManager.modelsList[eltIdDragging]._modelRotation = new Vector3(initialRotValue.X + diff.X / 20, CModelManager.modelsList[eltIdDragging]._modelRotation.Y, CModelManager.modelsList[eltIdDragging]._modelRotation.Z);
                        else if (axisDragging == 0)
                            CModelManager.modelsList[eltIdDragging]._modelRotation = new Vector3(CModelManager.modelsList[eltIdDragging]._modelRotation.X, initialRotValue.Y + diff.X / rotateIntensity, CModelManager.modelsList[eltIdDragging]._modelRotation.Z);
                        else if (axisDragging == 1)
                            CModelManager.modelsList[eltIdDragging]._modelRotation = new Vector3(CModelManager.modelsList[eltIdDragging]._modelRotation.X, CModelManager.modelsList[eltIdDragging]._modelRotation.Y, initialRotValue.Z + diff.Y / rotateIntensity);
                        scaleGizmo._modelRotation = CModelManager.modelsList[eltIdDragging]._modelRotation;
                    }
                    else if (shouldDrawScale)
                    {
                        Vector3 diff = contactPoint - initial3DPoint;
                        if (axisDragging == 1)
                            CModelManager.modelsList[eltIdDragging]._modelScale = new Vector3(initialScaleValue.X - diff.X, CModelManager.modelsList[eltIdDragging]._modelScale.Y, CModelManager.modelsList[eltIdDragging]._modelScale.Z);
                        else if (axisDragging == 2)
                            CModelManager.modelsList[eltIdDragging]._modelScale = new Vector3(CModelManager.modelsList[eltIdDragging]._modelScale.X, initialScaleValue.Y + diff.Y, CModelManager.modelsList[eltIdDragging]._modelScale.Z);
                        else if (axisDragging == 0)
                            CModelManager.modelsList[eltIdDragging]._modelScale = new Vector3(CModelManager.modelsList[eltIdDragging]._modelScale.X, CModelManager.modelsList[eltIdDragging]._modelScale.Y, initialScaleValue.Z - diff.Z);
                    }
                }
                else if (eltTypeDragging == "tree")
                {
                    if (shouldDrawPos)
                    {
                        Vector3 diff = contactPoint - initial3DPoint;
                        if (axisDragging == 1)
                            TreeManager._tTrees[eltIdDragging].Position = new Vector3(initialPosValue.X + diff.X, TreeManager._tTrees[eltIdDragging].Position.Y, TreeManager._tTrees[eltIdDragging].Position.Z);
                        else if (axisDragging == 2)
                            TreeManager._tTrees[eltIdDragging].Position = new Vector3(TreeManager._tTrees[eltIdDragging].Position.X, initialPosValue.Y + diff.Y, TreeManager._tTrees[eltIdDragging].Position.Z);
                        else if (axisDragging == 0)
                            TreeManager._tTrees[eltIdDragging].Position = new Vector3(TreeManager._tTrees[eltIdDragging].Position.X, TreeManager._tTrees[eltIdDragging].Position.Y, initialPosValue.Z + diff.Z);
                        posGizmo._modelPosition = TreeManager._tTrees[eltIdDragging].Position;
                        rotGizmo._modelPosition = TreeManager._tTrees[eltIdDragging].Position;
                        scaleGizmo._modelPosition = TreeManager._tTrees[eltIdDragging].Position;
                    }
                    else if (shouldDrawRot)
                    {
                        Vector2 diff = new Vector2(posX, posY) - initialMousePos;
                        if (axisDragging == 2)
                            TreeManager._tTrees[eltIdDragging].Rotation = new Vector3(initialRotValue.X + diff.X / rotateIntensity, TreeManager._tTrees[eltIdDragging].Rotation.Y, TreeManager._tTrees[eltIdDragging].Rotation.Z);
                        else if (axisDragging == 0)
                            TreeManager._tTrees[eltIdDragging].Rotation = new Vector3(TreeManager._tTrees[eltIdDragging].Rotation.X, initialRotValue.Y + diff.X / rotateIntensity, TreeManager._tTrees[eltIdDragging].Rotation.Z);
                        else if (axisDragging == 1)
                            TreeManager._tTrees[eltIdDragging].Rotation = new Vector3(TreeManager._tTrees[eltIdDragging].Rotation.X, TreeManager._tTrees[eltIdDragging].Rotation.Y, initialRotValue.Z + diff.Y / rotateIntensity);
                    }
                    else if (shouldDrawScale)
                    {
                        Vector3 diff = contactPoint - initial3DPoint;
                        if (axisDragging == 1)
                            TreeManager._tTrees[eltIdDragging].Scale = new Vector3(initialScaleValue.X - diff.X, TreeManager._tTrees[eltIdDragging].Scale.Y, TreeManager._tTrees[eltIdDragging].Scale.Z);
                        else if (axisDragging == 2)
                            TreeManager._tTrees[eltIdDragging].Scale = new Vector3(TreeManager._tTrees[eltIdDragging].Scale.X, initialScaleValue.Y + diff.Y, TreeManager._tTrees[eltIdDragging].Scale.Z);
                        else if (axisDragging == 0)
                            TreeManager._tTrees[eltIdDragging].Scale = new Vector3(TreeManager._tTrees[eltIdDragging].Scale.X, TreeManager._tTrees[eltIdDragging].Scale.Y, initialScaleValue.Z - diff.Z);
                    }
                }
                else if (eltTypeDragging == "pickup")
                {
                    if (shouldDrawPos)
                    {
                        Vector3 diff = contactPoint - initial3DPoint;
                        Vector3 oldPos = CPickUpManager._pickups[eltIdDragging]._Model._modelPosition;
                        if (axisDragging == 1)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelPosition = new Vector3(initialPosValue.X + diff.X, oldPos.Y, oldPos.Z);
                        else if (axisDragging == 2)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelPosition = new Vector3(oldPos.X, initialPosValue.Y + diff.Y, oldPos.Z);
                        else if (axisDragging == 0)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelPosition = new Vector3(oldPos.X, oldPos.Y, initialPosValue.Z + diff.Z);
                        posGizmo._modelPosition = CPickUpManager._pickups[eltIdDragging]._Model._modelPosition;
                        rotGizmo._modelPosition = CPickUpManager._pickups[eltIdDragging]._Model._modelPosition;
                        scaleGizmo._modelPosition = CPickUpManager._pickups[eltIdDragging]._Model._modelPosition;
                    }
                    else if (shouldDrawRot)
                    {
                        Vector3 oldPos = CPickUpManager._pickups[eltIdDragging]._Model._modelRotation;
                        Vector2 diff = new Vector2(posX, posY) - initialMousePos;
                        if (axisDragging == 2)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelRotation = new Vector3(initialRotValue.X + diff.X / rotateIntensity, oldPos.Y, oldPos.Z);
                        else if (axisDragging == 0)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelRotation = new Vector3(oldPos.X, initialRotValue.Y + diff.X / rotateIntensity, oldPos.Z);
                        else if (axisDragging == 1)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelRotation = new Vector3(oldPos.X, oldPos.Y, initialRotValue.Z + diff.Y / rotateIntensity);
                    }
                    else if (shouldDrawScale)
                    {
                        Vector3 diff = contactPoint - initial3DPoint;
                        Vector3 oldPos = CPickUpManager._pickups[eltIdDragging]._Model._modelScale;
                        if (axisDragging == 1)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelScale = new Vector3(initialScaleValue.X - diff.X, oldPos.Y, oldPos.Z);
                        else if (axisDragging == 2)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelScale = new Vector3(oldPos.X, initialScaleValue.Y + diff.Y, oldPos.Z);
                        else if (axisDragging == 0)
                            CPickUpManager._pickups[eltIdDragging]._Model._modelScale = new Vector3(oldPos.X, oldPos.Y, initialScaleValue.Z - diff.Z);
                    }
                }
                else if (eltTypeDragging == "water")
                {
                    if (shouldDrawPos)
                    {
                        Vector3 diff = contactPoint - initial3DPoint;
                        Vector3 oldPos = CWaterManager.listWater[eltIdDragging].waterPosition;
                        if (axisDragging == 1)
                            CWaterManager.listWater[eltIdDragging].waterPosition = new Vector3(initialPosValue.X + diff.X, oldPos.Y, oldPos.Z);
                        else if (axisDragging == 2)
                            CWaterManager.listWater[eltIdDragging].waterPosition = new Vector3(oldPos.X, initialPosValue.Y + diff.Y, oldPos.Z);
                        else if (axisDragging == 0)
                            CWaterManager.listWater[eltIdDragging].waterPosition = new Vector3(oldPos.X, oldPos.Y, initialPosValue.Z + diff.Z);
                        posGizmo._modelPosition = CWaterManager.listWater[eltIdDragging].waterPosition;
                        rotGizmo._modelPosition = CWaterManager.listWater[eltIdDragging].waterPosition;
                        scaleGizmo._modelPosition = CWaterManager.listWater[eltIdDragging].waterPosition;
                    }
                    else if (shouldDrawScale)
                    {
                        Vector3 diff = contactPoint - initial3DPoint;
                        Vector2 oldPos = CWaterManager.listWater[eltIdDragging].waterSize;
                        if (axisDragging == 1)
                            CWaterManager.listWater[eltIdDragging].waterSize = new Vector2(initialScaleValue.X - diff.X, oldPos.Y);
                        else if (axisDragging == 0)
                            CWaterManager.listWater[eltIdDragging].waterSize = new Vector2(oldPos.X, initialScaleValue.Z - diff.Y);
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
