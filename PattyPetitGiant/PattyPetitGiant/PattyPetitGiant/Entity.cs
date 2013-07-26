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
        public struct SecondaryHitBox
        {
            private Vector2 position;
            private Vector2 dimensions;
            public Vector2 Position { get { return position; } set { position = value; } }
            public Vector2 Dimensions { get { return dimensions; } set { dimensions = value; } }

            public SecondaryHitBox(Vector2 position, Vector2 dimensions)
            {
                this.position = position;
                this.dimensions = dimensions;
            }

            public static bool hitTestBoxEntity(SecondaryHitBox box, Entity other)
            {
                return !(box.Position.X > other.position.X + other.dimensions.X || box.Position.X + box.Dimensions.X < other.position.X || box.Position.Y > other.position.Y + other.dimensions.Y || box.Position.Y + box.Dimensions.Y < other.position.Y);
            }

            public static bool hitTestBoxWithBox(SecondaryHitBox boxA, SecondaryHitBox boxB)
            {
                return !(boxA.Position.X > boxB.Position.X + boxB.Dimensions.X || boxA.Position.X + boxA.Dimensions.X < boxB.Position.X || boxA.Position.Y > boxB.Position.Y + boxB.Dimensions.Y || boxA.Position.Y + boxA.Dimensions.Y < boxB.Position.Y);
            }
        }

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

        public enum EnemyType
        {
            NoType = 0,
            Guard = 1,
            Prisoner = 2,
            Alien = 4,
            Player = 8,
        }

        protected EnemyType enemy_type;
        public EnemyType Enemy_Type { get { return enemy_type; } }

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

        protected bool death = false;
        public bool Death { get { return death; } }

        protected SecondaryHitBox[] secondaryHitBoxes = null;
        public SecondaryHitBox[] SecondaryHitBoxes { get { return secondaryHitBoxes; } }
        public bool HasSecondaryHitBoxes { get { return !(secondaryHitBoxes == null); } }

        public bool hitTest(Entity other)
        {
            bool returnValue = false;

            returnValue |= !(position.X > other.position.X + other.dimensions.X || position.X + dimensions.X < other.position.X || position.Y > other.position.Y + other.dimensions.Y || position.Y + dimensions.Y < other.position.Y);

            if (HasSecondaryHitBoxes)
            {
                for (int j = 0; j < secondaryHitBoxes.Length; j++)
                {
                    returnValue |= SecondaryHitBox.hitTestBoxEntity(secondaryHitBoxes[j], other);
                }
            }

            if (other.HasSecondaryHitBoxes)
            {
                for (int i = 0; i < other.SecondaryHitBoxes.Length; i++)
                {
                    returnValue |= SecondaryHitBox.hitTestBoxEntity(other.SecondaryHitBoxes[i], this);

                    if (HasSecondaryHitBoxes)
                    {
                        for (int j = 0; j < secondaryHitBoxes.Length; j++)
                        {
                            returnValue |= SecondaryHitBox.hitTestBoxWithBox(secondaryHitBoxes[j], other.SecondaryHitBoxes[i]);
                        }
                    }
                }
            }

            return returnValue;
        }

        //checks the position of the entity getting knocked back with the current position
        /// <summary>
        /// Players and Enemies get knocked back a certain velocity away
        /// </summary>
        /// <param name="direction">direction you want to send the entity</param>
        /// <param name="magnitude">how much force the entity will send you</param>
        /// <param name="damage">how damage the entity does</param>
        public abstract void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker);

        public void knockBack(Vector2 direction, float magnitude, int damage)
        {
            knockBack(direction, magnitude, damage, null);
        }

        public abstract void update(GameTime currentTime);

        public abstract void draw(Spine.SkeletonRenderer sb);
    }

    public interface SpineEntity
    {
        void spinerender(SkeletonRenderer renderer);
    }
}
