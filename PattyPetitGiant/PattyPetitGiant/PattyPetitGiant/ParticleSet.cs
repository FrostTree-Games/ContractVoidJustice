using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    public class ParticleSet
    {
        public struct Particle
        {
            public bool active;
            public float timeAlive;
            public float maxTimeAlive;
            public Vector2 position;
            public Vector2 velocity;
            public Vector2 acceleration;
            public float rotation;
            public float rotationSpeed;
            public float animationTime;
            public AnimationLib.FrameAnimationSet animation;
            public Color color;
        }

        private const int particlePoolSize = 100;
        private Particle[] particlePool = null;

        public ParticleSet()
        {
            particlePool = new Particle[particlePoolSize];
        }

        public void update(GameTime currentTime)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (!particlePool[i].active) { continue; }

                particlePool[i].timeAlive += currentTime.ElapsedGameTime.Milliseconds;
                if (particlePool[i].timeAlive > particlePool[i].maxTimeAlive)
                {
                    particlePool[i].active = false;
                    continue;
                }

                float relativeAge = particlePool[i].timeAlive / particlePool[i].maxTimeAlive;
                particlePool[i].position = 0.5f * particlePool[i].acceleration * relativeAge * relativeAge + particlePool[i].velocity * relativeAge + particlePool[i].position;

                particlePool[i].rotation += particlePool[i].rotationSpeed * currentTime.ElapsedGameTime.Milliseconds;
                particlePool[i].animationTime += currentTime.ElapsedGameTime.Milliseconds;
            }
        }

        /// <summary>
        /// Draws all active particles to the desired SpriteBatch.
        /// </summary>
        /// <param name="sb">Graphics to draw to.</param>
        /// <param name="cameraPosition">Camera position vector.</param>
        /// <param name="depthOffset">Depth placement (good for GPU handling and all that)</param>
        public void draw(SpriteBatch sb, Vector2 cameraPosition, float depthOffset)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (!particlePool[i].active) { continue; }
                if (Vector2.Distance(cameraPosition, particlePool[i].position) > 750f) { continue; }

                particlePool[i].animation.drawAnimationFrame(particlePool[i].animationTime, sb, particlePool[i].position - cameraPosition + new Vector2(GlobalGameConstants.GameResolutionWidth, GlobalGameConstants.GameResolutionHeight) / 2, new Vector2(1), depthOffset, particlePool[i].rotation, particlePool[i].animation.FrameDimensions / 2, particlePool[i].color);
            }
        }

        public void pushParticleTEST(Vector2 position)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                particlePool[i].active = true;
                particlePool[i].timeAlive = 0;
                particlePool[i].maxTimeAlive = 1500f + (float)Game1.rand.NextDouble() * 200;
                particlePool[i].position = position;
                particlePool[i].rotation = (float)Game1.rand.NextDouble();
                particlePool[i].rotationSpeed = (float)Game1.rand.NextDouble() / 100;
                particlePool[i].velocity = new Vector2((float)Game1.rand.NextDouble() - 0.5f, (float)Game1.rand.NextDouble() - 0.5f) * 9;
                particlePool[i].acceleration = new Vector2((float)Game1.rand.NextDouble() - 0.5f, (float)Game1.rand.NextDouble() - 0.5f) * 9;
                particlePool[i].animation = AnimationLib.getFrameAnimationSet("gamepadA");
                particlePool[i].animationTime = 0;
                particlePool[i].color = new Color((float)Game1.rand.NextDouble(), (float)Game1.rand.NextDouble(), (float)Game1.rand.NextDouble());
            }
        }
    }
}
