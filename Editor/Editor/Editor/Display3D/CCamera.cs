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
    /// <summary>
    /// The camera class process all the camera movement and the world view & projection matrix.
    /// </summary>
    class CCamera
    {
        public Matrix _view { get; private set; }
        public Matrix _projection{ get; private set; }

        public Vector3 _cameraPos { get; set; }
        public Vector3 _cameraTarget { get; private set; }

        // This vector take the movement (Forward, etc...) & the rotatio, so, movement follow the view
        private Vector3 _translation;
        private Vector3 _up;

        private Game.Settings.CGameSettings _gameSettings;

        // Rotations angles
        public float _yaw { get; private set; }
        public float _pitch { get; private set; }
        
        private float _nearClip;
        private float _farClip;
        private float _cameraVelocity;
        private float _aspectRatio;

        private GraphicsDevice _graphics;


        /// <summary>
        /// Initialize the class
        /// </summary>
        /// <param name="device">GraphicsDevice class</param>
        /// <param name="cameraPos">Default position of the camera</param>
        /// <param name="target">Default target of the camera</param>
        /// <param name="nearClip">Closest elements to be rendered</param>
        /// <param name="farClip">Farthest elements to be rentered</param>
        /// <param name="camVelocity">Camera movement speed</param>
        public CCamera(GraphicsDevice device, Vector3 cameraPos, Vector3 target, float nearClip, float farClip, float camVelocity)
        {
            this._graphics = device;
            _aspectRatio = _graphics.Viewport.AspectRatio; // 16::9 - 4::3 etc

            this._nearClip = nearClip;
            this._farClip = farClip;

            this._cameraVelocity = camVelocity;

            this._cameraPos = cameraPos;
            this._cameraTarget = target;

            this._up = Vector3.Up;

            this._translation = Vector3.Zero;

            this._gameSettings = Game.Settings.CGameSettings.getInstance();

            _view = Matrix.CreateLookAt(cameraPos, target, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(40), _aspectRatio, _nearClip, _farClip);
        }

        /// <summary>
        /// Used to update the camera frame-per-frame
        /// </summary>
        /// <param name="gametime">GameTime snapshot</param>
        /// <param name="keyState">Current KeyboardState</param>
        /// <param name="mouseState">Current mouseState</param>
        public void Update(GameTime gametime, KeyboardState keyState, MouseState mouseState)
        {
            CameraUpdates(gametime, keyState, mouseState);
           
            _view = Matrix.CreateLookAt(_cameraPos, _cameraTarget, _up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _aspectRatio, _nearClip, _farClip);

        }

        /// <summary>
        /// Update the movements and rotations of the camera
        /// </summary>
        /// <param name="gametime">GameTime snapshot</param>
        /// <param name="keyState">Current keyboardState</param>
        /// <param name="mouseState">Current mouseState</param>
        private void CameraUpdates (GameTime gametime, KeyboardState keyState, MouseState mouseState)
        {
            Mouse.SetPosition(_graphics.Viewport.Width / 2, _graphics.Viewport.Height / 2); 
            Rotation(mouseState, gametime);
            Matrix rotation = Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0);

            _translation = Vector3.Transform(_translation, rotation);
            _cameraPos += _translation;
            _translation = Vector3.Zero;

            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MForward))
                _translation += Vector3.Forward;

            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MBackward))
                _translation += Vector3.Backward;

            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MLeft))
                _translation += Vector3.Left;

            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MRight))
                _translation += Vector3.Right;

            _translation = _translation * gametime.ElapsedGameTime.Milliseconds;

            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            _cameraTarget = _cameraPos + forward;
        }

        /// <summary>
        /// Update the camera rotation from the mouse movements
        /// </summary>
        /// <param name="mouseState">Current mouseState</param>
        /// <param name="gametime">GameTime snapshot</param>
        private void Rotation(MouseState mouseState, GameTime gametime)
        {
            float targetYaw = this._yaw - ((float)mouseState.X - (float)_graphics.Viewport.Width / 2);
            float targetPitch = this._pitch - ((float)mouseState.Y - (float)_graphics.Viewport.Height / 2);

            this._yaw = MathHelper.SmoothStep(_yaw, targetYaw, _gameSettings._gameSettings.KeyMapping.MouseSensibility);
            this._pitch = MathHelper.SmoothStep(_pitch, targetPitch, _gameSettings._gameSettings.KeyMapping.MouseSensibility);
        }


    }
}
