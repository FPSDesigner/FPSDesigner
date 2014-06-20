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
    class CPlayer
    {
        private CEnemy botController;
        public string userName;
        public int ID;

        private bool isCrouched = false;

        public CPlayer(int PlayerID, string Name, Vector3 pos)
        {
            this.userName = Name;
            this.ID = PlayerID;

            Texture2D[] textures = new Texture2D[1];
            textures[0] = Display2D.C2DEffect._content.Load<Texture2D>("Textures\\MedievalCharacter");
            botController = new CEnemy("MedievalCharacter", textures, pos, Matrix.CreateFromYawPitchRoll(0, 0, 0), 100f, 20, 10, false, Name, 2);
            Game.CEnemyManager.AddEnemy(Display2D.C2DEffect._content, CConsole._Camera, botController);
        }

        public void SetNewPos(Vector3 pos, Vector3 rot)
        {
            botController._position = pos;
            botController.rotationValue = rot.X;
        }

        public void Disconnect()
        {
            Game.CEnemyManager.RemoveBot(botController);
        }

        public void SetCrouched(bool toggle)
        {
            if (isCrouched != toggle)
            {
                isCrouched = toggle;
                botController.Crouch(toggle);
            }
        }

        public void SetJump(bool toggle)
        {
            if (toggle)
                botController.Jump();
        }

        public string CheckIntersects(Ray ray, out float? distance)
        {
            return botController.RayIntersectsHitbox(ray, out distance);
        }
    }
}
