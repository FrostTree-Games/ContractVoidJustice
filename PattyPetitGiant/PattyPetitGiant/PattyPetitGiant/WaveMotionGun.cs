using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class WaveMotionGun : Item
    {
        private enum WaveMotionState
        {
            InvalidState = -1,
            Wait = 0,
            Shooting = 1,
            Cooldown = 2,
        }

        // didn't want to use an array for garbage reasons
        private WaveMotionBullet bullet1;
        private WaveMotionBullet bullet2;
        private WaveMotionBullet bullet3;

        private float shotTime = 0.0f;
        private const float shotWaitTime = 0;

        private float timeBetweenShots = 0.0f;
        private const float timeBetweenShotsDelay = 1000f;

        private WaveMotionState state;

        public static AnimationLib.FrameAnimationSet bulletAnimation = null;

        public WaveMotionGun()
        {
            if (bulletAnimation == null)
            {
                bulletAnimation = AnimationLib.getFrameAnimationSet("waveMotionBullet");
            }

            bullet1.active = false;
            bullet2.active = false;
            bullet3.active = false;

            state = WaveMotionState.Wait;
        }

        private void updateBullets(LevelState parentWorld, GameTime currentTime)
        {
            timeBetweenShots += currentTime.ElapsedGameTime.Milliseconds;

            bullet1.update(parentWorld, currentTime);
            bullet2.update(parentWorld, currentTime);
            bullet3.update(parentWorld, currentTime);
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            updateBullets(parentWorld, currentTime);

            if (state == WaveMotionState.Wait)
            {
                shotTime = 0.0f;
                parent.Disable_Movement = true;
                parent.Velocity = Vector2.Zero;

                float shotDirection = 0.0f;

                switch (parent.Direction_Facing)
                {
                    case GlobalGameConstants.Direction.Up:
                        shotDirection = (float)Math.PI / -2;
                        break;
                    case GlobalGameConstants.Direction.Down:
                        shotDirection = (float)Math.PI / 2;
                        break;
                    case GlobalGameConstants.Direction.Left:
                        shotDirection = (float)Math.PI;
                        break;
                    case GlobalGameConstants.Direction.Right:
                        shotDirection = 0.0f;
                        break;
                }

                Vector2 bulletPos = Vector2.Zero;

                if (GameCampaign.Player_Item_1 == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1))
                {
                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ?"lRayGun" : "rRayGun");
                    bulletPos = new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY);
                }
                else if (GameCampaign.Player_Item_2 == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem2))
                {
                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rRayGun" : "lRayGun");
                    bulletPos = new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rGunMuzzle" : "lGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rGunMuzzle" : "lGunMuzzle").WorldY);
                }

                if (!bullet1.active)
                {
                    bullet1 = new WaveMotionBullet(bulletPos, shotDirection);
                }
                else if (!bullet2.active)
                {
                    bullet2 = new WaveMotionBullet(bulletPos, shotDirection);
                }
                else if (!bullet3.active)
                {
                    bullet3 = new WaveMotionBullet(bulletPos, shotDirection);
                }

                AudioLib.playSoundEffect("waveShot");

                state = WaveMotionState.Shooting;
                parent.Animation_Time = 0.0f;

                parent.LoopAnimation = false;
            }
            else if (state == WaveMotionState.Shooting)
            {
                shotTime += currentTime.ElapsedGameTime.Milliseconds;

                if (shotTime > shotWaitTime)
                {
                    parent.Disable_Movement = false;
                    parent.State = Player.playerState.Moving;

                    state = WaveMotionState.Wait;
                }
            }

            
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            updateBullets(parentWorld, currentTime);
        }

        public void draw(Spine.SkeletonRenderer sb)
        {
            bullet1.draw(sb);
            bullet2.draw(sb);
            bullet3.draw(sb);
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.WaveMotionGun;
        }

        public string getEnumType()
        {
            return "WaveMotionGun";
        }

        private struct WaveMotionBullet
        {
            public bool active;

            public Vector2 position;
            private const float radius = 24f;
            public float direction;
            public float timePassed;
            private const float maxWaveMotionBulletTime = 1750f;

            private const float motionBulletSpeed = 0.5f;

            private float colorSpinOffset;

            private Vector2 prevPosition1;
            private Vector2 prevPosition2;

            public WaveMotionBullet(Vector2 position, float direction)
            {
                this.position = position;
                this.prevPosition1 = position;
                this.prevPosition2 = position;
                this.direction = direction;
                this.active = true;
                timePassed = 0.0f;

                colorSpinOffset = (float)(Game1.rand.NextDouble() * 500);
            }

            public bool hitTestEntity(Entity en)
            {
                return (Vector2.Distance(en.CenterPoint, position) < radius);
            }

            public void update(LevelState parentWorld, GameTime currentTime)
            {
                if (!active)
                {
                    return;
                }

                timePassed += currentTime.ElapsedGameTime.Milliseconds;

                bool hitWall = false;
                if (timePassed > maxWaveMotionBulletTime || (hitWall = parentWorld.Map.hitTestWall(position)))
                {
                    if (hitWall)
                    {
                        AudioLib.playSoundEffect("waveHit");
                        parentWorld.Particles.pushImpactEffect(position, Color.Lerp(new Color(224, 255, 255, 127), new Color(0.0f, 0.0f, 1.0f, 0.5f), (float)(Math.Sin(timePassed / 1000f + colorSpinOffset))));
                    }

                    active = false;
                    return;
                }

                //calculate directional velocity by taking d/dt of a predefined parametric path (Dan just did some calc on a rotation matrix and tweaked the values until it looked good)
                Vector2 velocity = new Vector2((float)(Math.Cos(direction) - (15 * 2 * (Math.PI / 100) * Math.Sin(direction) * Math.Cos(2 * Math.PI / 13 * timePassed / 20))), (float)(Math.Sin(direction) + (15 * 2 * (Math.PI / 100) * Math.Cos(direction) * Math.Cos(2 * Math.PI / 13 * timePassed / 20))));

                position += motionBulletSpeed * velocity * currentTime.ElapsedGameTime.Milliseconds;

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is Player)
                    {
                        continue;
                    }

                    if (hitTestEntity(en))
                    {
                        en.knockBack(Vector2.Normalize(velocity), 1.5f, 4);
                        AudioLib.playSoundEffect("waveHit");
                        parentWorld.Particles.pushImpactEffect(position, Color.Lerp(new Color(224, 255, 255, 127), new Color(0.0f, 0.0f, 1.0f, 0.5f), (float)(Math.Sin(timePassed / 1000f + colorSpinOffset))));
                        this.active = false;
                    }
                }

                prevPosition2 = prevPosition1;
                prevPosition1 = position;
            }

            public void draw(Spine.SkeletonRenderer sb)
            {
                if (active)
                {
                    //sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), position - new Vector2(radius), Color.Red, 0.0f, new Vector2(radius / 2));
                    WaveMotionGun.bulletAnimation.drawAnimationFrame(timePassed, sb, prevPosition2 - new Vector2(radius), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.Lerp(Color.LightCyan, Color.Blue, (float)(Math.Sin(timePassed / 1000f + colorSpinOffset))));
                    WaveMotionGun.bulletAnimation.drawAnimationFrame(timePassed, sb, prevPosition1 - new Vector2(radius), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.Lerp(Color.LightCyan, Color.Blue, (float)(Math.Sin(timePassed / 1000f + colorSpinOffset))));
                    WaveMotionGun.bulletAnimation.drawAnimationFrame(timePassed, sb, position - new Vector2(radius), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.Lerp(Color.LightCyan, Color.Blue, (float)(Math.Sin(timePassed / 1000f + colorSpinOffset))));
                }
            }
        }
    }
}
