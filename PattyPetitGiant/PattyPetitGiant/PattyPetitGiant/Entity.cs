﻿using System;
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

        protected float horizontal_pos = 150.0f;
        protected float vertical_pos = 150.0f;

        protected Vector2 velocity = Vector2.Zero;
        protected Vector2 dimensions = Vector2.Zero;

        public Entity()
        {
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

        public virtual void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, new Vector2(horizontal_pos, vertical_pos), Color.White);
        }
    }
}
