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

        private float item_state_time = 0.0f;

        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.Melee;

        public GlobalGameConstants.itemType itemCheck
        {
            get { return item_type; }
        }

        public Sword(Vector2 initial_position)
        {
            position = initial_position;
            hitbox.X = 48.0f;
            hitbox.Y = 48.0f;
            item_state_time = 0.0f;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            position = parent.Position;
            item_direction = parent.Direction_Facing;

            item_state_time += currentTime.ElapsedGameTime.Milliseconds;

            if (item_state_time > 10)
            {
                parent.State = Player.playerState.Moving;
                item_state_time = 0.0f;
                parent.Disable_Movement = true;
            }

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
                        parent.knockBack(en, position, hitbox);
                    }
                }
            }
            
        }

        public void draw(SpriteBatch sb)
        {
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
