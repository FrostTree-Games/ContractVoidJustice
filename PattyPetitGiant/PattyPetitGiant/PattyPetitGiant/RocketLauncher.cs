using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spine;

namespace PattyPetitGiant
{
    class RocketLauncher : Item
    {
        private enum RocketLauncherState
        {
            InvalidState = -1,
            IdleWait = 0,
            WindUp = 1,
            Shooting = 2,
            CoolDown = 3,
        }

        private RocketLauncherState state;

        private Rocket rocket;
        private Explosion explosion;

        private GameTime lastUpdateTime = null;

        private float timer;
        private const float windUpDuration = 100f;
        private const float shootDuration = 500f;
        private const float coolDownDuration = 0;

        private static AnimationLib.FrameAnimationSet explosionAnim = null;

        private const string rocketSound = "testRocket";
        private const string explosionSound = "testExplosion";

        private static AnimationLib.FrameAnimationSet rocketSprite = null;

        /// <summary>
        /// Quick boolean hack to determine which hand to fire from.
        /// </summary>
        private bool slot1 = false;

        public RocketLauncher()
        {
            if (rocketSprite == null)
            {
                rocketSprite = AnimationLib.getFrameAnimationSet("rocketProjectile");
            }
            if (explosionAnim == null)
            {
                explosionAnim = AnimationLib.getFrameAnimationSet("rocketExplode");
            }

            rocket.active = false;

            state = RocketLauncherState.IdleWait;
        }

        private void updateRocketAndExplosion(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            if (rocket.active)
            {
                rocket.update(currentTime, parentWorld, this);
            }

            if (explosion.active)
            {
                explosion.update(currentTime, parentWorld, parent);
            }

            lastUpdateTime = currentTime;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            updateRocketAndExplosion(parent, currentTime, parentWorld);
            
            if (state == RocketLauncherState.IdleWait)
            {
                if (!rocket.active && !explosion.active)
                {
                    parent.Velocity = Vector2.Zero;

                    timer = 0;
                    state = RocketLauncherState.WindUp;

                    if (GameCampaign.Player_Item_1 == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1))
                    {
                        slot1 = true;
                        parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lRocket" : "rRocket");
                    }
                    else if (GameCampaign.Player_Item_2 == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem2))
                    {
                        slot1 = false;
                        parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rRocket" : "lRocket");
                    }

                    parent.Animation_Time = 0;
                    parent.LoopAnimation = false;
                }
                else
                {
                    parent.Disable_Movement = false;
                    parent.State = Player.playerState.Moving;
                }
            }
            else if (state == RocketLauncherState.WindUp)
            {
                timer += currentTime.ElapsedGameTime.Milliseconds;

                if (timer > windUpDuration)
                {
                    timer = 0;
                    state = RocketLauncherState.Shooting;

                    AudioLib.playSoundEffect(rocketSound);

                    if (slot1)
                    {
                        rocket = new Rocket(new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY), parent.Direction_Facing);
                    }
                    else
                    {
                        rocket = new Rocket(new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rGunMuzzle" : "lGunMuzzle").WorldY), parent.Direction_Facing);
                    }
                }
            }
            else if (state == RocketLauncherState.Shooting)
            {
                timer += currentTime.ElapsedGameTime.Milliseconds;

                if (timer > shootDuration)
                {
                    timer = 0;
                    state = RocketLauncherState.CoolDown;
                }
            }
            else if (state == RocketLauncherState.CoolDown)
            {
                parent.Disable_Movement = false;
                parent.State = Player.playerState.Moving;

                timer += currentTime.ElapsedGameTime.Milliseconds;

                if (timer > coolDownDuration)
                {
                    timer = 0;
                    state = RocketLauncherState.IdleWait;
                }
            }
            else if (state == RocketLauncherState.InvalidState)
            {
                throw new Exception("Invalid Rocket Launcher State");
            }
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            updateRocketAndExplosion(parent, currentTime, parentWorld);

