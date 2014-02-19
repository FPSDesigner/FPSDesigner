using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Game
{
    class CSoundManager
    {
        public Display3D.CWater _water;

        public CSoundManager()
        {
        }

        public virtual void Play(float vol = 1f, float pitch = 0f, float pan = 0f)
        {
        }
    }
}
