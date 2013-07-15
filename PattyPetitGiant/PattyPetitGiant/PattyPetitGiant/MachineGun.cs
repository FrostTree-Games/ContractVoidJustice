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

            private const float motionBulletSpeed = 0.80f;

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

                if (timePassed > maxBulletTime || parentWorld.Map.hitTestWall(position))
                {
                    active = false;
                    timePassed = 0;
                    return;
                }

                Vector2 velocity = new Vector2((float)Math.Cos(direction), (float)Math.Sin(direction)) * motionBulletSpeed;

                position += velocity * currentTime.ElapsedGameTime.Milliseconds;

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is Player)
                    {
                        continue;
                    }

                    if (hitTestEntity(en))
                    {
                        en.knockBack(Vector2.Normalize(velocity), 0.3f, 1, parent);
                        this.active = false;
                        timePassed = 0;
                    }
                }
            }

            public void draw(SpriteBatch sb)
            {
                if (active)
                {
                    sb.Draw(Game1.whitePixel, position, null, Color.Green, 0, Vector2.Zero, dimensions, SpriteEffects.None, 0.69f);
                }
            }
        }

        private const int bulletCount = 100;
        private GunBullet[] bullets = null;

        private float fireTimer;
        private const float durationBetweenShots = 100;

        public MachineGun()
        {
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

            if (GameCampaign.Player_Item_1 == ItemType() && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1))
            {
                fireTimer = float.MaxValue;

                parent.Disable_Movement = false;
                parent.State = Player.playerState.Moving;

                parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation("idle");
            }
            else if (GameCampaign.Player_Item_2 == ItemType() && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem2))
            {
                fireTimer = float.MaxValue;

                parent.Disable_Movement = false;
                parent.State = Player.playerState.Moving;

                parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation("idle");
            }
            else
            {
                fireTimer += currentTime.ElapsedGameTime.Milliseconds;

                if (fireTimer > durationBetweenShots)
                {
                    fireTimer = 0;
                    parent.Animation_Time = 0;
                    pushBullet(new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY), (float)((int)(parent.Direction_Facing) * (Math.PI / 2)));
                }

                if (GameCampaign.Player_Item_1 == ItemType())
                {
                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lMGun" : "rMGun");
                }
                else if (GameCampaign.Player_Item_2 == ItemType())
                {
                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rMGun" : "lMGun");
                }

                parent.Velocity = Vector2.Zero;
            }
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            updateBullets(parent, currentTime, parentWorld);
        }

        public void draw(SpriteBatch sb)
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
