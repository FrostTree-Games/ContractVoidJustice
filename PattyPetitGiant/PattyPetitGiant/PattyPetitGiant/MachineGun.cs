using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class MachineGun : Item
    {
        private struct GunBullet
        {
            public bool active;

            public Vector2 position;
            public Vector2 dimensions;
            private const float radius = 8;
            public float Radius { get { return radius; } }
            public float direction;
            public float timePassed;
            private const float maxBulletTime = 700;

            private const float motionBulletSpeed = 1.20f;

            public GunBullet(Vector2 position, float direction)
            {
                this.position = position;
                dimensions = new Vector2(radius);
                this.direction = direction;
                this.active = true;
        
                timePassed = 0.0f;
            }

            public bool hitTestEntity(Entity other)
            {
                if (position.X > other.Position.X + other.Dimensions.X || position.X + dimensions.X < other.Position.X || position.Y > other.Position.Y + other.Dimensions.Y || position.Y + dimensions.Y < other.Position.Y)
                {
                    return false;
                }

                return true;
            }

            public void update(LevelState parentWorld, GameTime currentTime, Player parent)
            {
                if (!active)
                {
                    timePassed += currentTime.ElapsedGameTime.Milliseconds;
                    return;
                }

                timePassed += currentTime.ElapsedGameTime.Milliseconds;

                bool hitWall = false;
                if (timePassed > maxBulletTime || (hitWall = parentWorld.Map.hitTestWall(position)))
                {
                    if (hitWall)
                    {
                        parentWorld.Particles.pushImpactEffect(position - new Vector2(24), Color.White);
                    }

                    active = false;
                    timePassed = 0;
                    return;
                }

                Vector2 velocity = new Vector2((float)Math.Cos(direction), (float)Math.Sin(direction)) * motionBulletSpeed;

                position += velocity * currentTime.ElapsedGameTime.Milliseconds;

                for (int i = 0; i < parentWorld.EntityList.Count; i++)
                {
                    if (parentWorld.EntityList[i] is Player)
                    {
                        continue;
                    }

                    if (hitTestEntity(parentWorld.EntityList[i]))
                    {
                        parentWorld.EntityList[i].knockBack(Vector2.Normalize(velocity), 0.3f, 1, parent);
                        parentWorld.Particles.pushImpactEffect(position - new Vector2(24), Color.White);
                        this.active = false;
                        timePassed = 0;
                    }
                }
            }

            public void draw(Spine.SkeletonRenderer sb)
            {
                if (active)
                {
                    //sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), position, Color.Green, 0.0f, dimensions);
                    MachineGun.bulletPic.drawAnimationFrame(0.0f, sb, position - (MachineGun.bulletPic.FrameDimensions / 2), new Vector2(1.0f), 0.5f, direction, Vector2.Zero, Color.White);
                }
            }
        }

        private const int bulletCount = 100;
        private GunBullet[] bullets = null;

        private float fireTimer;
        private const float durationBetweenShots = 100;

        private const int ammo_consumption = 1;

        public const string machineGunSoundEffect = "machineGun";

        public static AnimationLib.FrameAnimationSet bulletPic = null;

        public MachineGun()
        {
            if (bulletPic == null)
            {
                bulletPic = AnimationLib.getFrameAnimationSet("testBullet");
            }

            bullets = new GunBullet[bulletCount];

            fireTimer = float.MaxValue;
        }

        private void pushBullet(Vector2 position, float direction)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                if (!bullets[i].active)
                {
                    bullets[i] = new GunBullet(position, direction + (float)(Math.PI / 9 * Game1.rand.NextDouble() - (Math.PI / 18)));
                    AudioLib.playSoundEffect(machineGunSoundEffect);
                    return;
                }
            }
        }

        private void updateBullets(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                bullets[i].update(parentWorld, currentTime, parent);
            }
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            updateBullets(parent, currentTime, parentWorld);
            if (GameCampaign.Player_Ammunition >= 1.0f)
            {
                if (GameCampaign.Player_Item_1 == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1))
                {
                    fireTimer += currentTime.ElapsedGameTime.Milliseconds;

                    if (fireTimer > durationBetweenShots)
                    {
                        GameCampaign.Player_Ammunition -= ammo_consumption;
                        fireTimer = 0;
                        parent.Animation_Time = 0;
                        pushBullet(new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY), (float)((int)(parent.Direction_Facing) * (Math.PI / 2)));
                        parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lMGun" : "rMGun");
                        parent.Velocity = Vector2.Zero;
                    }
                }
                else if (GameCampaign.Player_Item_2 == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem2))
                {
                    fireTimer += currentTime.ElapsedGameTime.Milliseconds;

                    if (fireTimer > durationBetweenShots)
                    {
                        GameCampaign.Player_Ammunition -= ammo_consumption;
                        fireTimer = 0;
                        parent.Animation_Time = 0;
                        pushBullet(new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rGunMuzzle" : "lGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rGunMuzzle" : "lGunMuzzle").WorldY), (float)((int)(parent.Direction_Facing) * (Math.PI / 2)));
                        parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rMGun" : "lMGun");
                        parent.Velocity = Vector2.Zero;
                        for (int i = 0; i < parentWorld.EntityList.Count; i++)
                        {
                            float distance = Vector2.Distance(parentWorld.EntityList[i].Position, new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY));
                            if (distance <= 600 && parentWorld.EntityList[i] is Enemy)
                            {
                                ((Enemy)parentWorld.EntityList[i]).Sound_Alert = true;
                                ((Enemy)parentWorld.EntityList[i]).Sound_Position = new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY);
                            }
                        }
                    }
                }
                else
                {
                    fireTimer = float.MaxValue;

                    parent.Disable_Movement = false;
                    parent.State = Player.playerState.Moving;

                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation("idle");
                }
            }
            else
            {
                parent.State = Player.playerState.Moving;
                return;
            }
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            updateBullets(parent, currentTime, parentWorld);
        }

        public void draw(Spine.SkeletonRenderer sb)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                bullets[i].draw(sb);
            }
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.MachineGun;
        }

        public string getEnumType()
        {
            return ItemType().ToString();
        }
    }
}
