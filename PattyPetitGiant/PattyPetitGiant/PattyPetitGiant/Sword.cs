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
 
        public Sword(Vector2 initial_position)
        {
            position = initial_position;
            hitbox.X = 16.0f;
            hitbox.Y = 16.0f;
            item_state_time = 0.0f;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            item_state_time += currentTime.ElapsedGameTime.Milliseconds;
            
            position = parent.Position;
            item_direction = parent.Direction_Facing;

            Console.WriteLine("Direction Item Facing: " + item_direction);

            if(item_state_time > 400)
            {
                Console.WriteLine("state changes");
                parent.State = Player.playerState.Moving;
                item_state_time = 0.0f;
                parent.Disable_Movement = true;
            }
        }

        public void draw(SpriteBatch sb)
        {
        }
    }
}
