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

namespace Editor.Display3D
{
    class Camera
    {
        public Matrix _view { get; private set; }
        public Matrix _projection{ get; private set; }

        public Vector3 _cameraPos { get; private set; }
        public Vector3 _cameraTarget { get; private set; }
        
        private float _nearClip;
        private float _farClip;

        private GraphicsDevice _graphics;

        private float _aspectRatio;

        public Camera(GraphicsDevice device, Vector3 cameraPos, Vector3 target, float nearClip, float farClip)
        {
            this._graphics = device;
            _aspectRatio = device.PresentationParameters.BackBufferWidth / device.PresentationParameters.BackBufferHeight;

            this._nearClip = nearClip;
            this._farClip = farClip;

            _view = Matrix.CreateLookAt(cameraPos, target, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), _aspectRatio, _nearClip, _farClip);
        }

        public void Update(GameTime gametime, Vector3 cameraPos, Vector3 target)
        {
            _view = Matrix.CreateLookAt(cameraPos, target, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), _aspectRatio, _nearClip, _farClip);
        }
    }
}
