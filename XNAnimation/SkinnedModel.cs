/*
 * SkinnedModel.cs
 * Author: Bruno Evangelista
 * Copyright (c) 2008 Bruno Evangelista. All rights reserved.
 *
 * THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace XNAnimation
{
    public class SkinnedModel
    {
        private Model model;
        private SkinnedModelBoneCollection skeleton;
        private AnimationClipDictionary animationClips;

        public Model Model
        {
            get { return model; }
        }

        public SkinnedModelBoneCollection SkeletonBones
        {
            get { return skeleton; }
        }

        public AnimationClipDictionary AnimationClips
        {
            get { return animationClips; }
        }

        internal SkinnedModel()
        {
        }

        public void Draw(TimeSpan elapsedTime)
        {

        }

        internal static SkinnedModel Read(ContentReader input)
        {
            SkinnedModel skinnedModel = new SkinnedModel();

            skinnedModel.model = input.ReadObject<Model>();
            skinnedModel.ReadBones(input);
            skinnedModel.ReadAnimations(input);
            return skinnedModel;
        }

        private void ReadBones(ContentReader input)
        {
            int numSkeletonBones = input.ReadInt32();
            List<SkinnedModelBone> skinnedModelBoneList = new List<SkinnedModelBone>(numSkeletonBones);

            // Read all bones
            for (int i = 0; i < numSkeletonBones; i++)
            {
                input.ReadSharedResource<SkinnedModelBone>(
                    delegate(SkinnedModelBone skinnedBone) { skinnedModelBoneList.Add(skinnedBone); });
            }

            // Create the skeleton
            skeleton = new SkinnedModelBoneCollection(skinnedModelBoneList);
        }

        private void ReadAnimations(ContentReader input)
        {
            int numAnimationClips = input.ReadInt32();
            Dictionary<string, AnimationClip> animationClipDictionary =
                new Dictionary<string, AnimationClip>();

            // Read all animation clips
            for (int i = 0; i < numAnimationClips; i++)
            {
                input.ReadSharedResource<AnimationClip>(
                    delegate(AnimationClip animationClip) { animationClipDictionary.Add(animationClip.Name, animationClip); });
            }

            animationClips = new AnimationClipDictionary(animationClipDictionary);
        }
    }
}