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

using Engine.Game.Settings;

namespace Engine.Display3D
{

    /// <summary>
    /// The camera class process all the camera movement and the world view & projection matrix.
    /// </summary>
    class CCamera
    {
        public Matrix _view { get; private set; }
        public Matrix _projection { get; private set; }
        public Matrix _nearProjection { get; private set; }

        public Vector3 _cameraPos { get; set; }
        public Vector3 _cameraTarget { get; set; }

        // This vector take the movement (Forward, etc...) & the rotatio, so, movement follow the view
        public Vector3 _translation;
        public Vector3 _up;
        public Vector3 _right;

        public Point _middleScreen;
        
        private bool hasPlayerUwEffect = false;
        public bool isFreeCam = false;
        public bool isCamFrozen = false;
        public bool _isMoving { get; private set; } //If the player move, useful for animations

        // Rotations angles
        public float _yaw { get; set; }
        public float _pitch { get; set; }
        public float _roll { get; set; }

        private float _nearClip;
        private float _farClip;
        private float _aspectRatio;
        public float fieldOfView;

        public float sensibilityMultiplier = 1;

        public float _playerHeight = 1.9f;

        public bool shouldUpdateMiddleScreen = false;

        private float lowestPitchAngle = -MathHelper.PiOver2 + 0.1f;
        private float highestPitchAngle = MathHelper.PiOver2 - 0.1f;

        private KeyboardState _oldKeyState;

        private GraphicsDevice _graphics;
        private Viewport _Viewport;

        public BoundingFrustum _Frustum { get; private set; }

        public Game.CPhysics _physicsMap { get; private set; }

        /// <summary>
        /// Initialize the class
        /// </summary>
        /// <param name="device">GraphicsDevice class</param>
        /// <param name="cameraPos">Default position of the camera</param>
        /// <param name="target">Default target of the camera</param>
        /// <param name="nearClip">Closest elements to be rendered</param>
        /// <param name="farClip">Farthest elements to be rentered</param>
        /// <param name="camVelocity">Camera movement speed</param>
        /// <param name="isCamFrozen">Camera Frozen or not</param>
        /// <param name="freeCam">Is the camera free of physics or not</param>
        /// /// <param name="camVelocity">Give an map (heightmap) instance</param>
        public CCamera(GraphicsDevice device, Vector3 cameraPos, Vector3 target, float nearClip, float farClip, bool isCamFrozen, bool freeCam = false, CTerrain map = null, bool[] componentUsages = null)
        {
            this._graphics = device;
            _Viewport = device.Viewport;
            _aspectRatio = _graphics.Viewport.AspectRatio; // 16::9 - 4::3 etc

            this._nearClip = nearClip;
            this._farClip = farClip;

            this._cameraPos = cameraPos;
            this._cameraTarget = target;

            this._pitch = 0f;
            this._yaw = 0f;
            this._roll = 0f;

            this.isCamFrozen = isCamFrozen;
            this.isFreeCam = freeCam;

            //this._physicsMap = new Game.CPhysics2(9.81f / 500, _map, _playerHeight);
            this._physicsMap = new Game.CPhysics();

            if (map != null)
            {
                _physicsMap.LoadContent(_playerHeight, componentUsages);
                _physicsMap._terrain = map;
            }

            this._up = Vector3.Up;
            this._right = Vector3.Cross(Vector3.Forward, _up);
            this._translation = Vector3.Zero;

            this._middleScreen = new Point(_graphics.Viewport.Width / 2, _graphics.Viewport.Height / 2);

            _isMoving = false;

            _view = Matrix.CreateLookAt(cameraPos, target, Vector3.Up);

            fieldOfView = MathHelper.ToRadians(40);
            _projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, _aspectRatio, _nearClip, _farClip);
            _nearProjection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, _aspectRatio, 0.02f, 1f);

            generateFrustum();
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
            if (shouldUpdateMiddleScreen)
            {
                _middleScreen = new Point(mouseState.X, mouseState.Y);
                shouldUpdateMiddleScreen = false;
            }

