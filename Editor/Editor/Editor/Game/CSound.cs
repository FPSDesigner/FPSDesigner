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
    class CSound : CSoundManager
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

        public override void Play(float vol = 1f, float pitch = 0f, float pan = 0f)
        {
            base.Play(vol, pitch, pan);
            _instance.Play((vol == 1f) ? Volume : vol, (pitch == 0f) ? Pitch : pitch, (pan == 0f) ? Pan : pan);
        }
    }
}
