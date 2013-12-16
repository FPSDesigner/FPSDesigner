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

        private Model _rHandModel;
        private SkinnedModel.SkinningData _skinningData;
        private AnimationPlayer _animation;
        private Matrix[] boneTransforms;

        float _initSpeed = 0.2f;
        float _velocity = 0.3f;

        public void Initialize()
        {
            this._gameSettings = Game.Settings.CGameSettings.getInstance();
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics)
        {
            _rHandModel = content.Load<Model>("3D//3DModels//Arm_Animation");
            // Look up our custom skinning information.
            _skinningData = _rHandModel.Tag as SkinningData;

            if (_skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            boneTransforms = new Matrix[_skinningData.BindPose.Count];

            // Create an animation player, and start decoding an animation clip.
            _animation = new AnimationPlayer(_skinningData);

            //AnimationClip clip = _skinningData.AnimationClips["RunCycleMove"];

            _animation.StartClip(_skinningData.AnimationClips["RunCycleMove"]);
        }

        public void Update(MouseState mouseState, MouseState oldMouseState, KeyboardState kbState,CWeapon weapon, GameTime gameTime, Matrix camView,
            Matrix camProjection, Vector3 camPos)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                weapon.Shot(true, gameTime);
            else if (mouseState.LeftButton == ButtonState.Pressed)
                weapon.Shot(false, gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gametime, Matrix view, Matrix projection, Vector3 camPos)
        {
            Matrix[] bones = _animation.GetSkinTransforms();
            // Render the skinned mesh.
            foreach (ModelMesh mesh in _rHandModel.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);
                    effect.EnableDefaultLighting();

                    effect.World = bones[mesh.ParentBone.Index] *
                    Matrix.CreateRotationY(0f)
                    * Matrix.CreateTranslation(new Vector3(-19.191149f, 59.28934f, 5.47f)); ;

                    effect.View = view;
                    effect.Projection = projection;
                    

                }

                mesh.Draw();
            }
        }


        //This function allows the player to Run
            //the fallVelocity argument is used to prevent the player from speed in the air
        public float Run(KeyboardState state, Vector3 fallVelocity)
        {
            if (state.IsKeyDown(_gameSettings._gameSettings.KeyMapping.MSprint))
            {
                if ((_velocity < _initSpeed + 0.25f) && fallVelocity.Y <= 0.0f)
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
