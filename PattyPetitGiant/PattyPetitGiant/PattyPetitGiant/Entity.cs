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
        public Vector2 Velocity 
        {
            set { velocity = value; }
            get { return velocity; }
        }

        protected Vector2 dimensions = Vector2.Zero;
        public Vector2 Dimensions { get { return dimensions; } }

        protected LevelState parentWorld = null;

        protected GlobalGameConstants.Direction direction_facing = GlobalGameConstants.Direction.Right;
        public GlobalGameConstants.Direction Direction_Facing
        {
            set { direction_facing = value; }
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

                Vector2 player_center = new Vector2(other.position.X + (other.Dimensions.X / 2), other.position.Y + (other.Dimensions.Y / 2));
                Vector2 enemy_center = new Vector2(position.X + (dimensions.X/2), position.Y + (dimensions.Y/2));

                other.velocity = player_center - enemy_center;

                other.velocity.X = other.velocity.X / 10.0f;
                other.velocity.Y = other.velocity.Y / 10.0f;

                GlobalGameConstants.Player_Health = GlobalGameConstants.Player_Health - 1.0f;
            }
                //items knock back enemy
            else if (other is Enemy)
            {
                other.disable_movement = true;

                Vector2 player_center = new Vector2(position.X + (dimensions.X / 2), position.Y + (dimensions.X / 2));
                Vector2 enemy_center = new Vector2(other.position.X + (other.dimensions.X/2), other.position.Y + (other.dimensions.Y/2));

                other.velocity = (enemy_center - player_center);
                //Console.WriteLine(other.velocity);
                other.velocity.X = other.velocity.X / 10.0f;
                other.velocity.Y = other.velocity.Y / 10.0f;

                /*if (Math.Abs(enemy_center.X - player_center.X) > Math.Abs(enemy_center.Y - player_center.Y))
                {
                    if (other.velocity.X < 0)
                    {
                        other.velocity.X = -5.0f;
                    }
                    else
                    {
                        other.velocity.X = 5.0f;
                    }
                }
                else
                {
                    if (other.velocity.Y < 0)
                    {
                        other.velocity.Y = -5.0f;
                    }
                    else
                    {
                        other.velocity.Y = 5.0f;
                    }
                }*/
            }
        }

        public abstract void update(GameTime currentTime);

        public abstract void draw(SpriteBatch sb);
    }
}
