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

namespace Engine.Game
{
    static class CSoundManager
    {
        public static Display3D.CWater Water;

        public static Dictionary<string, CSound> soundList;

        #region CSound class
        public class CSound
        {
            public SoundEffect _instance;

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

            public void Play(float vol = 1f, float pitch = 0f, float pan = 0f)
            {
                _instance.Play((vol == 1f) ? Volume : vol, (pitch == 0f) ? Pitch : pitch, (pan == 0f) ? Pan : pan);
            }
        }
        #endregion

        public static void LoadContent(Display3D.CWater water)
        {
            Water = water;
            soundList = new Dictionary<string, CSound>();
        }

        public static void AddSound(string soundName, SoundEffect sound)
        {
            if (!soundList.ContainsKey(soundName))
                soundList.Add(soundName, new CSound(sound));
        }

        public static void Play(string soundName, float vol = 1f, float pitch = 0f, float pan = 0f)
        {
            if (soundList.ContainsKey(soundName))
            {
                if (Water.isUnderWater)
                {
                    pitch = -1f; // Underwater effect
                    pan = 1f;
                }

                soundList[soundName]._instance.Play((vol == 1f) ? soundList[soundName].Volume : vol, (pitch == 0f) ? soundList[soundName].Pitch : pitch, (pan == 0f) ? soundList[soundName].Pan : pan);
            }
        }
    }
}
