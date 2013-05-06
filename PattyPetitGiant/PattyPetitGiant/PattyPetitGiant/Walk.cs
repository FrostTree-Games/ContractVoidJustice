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
    class Walk : EnemyComponents
    {
        private int change_direction = 0;
        public void update(Enemy parent, GameTime currentTime, LevelState parentWorld)
        {
            int check_corners = 0;
            Vector2 nextStep_temp = new Vector2(parent.Position.X + parent.Velocity.X, parent.Position.Y + parent.Velocity.Y);
            bool on_wall = parentWorld.Map.hitTestWall(nextStep_temp);

            while (check_corners != 4)
            {
                if (on_wall != true)
                {
                    if (check_corners == 0)
                    {
                        nextStep_temp = new Vector2(parent.Position.X + parent.Dimensions.X + parent.Velocity.X, parent.Position.Y + parent.Velocity.Y);
                    }
                    else if (check_corners == 1)
                    {
                        nextStep_temp = new Vector2(parent.Position.X + parent.Velocity.X, parent.Position.Y + parent.Dimensions.Y + parent.Velocity.Y);
                    }
                    else if (check_corners == 2)
                    {
                        nextStep_temp = new Vector2(parent.Position.X + parent.Dimensions.X + parent.Velocity.X, parent.Position.Y + parent.Dimensions.Y + parent.Velocity.Y);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (parent.Direction_Facing == GlobalGameConstants.Direction.Right)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Left;
                        parent.Velocity = new Vector2(-1.0f, 0.0f);
                        break;
                    }
                    else if (parent.Direction_Facing == GlobalGameConstants.Direction.Left)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Right;
                        parent.Velocity = new Vector2(1.0f, 0.0f);
                        break;
                    }
                    else if (parent.Direction_Facing == GlobalGameConstants.Direction.Up)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Down;
                        parent.Velocity = new Vector2(0.0f, 1.0f);
                        break;
                    }
                    else if (parent.Direction_Facing == GlobalGameConstants.Direction.Down)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Up;
                        parent.Velocity = new Vector2(0.0f, -1.0f);
                        break;
                    }

                }
                on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                check_corners++;
            }

            if (parent.Change_Direction_Time > 1000)
            {
                Random rand = new Random();
                change_direction = rand.Next(4);
                parent.Change_Direction_Time = 0.0f;
                if (change_direction == 0)
                {
                    parent.Velocity = new Vector2( 1.0f, 0.0f);
                    parent.Direction_Facing = GlobalGameConstants.Direction.Right;
                }
                else if (change_direction == 1)
                {
                    parent.Velocity = new Vector2(-1.0f, 0.0f);
                    parent.Direction_Facing = GlobalGameConstants.Direction.Left;
                }
                else if (change_direction == 2)
                {
                    parent.Velocity = new Vector2(0.0f, -1.0f);
                    parent.Direction_Facing = GlobalGameConstants.Direction.Up;
                }
                else if (change_direction == 3)
                {
                    parent.Velocity = new Vector2(0.0f, 1.0f);
                    parent.Direction_Facing = GlobalGameConstants.Direction.Down;
                }
            }
        }
    }
}
