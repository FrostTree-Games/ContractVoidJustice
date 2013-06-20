using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    class Gun : Item
    {
        private struct GunBullet
        {
            public bool active;

            public Vector2 position;
            private const float radius = 24f;
            public float Radius { get { return radius; } }
            public float direction;
            public float timePassed;
            private const float maxBulletTime = 1250;

            private const float motionBulletSpeed = 0.95f;

            public GunBullet(Vector2 position, float direction)
            {
                this.position = position;
                this.direction = direction;
                this.active = true;
                timePassed = 0.0f;
            }

            public bool hitTestEntity(Entity en)
            {
                return (Vector2.Distance(en.CenterPoint, position) < radius);
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

                //calculate directional velocity by taking d/dt of a predefined parametric path (Dan just did some calc on a rotation matrix and tweaked the values until it looked good)
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
                        en.knockBack(Vector2.Normalize(velocity), 1.5f, 4, parent);
                        this.active = false;
                        timePassed = 0;
                    }
                }
            }

            public void draw(SpriteBatch sb)
            {
                if (active)
                {
                    sb.Draw(Game1.whitePixel, position - new Vector2(radius / 2), null, Color.Green, 0, Vector2.Zero, radius, SpriteEffects.None, 0.69f);
                }
            }
        }

        private enum GunState
        {
            Idle = 0,
            WindUp = 1,
            Fire = 2,
        }

        private GunBullet[] b = null;

        private GunState state;
        private float timer;
        private const float windUpTime = 0;
        private const float timeBetweenShots = 0f;

        private const int numberOfOnScreenBullets = 10;

        private const float gunAmmoCost = 2f;
        public float GunAmmoCost { get { return gunAmmoCost; } }

        private void pushBullet(Vector2 position, float direction)
        {
            for (int i = 0; i < numberOfOnScreenBullets; i++)
            {
                if (!b[i].active)
                {
                    b[i] = new GunBullet(position, direction);
                    return;
                }
            }
        }

        public Gun()
        {
            b = new GunBullet[numberOfOnScreenBullets];

            timer = 9999;

            state = GunState.Idle;
        }

        private void updateBullets(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            for (int i = 0; i < numberOfOnScreenBullets; i++)
            {
                b[i].update(parentWorld, currentTime, parent);
            }
        }

        private bool anyBulletsFree()
        {
            bool aBulletIsFree = false;

            for (int i = 0; i < numberOfOnScreenBullets; i++)
            {
                aBulletIsFree = aBulletIsFree || (!b[i].active);
            }

            return aBulletIsFree;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            if (state == GunState.Idle)
            {
                timer += currentTime.ElapsedGameTime.Milliseconds;

                if (timer > timeBetweenShots)
                {
                    state = GunState.WindUp;
                    timer = 0;

                    pushBullet(new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY), (float)((int)(parent.Direction_Facing) * (Math.PI / 2)));

                    parent.Animation_Time = 0;
                }
                else
                {
                    parent.Disable_Movement = true;
                    parent.State = Player.playerState.Moving;
                }
            }
            else if (state == GunState.WindUp)
            {
                timer += currentTime.ElapsedGameTime.Milliseconds;

                if (GameCampaign.Player_Item_1 == "Gun" && parent.State == Player.playerState.Item1)
                {
                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lPistol" : "rPistol");
                }
                else if (GameCampaign.Player_Item_2 == "Gun" && parent.State == Player.playerState.Item2)
                {
                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rPistol" : "lPistol");
                }

                if (timer > windUpTime)
                {
                    state = GunState.Fire;
                }
            }
            else if (state == GunState.Fire)
            {
                timer = 0;
                state = GunState.Idle;

                parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation("idle");

                parent.State = Player.playerState.Moving;
                parent.Disable_Movement = true;
                parent.Velocity = Vector2.Zero;
            }
            else
            {
                throw new Exception("Gun placed into invalid state");
            }

            updateBullets(parent, currentTime, parentWorld);
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            updateBullets(parent, currentTime, parentWorld);

            if (state == GunState.Idle)
            {
                timer += currentTime.ElapsedGameTime.Milliseconds;
            }
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.Gun;
        }

        public string getEnumType()
        {
            return "Gun";
        }

        public void draw(SpriteBatch sb)
        {
            for (int i = 0; i < numberOfOnScreenBullets; i++)
            {
                if (b[i].active)
                {
                    sb.Draw(Game1.whitePixel, b[i].position - (new Vector2(b[i].Radius) / 2), null, Color.Pink, 0.0f, Vector2.Zero, new Vector2(b[i].Radius), SpriteEffects.None, 0.5f);
                }
            }
        }
    }
}
