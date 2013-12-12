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
        private Matrix[] _modelTransforms;

        float _initSpeed = 0.3f;
        float _velocity = 0.3f;

        public void Initialize()
        {
            this._gameSettings = Game.Settings.CGameSettings.getInstance();
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics)
        {
            _rHandModel = content.Load<Model>("3D//3DModels//Arm_Animation");
            _modelTransforms = new Matrix[_rHandModel.Bones.Count];
            _rHandModel.CopyAbsoluteBoneTransformsTo(_modelTransforms);

            // Look up our custom skinning information.
            _skinningData = _rHandModel.Tag as SkinningData;

            if (_skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            boneTransforms = new Matrix[_skinningData.BindPose.Count];

            // Create an animation player, and start decoding an animation clip.
            _animation = new AnimationPlayer(_skinningData);

            AnimationClip clip = _skinningData.AnimationClips["RunCycleMove"];

            _animation.StartClip(clip);
        }

        public void Update(MouseState mouseState, MouseState oldMouseState, KeyboardState kbState,CWeapon weapon, GameTime gameTime, Matrix camView,
            Matrix camProjection, Vector3 camPos)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                weapon.Shot(true, gameTime);
            else if (mouseState.LeftButton == ButtonState.Pressed)
                weapon.Shot(false, gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gametime, Matrix view, Matrix projection)
        {
            Matrix[] bones = _animation.GetSkinTransforms();
            Matrix world = Matrix.CreateTranslation(new Vector3(-10.191149f, 57.28934f, -6.645897f))* Matrix.CreateScale(20.0f);
            // Render the skinned mesh.
            foreach (ModelMesh mesh in _rHandModel.Meshes)
            {
                Matrix localWorld = _modelTransforms[mesh.ParentBone.Index] * world;

                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = localWorld;
                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
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
