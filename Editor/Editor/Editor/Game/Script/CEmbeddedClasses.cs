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

namespace Editor.Game.Script.Embedded
{
    // Convenction: C + type + Script + name. Ex: C2DScriptRectangle.


    // 2D Related
    class C2DScriptRectangle
    {
        public Rectangle Rectangle;
        public Texture2D Texture;
        public Color Color;

        public bool isActive;

        private int _drawOrder;
        public int drawOrder
        {
            get
            {
                return _drawOrder;
            }
            set
            {
                _drawOrder = value;
                Display2D.C2DEffect.ScriptableRectangle = Display2D.C2DEffect.ScriptableRectangle.OrderBy(ord => ord.drawOrder).ToList();
            }
        }

        public C2DScriptRectangle(Rectangle rect, Color color, bool active = true, int order = 1)
        {
            Texture = new Texture2D(Display2D.C2DEffect._graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Texture.SetData<Color>(new Color[] { color });

            Rectangle = rect;
            Color = color;
            isActive = active;
            _drawOrder = order;
        }
    }
}
