﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    class FlameThrower : Item
    {
        private enum FlameThrowerState
        {
            Neutral,
            Fire,
            Reset
        }

        private FlameThrowerState flamethrower_state = FlameThrowerState.Neutral;
        private Vector2 position = Vector2.Zero;
        private Vector2 dimensions = new Vector2(48, 48);
        private float flame_timer = 0.0f;
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.FlameThrower;
        private GlobalGameConstants.Direction item_direction = GlobalGameConstants.Direction.Right;
        
        private const int max_flames = 10;
        private const float knockback_magnitude = 5.0f;
        private const float max_flame_range = 96.0f;

        private int damage;
        private float angle1;
        private float angle2;
        private Vector2 bound_point_1;
        private Vector2 bound_point_2;
        
        public FlameThrower()
        {
            dimensions = new Vector2(48, 48);
            position = Vector2.Zero;

            damage = 3;
        }

        public void update(Player parent, GameTime curentTime, LevelState parentWorld)
        {
            item_direction = parent.Direction_Facing;
            parent.Velocity = Vector2.Zero;
            switch (flamethrower_state)
            {
                case FlameThrowerState.Neutral:
                    if ((GameCampaign.Player_Item_1 == getEnumType() && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1)) || (GameCampaign.Player_Item_2 == getEnumType() && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem2)))
                    {
                        switch (parent.Direction_Facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                position = parent.CenterPoint + new Vector2(parent.Dimensions.X / 2, 0);
                                angle1 = (float)(-1 * Math.PI / 6);
                                angle2 = (float)(Math.PI / 6);
                                break;
                            case GlobalGameConstants.Direction.Left:
                                position = parent.CenterPoint - new Vector2(parent.Dimensions.X / 2, 0);
                                angle1 = (float)(1 * Math.PI / 1.2);
                                angle2 = (float)(-1 * Math.PI / 1.2);
                                break;
                            case GlobalGameConstants.Direction.Up:
                                position = parent.CenterPoint - new Vector2(0, parent.Dimensions.Y / 2);
                                angle1 = (float)(-1 * Math.PI / 1.5);
                                angle2 = (float)(-1 * Math.PI / 3);
                                break;
                            default:
                                position = parent.CenterPoint + new Vector2(0, parent.Dimensions.Y / 2);
                                angle1 = (float)(Math.PI / 3);
                                angle2 = (float)(Math.PI / 1.5);
                                break;
                        }
                    }
                    flamethrower_state = FlameThrowerState.Fire;
                    break;
                case FlameThrowerState.Fire:

                    /*
                     * bullet_timer += currentTime.ElapsedGameTime.Milliseconds;
                    firing_timer += currentTime.ElapsedGameTime.Milliseconds;
                    angle = (float)Math.Atan2(current_skeleton.Skeleton.FindBone("muzzle").WorldY - current_skeleton.Skeleton.FindBone("gun").WorldY, current_skeleton.Skeleton.FindBone("muzzle").WorldX - current_skeleton.Skeleton.FindBone("gun").WorldX);

                    if (bullet_count < max_bullet_count && bullet_timer>100)
                    {
                        bullets[bullet_count] = new SquadBullet(new Vector2(current_skeleton.Skeleton.FindBone("muzzle").WorldX, current_skeleton.Skeleton.FindBone("muzzle").WorldY));

                        bullets[bullet_count].velocity = new Vector2((float)(8.0*Math.Cos(angle)), (float)(8.0 * Math.Sin(angle)));
                        bullet_count++;
                        bullet_timer = 0.0f;
                    }
                    if (firing_timer > 3000)
                    {
                        reset_state_flag = true;
                        enemy_found = false;
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                        if (leader != null)
                        {
                            state = SquadSoldierState.Patrol;
                        }
                        else
                        {
                            state = SquadSoldierState.IndividualPatrol;
                        }
                    }
                    * */

                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en is Enemy)
                        {
                            if (hitTest(en))
                            {
                            }
                        }
                    }

                    if ((GameCampaign.Player_Item_1 == getEnumType() && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1)) || (GameCampaign.Player_Item_2 == getEnumType() && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem2)))
                    {
                        flamethrower_state = FlameThrowerState.Neutral;
                        parent.State = Player.playerState.Moving;
                    }                    
                    break;
                case FlameThrowerState.Reset:
                    break;
            }
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return item_type;
        }

        public string getEnumType()
        {
            return item_type.ToString();
        }

        public void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.White, angle1, Vector2.Zero, new Vector2(64.0f, 10.0f), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, position, null, Color.White, angle2, Vector2.Zero, new Vector2(64.0f, 10.0f), SpriteEffects.None, 0.5f);
        }

        public bool hitTest(Entity other)
        {
            int check_enemy_corners = 0;
            float angle_to_enemy = 0.0f;
            while (check_enemy_corners != 4)
            {
                if (check_enemy_corners == 0)
                {
                    angle_to_enemy = (float)(Math.Atan2(player.CenterPoint.Y - parent.CenterPoint.Y, player.CenterPoint.X - parent.CenterPoint.X));
                }
                else if (check_enemy_corners == 1)
                {
                }
                else if (check_enemy_corners == 2)
                {
                }
                else
                {
                }
                
            }

            return false;
            /*if (position.X > other.Position.X + other.Dimensions.X || position.X + dimensions.X < other.Position.X || position.Y > other.Position.Y + other.Dimensions.Y || position.Y + dimensions.Y < other.Position.Y)
            {
                return false;
            }

            return true;*/
        }
    }
}