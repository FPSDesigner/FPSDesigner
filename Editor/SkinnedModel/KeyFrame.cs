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
    public class KeyFrame
    {
        //bone's index
        [ContentSerializer]
        public int _bone{get;set;}

        //Tranformations saved in a matrix
        [ContentSerializer]
        public Matrix _tranform { get; set; }

        // Time from the beginning of the animation of this keyframe  
        [ContentSerializer]  
        public TimeSpan _time { get; private set; }

        public KeyFrame(int bone, Matrix transform, TimeSpan time)
        {
            this._bone = bone;
            this._tranform = transform;
            this._time = time;
        }

        private KeyFrame()
        {

        }
 
    }
}
