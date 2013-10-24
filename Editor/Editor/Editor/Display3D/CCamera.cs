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
    class CCamera
    {
        public Matrix _view { get; private set; }
        public Matrix _projection{ get; private set; }

        public Vector3 _cameraPos { get; set; }
        public Vector3 _cameraTarget { get; private set; }
        
        private float _nearClip;
        private float _farClip;

        private GraphicsDevice _graphics;

        private float _aspectRatio;

        public CCamera(GraphicsDevice device, Vector3 cameraPos, Vector3 target, float nearClip, float farClip)
        {
            this._graphics = device;
            _aspectRatio = device.PresentationParameters.BackBufferWidth / device.PresentationParameters.BackBufferHeight;

            this._nearClip = nearClip;
            this._farClip = farClip;

            this._cameraPos = cameraPos;
            this._cameraTarget = target;

            _view = Matrix.CreateLookAt(cameraPos, target, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), _aspectRatio, _nearClip, _farClip);
        }

        public void Update(GameTime gametime, KeyboardState keyState)
        {
            if (keyState.IsKeyDown(Keys.Z)) { _cameraPos += Vector3.Forward * 4.0f * gametime.ElapsedGameTime.Milliseconds; }
            if (keyState.IsKeyDown(Keys.S)) { _cameraPos += Vector3.Backward * 4.0f * gametime.ElapsedGameTime.Milliseconds; }
            if (keyState.IsKeyDown(Keys.Q)) { _cameraPos += Vector3.Left * 4.0f * gametime.ElapsedGameTime.Milliseconds; }
            if (keyState.IsKeyDown(Keys.D)) { _cameraPos += Vector3.Right * 4.0f * gametime.ElapsedGameTime.Milliseconds; }

            _view = Matrix.CreateLookAt(_cameraPos, _cameraTarget, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), _aspectRatio, _nearClip, _farClip);
        }

    }
}
