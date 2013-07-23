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

            public void updateShot(GameTime currentTime, LevelState parentWorld)
            {
                if (!active) { return; }

                timeAlive += currentTime.ElapsedGameTime.Milliseconds;
                if (timeAlive > shotLifeTime)
                {
                    active = false;
                }

                centerPoint += currentTime.ElapsedGameTime.Milliseconds * velocity;
                parentWorld.Particles.pushDirectedParticle2(centerPoint, Color.Cyan, (float)(Game1.rand.NextDouble() * Math.PI * 2));
            }
        }

        private AnimationLib.FrameAnimationSet wandPic = null;

        private MagicalShot shot;

        private float delayTimer;
        private const float delayDuration = 200f;
        private const float reloadDuration = 500f;

        public WandOfGyges()
        {
            wandPic = AnimationLib.getFrameAnimationSet("wandProjectile");

            shot.active = false;

            delayTimer = 0;
        }

        private void updateShot(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            shot.updateShot(currentTime, parentWorld);

            if (parentWorld.Map.hitTestWall(shot.centerPoint))
            {
                shot.active = false;
            }
            else
            {
                for (int it = 0; it < parentWorld.EntityList.Count; it++)
                {
                    if (parentWorld.EntityList[it] is Player)
                    {
                        continue;
                    }

                    if (parentWorld.EntityList[it] is Enemy || parentWorld.EntityList[it] is ShopKeeper || parentWorld.EntityList[it] is Pickup || parentWorld.EntityList[it] is Key)
                    {
                        if (shot.hitTestEntity(parentWorld.EntityList[it]))
                        {
                            if (parentWorld.EntityList[it] is ShopKeeper)
                            {
                                ((ShopKeeper)parentWorld.EntityList[it]).poke();
                            }

                            Vector2 swap = parent.Position;
                            parent.Position = parentWorld.EntityList[it].Position;
                            parentWorld.EntityList[it].Position = swap;

                            shot.active = false;

                            if (parentWorld.EntityList[it] is AntiFairy)
                            {
                                ((AntiFairy)parentWorld.EntityList[it]).duplicate();
                            }
                        }
                    }
                }
            }
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            if (!shot.active && delayTimer > reloadDuration)
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
                delayTimer = 0;

                if (GameCampaign.Player_Item_1 == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1))
                {
                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lWand" : "rWand");
                }
                else if (GameCampaign.Player_Item_2 == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem2))
                {
                    parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rWand" : "lWand");
                }

                parent.LoopAnimation = false;
                parent.Animation_Time = 0;
                parent.Velocity = Vector2.Zero;
            }

            delayTimer += currentTime.ElapsedGameTime.Milliseconds;

            updateShot(parent, currentTime, parentWorld);

            if (delayTimer > delayDuration)
            {
                parent.Disable_Movement = false;
                parent.State = Player.playerState.Moving;
            }
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            delayTimer += currentTime.ElapsedGameTime.Milliseconds;

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

        public void draw(Spine.SkeletonRenderer sb)
        {
            if (shot.active)
            {
                //wandPic.drawAnimationFrame(0.0f, sb, shot.centerPoint, new Vector2(3.0f, 3.0f), 0.6f, shot.timeAlive, new Vector2(8.0f, 8.0f));
                wandPic.drawAnimationFrame(0.0f, sb, shot.centerPoint, new Vector2(1), 0.5f, shot.timeAlive, Vector2.Zero, Color.White);
            }
        }
    }
}
