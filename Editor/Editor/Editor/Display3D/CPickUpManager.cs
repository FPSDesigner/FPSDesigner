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

        public static void AddPickup(GraphicsDevice graphics, Model model, Texture2D texture, Vector3 position, Vector3 scale, string weaponName, int weaponBullets)
        {
            _pickups.Add(new CPickUp(graphics, model, texture, position, scale, weaponName, weaponBullets));
        }

        public static void Draw(CCamera cam, GameTime gameTime)
        {
            foreach(CPickUp pickup in _pickups)
            {
                pickup.Draw(cam, gameTime);
            }
        }

        public static void Update(GameTime gameTime)
        {
            foreach(CPickUp pickup in _pickups)
            {
                pickup._Rotation.Y += 0.01f;
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
    }

    class CPickUp
    {
        public CModel _Model;

        public Vector3 _Position;
        public Vector3 _Scale;
        public Vector3 _Rotation;

        public string _weaponName;
        public int _weaponBullets;


        public CPickUp(GraphicsDevice graphics, Model model, Texture2D texture, Vector3 position, Vector3 scale, string weaponName, int weaponBullets)
        {
            _Rotation = Vector3.Zero;
            _Position = position;
            _Scale = scale;

            Dictionary<string, Texture2D> textureList = new Dictionary<string, Texture2D>();
            textureList.Add("ApplyAllMesh", texture);
            _Model = new CModel(model, _Position, _Rotation, _Scale, graphics, textureList, 0, 1);

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
