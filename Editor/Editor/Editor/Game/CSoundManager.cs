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
        public static Dictionary<string, CSound> soundList;

        public static Dictionary<string, CSound> soundArea;

        public static void LoadContent()
        {
            soundList = new Dictionary<string, CSound>();
        }

        public static void AddSound(string soundName, SoundEffect sound, bool isLooped, float delay, AudioListener listener = null, AudioEmitter emitter = null)
        {
            if (!soundList.ContainsKey(soundName))
                soundList.Add(soundName, new CSound(sound, isLooped, listener, emitter, delay));
        }

        // Play the normal sound
        public static void PlaySound(string soundName, float vol = 1f, float pitch = 0f, float pan = 0f)
        {
            if (soundList.ContainsKey(soundName))
            {
                if (Display3D.CWaterManager.isUnderWater)
                {
                    pitch = -1f; // Underwater effect
                    pan = 1f;
                }

                // We play the sound
                soundList[soundName]._sound.Play(vol = (vol <= 0f) ? 0f : vol, pitch = (pitch <= -1f) ? -1f : pitch, pan = (pan <= -1f) ? -1f : pan);
            }
        }

        // Play the instanciate sound
        public static void PlayInstance(string soundName, float vol = 1f, float pitch = 0f, float pan = 0f)
        {
            if (soundList.ContainsKey(soundName))
            {
                if (Display3D.CWaterManager.isUnderWater)
                {
                    pitch = -1f; // Underwater effect
                    pan = 1f;
                }

                // We change all the sound parameters
                soundList[soundName]._soundInstance.Volume = (vol == 1f) ? soundList[soundName]._soundInstance.Volume : vol;
                soundList[soundName]._soundInstance.Pitch = (vol == 0f) ? soundList[soundName]._soundInstance.Pitch : pitch;
                soundList[soundName]._soundInstance.Pan = (vol == 0f) ? soundList[soundName]._soundInstance.Pan : pan;

                // We play the sound
                soundList[soundName]._soundInstance.Play();
            }
        }

        // Put the instanciated sound in pause
        public static void PauseInstance(string soundName)
        {
            soundList[soundName]._soundInstance.Pause();
        }

        // Stop the instanciated sound
        public static void StopInstance(string soundName)
        {
            soundList[soundName]._soundInstance.Stop();
        }

    }

    #region CSound class
    public class CSound
    {
        public SoundEffect _sound;
        public SoundEffectInstance _soundInstance;
        public float _delay; // Used to play the song when after an elapsed time

        public AudioListener _audioListener;
        public AudioEmitter _audioEmitter;

        public CSound(SoundEffect sound, bool isLooped, AudioListener listener = null, AudioEmitter emitter = null, float delay = 0f, float volume = 1f, float pitch = 0f, float pan = 0f)
        {
            this._sound = sound;

            this._soundInstance = _sound.CreateInstance();
            _soundInstance.IsLooped = isLooped; // Only for sound instance

            this._delay = delay;

            this._soundInstance.Volume = volume;
            this._soundInstance.Pitch = pitch;
            this._soundInstance.Pan = pan;

            this._audioListener = listener;
            this._audioEmitter = emitter;

            // If we want to place the sound in the 3D world
            if (_audioEmitter != null && _audioListener != null)
            {
                _soundInstance.Apply3D(_audioListener, _audioEmitter);
            }

        }

    }
    #endregion

    #region "Sound Area Class"

    public struct SoundArea
    {
        private BoundingBox _area;

        private string _sound;

        private string _name;

        public SoundArea(BoundingBox Area, string Sound, string Name)
        {
            this._area = Area;
            this._name = Name;
            this._sound = Sound;
        }

    }

    #endregion
}
