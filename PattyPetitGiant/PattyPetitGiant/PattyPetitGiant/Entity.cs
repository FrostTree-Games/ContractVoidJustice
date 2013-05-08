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

        public bool Disable_Movement { set { disable_movement = value; } }

        protected Vector2 position = Vector2.Zero;
        public Vector2 Position { get { return position; } }
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
        /// <param name="other">Entity that is either the player or the enemy depending on who is hitting who</param>
        /// <param name="position">center position of object that hits other is</param>
        /// <param name="dimensions">size of object that hits other is</param>
        public void knockBack(Entity other, Vector2 position, Vector2 dimensions, int damage)
        {
            //Enemy knocks back player
            if (other is Player)
            {
                Player player = (Player)other;
                Console.WriteLine(player.disable_movement_time);
                if (player.disable_movement_time == 0.0)
                {
                    player.disable_movement = true;
                    //temporary fix will consult dan to be sure.

                    Vector2 player_center = new Vector2(player.position.X + (player.Dimensions.X / 2), player.position.Y + (player.Dimensions.Y / 2));
                    Vector2 enemy_center = new Vector2(position.X + (dimensions.X / 2), position.Y + (dimensions.Y / 2));

                    player.velocity = player_center - enemy_center;

                    player.velocity.X = player.velocity.X / 10.0f;
                    player.velocity.Y = player.velocity.Y / 10.0f;

                    GlobalGameConstants.Player_Health = GlobalGameConstants.Player_Health - damage;
                }
            }
            //items knock back enemy
            else if (other is Enemy)
            {
                Enemy enemy = (Enemy)other;
                enemy.disable_movement = true;

                Console.WriteLine(enemy.disable_movement_time);

                Vector2 player_center = new Vector2(position.X + (dimensions.X / 2), position.Y + (dimensions.X / 2));
                Vector2 enemy_center = new Vector2(enemy.position.X + (enemy.dimensions.X / 2), enemy.position.Y + (enemy.dimensions.Y / 2));

                enemy.velocity = (enemy_center - player_center);
                enemy.velocity.X = enemy.velocity.X / 10.0f;
                enemy.velocity.Y = enemy.velocity.Y / 10.0f;

                enemy.Enemy_Life = enemy.Enemy_Life - damage;
            }
        }

        public abstract void update(GameTime currentTime);

        public abstract void draw(SpriteBatch sb);
    }

    public interface SpineEntity
    {
        void spinerender(SkeletonRenderer renderer);
    }
}
