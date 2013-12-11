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
using SkinnedModel;

namespace Editor.Game
{
    class CCharacter
    {
        private Game.Settings.CGameSettings _gameSettings;

        private Display3D.CModel _rHandModel;
        private SkinnedModel.SkinningData _skinningData;
        private AnimationPlayer _animation;
        private Matrix[] boneTransforms;

        float _initSpeed = 0.3f;
        float _velocity = 0.3f;

        public void Initialize()
        {
            this._gameSettings = Game.Settings.CGameSettings.getInstance();
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics)
        {
            _rHandModel = new Display3D.CModel(content.Load<Model>("3D//3DModels//Arm_Animation"), new Vector3(0, 53.4f, 0), Vector3.Zero, new Vector3(1.0f), graphics);
            if (_skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            AnimationClip clip = _skinningData.AnimationClips["HandMove"];
            _animation.StartClip(clip);
        }

        public void Update(MouseState mouseState, MouseState oldMouseState, KeyboardState kbState,CWeapon weapon, GameTime gameTime, Matrix camView,
            Matrix camProjection, Vector3 camPos)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                weapon.Shot(true, gameTime);
            else if (mouseState.LeftButton == ButtonState.Pressed)
                weapon.Shot(false, gameTime);

            // Tell the animation player to compute the latest bone transform matrices.
            _animation.UpdateBoneTransforms(gameTime.ElapsedGameTime, true);

            // Copy the transforms into our own array, so we can safely modify the values.
            _animation.GetBoneTransforms().CopyTo(boneTransforms, 0);

            _rHandModel.Draw(camView,camProjection,camPos);
        }


        //This function allows the player to Run
            //the fallVelocity argument is used to prevent the player from speed in the air
        public float Run(KeyboardState state, Vector3 fallVelocity)
        {
            if (state.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MSprint))
            {
                if ((_velocity < _initSpeed + 0.4f) && fallVelocity.Y <= 0.0f)
                {
                    _velocity += .01f;
                    //_velocity += 2f;
                }
            }
            else
            {
                if (_velocity > _initSpeed)
                {
                    _velocity -= .01f;
                }
            }
            return _velocity;
        }

    }
}
