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
    public class Keyframe 
    {  
        // Index of the bone this keyframe animates  
        [ContentSerializer]  public int Bone { get; private set; }
        // Time from the beginning of the animation of this keyframe  
        [ContentSerializer]  public TimeSpan Time { get; private set; }
        // Bone transform for this keyframe  
        [ContentSerializer]  public Matrix Transform { get; private set; }

        public Keyframe(int Bone, TimeSpan Time, Matrix Transform)  
        {    
            this.Bone = Bone;    this.Time = Time;
            this.Transform = Transform;
        }
        private Keyframe() { }
    }
}
