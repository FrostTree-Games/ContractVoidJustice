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
    class IdleSearch : EnemyComponents
    {
        private float distance = 0.0f;
        private float angle = 0.0f;
        private float range_distance = 600.0f;
        private bool player_in_sight = false;
        private double sight_angle1 = 1.2;
        private double sight_angle2 = 1.8;

        public void update(Enemy parent, GameTime currentTime, LevelState parentWorld)
        {
            return;
        }
        public void update(Enemy parent, Entity player, GameTime currentTime, LevelState parentWorld)
        {
            angle = (float)(Math.Atan2(parent.CenterPoint.Y - player.CenterPoint.Y, player.CenterPoint.X - parent.CenterPoint.X));
            angle = -1* angle;

            Console.WriteLine("player angle: " + angle);
            distance = Vector2.Distance(parent.CenterPoint, player.CenterPoint);
                
            switch (parent.Direction_Facing)
            {
                case GlobalGameConstants.Direction.Right:
                    if ((angle > (-1 * sight_angle2 + Math.PI / 2) && angle < (-1 * sight_angle1 + Math.PI / 2)) && distance < range_distance)
                    {
                        /*player_in_sight = parentWorld.Map.playerInSight(angle, distance, parent.CenterPoint);
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        if(!player_in_sight)
                        {
                            Console.WriteLine("player right in view");
                            parent.State = Enemy.EnemyState.Chase;
                        }*/
                    }
                    ((IdleChaseEnemy)parent).Angle1 = (float)(-1 * sight_angle1 + Math.PI / 2);
                    ((IdleChaseEnemy)parent).Angle2 = (float)(-1 * sight_angle2 + Math.PI / 2);
                    break;
                case GlobalGameConstants.Direction.Left:
                    if ((angle > (sight_angle1 + Math.PI / 2) && angle < (sight_angle2 + Math.PI / 2)) && distance < range_distance)
                    {
                        /*player_in_sight = parentWorld.Map.playerInSight(angle, distance, parent.CenterPoint);
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        if (!player_in_sight)
                        {
                            Console.WriteLine("player left in view");
                            parent.State = Enemy.EnemyState.Chase;
                        }*/
                    }
                    ((IdleChaseEnemy)parent).Angle1 = (float)(sight_angle1 + Math.PI / 2);
                    ((IdleChaseEnemy)parent).Angle2 = (float)(sight_angle2 + Math.PI / 2);
                    break;
                case GlobalGameConstants.Direction.Up:
                    if ((angle > (-1*sight_angle2) && angle < (-1 * sight_angle1)) && distance < range_distance)
                    {
                        /*player_in_sight = parentWorld.Map.playerInSight(angle, distance, parent.CenterPoint);
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        if (!player_in_sight)
                        {
                            Console.WriteLine("player Up in view");
                            parent.State = Enemy.EnemyState.Chase;
                        }*/
                        
                    }
                    ((IdleChaseEnemy)parent).Angle1 = (float)(-1 * sight_angle1);
                    ((IdleChaseEnemy)parent).Angle2 = (float)(-1 * sight_angle2);
                    Console.WriteLine(-1 * sight_angle1);
                    break;
                default:
                    if ((angle > sight_angle1 && angle < sight_angle2 ) && distance < range_distance)
                    {
                        /*player_in_sight = parentWorld.Map.playerInSight(angle, distance, parent.CenterPoint);
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        if (!player_in_sight)
                        {
                            Console.WriteLine("player Down in view");
                            parent.State = Enemy.EnemyState.Chase;
                        }*/
                    }
                    ((IdleChaseEnemy)parent).Angle1 = (float)(sight_angle1);
                    ((IdleChaseEnemy)parent).Angle2 = (float)(sight_angle2);
                    
                    break;
            }
            
            ((IdleChaseEnemy)parent).Angle = angle;
            return;
        }

    }
}