            if (state == RocketLauncherState.CoolDown)
            {
                timer += currentTime.ElapsedGameTime.Milliseconds;

                if (timer > coolDownDuration)
                {
                    timer = 0;
                    state = RocketLauncherState.IdleWait;
                }
            }
        }

        public void draw(SkeletonRenderer sb)
        {
            if (rocket.active)
            {
                rocketSprite.drawAnimationFrame(rocket.timeAlive, sb, rocket.position - rocketSprite.FrameDimensions / 2, new Vector2(1), 0.5f, rocket.direction, Vector2.Zero, Color.White);
            }

            if (explosion.active)
            {
                explosionAnim.drawAnimationFrame(explosion.timeAlive, sb, explosion.centerPoint - explosionAnim.FrameDimensions * 0.75f, new Vector2(1), 0.5f, 0.0f, Vector2.Zero, Color.LightGoldenrodYellow);
            }
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.RocketLauncher;
        }

        public string getEnumType()
        {
            return "RocketLauncher";
        }

        public void popExplosion(Vector2 position)
        {
            AudioLib.playSoundEffect(explosionSound);
            explosion = new Explosion(position);
        }

        private struct Explosion
        {
            public bool active;

            public Vector2 position;
            public Vector2 dimensions;
            public Vector2 centerPoint { get { return position + (dimensions / 2); } }

            public float timeAlive;
            public const float maxTimeAlive = 700;

            public float animationTime;

            public Explosion(Vector2 position)
            {
                active = true;

                dimensions = GlobalGameConstants.TileSize * 3;
                this.position = position - (dimensions / 2);

                timeAlive = 0;

                animationTime = 0;
            }

            public bool hitTestWithEntity(Entity en)
            {
                if (position.X > en.Position.X + en.Dimensions.X || position.X + dimensions.X < en.Position.X || position.Y > en.Position.Y + en.Dimensions.Y || position.Y + dimensions.Y < en.Position.Y)
                {
                    return false;
                }

                return true;
            }

            public void update(GameTime currentTime, LevelState parentWorld, Player parent)
            {
                if (!active)
                {
                    return;
                }

                animationTime += currentTime.ElapsedGameTime.Milliseconds;

                timeAlive += currentTime.ElapsedGameTime.Milliseconds;
                if (timeAlive > maxTimeAlive)
                {
                    active = false;
                }

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (Vector2.Distance(en.CenterPoint, centerPoint) > 250)
                    {
                        continue;
                    }

                    if (hitTestWithEntity(en))
                    {
                        Vector2 direction = en.CenterPoint - centerPoint;
                        float rocket_knockback_power = Vector2.Distance(centerPoint, en.CenterPoint) / dimensions.X;
                        
                        if (rocket_knockback_power < 1 || float.IsNaN(rocket_knockback_power))
                        {
                            rocket_knockback_power = 1.0f;
                        }

                        float knockback_magnitude = 5 / rocket_knockback_power;
                                                                        

                        en.knockBack(en.CenterPoint - centerPoint, knockback_magnitude, 5, parent);
                    }
                }
            }
        }

        private struct Rocket
        {
            public bool active;

            public Vector2 position;
            public Vector2 dimensions;
            public Vector2 centerPoint { get { return position + (dimensions / 2); } }
            public Vector2 velocity;
            public float direction;
            private const float rocketSpeed = 1.0f;

            public float timeAlive;
            public const float maxTimeAlive = 2000;

            public float animationTime;

            public Rocket(Vector2 position, GlobalGameConstants.Direction direction)
            {
                active = true;

                this.position = position;
                this.direction = 0;
                dimensions = GlobalGameConstants.TileSize;

                timeAlive = 0;

                animationTime = 0;

                switch (direction)
                {
                    case GlobalGameConstants.Direction.Down:
                        velocity = new Vector2(0, rocketSpeed);
                        this.direction = (float)(Math.PI / 2);
                        break;
                    case GlobalGameConstants.Direction.Up:
                        velocity = new Vector2(0, -rocketSpeed);
                        this.direction = (float)(Math.PI / -2);
                        break;
                    case GlobalGameConstants.Direction.Left:
                        velocity = new Vector2(-rocketSpeed, 0);
                        this.direction = (float)(Math.PI);
                        break;
                    case GlobalGameConstants.Direction.Right:
                        velocity = new Vector2(rocketSpeed, 0);
                        this.direction = 0;
                        break;
                    default:
                        velocity = Vector2.Zero;
                        break;
                }
            }

            public bool hitTestWithEntity(Entity en)
            {
                if (position.X > en.Position.X + en.Dimensions.X || position.X + dimensions.X < en.Position.X || position.Y > en.Position.Y + en.Dimensions.Y || position.Y + dimensions.Y < en.Position.Y)
                {
                    return false;
                }

                return true;
            }

            public void update(GameTime currentTime, LevelState parentWorld, RocketLauncher launcherItem)
            {
                if (!active)
                {
                    return;
                }

                timeAlive += currentTime.ElapsedGameTime.Milliseconds;

                if (timeAlive > maxTimeAlive || parentWorld.Map.hitTestWall(centerPoint))
                {
                    launcherItem.popExplosion(centerPoint);
                    active = false;
                    return;
                }

                foreach (Entity en in parentWorld.EntityList)
                {
                    //this is a quick hack to prevent the rocket from exploding in the Player's face for now
                    if (en is Player)
                    {
                        continue;
                    }

                    if (Vector2.Distance(centerPoint, en.CenterPoint) < 150)
                    {
                        if (hitTestWithEntity(en))
                        {
                            launcherItem.popExplosion(centerPoint);
                            active = false;
                            return;
                        }
                    }
                }

                position += velocity * currentTime.ElapsedGameTime.Milliseconds;
            }
        }
    }
}
