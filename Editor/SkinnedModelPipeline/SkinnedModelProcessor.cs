using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using SkinnedModel;

// TODO: replace these with the processor input and output types.
using TInput = System.String;
using TOutput = System.String;

namespace SkinnedModelPipeline
{
    [ContentProcessor]
    public class SkinnedModelProcessor : ModelProcessor 
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context) 
        {
            // Find the skeleton. 
            BoneContent skeleton = MeshHelper.FindSkeleton(input);
            // Read the bind pose and skeleton hierarchy data. 
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> inverseBindPose = new List<Matrix>();
            List<int> skeletonHierarchy = new List<int>();
            // Extract the bind pose transform, inverse bind pose transform, 
            // and parent bone index of each bone in order 
            foreach (BoneContent bone in bones)
            {
                bindPose.Add(bone.Transform);
                inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
            }

            // Convert animation data to our runtime format. 
            Dictionary<string, AnimationClip> animationClips; animationClips = ProcessAnimations(skeleton.Animations, bones);

            // Chain to the base ModelProcessor class so it can convert the model data. 
            ModelContent model = base.Process(input, context);
            // Store our custom animation data in the Tag property of the model. 
            model.Tag = new SkinningData(animationClips, bindPose, inverseBindPose,     skeletonHierarchy);

            return model; 
        }

        static Dictionary<string, AnimationClip> ProcessAnimations(AnimationContentDictionary animations, IList<BoneContent> bones)
        {
            // Build up a table mapping bone names to indices.  
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
                boneMap.Add(bones[i].Name, i);
            Dictionary<string, AnimationClip> animationClips = new Dictionary<string, AnimationClip>();
            // Convert each animation
            foreach (KeyValuePair<string, AnimationContent> animation in animations)
            {
                AnimationClip processed = ProcessAnimation(animation.Value, boneMap);
                animationClips.Add(animation.Key, processed);
            }

            return animationClips;
        }

        static AnimationClip ProcessAnimation(AnimationContent animation, Dictionary<string, int> boneMap)
        {
            List<Keyframe> keyframes = new List<Keyframe>();
            // For each input animation channel.  
            foreach (KeyValuePair<string, AnimationChannel> channel in animation.Channels)  
            {    
                // Look up what bone this channel is controlling.     
                int boneIndex = boneMap[channel.Key];                     
                // Convert the keyframe data.     
                foreach (AnimationKeyframe keyframe in channel.Value)        
                    keyframes.Add(new Keyframe(boneIndex, keyframe.Time,keyframe.Transform));  
            }

            // Sort the merged keyframes by time.  keyframes.Sort(CompareKeyframeTimes);
            return new AnimationClip(animation.Duration, keyframes);
        }

        static int CompareKeyframeTimes(Keyframe a, Keyframe b) 
        { 
            return a.Time.CompareTo(b.Time); 
        }

    }
}