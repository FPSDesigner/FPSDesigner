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
    class Enemy
    {
        //float _life;
        //private Display3D.MeshAnimation _model; //The 3Dmodel and all animations

        public void Initialize()
        {

        }

        public void LoadContent(ContentManager content)
        {
            //_model = new Display3D.MeshAnimation("test", 1, 1, 1.0f, new Vector3(-165.2928f, 170f, 80f),
            //    Matrix.Identity, 5.0f, null, 10, 0.0f, true);

            //_model.LoadContent(content);
        }

        public void Update(GameTime gameTime)
        {
            //_model.Update(gameTime, new Vector3(-165.2928f, 170f, 80f), Matrix.Identity);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Matrix view, Matrix projection)
        {
            //_model.Draw(gameTime, spriteBatch, view, projection);
        }
    }
}
