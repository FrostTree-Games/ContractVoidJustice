using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    class Chase : EnemyComponents
    {
        public void update(Enemy parent,GameTime currentTime, LevelState parentWorld)
        {
            return;
        }
        public void update(Enemy parent, Entity player, GameTime currentTime, LevelState parentWorld)
        {
            Console.WriteLine("Inside the Chase Component");

            //distance in the X direction
            float direction_x = (player.Position.X + (parent.Dimensions.X/2.0f)) - parent.CenterPoint.X;
            float direction_y = (player.Position.Y + (parent.Dimensions.Y / 2.0f)) - parent.CenterPoint.Y;

            if (Math.Abs(direction_x) > Math.Abs(direction_y))
            {
                if (direction_x < 0)
                {
                    parent.Velocity = new Vector2(-1.51f, direction_y/100);
                }
                else
                {
                    parent.Velocity = new Vector2(1.51f, direction_y/100);
                }
            }
            else
            {
                if (direction_y < 0)
                {
                    parent.Velocity = new Vector2(direction_x / 100f, -1.51f);
                }
                else
                {
                    parent.Velocity = new Vector2(direction_x / 100f, 1.51f);
                }
            }

            Console.WriteLine("player center minus enemy Center: " + direction_x);
            
            
            Console.WriteLine("Enemy Velocity: " + parent.Velocity);
        }
    }
}
