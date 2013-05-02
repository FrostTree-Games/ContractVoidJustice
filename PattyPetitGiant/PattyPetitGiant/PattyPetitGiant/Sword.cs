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
 
        public Sword(Vector2 initial_position)
        {
            position = initial_position;
            hitbox.X = 16.0f;
            hitbox.Y = 16.0f;
        }

        public void update(Player parent, GameTime currentTime)
        {
            /*foreach (Entity en in )
            {
            }*/
        }

        public void draw(SpriteBatch sb)
        {
        }
    }
}
