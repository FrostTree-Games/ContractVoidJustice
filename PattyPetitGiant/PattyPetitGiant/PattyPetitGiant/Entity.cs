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
    class Entity
    {
        public static List<Entity> level_entity_list = null;
        protected float width = 47.9f;
        protected float height = 47.9f;
        
        protected Vector2 position = Vector2.Zero; 

        protected Vector2 velocity = Vector2.Zero;
        protected Vector2 dimensions = Vector2.Zero;

        public Entity()
        {
        }

        public Entity(List<Entity> entity_list)
        {
            level_entity_list = entity_list;
        }

        protected void creation()
        {
            if (level_entity_list == null)
            {
                level_entity_list = new List<Entity>();
            }

            level_entity_list.Add(this);

        }

        public virtual void update(GameTime currentTime)
        {
            return;
        }

        public bool hitTest(Entity other)
        {
            if (position.X > other.position.X + other.dimensions.X || position.X + dimensions.X < other.position.X || position.Y > other.position.Y + other.dimensions.Y || position.Y + dimensions.Y < other.position.Y)
            {
                return false;
            }
            return true;
        }

        public virtual void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, new Vector2(position.X, position.Y), Color.White);
        }
    }
}
