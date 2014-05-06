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

namespace Engine.Display3D
{
    class MeshAnimation
    {
        private string _modelName;
        private int _animationNumber;
        private int _meshNumber;
        private float _animationSpeed;
        private float _scale;

        private int _specPower;
        private float _specColor;

        private bool _isLooped;
        private Texture2D[] _textures;

        public Vector3 _position;
        private Matrix _rotation;

        public SkinnedModel skinnedModel;
        private AnimationController animationController;

        public Matrix[] _modelTransforms;

        public MeshAnimation(string model, int animNbr,int meshNbr,float animSpeed,Vector3 pos, Matrix rot,float scale,Texture2D[] text, int specPower, float specColor,bool isLooped)
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

            this._specPower = specPower;
            this._specColor = specColor;

        }

        public void LoadContent(ContentManager content)
        {
            skinnedModel = content.Load<SkinnedModel>("Models\\"+_modelName);

            foreach (ModelMesh mesh in skinnedModel.Model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    if(_textures != null)
                        effect.Texture = _textures[0];
                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(_specColor);
                    effect.SpecularPower = _specPower;
                }
            }

            // Create an animation controller and start a clip
            animationController = new AnimationController(skinnedModel.SkeletonBones);
            animationController.Speed = _animationSpeed;

            animationController.TranslationInterpolation = InterpolationMode.Linear;
            animationController.OrientationInterpolation = InterpolationMode.Linear;
            animationController.ScaleInterpolation = InterpolationMode.Linear;
            animationController.LoopEnabled = _isLooped;

            _modelTransforms = new Matrix[skinnedModel.Model.Bones.Count];
            skinnedModel.Model.CopyAbsoluteBoneTransformsTo(_modelTransforms);

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

                string newName = mesh.Name.Split('_')[0];
                
                // If the mesh is not a bounding box
                if (newName != "Bb")
                {
                    mesh.Draw();
                }
            }

        }

        public void ChangeAnimSpeed(float newSpeed)
        {
            _animationSpeed = newSpeed;
            animationController.Speed = _animationSpeed;
        }

        public float GetAnimSpeed()
        {
            return _animationSpeed;
        }

        public void BeginAnimation(string name, bool looping)
        {
            //Begin an animation
            animationController.StartClip(skinnedModel.AnimationClips[name]);
            animationController.LoopEnabled = looping;
        }

        public void ChangeAnimation(string name, bool looping, float velocity = 0.4f)
        {
            //Change animation smoothly
            animationController.LoopEnabled = looping;
            animationController.CrossFade(skinnedModel.AnimationClips[name], TimeSpan.FromSeconds(velocity));
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

        public Matrix GetBoneMatrix(string boneName, Matrix rotation,float scale, Vector3 offset)
        {
            int index = skinnedModel.Model.Bones[boneName].Index;

            Matrix boneLocal = animationController.SkinnedBoneTransforms[index];

            boneLocal = Matrix.CreateTranslation(offset)
                            * rotation
                            * Matrix.CreateScale(scale)
                            * Matrix.Invert(skinnedModel.SkeletonBones[index].InverseBindPoseTransform)
                            * animationController.SkinnedBoneTransforms[index]
                            * CreateWorldMatrix(_position, _rotation, _scale);
            return boneLocal;
        }

        public Matrix CreateWorldMatrix(Vector3 Translation, Matrix Rotation, float Scale)
        {
            return Matrix.CreateScale(Scale) * Rotation * Matrix.CreateTranslation(Translation);
        }

        // The animation will be played in the other way
        public void InverseMode(string mode)
        {
            mode.ToLower();
            switch (mode)
            {
                case "backward":
                    animationController.PlaybackMode = PlaybackMode.Backward;
                    break;
                case "forward":
                    animationController.PlaybackMode = PlaybackMode.Forward;
                    break;
                default:
                    animationController.PlaybackMode = PlaybackMode.Forward;
                    break;
            }
        }
    }
}
