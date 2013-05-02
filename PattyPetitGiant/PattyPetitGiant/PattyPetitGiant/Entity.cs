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

        protected bool disable_movement = false;
        protected float disable_movement_time = 0.0f;

        public bool Disable_Movement { set { disable_movement = value; } }

        protected Vector2 position = Vector2.Zero;
        public Vector2 Position { get { return position; } }
        public Vector2 CenterPoint { get { return new Vector2(position.X + width/2, position.Y + height/2); } }

        protected Vector2 velocity = Vector2.Zero;
        protected Vector2 dimensions = Vector2.Zero;

        protected LevelState parentWorld = null;

        protected GlobalGameConstants.Direction direction_facing = GlobalGameConstants.Direction.Right;
        public GlobalGameConstants.Direction Direction_Facing
        {
            get { return direction_facing; }
        }

        public bool hitTest(Entity other)
        {
            if (position.X > other.position.X + other.dimensions.X || position.X + dimensions.X < other.position.X || position.Y > other.position.Y + other.dimensions.Y || position.Y + dimensions.Y < other.position.Y)
            {
                return false;
            }

            return true;
        }

        public void knockBack(Entity other)
        {
            if (other is Player)
            {
                other.disable_movement = true;

                if (velocity.X > 0)
                {
                    velocity.X = -5.0f;
                }
                else if (velocity.X < 0)
                {
                    velocity.X = 5.0f;
                }

                if (velocity.Y > 0)
                {
                    velocity.Y = -5.0f;
                }
                else if (velocity.Y < 0)
                {
                    velocity.Y = 5.0f;
                }
            }
        }

        public abstract void update(GameTime currentTime);

        public abstract void draw(SpriteBatch sb);
    }
}
