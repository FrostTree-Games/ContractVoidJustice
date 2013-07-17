using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    public class ParticleSet
    {
        public enum ParticleType
        {
            Blood = 0,
        }

        private const float bloodInitialSpeed = 10f;

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

            public static void NewBloodParticle(ref Particle p, Vector2 position)
            {
                p.active = true;
                p.timeAlive = 0;
                p.maxTimeAlive = 500f + (float)Game1.rand.NextDouble() * 200f;
                p.rotation = 0;
                p.rotationSpeed = 0;
                p.animationTime = 0;
                p.animation = AnimationLib.getFrameAnimationSet("bloodSpray");
                p.position = position;
                p.color = Color.White;

                float direction = (float)((-Math.PI * 3 / 8) - (Game1.rand.NextDouble() * Math.PI / 4));
                p.velocity = new Vector2((float)(Math.Cos(direction)), (float)(Math.Sin(direction))) * bloodInitialSpeed;
                p.acceleration = new Vector2((float)(Math.Cos(direction)), (float)(Math.Sin(direction)) * -13) * 2.5f;
            }

            public static void NewImpactEffect(ref Particle p, Vector2 position, Color c)
            {
                p.active = true;
                p.position = position;
                p.timeAlive = 0;
                p.maxTimeAlive = 500f + (float)Game1.rand.NextDouble() * 200f;
                p.rotation = (float)(Game1.rand.NextDouble() * Math.PI * 2);
                p.rotationSpeed = 0;
                p.animationTime = 0;
                p.animation = AnimationLib.getFrameAnimationSet("bulletImpact");
                p.color = c;
                p.velocity = Vector2.Zero;
                p.acceleration = Vector2.Zero;
            }
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

        public void drawSpineSet(Spine.SkeletonRenderer sb, Vector2 cameraPosition, float depthOffset)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (!particlePool[i].active) { continue; }
                if (Vector2.Distance(cameraPosition, particlePool[i].position) > 750f) { continue; }

                particlePool[i].animation.drawAnimationFrame(particlePool[i].animationTime, sb, particlePool[i].position, new Vector2(1), depthOffset, particlePool[i].rotation, particlePool[i].animation.FrameDimensions / 2, particlePool[i].color);
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

        public void pushBloodParticle(Vector2 position)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewBloodParticle(ref particlePool[i], position);
                return;
            }
        }

        public void pushImpactEffect(Vector2 position, Color color)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewImpactEffect(ref particlePool[i], position, color);
                return;
            }
        }
    }
}
