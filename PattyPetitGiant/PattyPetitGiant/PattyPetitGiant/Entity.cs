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
        public Vector2 Dimensions { get { return dimensions; } }

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

        //checks the position of the entity getting knocked back with the current position
        public void knockBack(Entity other, Vector2 position, Vector2 dimensions)
        {
            //Enemy knocks back player
            if (other is Player)
            {
                other.disable_movement = true;

                if ((position.X + dimensions.X > other.position.X) && other.velocity.X > 0)
                {
                    //pushes left
                    other.velocity.X = -5.0f;
                }
                else if ((position.X < other.position.X + other.dimensions.X) && other.velocity.X < 0)
                {
                    //push right
                    other.velocity.X = 5.0f;
                }

                if ((position.Y < other.position.Y + other.dimensions.Y) && other.velocity.Y > 0)
                {
                    //pushes up
                    other.velocity.Y = -5.0f;
                }
                else if ((position.Y + dimensions.Y > other.position.Y) && other.velocity.Y < 0)
                {
                    //pushes down
                    other.velocity.Y = 5.0f;
                }
            }
                //items knock back enemy
            else if (other is Enemy)
            {
                other.disable_movement = true;

                if (position.X < other.position.X + other.dimensions.X && this.direction_facing == GlobalGameConstants.Direction.Left)
                {
                    //pushes left
                    other.velocity.X = -5.0f;
                }
                else if (position.X + dimensions.X > other.position.X && this.direction_facing == GlobalGameConstants.Direction.Right)
                {
                    //push right
                    other.velocity.X = 5.0f;
                }

                if (position.Y < other.position.Y + other.dimensions.Y && this.direction_facing == GlobalGameConstants.Direction.Up)
                {
                    //pushes up
                    other.velocity.Y = -5.0f;
                }
                else if (position.Y + dimensions.Y > other.position.Y && this.direction_facing == GlobalGameConstants.Direction.Down)
                {
                    //pushes down
                    other.velocity.Y = 5.0f;
                }
            }
        }

        public abstract void update(GameTime currentTime);

        public abstract void draw(SpriteBatch sb);
    }
}
