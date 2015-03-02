using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Engine.Display3D.Particles;

namespace Engine.Display3D.Particles.Elements
{
    /// <summary>
    /// Custom particle system for creating a flame effect.
    /// </summary>
    class GSDirtParticleSystem : ParticleSystem
    {
        public GSDirtParticleSystem(ContentManager content)
            : base(content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Particles/smoke";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromMilliseconds(200);

            settings.DurationRandomness = 1;

            settings.EndVelocity = 0;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0.2f;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 15, 0);

            settings.MinColor = new Color(131, 104, 64);
            settings.MaxColor = Color.White;

            settings.MinStartSize = 0.5f;
            settings.MaxStartSize = 1;

            settings.MinEndSize = 0.5f;
            settings.MaxEndSize = 1;

            settings.DelayBetweenParticles = 1;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
