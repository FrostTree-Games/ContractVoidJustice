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
    class Sword : Item
    {
        private Vector2 hitbox = Vector2.Zero;
        private Vector2 max_hitbox = Vector2.Zero;
        private Vector2 position = Vector2.Zero;
        private GlobalGameConstants.Direction item_direction = GlobalGameConstants.Direction.Right;
        private float max_item_state_time = 20.0f;
        private float item_state_time = 0.0f;
        private bool sword_swing = false;
        protected int sword_damage;

        public Sword(Vector2 initial_position)
        {
            position = initial_position;
            hitbox.X = 48.0f;
            hitbox.Y = 48.0f;
            item_state_time = 0.0f;
            sword_damage = 5;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            position = parent.Position;
            item_direction = parent.Direction_Facing;

            item_state_time += currentTime.ElapsedGameTime.Milliseconds;

            //sword is on the right hand side of the player, if hitboxes are different dimensions, need to adjust the position of sword.
            if (item_direction == GlobalGameConstants.Direction.Right)
            {
                position.X = parent.Position.X + parent.Dimensions.X;
               // position.Y = parent.Position.Y;
            }
            else if(item_direction == GlobalGameConstants.Direction.Left)
            {
                position.X = parent.Position.X - hitbox.X;
               // position.Y = parent.Position.Y;
            }
            else if (item_direction == GlobalGameConstants.Direction.Up)
            {
                position.Y = parent.Position.Y - hitbox.X;
                //position.X = parent.Position.X;
            }
            else
            {
                position.Y = parent.Position.Y + parent.Dimensions.Y;
                //position.X = parent.Position.X;
            }

            foreach (Entity en in parentWorld.EntityList)
            {
                if(en is Enemy)
                {
                    if (hitTest(en))
                    {
                        if (item_state_time > max_item_state_time)
                        {
                            parent.knockBack(en, parent.Position, parent.Dimensions, sword_damage);
                            item_state_time = 0.0f;
                        }
                    }
                }
            }
            sword_swing = true;
            if (item_state_time > max_item_state_time)
            {
                parent.State = Player.playerState.Moving;
                item_state_time = 0.0f;
                parent.Disable_Movement = true;
                sword_swing = false;
            }
        }

        public void daemonupdate(GameTime currentTime, LevelState parentWorld)
        {
            return;
        }

        public void draw(SpriteBatch sb)
        {
            if (sword_swing)
            {
                sb.Draw(Game1.whitePixel, position, null, Color.Pink, 0.0f, Vector2.Zero, hitbox, SpriteEffects.None, 0.5f);
            }
        }

        public bool hitTest(Entity other)
        {
            if (position.X > other.Position.X + other.Dimensions.X || position.X + hitbox.X < other.Position.X || position.Y > other.Position.Y + other.Dimensions.Y || position.Y + hitbox.Y < other.Position.Y)
            {
                return false;
            }

            return true;
        }
    }
}
