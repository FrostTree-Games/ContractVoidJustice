using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using FuncWorks.XNA.XTiled;

namespace PattyPetitGiant
{
    public abstract class Entity
    {
        protected float width = 47.9f;
        protected float height = 47.9f;
        
        protected Vector2 position = Vector2.Zero; 

        protected Vector2 velocity = Vector2.Zero;
        protected Vector2 dimensions = Vector2.Zero;

        protected LevelState parentWorld = null;

        public bool hitTest(Entity other)
        {
            if (position.X > other.position.X + other.dimensions.X || position.X + dimensions.X < other.position.X || position.Y > other.position.Y + other.dimensions.Y || position.Y + dimensions.Y < other.position.Y)
            {
                return false;
            }

            return true;
        }

        public abstract void update(GameTime currentTime);

        public abstract void draw(SpriteBatch sb);
    }
}
