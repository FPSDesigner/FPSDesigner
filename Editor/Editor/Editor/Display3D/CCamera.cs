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
    class CPhysicsMap
    {
        public float _gravityAcceleration { get; private set; }
        public float _charHeight { get; private set; }
        public float _time { get; private set; }

        public Vector3 _velocity;

        private bool _isJumping = false;

        public Display3D.CTerrain _map;

        public CPhysicsMap(float gravAcc, Display3D.CTerrain map, float height)
        {
            this._gravityAcceleration = gravAcc;
            this._charHeight = height;

            this._map = map;
        }

        public Vector3 CollisionCheck(GameTime gameTime, Vector3 pos)
        {
            float Steepness;
            float terrainHeight = _map.GetHeightAtPosition(pos.X, pos.Z, out Steepness);
            _time += gameTime.ElapsedGameTime.Milliseconds;

            if (pos.Y > terrainHeight + _charHeight || _isJumping)
            {
                _velocity.Y -= _gravityAcceleration * gameTime.ElapsedGameTime.Milliseconds;
                _isJumping = false;
            }
            else 
            {
                pos.Y = terrainHeight + _charHeight;
               _time = 0.0f;
                _velocity = Vector3.Zero;

                Vector3 normal = _map.getNormalAtPoint(pos.X, pos.Z);
                _velocity = Vector3.Zero;

                if (normal.Y > -0.6)
                {
                    _velocity = -0.5f * normal;
                    _velocity.Y = -0.1f;
                }
            }
            return pos + _velocity;
        }

        public void Jump(Vector3 Position)
        {
            float Steepness;
            float terrainHeight = _map.GetHeightAtPosition(Position.X, Position.Z, out Steepness);

            if (Position.Y <= terrainHeight + _charHeight && !_isJumping)
            {
                _velocity.Y += 0.3f;
                _isJumping = true;
            }

        }
    }

    /// <summary>
    /// The camera class process all the camera movement and the world view & projection matrix.
    /// </summary>
    class CCamera
    {
        public Matrix _view { get; private set; }
        public Matrix _projection{ get; private set; }

        public Vector3 _cameraPos { get; private set; }
        public Vector3 _cameraTarget { get; private set; }

        // This vector take the movement (Forward, etc...) & the rotatio, so, movement follow the view
        private Vector3 _translation;
        private Vector3 _up;

        private Point _middleScreen;

        private Display3D.CTerrain _map;

        private Game.Settings.CGameSettings _gameSettings;

        private bool isCamFrozen = false;

        // Rotations angles
        public float _yaw { get; private set; }
        public float _pitch { get; private set; }
        
        private float _nearClip;
        private float _farClip;
        private float _cameraVelocity;
        private float _aspectRatio;

        private float lowestPitchAngle = -MathHelper.PiOver2 + 0.1f;
        private float highestPitchAngle = MathHelper.PiOver2 - 0.1f;

        private KeyboardState _oldKeyState;

        private GraphicsDevice _graphics;

        public BoundingFrustum Frustum { get; private set; }

        public CPhysicsMap _physicsMap { get; private set; }

        /// <summary>
        /// Initialize the class
        /// </summary>
        /// <param name="device">GraphicsDevice class</param>
        /// <param name="cameraPos">Default position of the camera</param>
        /// <param name="target">Default target of the camera</param>
        /// <param name="nearClip">Closest elements to be rendered</param>
        /// <param name="farClip">Farthest elements to be rentered</param>
        /// <param name="camVelocity">Camera movement speed</param>
        /// <param name="camVelocity">Camera Frozen or not</param>
        /// /// <param name="camVelocity">Give an map (heightmap) instance</param>
        public CCamera(GraphicsDevice device, Vector3 cameraPos,Vector3 target, float nearClip, float farClip, float camVelocity,bool isCamFrozen, Display3D.CTerrain map)
        {
            this._graphics = device;
            _aspectRatio = _graphics.Viewport.AspectRatio; // 16::9 - 4::3 etc

            this._nearClip = nearClip;
            this._farClip = farClip;

            this._cameraVelocity = camVelocity;

            this._cameraPos = cameraPos;
            this._cameraTarget = target;

            this._pitch = 0f;
            this._yaw = 0f;

            this.isCamFrozen = isCamFrozen;

            this._map = map;

            this._physicsMap = new CPhysicsMap(9.81f/500, _map, 3.0f);

            this._up = Vector3.Up;
            this._translation = Vector3.Zero;

            this._middleScreen = new Point(_graphics.Viewport.Width / 2, _graphics.Viewport.Height / 2);

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
        public void Update(GameTime gametime, KeyboardState keyState = default(KeyboardState), MouseState mouseState = default(MouseState), 
            KeyboardState oldKeyState = default(KeyboardState))
        {
            if (!isCamFrozen)
                CameraUpdates(gametime, keyState, oldKeyState,mouseState);

            _oldKeyState = keyState;
            _view = Matrix.CreateLookAt(_cameraPos, _cameraTarget, _up);
            generateFrustum();
        }

        /// <summary>
        /// Update the movements and rotations of the camera
        /// </summary>
        /// <param name="gametime">GameTime snapshot</param>
        /// <param name="keyState">Current keyboardState</param>
        /// <param name="mouseState">Current mouseState</param>
        private void CameraUpdates (GameTime gametime, KeyboardState keyState, KeyboardState oldKeySate ,MouseState mouseState)
        {
            Mouse.SetPosition(_middleScreen.X, _middleScreen.Y); 

            Rotation(mouseState, gametime);

            _translation = Vector3.Transform(_translation, Matrix.CreateFromYawPitchRoll(_yaw, 0, 0));

            _cameraPos = Vector3.Lerp(_cameraPos, _physicsMap.CollisionCheck(gametime, _cameraPos + _translation * _cameraVelocity), 0.1f);

            _translation = Vector3.Zero;

            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MForward))
                _translation += Vector3.Forward;

            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MBackward))
                _translation += Vector3.Backward;

            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MLeft))
                _translation += Vector3.Left;

            if (keyState.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MRight))
                _translation += Vector3.Right;

            if (keyState.IsKeyDown(Keys.Space) && oldKeySate.IsKeyUp(Keys.Space))
                _physicsMap.Jump(_cameraPos);

            Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0));
            _cameraTarget = _cameraPos + forward;
            
        }

        /// <summary>
        /// Update the camera rotation from the mouse movements
        /// </summary>
        /// <param name="mouseState">Current mouseState</param>
        /// <param name="gametime">GameTime snapshot</param>
        private void Rotation(MouseState mouseState, GameTime gametime)
        {
            this._yaw -= _gameSettings._gameSettings.KeyMapping.MouseSensibility * (mouseState.X - _middleScreen.X);
            this._pitch -= _gameSettings._gameSettings.KeyMapping.MouseSensibility * (mouseState.Y - _middleScreen.Y);

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
            return true;
            //return (Frustum.Contains(sphere) != ContainmentType.Disjoint);
        }

        /// <summary>
        /// Checks if a BoundingBox is in view
        /// </summary>
        /// <param name="box">Box to check if it is in view</param>
        /// <returns></returns>
        public bool BoundingVolumeIsInView(BoundingBox box)
        {
            return (Frustum.Contains(box) != ContainmentType.Disjoint);
        }

        /// <summary>
        /// Generates a Frustum using the view and projection matrices
        /// </summary>
        private void generateFrustum()
        {
            Matrix viewProjection = _view * _projection;
            Frustum = new BoundingFrustum(viewProjection);
        }
    }
}
