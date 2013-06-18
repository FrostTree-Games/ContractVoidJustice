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
        private enum LazerState
        {
            Neutral,
            Charging,
            Fire,
            Reset
        }
        private LazerState lazer_state = LazerState.Neutral;
        private Vector2 dimensions = Vector2.Zero;
        private Vector2 position = Vector2.Zero;
        private GlobalGameConstants.Direction item_direction = GlobalGameConstants.Direction.Right;
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.LazerGun;

        private const float lazer_range_multiplier = 2.0f;
        private const float lazer_damage_multiplier = 1.015f;
        private const float max_lazer_range = 500.0f;
        private const float dimensions_reset = 10.0f;
        private const float damage_reset = 3.0f;
        private const float max_fire_timer = 300.0f;
        
        private float lazer_range = 0.0f;
        private float damage = 3.0f;
        private float knockback_magnitude = 3.0f;
        private float fire_timer = 0.0f;

        private Vector2 start_point = Vector2.Zero;

        public LazerGun()
        {
            dimensions = new Vector2(10, 10);
            position = Vector2.Zero;

            damage = 3;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            item_direction = parent.Direction_Facing;
            parent.Velocity = Vector2.Zero;
            switch (lazer_state)
            {
                case LazerState.Neutral:                    
                    if ((GameCampaign.Player_Item_1 == getEnumType() && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1)) || (GameCampaign.Player_Item_1 == getEnumType() && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1)))
                    {
                        switch (parent.Direction_Facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                position = parent.CenterPoint + new Vector2(parent.Dimensions.X / 2, 0);
                                start_point = position;
                                break;
                            case GlobalGameConstants.Direction.Left:
                                position = parent.CenterPoint - new Vector2(parent.Dimensions.X / 2, 0);
                                start_point = position;
                                break;
                            case GlobalGameConstants.Direction.Up:
                                position = parent.CenterPoint - new Vector2(0, parent.Dimensions.Y / 2);
                                start_point = position;
                                break;
                            default:
                                position = parent.CenterPoint + new Vector2(0, parent.Dimensions.Y / 2);
                                start_point = position;
                                break;
                        }
                        lazer_state = LazerState.Charging;
                    }
                    break;
                case LazerState.Charging:
                    if (lazer_range < 300)
                    {
                        switch (parent.Direction_Facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                dimensions += new Vector2(lazer_range_multiplier, 0);                                
                                break;
                            case GlobalGameConstants.Direction.Left:
                                dimensions += new Vector2(lazer_range_multiplier, 0);
                                position -= new Vector2(lazer_range_multiplier, 0);
                                break;
                            case GlobalGameConstants.Direction.Up:
                                dimensions += new Vector2(0, lazer_range_multiplier);
                                position -= new Vector2(0, lazer_range_multiplier);
                                break;
                            default:
                                dimensions += new Vector2(0, lazer_range_multiplier);
                                break;
                        }
                        lazer_range += lazer_range_multiplier;
                        damage = damage * lazer_damage_multiplier;
                    }

                    if (GameCampaign.Player_Item_1 == getEnumType() && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1))
                    {
                        lazer_state = LazerState.Fire;
                    }
                    break;
                case LazerState.Fire:
                    fire_timer += currentTime.ElapsedGameTime.Milliseconds;
                    foreach(Entity en in parentWorld.EntityList)
                    {
                        if (en == parent)
                            continue;
                        else if (en is Enemy)
                        {
                            if (hitTest(en))
                            {
                                Vector2 direction = en.CenterPoint - parent.CenterPoint;
                                en.knockBack(direction, knockback_magnitude, (int)damage, parent);
                            }
                        }
                    }

                    if(fire_timer>max_fire_timer)
                    {
                        lazer_state = LazerState.Reset;
                        fire_timer = 0.0f;
                    }
                    break;
                case LazerState.Reset:
                    dimensions = new Vector2(dimensions_reset, dimensions_reset);
                    position = Vector2.Zero;
                    damage = damage_reset;
                    lazer_state = LazerState.Neutral;
                    parent.State = Player.playerState.Moving;
                    lazer_range = 0.0f;
                    break;
                default:
                    break;
            }
        }
        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            return;
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
            sb.Draw(Game1.whitePixel, position, null, Color.Pink, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
        }

        public bool hitTest(Entity other)
        {
            if (position.X > other.Position.X + other.Dimensions.X || position.X + dimensions.X < other.Position.X || position.Y > other.Position.Y + other.Dimensions.Y || position.Y + dimensions.Y < other.Position.Y)
            {
                return false;
            }

            return true;
        }
    }
}
