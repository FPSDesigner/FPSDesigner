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

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0.2f;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 15, 0);

            settings.MinColor = Color.White;
            settings.MaxColor = Color.White;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 2;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
