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
    class LazerGun : Item
    {
        
        private Vector2 position = Vector2.Zero;
        private GlobalGameConstants.Direction item_direction = GlobalGameConstants.Direction.Right;
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.LazerGun;

        private laserProjectile[] laser_projectile = new laserProjectile[3];

        private const float cool_down = 1500;
        private float fire_timer = 0.0f;

        private bool fire_projectile = true;

        private float ammo_consumption = 15;

        private AnimationLib.FrameAnimationSet laserAnim = AnimationLib.getFrameAnimationSet("laser");

        private AnimationLib.FrameAnimationSet bulletPic = AnimationLib.getFrameAnimationSet("testBullet");

        public LazerGun()
        {
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            for (int i = 0; i < laser_projectile.Count(); i++)
            {
                if (laser_projectile[i].active)
                    laser_projectile[i].update(parentWorld, currentTime, parent);
            }
            item_direction = parent.Direction_Facing;
            if (fire_projectile)
            {
                for (int i = 0; i < laser_projectile.Count(); i++)
                {
                    if (laser_projectile[i].active)
                        continue;
                    else
                    {
                        float bullet_angle = 0.0f;
                        if (item_direction == GlobalGameConstants.Direction.Right || item_direction == GlobalGameConstants.Direction.Left)
                        {
                            bullet_angle = 0.0f;
                        }
                        else
                        {
                            bullet_angle = (float)Math.PI / 2;
                        }

                        if ((GameCampaign.Player_Right_Item == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1)))
                        {
                            if ((parent.Index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Ammunition : GameCampaign.Player2_Ammunition) >= ammo_consumption)
                            {
                                AudioLib.playSoundEffect("lazorFire");
                                laser_projectile[i] = new laserProjectile(new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY), (parent.Direction_Facing == GlobalGameConstants.Direction.Right) ? new Vector2(10, 0) : (parent.Direction_Facing == GlobalGameConstants.Direction.Left) ? new Vector2(-10, 0) : (parent.Direction_Facing == GlobalGameConstants.Direction.Up) ? new Vector2(0, -10) : new Vector2(0, 10), bullet_angle);
                                if (parent.Index == InputDevice2.PPG_Player.Player_1)
                                {
                                    GameCampaign.Player_Ammunition -= ammo_consumption;
                                }
                                else
                                {
                                    GameCampaign.Player2_Ammunition -= ammo_consumption;
                                }
                            }
                        }
                        else if ((GameCampaign.Player_Left_Item == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem2)))
                        {
                            if ((parent.Index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Ammunition : GameCampaign.Player2_Ammunition) >= ammo_consumption)
                            {
                                AudioLib.playSoundEffect("lazorFire");
                                
                                laser_projectile[i] = new laserProjectile(new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rGunMuzzle" : "lGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "rGunMuzzle" : "lGunMuzzle").WorldY), (parent.Direction_Facing == GlobalGameConstants.Direction.Right) ? new Vector2(15, 0) : (parent.Direction_Facing == GlobalGameConstants.Direction.Left) ? new Vector2(-15, 0) : (parent.Direction_Facing == GlobalGameConstants.Direction.Up) ? new Vector2(0, -15) : new Vector2(0, 15), bullet_angle);
                                if (parent.Index == InputDevice2.PPG_Player.Player_1)
                                {
                                    GameCampaign.Player_Ammunition -= ammo_consumption;
                                }
                                else
                                {
                                    GameCampaign.Player2_Ammunition -= ammo_consumption;
                                }
                            }
                        }
                        parent.State = Player.playerState.Moving;
                        parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lLaser" : "rLaser");
                        fire_timer = 0.0f;
                        fire_projectile = false;
                        return;
                    }
                }
            }
            else
            {
                parent.State = Player.playerState.Moving;
            }
        }
        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            fire_timer += currentTime.ElapsedGameTime.Milliseconds;

            if (fire_timer > cool_down)
            {
                fire_projectile = true;
            }

            for (int i = 0; i < laser_projectile.Count(); i++)
            {
                if (laser_projectile[i].active)
                    laser_projectile[i].update(parentWorld, currentTime, parent);
            }
        }
        public GlobalGameConstants.itemType ItemType()
        {
            return item_type;
        }
        public string getEnumType()
        {
            return item_type.ToString();
        }
        public void draw(Spine.SkeletonRenderer sb)
        {
            
            for (int i = 0; i < laser_projectile.Count(); i++)
            {
                if (laser_projectile[i].active)
                {
                    //sb.DrawSpriteToSpineVertexArray(Game1.laserPic, new Rectangle(1, 1, 0, 0), laser_projectile[i].position, Color.White, 0.0f, laser_projectile[i].dimensions);
                    bulletPic.drawAnimationFrame(0.0f, sb, laser_projectile[i].position - bulletPic.FrameDimensions / 2, new Vector2(1.0f), 0.5f, laser_projectile[i].angle, Vector2.Zero, Color.Red);
                }
            }
        }

        private struct laserProjectile
        {
            public bool active;

            public Vector2 position;
            public Vector2 dimensions;
            private Vector2 velocity;
            private float time_passed;

            public float angle;

            public Vector2 nextStep_temp;
            public Vector2 CenterPoint
            {
                get { return position + (dimensions / 2); }
            }

            private float knockback_magnitude;
            private int bullet_damage;

            private const float max_bullet_time = 1000f;

            public laserProjectile(Vector2 position, Vector2 velocity, float angle)
            {
                this.position = position;
                dimensions = new Vector2(8, 8);
                active = true;
                this.velocity = velocity;

                knockback_magnitude = 10;
                bullet_damage = 25;
                time_passed = 0.0f;

                this.angle = angle;

                nextStep_temp = Vector2.Zero; 
            }

            public bool hitTestEntity(Entity other)
            {
                if (position.X > other.Position.X + other.Dimensions.X || position.X + dimensions.X < other.Position.X || position.Y > other.Position.Y + other.Dimensions.Y || position.Y + dimensions.Y < other.Position.Y)
                {
                    return false;
                }

                return true;
            }

            public void update(LevelState parentWorld, GameTime currentTime, Entity parent)
            {
                time_passed += currentTime.ElapsedGameTime.Milliseconds;

                for (int i = 0; i < parentWorld.EntityList.Count; i++)
                {
                    if (parentWorld.EntityList[i] == parent)
                        continue;
                    if (hitTestEntity(parentWorld.EntityList[i]))
                    {
                        Vector2 direction = parentWorld.EntityList[i].CenterPoint - position;
                        parentWorld.EntityList[i].knockBack(direction, knockback_magnitude, bullet_damage, parent);
                    }
                }

                if (time_passed > max_bullet_time)
                {
                    parentWorld.Particles.pushImpactEffect(position - new Vector2(24), Color.White);
                    time_passed = 0.0f;
                    active = false;
                }
                else
                {
                    nextStep_temp = new Vector2(position.X - (dimensions.X / 2) + velocity.X, (position.Y + velocity.X));
                }

                bool on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                int check_corners = 0;
                while (check_corners != 4)
                {
                    if (on_wall == false)
                    {
                        if (check_corners == 0)
                        {
                            nextStep_temp = new Vector2(position.X + (dimensions.X / 2) + velocity.X, position.Y + velocity.Y);
                        }
                        else if (check_corners == 1)
                        {
                            nextStep_temp = new Vector2(position.X + velocity.X, position.Y - (dimensions.Y / 2) + velocity.Y);
                        }
                        else if (check_corners == 2)
                        {
                            nextStep_temp = new Vector2(position.X + velocity.X, position.Y + dimensions.Y + velocity.Y);
                        }
                        else
                        {
                            position += velocity;
                        }
                        on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                    }
                    else
                    {
                        parentWorld.Particles.pushImpactEffect(position - new Vector2(24), Color.White);

                        active = false;
                        time_passed = 0.0f;
                    }
                    check_corners++;
                }
            }
        }
    }
}
