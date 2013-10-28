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

        private Vector3 _translation; // This vector take the movement (Forward, etc...) & the rotatio, so, movement follow the view
        private Vector3 _up;

        private Game.Settings.CGameSettings _gameSettings;

        public float _yaw { get; private set; } //---| Param for rotation matrix
        public float _pitch { get; private set; }//--|
        
        private float _nearClip;
        private float _farClip;

        private GraphicsDevice _graphics;

        private float _aspectRatio;

        public CCamera(GraphicsDevice device, Vector3 cameraPos, Vector3 target, float nearClip, float farClip)
        // used to initialize all the values
        {
            this._graphics = device;
            _aspectRatio = _graphics.Viewport.AspectRatio; // 16::9 - 4::3 etc

            this._nearClip = nearClip;
            this._farClip = farClip;

            this._cameraPos = cameraPos;
            this._cameraTarget = target;

            this._up = Vector3.Up;

            this._translation = Vector3.Zero;

            this._gameSettings = Game.Settings.CGameSettings.getInstance();

            _view = Matrix.CreateLookAt(cameraPos, target, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(40), _aspectRatio, _nearClip, _farClip);
        }

        public void Update(GameTime gametime, KeyboardState keyState, MouseState mouseState)
        {
            CameraUpdates(gametime, keyState, mouseState); // Moving the camera
           
            _view = Matrix.CreateLookAt(_cameraPos, _cameraTarget, _up); //View with camera
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _aspectRatio, _nearClip, _farClip); // Displaying on the screen

        }

        public void CameraUpdates (GameTime gametime, KeyboardState keyState, MouseState mouseState)
            // Keyboard displacement
        {
            Mouse.SetPosition(_graphics.Viewport.Width / 2, _graphics.Viewport.Height / 2); 
            Rotation(mouseState, gametime); //Get back Yaw & Pitch
            Matrix rotation = Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0);

            _translation = Vector3.Transform(_translation, rotation);//--| Translation added to position
            _cameraPos += _translation;//-------------------------------| 
            _translation = Vector3.Zero;//------------------------------|

            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MForward)) { 
                _translation += Vector3.Forward * gametime.ElapsedGameTime.Milliseconds; 
            }
            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MBackward)) { 
                _translation += Vector3.Backward * gametime.ElapsedGameTime.Milliseconds;
            }
            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MLeft)) { 
                _translation += Vector3.Left * gametime.ElapsedGameTime.Milliseconds; 
            }
            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MRight)) { 
                _translation += Vector3.Right * gametime.ElapsedGameTime.Milliseconds; 
            }

            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            _cameraTarget = _cameraPos + forward; //TARGETING
        }

        public void Rotation(MouseState mouseState, GameTime gametime)
            // Use to modify yaw & pitch
        {
            this._yaw -= 0.001f * gametime.ElapsedGameTime.Milliseconds *((float)mouseState.X - (float)_graphics.Viewport.Width/2) ;
            this._pitch -= 0.001f * gametime.ElapsedGameTime.Milliseconds * ((float)mouseState.Y - (float)_graphics.Viewport.Height / 2); 
        }


    }
}
