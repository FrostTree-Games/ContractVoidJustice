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
 
        public Sword(Vector2 position)
        {

        }

        public void update(Player parent, GameTime currentTime)
        {
        }

        public void draw(SpriteBatch sb)
        {
        }
    }
}
