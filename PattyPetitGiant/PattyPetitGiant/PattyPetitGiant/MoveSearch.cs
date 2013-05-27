﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    class MoveSearch : EnemyComponents
    {
        private int check_corners = 0;

        private float distance = 0.0f;
        private float angle = 0.0f;
        private float range_distance = 600.0f;
        private bool wall_between = false;
        private double sight_angle1 = 1.2;
        private double sight_angle2 = 1.8;

        private int change_direction;

        public void update(Enemy parent, GameTime currentTime, LevelState parentWorld)
        {
            return;
        }
        public void update(Enemy parent, Entity player, GameTime currentTime, LevelState parentWorld)
        {

            /*Search for player***********************************************************************************************************/
            angle = (float)(Math.Atan2(player.CenterPoint.Y - parent.CenterPoint.Y, player.CenterPoint.X - parent.CenterPoint.X));
            distance = Vector2.Distance(parent.CenterPoint, player.CenterPoint);
            wall_between = false;
            switch (parent.Direction_Facing)
            {
                case GlobalGameConstants.Direction.Right:
                    if ((angle > (-1 * sight_angle2 + Math.PI / 2) && angle < (-1 * sight_angle1 + Math.PI / 2)) && distance < range_distance)
                    {
                        wall_between = parentWorld.Map.playerInSight(angle, distance, parent, player);

                        if (!wall_between)
                        {
                            parent.Player_Found = true;
                        }
                    }
                    //((RangeEnemy)parent).Angle1 = (float)(-1 * sight_angle1 + Math.PI / 2);
                    //((RangeEnemy)parent).Angle2 = (float)(-1 * sight_angle2 + Math.PI / 2);
                    break;
                case GlobalGameConstants.Direction.Left:
                    if ((angle > (sight_angle1 + Math.PI / 2) && angle < (sight_angle2 + Math.PI / 2)) && distance < range_distance)
                    {
                        wall_between = parentWorld.Map.playerInSight(angle, distance, parent, player);

                        if (!wall_between)
                        {
                            parent.Player_Found = true;
                        }
                    }
                    //((RangeEnemy)parent).Angle1 = (float)(sight_angle1 + Math.PI / 2);
                    //((RangeEnemy)parent).Angle2 = (float)(sight_angle2 + Math.PI / 2);
                    break;
                case GlobalGameConstants.Direction.Up:
                    if ((angle > (-1 * sight_angle2) && angle < (-1 * sight_angle1)) && distance < range_distance)
                    {
                        wall_between = parentWorld.Map.playerInSight(angle, distance, parent, player);

                        if (!wall_between)
                        {
                            parent.Player_Found = true;
                        }

                    }
                    //((RangeEnemy)parent).Angle1 = (float)(-1 * sight_angle1);
                    //((RangeEnemy)parent).Angle2 = (float)(-1 * sight_angle2);
                    break;
                default:
                    if ((angle > sight_angle1 && angle < sight_angle2) && distance < range_distance)
                    {
                        wall_between = parentWorld.Map.playerInSight(angle, distance, parent, player);

                        if (!wall_between)
                        {
                            parent.Player_Found = true;
                        }
                    }
                    //((RangeEnemy)parent).Angle1 = (float)(sight_angle1);
                    //((RangeEnemy)parent).Angle2 = (float)(sight_angle2);
                    break;
            }
            //((RangeEnemy)parent).Angle = angle;
            /*End search for player ***************************************************************************************************************************/

            if (parent.Player_Found == false)
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

                        switch (parent.Direction_Facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                parent.Direction_Facing = GlobalGameConstants.Direction.Left;
                                parent.Velocity = new Vector2(-1.0f, 0.0f);
                                break;
                            case GlobalGameConstants.Direction.Left:
                                parent.Direction_Facing = GlobalGameConstants.Direction.Right;
                                parent.Velocity = new Vector2(1.0f, 0.0f);
                                break;
                            case GlobalGameConstants.Direction.Up:
                                parent.Direction_Facing = GlobalGameConstants.Direction.Down;
                                parent.Velocity = new Vector2(0.0f, 1.0f);
                                break;
                            default:
                                parent.Direction_Facing = GlobalGameConstants.Direction.Up;
                                parent.Velocity = new Vector2(0.0f, -1.0f);
                                break;
                        }
                        break;
                    }
                    on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                    check_corners++;
                }

                if (parent.Change_Direction_Time > 2000)
                {
                    change_direction = Game1.rand.Next(4);
                    //change_direction_time = 0.0f;
                    if (parent.Change_Direction_Time > 2300)
                    {
                        switch (change_direction)
                        {
                            case 0:
                                parent.Direction_Facing = GlobalGameConstants.Direction.Right;
                                parent.Velocity = new Vector2(1.0f, 0.0f);
                                break;
                            case 1:
                                parent.Direction_Facing = GlobalGameConstants.Direction.Left;
                                parent.Velocity = new Vector2(-1.0f, 0.0f);
                                break;
                            case 2:
                                parent.Direction_Facing = GlobalGameConstants.Direction.Up;
                                parent.Velocity = new Vector2(0.0f, -1.0f);
                                break;
                            default:
                                parent.Direction_Facing = GlobalGameConstants.Direction.Down;
                                parent.Velocity = new Vector2(0.0f, 1.0f);
                                break;
                        }
                        parent.Change_Direction_Time = 0.0f;
                    }
                    else
                    {
                        parent.Velocity = Vector2.Zero;
                    }
                }
            }

        }
    }
}