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

        public MouseState _oldMouseState;
        private Game.Settings.CGameSettings _gameSettings;

        public float _yaw { get; set; }
        public float _pitch { get; set; }
        
        private float _nearClip;
        private float _farClip;

        private GraphicsDevice _graphics;

        private float _aspectRatio;

        public CCamera(GraphicsDevice device, Vector3 cameraPos, Vector3 target, float nearClip, float farClip)
        // used to initialize all the values
        {
            this._graphics = device;
            _aspectRatio = device.PresentationParameters.BackBufferWidth / device.PresentationParameters.BackBufferHeight;

            this._nearClip = nearClip;
            this._farClip = farClip;

            this._cameraPos = cameraPos;
            this._cameraTarget = target;

            this._gameSettings = Game.Settings.CGameSettings.getInstance();

            _view = Matrix.CreateLookAt(cameraPos, target, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), _aspectRatio, _nearClip, _farClip);
        }

        public void Update(GameTime gametime, KeyboardState keyState, MouseState mouseState)
        {

            Displacement(gametime, keyState); // Moving the camera

            if (_oldMouseState.X == 0 && _oldMouseState.Y == 0)
                _oldMouseState = mouseState;

            Rotate(mouseState);
            Matrix rotation = Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0);

            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation); 
            _cameraTarget = _cameraPos + forward;

            Vector3 up = Vector3.Transform(Vector3.Up, rotation);


            _view = Matrix.CreateLookAt(_cameraPos, _cameraTarget, up); //View with camera
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), _aspectRatio, _nearClip, _farClip); // Displaying on the screen
            _oldMouseState = mouseState;
        }

        public void Displacement (GameTime gametime,KeyboardState keyState)
            // Keyboard displacement
        {
            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MForward)) { _cameraPos += Vector3.Forward * gametime.ElapsedGameTime.Milliseconds; _cameraTarget += Vector3.Forward * gametime.ElapsedGameTime.Milliseconds; }
            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MBackward)) { _cameraPos += Vector3.Backward * gametime.ElapsedGameTime.Milliseconds; _cameraTarget += Vector3.Backward * gametime.ElapsedGameTime.Milliseconds; }
            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MLeft)) { _cameraPos += Vector3.Left * gametime.ElapsedGameTime.Milliseconds; _cameraTarget += Vector3.Left * gametime.ElapsedGameTime.Milliseconds; }
            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MRight)) { _cameraPos += Vector3.Right * gametime.ElapsedGameTime.Milliseconds; _cameraTarget += Vector3.Right * gametime.ElapsedGameTime.Milliseconds; }
        }

        public void Rotate(MouseState mouseState)
        {
            this._yaw -= 0.01f * ((float)mouseState.X - (float)_oldMouseState.X);
            this._pitch -= 0.01f * ((float)mouseState.Y - (float)_oldMouseState.Y); 
        }


    }
}
