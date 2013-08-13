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
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.FlameThrower;
        private GlobalGameConstants.Direction item_direction = GlobalGameConstants.Direction.Right;
        
        private const int max_flames = 10;
        private const float knockback_magnitude = 0.8f;
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
            switch (flamethrower_state)
            {
                case FlameThrowerState.Neutral:
                    if (((parent.Index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Right_Item : GameCampaign.Player2_Item_1) == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1)) || ((parent.Index == InputDevice2.PPG_Player.Player_1 ? GameCampaign.Player_Left_Item : GameCampaign.Player2_Item_2) == ItemType() && InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem2)))
                    {
                        position = new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY);
                        switch (parent.Direction_Facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                angle1 = (float)(-1 * Math.PI / 6);
                                angle2 = (float)(Math.PI / 6);
                                break;
                            case GlobalGameConstants.Direction.Left:
                                angle1 = (float)(1 * Math.PI / 1.2);
                                angle2 = (float)(-1 * Math.PI / 1.2);
                                break;
                            case GlobalGameConstants.Direction.Up:
                                angle1 = (float)(-1 * Math.PI / 1.5);
                                angle2 = (float)(-1 * Math.PI / 3);
                                break;
                            default:
                                angle1 = (float)(Math.PI / 3);
                                angle2 = (float)(Math.PI / 1.5);
                                break;
                        }
                    }

                    if ((parent.Index == InputDevice2.PPG_Player.Player_1) ? GameCampaign.Player_Ammunition >= 1 : GameCampaign.Player2_Ammunition >= 1)
                    {
                        flamethrower_state = FlameThrowerState.Fire;
                        parent.Velocity = Vector2.Zero;
                        parent.LoadAnimation.Animation = parent.LoadAnimation.Skeleton.Data.FindAnimation("idle");

                    }
                    else
                    {
                        parent.State = Player.playerState.Moving;
                        flamethrower_state = FlameThrowerState.Neutral;
                    }
                    break;
                case FlameThrowerState.Fire:
                    parentWorld.Particles.pushFlame(new Vector2(parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldX, parent.LoadAnimation.Skeleton.FindBone(parent.Direction_Facing == GlobalGameConstants.Direction.Left ? "lGunMuzzle" : "rGunMuzzle").WorldY), (float)((int)parent.Direction_Facing * Math.PI / 2));

                    if (parent.Index == InputDevice2.PPG_Player.Player_1)
                    {
                        GameCampaign.Player_Ammunition -= 1f;
                    }
                    else if (parent.Index == InputDevice2.PPG_Player.Player_2)
                    {
                        GameCampaign.Player2_Ammunition -= 1f;
                    }

                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en is Enemy)
                        {
                            if (hitTest(en))
                            {
                                Vector2 direction = en.CenterPoint - parent.CenterPoint;
                                en.knockBack(direction, knockback_magnitude, damage, parent);   
                            }
                        }
                    }

                    if (parent.Index == InputDevice2.PPG_Player.Player_1)
                    {
                        if ((GameCampaign.Player_Right_Item == ItemType() && !InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1)) || (GameCampaign.Player_Left_Item == ItemType() && !InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1)) || GameCampaign.Player_Ammunition < 1)
                        {
                            flamethrower_state = FlameThrowerState.Neutral;
                            parent.State = Player.playerState.Moving;
                        }
                    }
                    else if (parent.Index == InputDevice2.PPG_Player.Player_2)
                    {
                        if ((GameCampaign.Player2_Item_1 == ItemType() && !InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1)) || (GameCampaign.Player2_Item_2 == ItemType() && !InputDevice2.IsPlayerButtonDown(parent.Index, InputDevice2.PlayerButton.UseItem1)) || GameCampaign.Player2_Ammunition < 1)
                        {
                            flamethrower_state = FlameThrowerState.Neutral;
                            parent.State = Player.playerState.Moving;
                        }
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

        public void draw(Spine.SkeletonRenderer sb)
        {
            //sb.Draw(Game1.whitePixel, position, null, Color.White, angle1, Vector2.Zero, new Vector2(96.0f, 10.0f), SpriteEffects.None, 0.5f);
            //sb.Draw(Game1.whitePixel, position, null, Color.White, angle2, Vector2.Zero, new Vector2(96.0f, 10.0f), SpriteEffects.None, 0.5f);
        }

        public bool hitTest(Entity other)
        {
            int check_enemy_corners = 0;
            float angle_to_enemy = 0.0f;
            float distance_to_enemy = 0.0f;

            //checks to see if any of the entity's corners are touching the flame

            //need to write other check for left hand side
            while (check_enemy_corners != 4)
            {
                if (check_enemy_corners == 0)
                {
                    angle_to_enemy = (float)(Math.Atan2(other.Position.Y - position.Y, other.Position.X - position.X));
                    distance_to_enemy = Vector2.Distance(position, other.Position);

                    if (item_direction == GlobalGameConstants.Direction.Left)
                    {
                        if (((angle_to_enemy > angle1 && angle_to_enemy > 0) || (angle_to_enemy < angle2 && angle_to_enemy < 0)) && Math.Abs(distance_to_enemy) <= 96.0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (angle_to_enemy > angle1 && angle_to_enemy < angle2 && Math.Abs(distance_to_enemy) <= 96.0)
                        {
                            return true;
                        }
                    }
                }
                else if (check_enemy_corners == 1)
                {
                    angle_to_enemy = (float)(Math.Atan2(other.Position.Y - position.Y, (other.Position.X + other.Dimensions.X) - position.X));
                    distance_to_enemy = Vector2.Distance(position, other.Position + new Vector2(other.Dimensions.X, 0));
                    if (item_direction == GlobalGameConstants.Direction.Left)
                    {
                        if (((angle_to_enemy > angle1 && angle_to_enemy > 0) || (angle_to_enemy < angle2 && angle_to_enemy < 0)) && Math.Abs(distance_to_enemy) <= 96.0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (angle_to_enemy > angle1 && angle_to_enemy < angle2 && Math.Abs(distance_to_enemy) <= 96.0)
                        {
                            return true;
                        }
                    }
                }
                else if (check_enemy_corners == 2)
                {
                    angle_to_enemy = (float)(Math.Atan2((other.Position.Y + other.Dimensions.Y)- position.Y, other.Position.X - position.X));
                    distance_to_enemy = Vector2.Distance(position, other.Position + new Vector2(0, other.Dimensions.Y));
                    if (item_direction == GlobalGameConstants.Direction.Left)
                    {
                        if (((angle_to_enemy > angle1 && angle_to_enemy > 0) || (angle_to_enemy < angle2 && angle_to_enemy < 0)) && Math.Abs(distance_to_enemy) <= 96.0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (angle_to_enemy > angle1 && angle_to_enemy < angle2 && Math.Abs(distance_to_enemy) <= 96.0)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    angle_to_enemy = (float)(Math.Atan2((other.Position.Y + other.Dimensions.Y)- position.Y, (other.Position.X + other.Dimensions.X)- position.X));
                    distance_to_enemy = Vector2.Distance(position, other.Position + other.Dimensions);
                    if (item_direction == GlobalGameConstants.Direction.Left)
                    {
                        if (((angle_to_enemy > angle1 && angle_to_enemy > 0) || (angle_to_enemy < angle2 && angle_to_enemy < 0)) && Math.Abs(distance_to_enemy) <= 96.0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if ((angle_to_enemy > angle1 && angle_to_enemy < angle2) && Math.Abs(distance_to_enemy) <= 96.0)
                        {
                            return true;
                        }
                    }
                }
                check_enemy_corners++;
            }

            return false;
        }
    }
}
