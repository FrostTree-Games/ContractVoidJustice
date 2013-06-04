using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spine;
//using FuncWorks.XNA.XTiled;

namespace PattyPetitGiant
{
    public abstract class Entity
    {
        protected bool disable_movement = false;
        protected float disable_movement_time = 0.0f;

        public bool Disable_Movement 
        {
            get { return disable_movement; }
            set { disable_movement = value; } 
        }

        public float Disable_Movement_Time
        {
            get { return disable_movement_time; }
            set { disable_movement_time = value; } 
        }

        protected Vector2 position = Vector2.Zero;
        public Vector2 Position { get { return position; } set { position = value; } }
        public Vector2 CenterPoint { get { return new Vector2(position.X + dimensions.X/2, position.Y + dimensions.Y/2); } }

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

        protected bool remove_from_list = false;
        public bool Remove_From_List { get { return remove_from_list; } }

        public bool hitTest(Entity other)
        {
            if (position.X > other.position.X + other.dimensions.X || position.X + dimensions.X < other.position.X || position.Y > other.position.Y + other.dimensions.Y || position.Y + dimensions.Y < other.position.Y)
            {
                return false;
            }

            return true;
        }

        //checks the position of the entity getting knocked back with the current position
        /// <summary>
        /// Players and Enemies get knocked back a certain velocity away
        /// </summary>
        /// <param name="direction">direction you want to send the entity</param>
        /// <param name="magnitude">how much force the entity will send you</param>
        /// <param name="damage">how damage the entity does</param>
        public abstract void knockBack(Vector2 direction, float magnitude, int damage);

        public abstract void update(GameTime currentTime);

        public abstract void draw(SpriteBatch sb);
    }

    public interface SpineEntity
    {
        void spinerender(SkeletonRenderer renderer);
    }
}
