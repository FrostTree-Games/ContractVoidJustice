using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class WandOfGyges : Item
    {
        private struct MagicalShot
        {
            public bool active;
            public Vector2 centerPoint;
            public Vector2 velocity;
            public float timeAlive;

            public const float radius = 24;
            public const float shotLifeTime = 500f;
            public const float shotVelocity = 0.75f;

            public MagicalShot(Vector2 centerPoint, float direction)
            {
                active = true;
                timeAlive = 0.0f;
                this.centerPoint = centerPoint;
                velocity = shotVelocity * new Vector2((float)Math.Cos(direction), (float)Math.Sin(direction));
            }

            public bool hitTestEntity(Entity other)
            {
                if (other == null || active == false) { return false; }

                return Vector2.Distance(centerPoint, other.CenterPoint) < MagicalShot.radius;
            }

            public void updateShot(GameTime currentTime)
            {
                if (!active) { return; }

                timeAlive += currentTime.ElapsedGameTime.Milliseconds;
                if (timeAlive > shotLifeTime)
                {
                    active = false;
                }

                centerPoint += currentTime.ElapsedGameTime.Milliseconds * velocity;
            }
        }

        private AnimationLib.FrameAnimationSet wandPic = null;

        private MagicalShot shot;

        public WandOfGyges()
        {
            wandPic = AnimationLib.getFrameAnimationSet("wandPic");

            shot.active = false;
        }

        private void updateShot(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            shot.updateShot(currentTime);

            if (parentWorld.Map.hitTestWall(shot.centerPoint))
            {
                shot.active = false;
            }
            else
            {
                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is Player)
                    {
                        continue;
                    }

                    if (en is Enemy || en is ShopKeeper || en is Pickup || en is Key)
                    {
                        if (shot.hitTestEntity(en))
                        {
                            if (en is ShopKeeper)
                            {
                                ((ShopKeeper)en).poke();
                            }

                            Vector2 swap = parent.Position;
                            parent.Position = en.Position;
                            en.Position = swap;

                            shot.active = false;

                            if (en is AntiFairy)
                            {
                                ((AntiFairy)en).duplicate();
                            }
                        }
                    }
                }
            }
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            if (!shot.active)
            {
                float direction = -1.0f;

                switch (parent.Direction_Facing)
                {
                    case GlobalGameConstants.Direction.Up:
                        direction = (float)(3 * Math.PI / 2);
                        break;
                    case GlobalGameConstants.Direction.Down:
                        direction = (float)(Math.PI / 2);
                        break;
                    case GlobalGameConstants.Direction.Left:
                        direction = (float)(Math.PI);
                        break;
                    case GlobalGameConstants.Direction.Right:
                        direction = 0.0f;
                        break;
                }

                shot = new MagicalShot(parent.CenterPoint, direction);
            }

            updateShot(parent, currentTime, parentWorld);

            parent.Disable_Movement = false;
            parent.State = Player.playerState.Moving;
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            updateShot(parent, currentTime, parentWorld);
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.WandOfGyges;
        }

        public string getEnumType()
        {
            return GlobalGameConstants.itemType.WandOfGyges.ToString();
        }

        public void draw(SpriteBatch sb)
        {
            if (shot.active)
            {
                wandPic.drawAnimationFrame(0.0f, sb, shot.centerPoint, new Vector2(3.0f, 3.0f), 0.6f, shot.timeAlive, new Vector2(8.0f, 8.0f));
            }
        }
    }
}
