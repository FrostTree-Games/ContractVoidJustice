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
    public class Entity
    {
        public static List<Entity> level_entity_list = null;
        protected float width = 47.9f;
        protected float height = 47.9f;

        protected float horizontal_pos = 150.0f;
        protected float vertical_pos = 150.0f;

        protected Vector2 Position; 

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
            if (horizontal_pos > other.horizontal_pos + other.dimensions.X || horizontal_pos + dimensions.X < other.horizontal_pos || vertical_pos > other.vertical_pos + other.dimensions.Y || vertical_pos + dimensions.Y < other.vertical_pos)
            {
                return false;
            }
            return true;
        }

        public virtual void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, new Vector2(horizontal_pos, vertical_pos), Color.White);
        }
    }
}
