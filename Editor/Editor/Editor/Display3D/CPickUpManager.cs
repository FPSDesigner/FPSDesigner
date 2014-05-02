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

namespace Engine.Display3D
{
    class CPickUpManager
    {
        public static List<CPickUp> _pickups = new List<CPickUp>();
        public static int selectedPickupId = -1;

        public static void AddPickup(GraphicsDevice graphics, Model model, Texture2D texture, Vector3 position, Vector3 rotation, Vector3 scale, string weaponName, int weaponBullets)
        {
            _pickups.Add(new CPickUp(graphics, model, texture, position, rotation, scale, weaponName, weaponBullets));
        }

        public static void Draw(CCamera cam, GameTime gameTime)
        {
            for (int i = 0; i < _pickups.Count; i++)
            {
                _pickups[i].Draw(cam, gameTime);
                if (selectedPickupId == i)
                    CSimpleShapes.AddBoundingSphere(_pickups[i]._Model.BoundingSphere, Color.Black);
            }
        }

        public static void Update(GameTime gameTime)
        {
            foreach(CPickUp pickup in _pickups)
            {
                pickup._Model._modelRotation = new Vector3(pickup._Model._modelRotation.X, pickup._Model._modelRotation.Y, pickup._Model._modelRotation.Z);
            }
        }

        public static bool CheckEnteredPickUp(Vector3 pos, out CPickUp EnteredPickup)
        {
            foreach (CPickUp pickup in _pickups)
            {
                if (pickup.PointTouchPickUp(pos))
                {
                    EnteredPickup = pickup;
                    return true;
                }
            }
            EnteredPickup = null;
            return false;
        }

        public static float? CheckRayIntersectsAnyPickup(Ray ray, out int idIntersected)
        {
            for (int i = 0; i < _pickups.Count; i++)
            {
                float? distance = _pickups[i].RayIntersectsPickup(ray);
                if (distance.HasValue)
                {
                    idIntersected = i;
                    return distance;
                }
            }
            idIntersected = 0;
            return null;
        }
    }

    class CPickUp
    {
        public CModel _Model;

        public string _weaponName;
        public int _weaponBullets;


        public CPickUp(GraphicsDevice graphics, Model model, Texture2D texture, Vector3 position, Vector3 rotation, Vector3 scale, string weaponName, int weaponBullets)
        {
            Dictionary<string, Texture2D> textureList = new Dictionary<string, Texture2D>();
            textureList.Add("ApplyAllMesh", texture);
            _Model = new CModel(model, position, rotation, scale, graphics, textureList, 0, 1);
            _Model.shouldNotUpdateTriangles = true;

            _weaponName = weaponName;
            _weaponBullets = weaponBullets;
        }

        public void Draw(CCamera cam, GameTime gameTime)
        {
            _Model.Draw(cam._view, cam._projection, cam._cameraPos);
        }

        public bool PointTouchPickUp(Vector3 position)
        {
            return _Model.BoundingSphere.Contains(position) == ContainmentType.Contains;
        }

        public float? RayIntersectsPickup(Ray ray)
        {
            return ray.Intersects(_Model.BoundingSphere);
        }
    }
}
