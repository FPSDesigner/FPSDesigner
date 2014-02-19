using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Editor.Game
{
    class CSound
    {
        private SoundEffect _instance;

        public float Volume;
        public float Pitch;
        public float Pan;
        
        public CSound(SoundEffect instance, float volume = 1f, float pitch = 0f, float pan = 0f)
        {
            _instance = instance;
            Volume = volume;
            Pitch = pitch;
            Pan = pan;
        }

        public void Play()
        {
            _instance.Play();
        }

        public void Play(Single x, Single y, Single z)
        {
            _instance.Play(Volume, Pitch, Pan);
        }
    }
}
