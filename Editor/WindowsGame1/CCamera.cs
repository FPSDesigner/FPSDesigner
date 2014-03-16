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

namespace ModelViewer
{

    /// <summary>
    /// The camera class process all the camera movement and the world view & projection matrix.
    /// </summary>
    class CCamera
    {
        public Matrix _view { get; private set; }
        public Matrix _projection { get; private set; }

        public Vector3 _cameraPos { get; set; }
        public Vector3 _cameraTarget { get; set; }

        // Rotations angles
        public float _yaw { get; private set; }
        public float _pitch { get; private set; }

        private float _nearClip;
        private float _farClip;
        private float _aspectRatio;
        private float fieldOfView;

        private KeyboardState _oldKeyState;

        private GraphicsDevice _graphics;

        /// <summary>
        /// Initialize the class
        /// </summary>
        /// <param name="device">GraphicsDevice class</param>
        /// <param name="cameraPos">Default position of the camera</param>
        /// <param name="target">Default target of the camera</param>
        /// <param name="nearClip">Closest elements to be rendered</param>
        /// <param name="farClip">Farthest elements to be rentered</param>
        public CCamera(GraphicsDevice device, Vector3 cameraPos, Vector3 target, float nearClip, float farClip)
        {
            this._graphics = device;
            _aspectRatio = _graphics.Viewport.AspectRatio; // 16::9 - 4::3 etc

            this._nearClip = nearClip;
            this._farClip = farClip;

            this._cameraPos = cameraPos;
            this._cameraTarget = target;

            this._pitch = 0f;
            this._yaw = 0f;

            _view = Matrix.CreateLookAt(cameraPos, target, Vector3.Up);

            fieldOfView = MathHelper.ToRadians(40);
            _projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, _aspectRatio, _nearClip, _farClip);
        }

        /// <summary>
        /// Used to update the camera frame-per-frame
        /// </summary>
        /// <param name="gametime">GameTime snapshot</param>
        /// <param name="keyState">Current KeyboardState</param>
        /// <param name="mouseState">Current mouseState</param>
        public void Update(GameTime gametime, float camVelocity = 0.3f, bool isUnderWater = false, float waterLevel = 0f, KeyboardState keyState = default(KeyboardState), MouseState mouseState = default(MouseState),
            KeyboardState oldKeyState = default(KeyboardState))
        {
            if (_graphics.Viewport.AspectRatio != _aspectRatio)
            {
                // Window size have changed
                _aspectRatio = _graphics.Viewport.AspectRatio;
                _projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, _aspectRatio, _nearClip, _farClip);
            }

            _view = Matrix.CreateLookAt(_cameraPos, _cameraTarget, Vector3.Up);
        }
    }
}
