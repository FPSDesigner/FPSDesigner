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

namespace SkinnedModel
{
    public class SkinningData
    {
        // Gets a collection of animation clips, stored by name  
        [ContentSerializer]
        public Dictionary<string, AnimationClip> AnimationClips { get; private set; }

        // Bind pose matrices for each bone in the skeleton,    
        // relative to the parent bone.  
        [ContentSerializer]  public List<Matrix> BindPose { get; private set; }

        // Vertex to bonespace transforms for each bone in the skeleton.  
        [ContentSerializer]  public List<Matrix> InverseBindPose { get; private set; }

        // For each bone in the skeleton, stores the index of the parent bone.  
        [ContentSerializer]  public List<int> SkeletonHierarchy { get; private set; }

        public SkinningData(Dictionary<string, AnimationClip> animationClips, List<Matrix> bindPose, List<Matrix> inverseBindPose, List<int> skeletonHierarchy)
        { 
            AnimationClips = animationClips; BindPose = bindPose; InverseBindPose = inverseBindPose; SkeletonHierarchy = skeletonHierarchy; 
        }
        private SkinningData() { } 
    }
}
