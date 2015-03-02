using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Display3D.Particles
{
    class ParticlesManager
    {
        private static Dictionary<string, ParticleSystem> particlesList;
        private static GraphicsDevice graphicsDevice;

        public static void LoadContent(GraphicsDevice graphics)
        {
            graphicsDevice = graphics;
            particlesList = new Dictionary<string, ParticleSystem>();
        }

        public static void AddNewParticle(string name, ParticleSystem particle, bool displayParticle = false, Vector3? position = null, Vector3? velocity = null, bool shouldRecreate = true)
        {
            if (!particlesList.ContainsKey(name))
            {
                particlesList.Add(name, particle);

                particlesList[name].Initialize();
                particlesList[name].LoadContent(graphicsDevice);

                particle.displayParticle = displayParticle;
                particle.constantRecreation = shouldRecreate;

                if (position != null)
                    particlesList[name].particlePosition = (Vector3)position;
                if (velocity != null)
                    particlesList[name].particleVelocity = (Vector3)velocity;
            }
        }

        public static void ToggleParticleDisplay(string particleName, bool display)
        {
            if (particlesList.ContainsKey(particleName))
            {
                particlesList[particleName].displayParticle = display;
            }
        }

        public static void SetParticlePosition(string particleName, Vector3 particlePosition)
        {
            if (particlesList.ContainsKey(particleName))
            {
                particlesList[particleName].particlePosition = particlePosition;
            }
        }

        public static void SetParticleVelocity(string particleName, Vector3 particleVelocity)
        {
            if (particlesList.ContainsKey(particleName))
            {
                particlesList[particleName].particleVelocity = particleVelocity;
            }
        }

        public static void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            foreach (KeyValuePair<string, ParticleSystem> particle in particlesList)
            {
                particle.Value.Draw(gameTime, view, projection);
            }
        }

        public static void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<string, ParticleSystem> particle in particlesList)
            {
                if (particle.Value.displayParticle)
                {
                    if (particle.Value.constantRecreation || particle.Value.numberOfCreation > 0)
                    {
                        particle.Value.AddParticle();
                        if (particle.Value.numberOfCreation > 0)
                            particle.Value.numberOfCreation--;
                    }

                    particle.Value.Update(gameTime);
                }
            }
        }

        public static void AddParticle(string particleName, Vector3? position = null, int amountOfParticles = 0)
        {
            if (particlesList.ContainsKey(particleName))
            {
                if (position != null)
                    SetParticlePosition(particleName, (Vector3)position);

                particlesList[particleName].numberOfCreation = amountOfParticles;
                particlesList[particleName].AddParticle();
            }
        }
    }
}
