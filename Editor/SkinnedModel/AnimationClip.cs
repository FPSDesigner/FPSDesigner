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
    class AnimationClip
    {
        //total time of an anim
        [ContentSerializer]
        public TimeSpan _totalTime { get; private set; }

        //all the keys
        [ContentSerializer]
        public List<KeyFrame> _keyFrames{get ; private set;}

        public AnimationClip(TimeSpan totalTime, List<KeyFrame> keys)
        {
            this._totalTime = totalTime;
            this._keyFrames = keys;
        }

        private AnimationClip()
        {

        }
    }
}
