﻿using System;
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

namespace Editor.Display3D
{
    class MeshAnimation
    {
        private string _modelName;
        private int _animationNumber;
        private int _meshNumber;
        private float _animationSpeed;
        private float _scale;
        private bool _isLooped;
        private Texture2D[] _textures;

        private Vector3 _position;
        private Matrix _rotation;

        private SkinnedModel skinnedModel;
        private AnimationController animationController;

        public MeshAnimation(string model, int animNbr,int meshNbr,float animSpeed,Vector3 pos, Matrix rot,float scale,Texture2D[] text, bool isLooped)
        {
            this._modelName = model;
            this._animationNumber = animNbr;
            this._textures = text;
            this._animationSpeed = animSpeed;
            this._meshNumber = meshNbr;
            this._scale = scale;
            this._isLooped = isLooped;
            this._position = pos;
            this._rotation = rot;

        }

        public void LoadContent(ContentManager content)
        {
            skinnedModel = content.Load<SkinnedModel>("Models\\"+_modelName);

            foreach (ModelMesh mesh in skinnedModel.Model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.Texture = _textures[0];
                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 8;
                }
            }

            // Create an animation controller and start a clip
            animationController = new AnimationController(skinnedModel.SkeletonBones);
            animationController.Speed = _animationSpeed;

            animationController.TranslationInterpolation = InterpolationMode.Linear;
            animationController.OrientationInterpolation = InterpolationMode.Linear;
            animationController.ScaleInterpolation = InterpolationMode.Linear;
            animationController.LoopEnabled = _isLooped;

        }

        public void Update(GameTime gameTime, Vector3 position, Matrix rotation)
        {
            _position = position;
            _rotation = rotation;

            // Update the models animation.
            animationController.Update(gameTime.ElapsedGameTime, Matrix.Identity);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Matrix view, Matrix projection)
        {
            //draw the model (also the anim obviously)
            foreach (ModelMesh mesh in skinnedModel.Model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(animationController.SkinnedBoneTransforms);
                    effect.World = Matrix.CreateScale(_scale) * _rotation *  Matrix.CreateTranslation(_position);
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }

        }

        public void ChangeAnimSpeed(float newSpeed)
        {
            _animationSpeed = newSpeed;
            animationController.Speed = _animationSpeed;

        }

        public void BeginAnimation(string name, bool looping)
        {
            //Begin an animation
            animationController.StartClip(skinnedModel.AnimationClips[name]);
            animationController.LoopEnabled = looping;
        }

        public void ChangeAnimation(string name, bool looping)
        {
            //Change animation smoothly
            animationController.LoopEnabled = looping;
            animationController.CrossFade(skinnedModel.AnimationClips[name], TimeSpan.FromSeconds(0.4f));
        }

        public bool isPlaying()
        {
            return animationController.IsPlaying;
        }

        public bool HasFinished()
        {
            return animationController.HasFinished;
        }

        public  SkinnedModel GetModel()
        {
            return skinnedModel;
        }

        public Matrix GetBoneMatrix(string boneName)
        {
            int index = skinnedModel.Model.Bones[boneName].Index;

            Matrix boneLocal = animationController.SkinnedBoneTransforms[index];

            boneLocal = Matrix.Invert(skinnedModel.SkeletonBones[index].InverseBindPoseTransform)
                            * animationController.SkinnedBoneTransforms[index]
                            * Matrix.CreateScale(0.2f)
                            * Matrix.CreateRotationY(0.0f) * Matrix.CreateTranslation(_position);

            return boneLocal;
        }
    }
}
