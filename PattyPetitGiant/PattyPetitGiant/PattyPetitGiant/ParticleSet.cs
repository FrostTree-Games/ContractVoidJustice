using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    /// <summary>
    /// The particle system engine for PattyPetitGiant.
    /// </summary>
    public class ParticleSet
    {
        public enum ParticleType
        {
            Blood = 0,
        }

        private const float bloodInitialSpeed = 10f;
        private const float directedInitialSpeed = 10f;
        private const float flameInitalSpeed = 7.5f;

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
            public Vector2 scale;
            public bool isGib;
            public bool isCasing;
            public Vector2 originalPosition;

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
                p.scale = new Vector2(1);
                p.isGib = false;
                p.isCasing = false;
                p.originalPosition = p.position;

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
                p.scale = new Vector2(1);
                p.isGib = false;
                p.isCasing = false;
                p.originalPosition = p.position;
            }

            public static void NewDirectedParticle(ref Particle p, Vector2 position, Color c, float direction)
            {
                p.active = true;
                p.position = position;
                p.timeAlive = 0;
                p.maxTimeAlive = 500f + (float)Game1.rand.NextDouble() * 200f;
                p.rotation = (float)(Game1.rand.NextDouble() * Math.PI * 2);
                p.rotationSpeed = 0;
                p.animationTime = 0;
                p.animation = AnimationLib.getFrameAnimationSet("directedParticle");
                p.color = c;
                p.velocity = Vector2.Zero;
                p.acceleration = Vector2.Zero;
                p.scale = new Vector2(1);
                p.isGib = false;
                p.isCasing = false;
                p.originalPosition = p.position;

                p.velocity = new Vector2((float)(Math.Cos(direction)), (float)(Math.Sin(direction))) * bloodInitialSpeed;
                p.acceleration = Vector2.Zero;
            }

            public static void NewDirectedParticle2(ref Particle p, Vector2 position, Color c, float direction)
            {
                p.active = true;
                p.position = position;
                p.timeAlive = 0;
                p.maxTimeAlive = 500f + (float)Game1.rand.NextDouble() * 200f;
                p.rotation = direction;
                p.rotationSpeed = 0;
                p.animationTime = 0;
                p.animation = AnimationLib.getFrameAnimationSet("directedParticle");
                p.color = c;
                p.velocity = Vector2.Zero;
                p.acceleration = Vector2.Zero;
                p.scale = new Vector2(1);
                p.isGib = false;
                p.isCasing = false;
                p.originalPosition = p.position;

                p.velocity = new Vector2((float)(Math.Cos(direction)), (float)(Math.Sin(direction))) * bloodInitialSpeed;
                p.acceleration = Vector2.Zero;
            }

            public static void NewDotParticle(ref Particle p, Vector2 position, Color c, float direction)
            {
                p.active = true;
                p.position = position;
                p.timeAlive = 0;
                p.maxTimeAlive = 300 + (float)Game1.rand.NextDouble() * 200f;
                p.rotation = direction;
                p.rotationSpeed = 0;
                p.animationTime = 0;
                p.animation = AnimationLib.getFrameAnimationSet("dotParticle");
                p.color = c;
                p.velocity = Vector2.Zero;
                p.acceleration = Vector2.Zero;
                p.scale = new Vector2(1);
                p.isGib = false;
                p.isCasing = false;
                p.originalPosition = p.position;

                p.velocity = new Vector2((float)(Math.Cos(direction)), (float)(Math.Sin(direction))) * 1;
                p.acceleration = new Vector2(0, -5);
            }

            public static void NewDotParticle2(ref Particle p, Vector2 position, Color c, float direction, float speed)
            {
                p.active = true;
                p.position = position;
                p.timeAlive = 0;
                p.maxTimeAlive = 1000 + (float)Game1.rand.NextDouble() * 100f;
                p.rotation = direction;
                p.rotationSpeed = 0.1f;
                p.animationTime = 0;
                p.animation = AnimationLib.getFrameAnimationSet("dotParticle");
                p.color = c;
                p.velocity = Vector2.Zero;
                p.acceleration = Vector2.Zero;
                p.scale = new Vector2(1);
                p.isGib = false;
                p.isCasing = false;
                p.originalPosition = p.position;

                p.velocity = new Vector2((float)(Math.Cos(direction)), (float)(Math.Sin(direction))) * speed;
                p.acceleration = Vector2.Zero;
            }

            public static void NewPistolFlash(ref Particle p, Vector2 position, Color c, GlobalGameConstants.Direction direction)
            {
                p.active = true;
                p.position = position;
                p.timeAlive = 0;
                p.maxTimeAlive = 75;
                p.rotation = (int)direction * (float)(Math.PI / 2);
                p.rotationSpeed = 0;
                p.animationTime = 0;
                p.animation = AnimationLib.getFrameAnimationSet("pistolFlash");
                p.color = c;
                p.velocity = Vector2.Zero;
                p.acceleration = Vector2.Zero;
                p.scale = new Vector2(1);
                p.isGib = false;
                p.isCasing = false;
                p.originalPosition = p.position;

                switch (direction)
                {
                    case GlobalGameConstants.Direction.Right:
                        p.position -= new Vector2(0, 28);
                        break;
                    case GlobalGameConstants.Direction.Left:
                        p.position -= new Vector2(78, 28);
                        break;
                    case GlobalGameConstants.Direction.Down:
                        p.position -= new Vector2(46, 0);
                        break;
                    case GlobalGameConstants.Direction.Up:
                        p.position -= new Vector2(28, 58);
                        break;
                    default:
                        break;
                }
            }

            public static void NewFlame(ref Particle p, Vector2 position, Color c, float direction)
            {
                p.active = true;
                p.animation = AnimationLib.getFrameAnimationSet(Game1.rand.Next() % 3 == 0 ? "flame1" : (Game1.rand.Next() % 2 == 0 ? "flame2" : "flame3"));
                p.position = position - p.animation.FrameDimensions / 2;
                p.timeAlive = 0;
                p.maxTimeAlive = 500 + (float)(Game1.rand.NextDouble() * 200);
                p.rotation = direction;
                p.rotationSpeed = (float)(Game1.rand.NextDouble() * 0.01);
                p.animationTime = 0;
                p.color = c;
                float offset = (float)(Game1.rand.NextDouble() * 1.0f - 0.5f);
                p.velocity = new Vector2((float)Math.Cos(direction + offset), (float)Math.Sin(direction + offset)) * flameInitalSpeed;
                p.acceleration = Vector2.Zero;
                p.scale = new Vector2(0.7f);
                p.isGib = false;
                p.isCasing = false;
                p.originalPosition = p.position;
            }

            public static void NewGib(ref Particle p, Vector2 position)
            {
                p.active = true;
                p.animation = AnimationLib.getFrameAnimationSet(Game1.rand.Next() % 8 != 0 ? "GibSmallGeneric" : (Game1.rand.Next() % 2 == 0 ? "heartIdle" : "lungIdle"));
                p.position = position - (p.animation.FrameDimensions * 0.7f) / 2;
                p.timeAlive = 0;
                p.maxTimeAlive = 3000f;
                p.rotation = (float)(Game1.rand.NextDouble() * Math.PI * 2);
                p.rotationSpeed = (float)(Game1.rand.NextDouble() * 0.01);
                p.animationTime = 0;
                p.color = Color.White;
                float offset = (float)(Game1.rand.NextDouble() * 1.0f - 0.5f);
                p.velocity = new Vector2((float)(Game1.rand.NextDouble() * 150 - 75), -280f + (float)(Game1.rand.NextDouble() * 50));
                p.acceleration = new Vector2(0, 500);
                p.scale = new Vector2(0.7f);
                p.isGib = true;
                p.isCasing = false;
                p.originalPosition = p.position;
            }

            public static void NewExplosiveGib(ref Particle p, Vector2 position)
            {
                p.active = true;
                p.animation = AnimationLib.getFrameAnimationSet(Game1.rand.Next() % 8 != 0 ? "GibSmallGeneric" : (Game1.rand.Next() % 2 == 0 ? "heartIdle" : "lungIdle"));
                p.position = position - (p.animation.FrameDimensions * 0.7f) / 2;
                p.timeAlive = 0;
                p.maxTimeAlive = 200f;
                p.rotation = (float)(Game1.rand.NextDouble() * Math.PI * 2);
                p.rotationSpeed = (float)(Game1.rand.NextDouble() * 0.01);
                p.animationTime = 0;
                p.color = Color.White;
                float offset = (float)(Game1.rand.NextDouble() * 1.0f - 0.5f);

                p.velocity = new Vector2((float)((Game1.rand.NextDouble()-0.5) * 1000), 1000f *(float)(Game1.rand.NextDouble()- 0.5));

                if (p.velocity.X < 0.0f)
                    p.acceleration = new Vector2(500, 0);
                else
                    p.acceleration = new Vector2(-500, 0);
                p.scale = new Vector2(1.0f);
                p.isGib = true;
                p.isCasing = false;
                p.originalPosition = p.position + new Vector2(0, 360);
            }

            public static void newBulletCasing(ref Particle p, Vector2 position)
            {
                p.active = true;
                p.animation = AnimationLib.getFrameAnimationSet("casing");
                p.position = position;
                p.timeAlive = 0;
                p.maxTimeAlive = 3000f;
                p.rotation = (float)(Game1.rand.NextDouble() * Math.PI * 2);
                p.rotationSpeed = (float)(Game1.rand.NextDouble() * 0.01);
                p.animationTime = 0;
                p.color = Color.White;
                float offset = (float)(Game1.rand.NextDouble() * 1.0f - 0.5f);
                p.velocity = new Vector2((float)(Game1.rand.NextDouble() * 75 - 75), -280f + (float)(Game1.rand.NextDouble() * 50));
                p.acceleration = new Vector2(0, 500);
                p.scale = new Vector2(0.7f);
                p.isGib = false;
                p.isCasing = true;
                p.originalPosition = p.position + new Vector2(0, 32f);
            }

            public static void newShotGunCasing(ref Particle p, Vector2 position)
            {
                p.active = true;
                p.animation = AnimationLib.getFrameAnimationSet("casing");
                p.position = position - (p.animation.FrameDimensions * 0.7f) / 2;
                p.timeAlive = 0;
                p.maxTimeAlive = 3000f;
                p.rotation = (float)(Game1.rand.NextDouble() * Math.PI * 2);
                p.rotationSpeed = (float)(Game1.rand.NextDouble() * 0.01);
                p.animationTime = 0;
                p.color = Color.White;
                float offset = (float)(Game1.rand.NextDouble() * 1.0f - 0.5f);
                p.velocity = new Vector2((float)(Game1.rand.NextDouble() * 75 - 75), -280f + (float)(Game1.rand.NextDouble() * 50));
                p.acceleration = new Vector2(0, 700);
                p.scale = new Vector2(1.0f);
                p.isGib = false;
                p.isCasing = true;
                p.originalPosition = p.position + new Vector2(0, 32f);
            }

            public static void newRocketCasing(ref Particle p, Vector2 position)
            {
                p.active = true;
                p.animation = AnimationLib.getFrameAnimationSet("casing");
                p.position = position - (p.animation.FrameDimensions * 0.7f) / 2;
                p.timeAlive = 0;
                p.maxTimeAlive = 3000f;
                p.rotation = (float)(Game1.rand.NextDouble() * Math.PI * 2);
                p.rotationSpeed = (float)(Game1.rand.NextDouble() * 0.01);
                p.animationTime = 0;
                p.color = Color.White;
                float offset = (float)(Game1.rand.NextDouble() * 1.0f - 0.5f);
                p.velocity = new Vector2((float)(Game1.rand.NextDouble() * 75 - 75), -280f + (float)(Game1.rand.NextDouble() * 50));
                p.acceleration = new Vector2(0, 900);
                p.scale = new Vector2(10.0f);
                p.isGib = false;
                p.isCasing = true;
                p.originalPosition = p.position + new Vector2(0, 32f);
            }

            public static void NewContractDollarSign(ref Particle p, Vector2 position)
            {
                p.active = true;
                p.animation = AnimationLib.getFrameAnimationSet("contractParticle");
                p.position = position;
                p.timeAlive = 0;
                p.maxTimeAlive = 500f;
                p.rotation = 0;
                p.rotationSpeed = 0;
                p.animationTime = 0;
                p.color = Color.White;
                float randDir = (float)(Math.PI * 1.35 + Game1.rand.Next() * 0.15);
                float velo = (float)(-3.5 * Game1.rand.NextDouble());
                p.velocity = new Vector2((float)Math.Cos(randDir), (float)Math.Sin(randDir) + velo);
                p.acceleration = Vector2.Zero;
                p.scale = new Vector2(1);
                p.isGib = false;
                p.isCasing = false;
                p.originalPosition = p.position + new Vector2(0, 32f);
            }
        }

        private const int particlePoolSize = 300;
        private Particle[] particlePool = null;

        public ParticleSet()
        {
            particlePool = new Particle[particlePoolSize];
        }

        private void updateGib(GameTime currentTime, ref Particle p)
        {
            p.timeAlive += currentTime.ElapsedGameTime.Milliseconds;
            if (p.timeAlive > p.maxTimeAlive)
            {
                pushBloodParticle(p.position);
                if (Game1.rand.Next() % 4 == 0)
                {
                    pushBloodParticle(p.position);
                }

                p.active = false;
                return;
            }

            p.animationTime += currentTime.ElapsedGameTime.Milliseconds;

            p.position += p.velocity * (currentTime.ElapsedGameTime.Milliseconds / 1000f);

            if (p.velocity.Y > 0 && p.position.Y - p.originalPosition.Y > 0)
            {
                p.position -= p.velocity * (currentTime.ElapsedGameTime.Milliseconds / 1000f);
                p.velocity.Y *= -0.5f;
                p.velocity.X *= 0.8f;
                p.position += p.velocity * (currentTime.ElapsedGameTime.Milliseconds / 1000f);

                if (Game1.rand.Next() % 10 == 0)
                {
                    pushBloodParticle(p.position);
                }
            }

            p.velocity += p.acceleration * (currentTime.ElapsedGameTime.Milliseconds / 1000f);

            if (p.velocity.Length() < 30f && Math.Abs(p.position.Y - p.originalPosition.Y) < 2f)
            {
                p.velocity = Vector2.Zero;
                p.acceleration = Vector2.Zero;
            }
        }

        private void updateCasing(GameTime currentTime, ref Particle p)
        {
            p.timeAlive += currentTime.ElapsedGameTime.Milliseconds;
            if (p.timeAlive > p.maxTimeAlive)
            {
                p.active = false;
                return;
            }

            p.animationTime += currentTime.ElapsedGameTime.Milliseconds;

            p.position += p.velocity * (currentTime.ElapsedGameTime.Milliseconds / 1000f);

            if (p.velocity.Y > 0 && p.position.Y - p.originalPosition.Y > 0)
            {
                p.position -= p.velocity * (currentTime.ElapsedGameTime.Milliseconds / 1000f);
                p.velocity.Y *= -0.5f;
                p.velocity.X *= 0.8f;
                p.position += p.velocity * (currentTime.ElapsedGameTime.Milliseconds / 1000f);

            }

            p.velocity += p.acceleration * (currentTime.ElapsedGameTime.Milliseconds / 1000f);

            if (p.velocity.Length() < 30f && Math.Abs(p.position.Y - p.originalPosition.Y) < 2f)
            {
                p.velocity = Vector2.Zero;
                p.acceleration = Vector2.Zero;
            }
        }

        public void update(GameTime currentTime)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (!particlePool[i].active) { continue; }

                if (particlePool[i].isGib)
                {
                    updateGib(currentTime, ref particlePool[i]);

                    continue;
                }

                if (particlePool[i].isCasing)
                {
                    updateCasing(currentTime, ref particlePool[i]);
                    continue;
                }

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

                particlePool[i].animation.drawAnimationFrame(particlePool[i].animationTime, sb, particlePool[i].position - cameraPosition + new Vector2(GlobalGameConstants.GameResolutionWidth, GlobalGameConstants.GameResolutionHeight) / 2, particlePool[i].scale, depthOffset, particlePool[i].rotation, particlePool[i].animation.FrameDimensions / 2, particlePool[i].color);
            }
        }

        public void drawSpineSet(Spine.SkeletonRenderer sb, Vector2 cameraPosition, float depthOffset)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (!particlePool[i].active) { continue; }
                if (Vector2.Distance(cameraPosition, particlePool[i].position) > 750f) { continue; }

                particlePool[i].animation.drawAnimationFrame(particlePool[i].animationTime, sb, particlePool[i].position, particlePool[i].scale, depthOffset, particlePool[i].rotation, particlePool[i].animation.FrameDimensions / 2, particlePool[i].color);
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
                particlePool[i].rotationSpeed = (float)Game1.rand.NextDouble() / 100 - 50f;
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

        public void pushDirectedParticle(Vector2 position, Color color, float direction)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewDirectedParticle(ref particlePool[i], position, color, direction);
                return;
            }
        }

        public void pushDirectedParticle2(Vector2 position, Color color, float direction)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewDirectedParticle2(ref particlePool[i], position, color, direction);
                return;
            }
        }

        public void pushPistolFlash(Vector2 position, Color color, GlobalGameConstants.Direction direction)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewPistolFlash(ref particlePool[i], position, color, direction);
                return;
            }
        }

        public void pushFlame(Vector2 position, float direction)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewFlame(ref particlePool[i], position, Color.White, direction);
                return;
            }
        }

        public void pushDotParticle(Vector2 position, float direction, Color c)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewDotParticle(ref particlePool[i], position, c, direction);
                return;
            }
        }

        public void pushDotParticle2(Vector2 position, float direction, Color c, float speed)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewDotParticle2(ref particlePool[i], position, c, direction, speed);
                return;
            }
        }

        public void pushGib(Vector2 position)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewGib(ref particlePool[i], position);
                return;
            }
        }

        public void pushExplosiveGib(Vector2 position)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewExplosiveGib(ref particlePool[i], position);
                return;
            }
        }

        public void pushBulletCasing(Vector2 position)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.newShotGunCasing(ref particlePool[i], position);
                return;
            }
        }

        public void pushShotGunCasing(Vector2 position)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.newShotGunCasing(ref particlePool[i], position);
                return;
            }
        }

        public void pushRocketCasing(Vector2 position)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.newShotGunCasing(ref particlePool[i], position);
                return;
            }
        }

        public void pushContractParticle(Vector2 position)
        {
            for (int i = 0; i < particlePoolSize; i++)
            {
                if (particlePool[i].active) { continue; }

                Particle.NewContractDollarSign(ref particlePool[i], position);
                return;
            }
        }
    }
}
