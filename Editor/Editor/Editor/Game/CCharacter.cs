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

using XNAnimation;
using XNAnimation.Controllers;

namespace Editor.Game
{
    class CCharacter
    {
        private Game.Settings.CGameSettings _gameSettings;

        private SkinnedModel _hand;
        private AnimationController animationController;

        float _initSpeed = 0.2f;
        float _velocity = 0.3f;

        public void Initialize()
        {
            this._gameSettings = Game.Settings.CGameSettings.getInstance();
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphics)
        {
            _hand = content.Load<SkinnedModel>("Models\\Arm_Animation");

            Texture2D armTexture = content.Load<Texture2D>("Textures\\Uvw_Hand");


            foreach (ModelMesh mesh in _hand.Model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.Texture = armTexture;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }
            }

            // Create an animation controller and start a clip
            animationController = new AnimationController(_hand.SkeletonBones);
            animationController.Speed = 0.5f;

            animationController.TranslationInterpolation = InterpolationMode.Linear;
            animationController.OrientationInterpolation = InterpolationMode.Linear;
            animationController.ScaleInterpolation = InterpolationMode.Linear;

            animationController.StartClip(_hand.AnimationClips["Take 001"]);

            animationController.LoopEnabled = true;
        }

        public void Update(MouseState mouseState, MouseState oldMouseState, KeyboardState kbState,CWeapon weapon, GameTime gameTime, Matrix camView,
            Matrix camProjection, Vector3 camPos)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                weapon.Shot(true, gameTime);
            else if (mouseState.LeftButton == ButtonState.Pressed)
                weapon.Shot(false, gameTime);

            // Update the models animation.
            animationController.Update(gameTime.ElapsedGameTime, Matrix.Identity);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gametime, Matrix view, Matrix projection, Vector3 camPos)
        {
            foreach (ModelMesh mesh in _hand.Model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(animationController.SkinnedBoneTransforms);
                    effect.World = Matrix.CreateRotationY(0.0f) * Matrix.CreateTranslation(13.0f,10.0f, 0.0f) *
                        Matrix.CreateScale(1.5f);

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
