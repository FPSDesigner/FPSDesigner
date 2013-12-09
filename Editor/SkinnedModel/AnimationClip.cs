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
    public class AnimationClip
    {
        // Total length of the clip  
        [ContentSerializer]  public TimeSpan Duration { get; private set; }

        // List of keyframes for all bones, sorted by time  
        [ContentSerializer]  public List<Keyframe> Keyframes { get; private set; }

        public AnimationClip(TimeSpan Duration, List<Keyframe> Keyframes) 
        { 
            this.Duration = Duration; this.Keyframes = Keyframes; 
        }

        private AnimationClip() { } 
    }
}