            if (_Viewport.Bounds != Display2D.C2DEffect.softwareViewport.Bounds)
            {
                // Window size have changed
                _Viewport = Display2D.C2DEffect.softwareViewport;

                _aspectRatio = _Viewport.AspectRatio;
                _projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, _aspectRatio, _nearClip, _farClip);
                _nearProjection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, _aspectRatio, 0.02f, 1f);
                generateFrustum();
            }

            if (!isCamFrozen)
            {
                CameraUpdates(gametime, keyState, oldKeyState, mouseState, camVelocity, isUnderWater, waterLevel);
                CGameSettings.reloadGamepadState();
                _oldKeyState = keyState;
            }

            if (Display2D.C2DEffect.isSoftwareEmbedded)
                _middleScreen = new Point(mouseState.X, mouseState.Y);

            _view = Matrix.CreateLookAt(_cameraPos, _cameraTarget, _up);


            generateFrustum();
        }

        /// <summary>
        /// Update the movements and rotations of the camera
        /// </summary>
        /// <param name="gametime">GameTime snapshot</param>
        /// <param name="keyState">Current keyboardState</param>
        /// <param name="mouseState">Current mouseState</param>
        private void CameraUpdates(GameTime gametime, KeyboardState keyState, KeyboardState oldKeySate, MouseState mouseState, float camVelocity, bool isUnderWater, float waterLevel)
        {
            // Just used for the animation in Character
            _isMoving = false;

            if(!Display2D.C2DEffect.isSoftwareEmbedded)
                Mouse.SetPosition(_middleScreen.X, _middleScreen.Y);

            Rotation(mouseState, gametime);

            if (_translation.LengthSquared() > 1)
            {
                _translation.Normalize();
            }

            // _cameraPos = Vector3.Lerp(_cameraPos, _physicsMap.checkCollisions(gametime, _cameraPos, _translation * camVelocity, isUnderWater, waterLevel), 0.5f);
            if (isFreeCam)
            {
                _translation = Vector3.Transform(_translation, Matrix.CreateFromYawPitchRoll(_yaw, _pitch, _roll));
                _cameraPos += _translation * camVelocity * (float)gametime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                _translation = Vector3.Transform(_translation, Matrix.CreateFromYawPitchRoll(_yaw, (isUnderWater || _physicsMap._isOnWaterSurface) ? _pitch : 0, 0));
                _cameraPos = _physicsMap.GetNewPosition(gametime, _cameraPos, _translation * camVelocity, ((isUnderWater || _physicsMap._isOnWaterSurface) && _cameraPos.Y - _playerHeight < waterLevel));
            }
            _translation = Vector3.Zero;

            if (!Game.CConsole._isConsoleEnabled)
            {
                if (CGameSettings.useGamepad)
                {
                    Vector2 direction = CGameSettings.gamepadState.ThumbSticks.Left;
                    _translation += new Vector3(direction.X, 0, -direction.Y);

                    if (CGameSettings.gamepadState.IsButtonDown(CGameSettings._gameSettings.KeyMapping.GPJump))
                        _physicsMap.Jump();
                }

                if (keyState.IsKeyDown(CGameSettings._gameSettings.KeyMapping.MForward))
                    _translation += Vector3.Forward;

                if (keyState.IsKeyDown(CGameSettings._gameSettings.KeyMapping.MBackward))
                    _translation += 0.48f*Vector3.Backward;

                if (keyState.IsKeyDown(CGameSettings._gameSettings.KeyMapping.MLeft))
                    _translation += Vector3.Left;

                if (keyState.IsKeyDown(CGameSettings._gameSettings.KeyMapping.MRight))
                    _translation += Vector3.Right;

                //if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MCrouch))


                if (keyState.IsKeyDown(Keys.Space))
                    _physicsMap.Jump();
            }

            if (!isFreeCam)
            {
                if (_translation.X != 0)
                    _roll = MathHelper.Lerp(_roll, -_translation.X * 0.05f, 0.2f);
                else if(_roll != 0)
                    _roll = MathHelper.Lerp(_roll, 0, 0.1f);
            }
            //_physicsMap.Swin(isUnderWater);

            if (hasPlayerUwEffect != isUnderWater)
            {
                Display2D.C2DEffect.UnderwaterEffect(isUnderWater);
                hasPlayerUwEffect = isUnderWater;
            }
            if (isUnderWater && !isFreeCam)
            {
                _translation = _translation * 0.3f;
            }

            Matrix rotation = Matrix.CreateFromYawPitchRoll(_yaw, _pitch, _roll);

            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            _cameraTarget = _cameraPos + forward;

            Vector3 up = Vector3.Transform(Vector3.Up, rotation);

            this._up = up;
            this._right = Vector3.Cross(forward, up);

            if (_translation != Vector3.Zero)
            {
                _isMoving = true;
            }

        }

        /// <summary>
        /// Update the camera rotation from the mouse movements
        /// </summary>
        /// <param name="mouseState">Current mouseState</param>
        /// <param name="gametime">GameTime snapshot</param>
        private void Rotation(MouseState mouseState, GameTime gametime)
        {
            if (CGameSettings.useGamepad)
            {
                this._yaw -= CGameSettings._gameSettings.KeyMapping.GPSensibility * sensibilityMultiplier * CGameSettings.gamepadState.ThumbSticks.Right.X;
                this._pitch -= CGameSettings._gameSettings.KeyMapping.GPSensibility * sensibilityMultiplier *- CGameSettings.gamepadState.ThumbSticks.Right.Y;
            }
            float coefRotation = 1f;
            if (Display2D.C2DEffect.isSoftwareEmbedded)
                coefRotation = 1.5f;

            this._yaw -= coefRotation * CGameSettings._gameSettings.KeyMapping.MouseSensibility * sensibilityMultiplier *(mouseState.X - _middleScreen.X);
            this._pitch -= coefRotation * CGameSettings._gameSettings.KeyMapping.MouseSensibility * sensibilityMultiplier * (mouseState.Y - _middleScreen.Y);

            if (this._pitch < lowestPitchAngle)
                this._pitch = lowestPitchAngle;
            else if (this._pitch > highestPitchAngle)
                this._pitch = highestPitchAngle;
        }

        /// <summary>
        /// Checks if a BoundingSphere is in view
        /// </summary>
        /// <param name="sphere">Sphere to check if it is in view</param>
        /// <returns></returns>
        public bool BoundingVolumeIsInView(BoundingSphere sphere)
        {
            return (_Frustum.Contains(sphere) != ContainmentType.Disjoint);
        }

        /// <summary>
        /// Checks if a BoundingBox is in view
        /// </summary>
        /// <param name="box">Box to check if it is in view</param>
        /// <returns></returns>
        public bool BoundingVolumeIsInView(BoundingBox box)
        {
            return (_Frustum.Contains(box) != ContainmentType.Disjoint);
        }

        /// <summary>
        /// Generates a Frustum using the view and projection matrices
        /// </summary>
        private void generateFrustum()
        {
            Matrix viewProjection = _view * _projection;
            _Frustum = new BoundingFrustum(viewProjection);
        }

        public void ChangeFieldOfView(float newFov)
        {
            fieldOfView = newFov;

            _projection = Matrix.CreatePerspectiveFieldOfView(newFov, _aspectRatio, _nearClip, _farClip);
            _nearProjection = Matrix.CreatePerspectiveFieldOfView(newFov, _aspectRatio, 0.02f, 1f);
        }
    }
}
