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

        public static void AddPickup(CModel model, Vector3 position, Vector3 scale, string weaponName, int weaponBullets)
        {
            _pickups.Add(new CPickUp(model, position, scale, weaponName, weaponBullets));
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

        public static void LoadFromXML(CModel model, List<Game.LevelInfo.MapModels_Pickups> levelInfo)
        {
            foreach(Game.LevelInfo.MapModels_Pickups pickup in levelInfo)
                AddPickup(model, pickup.Position.Vector3, pickup.Scale.Vector3, pickup.WeaponName, pickup.WeaponBullets);
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

        private BoundingSphere _boundingSphere;
        public BoundingSphere BoundingSphere
        {
            get
            {
                Matrix worldTransform = Matrix.CreateScale(_Scale) *
                    Matrix.CreateFromYawPitchRoll(_Rotation.Y, _Rotation.X, _Rotation.Z) *
                    Matrix.CreateTranslation(_Position);

                BoundingSphere transformed = _boundingSphere;
                transformed = transformed.Transform(worldTransform);

                return transformed;
            }
        }

        public CPickUp(CModel model, Vector3 position, Vector3 scale, string weaponName, int weaponBullets)
        {
            _Model = model;
            _Position = position;
            _Scale = scale;

            _weaponName = weaponName;
            _weaponBullets = weaponBullets;
        }

        public void Draw(CCamera cam, GameTime gameTime)
        {
            _Model.Draw(cam._view, cam._projection, cam._cameraPos);
        }

        public bool PointTouchWeapon(Vector3 position)
        {
            return _Model.BoundingSphere.Contains(position) == ContainmentType.Contains;
        }

        public float? RayIntersectsPickup(Ray ray)
        {
            return ray.Intersects(_Model.BoundingSphere);
        }
    }
}
