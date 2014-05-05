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

namespace Engine.Game
{

    class CEnemyManager
    {

    }

    class CEnemy
    {
        float _life;

        private Display3D.MeshAnimation _model; //The 3Dmodel and all animations

        private Vector3 _position; // The character positon
        private Vector3 _targetPos; // The target position

        private float rotationValue; // We give this float to the rotation mat
        private Matrix _rotation; // Model rotation

        private Game.CPhysics _physicEngine; // Ennemy will be submitted to forces

        private bool _isMoving;


        public CEnemy(string ModelName, Texture2D[] Textures ,Vector3 Position, Matrix Rotation)
        {
            _position = Position;
            _rotation = Rotation;

            // We Create the Enemy, giving its textures, models...

            _model = new Display3D.MeshAnimation(ModelName, 1, 1, 1.0f,_position,
                Matrix.CreateRotationX(-1 * MathHelper.PiOver2), 2f, Textures, 10, 0.0f, true);

            _isMoving = false;

        }

        public void LoadContent(ContentManager content, Display3D.CCamera cam)
        {

            // We load the ennemy content
            _model.LoadContent(content);
            _model.ChangeAnimSpeed(2f);
            _model.BeginAnimation("walk", true);

            // We Create the forces application on the Ennemy
            _physicEngine = new CPhysics();
            _physicEngine.LoadContent(2f, new bool[] { true,true});
            _physicEngine._triangleList = cam._physicsMap._triangleList;
            _physicEngine._triangleNormalsList = cam._physicsMap._triangleNormalsList;
            _physicEngine._terrain = cam._physicsMap._terrain;
            _physicEngine._waterHeight = cam._physicsMap._waterHeight;
        }

        public void Update(GameTime gameTime)
        {
            // Apply the physic on the character
            _position = _physicEngine.GetNewPosition(gameTime, _position,Vector3.Zero,false);

            // We update the character pos, rot...
            _model.Update(gameTime, _position, _rotation);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Matrix view, Matrix projection)
        {
            _model.Draw(gameTime, spriteBatch, view, projection);
        }

        public void MoveTo(Vector3 newPos)
        {
            _targetPos = _position + newPos;

            //_targetPos += _position;

            //if (_targetPos != Vector3.Zero)
            //{

            //}
        }
    }
}
