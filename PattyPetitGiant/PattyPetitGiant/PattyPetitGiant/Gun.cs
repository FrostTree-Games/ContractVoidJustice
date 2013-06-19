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
            private const float maxBulletTime = 1750f;

            private const float motionBulletSpeed = 0.5f;

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
                    return;
                }

                timePassed += currentTime.ElapsedGameTime.Milliseconds;

                if (timePassed > maxBulletTime || parentWorld.Map.hitTestWall(position))
                {
                    active = false;
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

        private GunBullet b;

        private GunState state;
        private float timer;
        private const float windUpTime = 125f;
        private const float timeBetweenShots = 300f;

        private const float gunAmmoCost = 2f;
        public float GunAmmoCost { get { return gunAmmoCost; } }

        public Gun()
        {
            b = new GunBullet(Vector2.Zero, 0.0f);
            b.active = false;

            timer = 9999;

            state = GunState.Idle;
        }

        private void updateBullets(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            b.update(parentWorld, currentTime, parent);
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            if (state == GunState.Idle)
            {
                timer += currentTime.ElapsedGameTime.Milliseconds;

                if (!b.active && timer > timeBetweenShots)
                {
                    state = GunState.WindUp;
                    timer = 0;

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

                if (GameCampaign.Player_Item_1 == "Gun")
                {
                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lPistol" : "rPistol");
                }
                else if (GameCampaign.Player_Item_2 == "Gun")
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
                float direction = (float)((int)(parent.Direction_Facing) * (Math.PI / 2));

                b = new GunBullet(parent.CenterPoint, direction);
                b.position = new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY);

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
            if (b.active)
            {
                sb.Draw(Game1.whitePixel, b.position - (new Vector2(b.Radius) / 2) , null, Color.Pink, 0.0f, Vector2.Zero, new Vector2(b.Radius), SpriteEffects.None, 0.5f);
            }
        }
    }
}
