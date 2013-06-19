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
            parent.Velocity = Vector2.Zero;
            switch (flamethrower_state)
            {
                case FlameThrowerState.Neutral:
                    if ((GameCampaign.Player_Item_1 == getEnumType() && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1)) || (GameCampaign.Player_Item_2 == getEnumType() && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem2)))
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
                    flamethrower_state = FlameThrowerState.Fire;
                    break;
                case FlameThrowerState.Fire:
                    
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
            sb.Draw(Game1.whitePixel, position, null, Color.White, angle1, Vector2.Zero, new Vector2(96.0f, 10.0f), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, position, null, Color.White, angle2, Vector2.Zero, new Vector2(96.0f, 10.0f), SpriteEffects.None, 0.5f);
        }

        public bool hitTest(Entity other)
        {
            int check_enemy_corners = 0;
            float angle_to_enemy = 0.0f;
            float distance_to_enemy = 0.0f;
            while (check_enemy_corners != 4)
            {
                if (check_enemy_corners == 0)
                {
                    angle_to_enemy = (float)(Math.Atan2(other.Position.Y - position.Y, other.Position.X - position.X));
                    distance_to_enemy = Vector2.Distance(position, other.Position);
                    if (angle_to_enemy > angle1 && angle_to_enemy < angle2 && Math.Abs(distance_to_enemy) <= 96.0)
                    {
                        return true;
                    }
                }
                else if (check_enemy_corners == 1)
                {
                    angle_to_enemy = (float)(Math.Atan2(other.Position.Y - position.Y, (other.Position.X + other.Dimensions.X) - position.X));
                    distance_to_enemy = Vector2.Distance(position, other.Position + new Vector2(other.Dimensions.X, 0));
                    if (angle_to_enemy > angle1 && angle_to_enemy < angle2 && Math.Abs(distance_to_enemy) <= 96.0)
                    {
                        return true;
                    }
                }
                else if (check_enemy_corners == 2)
                {
                    angle_to_enemy = (float)(Math.Atan2((other.Position.Y + other.Dimensions.Y)- position.Y, other.Position.X - position.X));
                    distance_to_enemy = Vector2.Distance(position, other.Position + new Vector2(0, other.Dimensions.Y));
                    if (angle_to_enemy > angle1 && angle_to_enemy < angle2 && Math.Abs(distance_to_enemy) <= 96.0)
                    {
                        return true;
                    }
                }
                else
                {
                    angle_to_enemy = (float)(Math.Atan2((other.Position.Y + other.Dimensions.Y)- position.Y, (other.Position.X + other.Dimensions.X)- position.X));
                    distance_to_enemy = Vector2.Distance(position, other.Position + other.Dimensions);
                    if (angle_to_enemy > angle1 || angle_to_enemy < angle2 && Math.Abs(distance_to_enemy) <= 96.0)
                    {
                        return true;
                    }
                }
                check_enemy_corners++;
            }

            return false;
        }
    }
}
