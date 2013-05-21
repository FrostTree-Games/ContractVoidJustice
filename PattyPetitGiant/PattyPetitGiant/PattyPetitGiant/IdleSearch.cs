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

        public void update(Enemy parent, GameTime currentTime, LevelState parentWorld)
        {
            return;
        }
        public void update(Enemy parent, Entity player, GameTime currentTime, LevelState parentWorld)
        {
            angle = (float)(Math.Atan2(parent.CenterPoint.Y - player.CenterPoint.Y, parent.CenterPoint.X - player.CenterPoint.X));
            angle = -1 * angle;

            Console.WriteLine("player angle: " + angle);
            distance = Vector2.Distance(parent.CenterPoint, player.CenterPoint);
                
            switch (parent.Direction_Facing)
            {
                case GlobalGameConstants.Direction.Right:
                    if ((angle > (-1 * Math.PI / 7) && angle < Math.PI / 7) && distance < range_distance)
                    {
                        /*player_in_sight = parentWorld.Map.playerInSight(angle, distance, parent.CenterPoint);
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        if(!player_in_sight)
                        {
                            Console.WriteLine("player right in view");
                            parent.State = Enemy.EnemyState.Chase;
                        }*/
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        parent.State = Enemy.EnemyState.Chase;
                    }
                    ((IdleChaseEnemy)parent).Angle1 = (float)(-1 * Math.PI / 7);
                    ((IdleChaseEnemy)parent).Angle2 = (float)(Math.PI / 7);
                    break;
                case GlobalGameConstants.Direction.Left:
                    if ((angle > (Math.PI / 1.1) && angle < Math.PI * 1.1) && distance < range_distance)
                    {
                        /*player_in_sight = parentWorld.Map.playerInSight(angle, distance, parent.CenterPoint);
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        if (!player_in_sight)
                        {
                            Console.WriteLine("player left in view");
                            parent.State = Enemy.EnemyState.Chase;
                        }*/
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        parent.State = Enemy.EnemyState.Chase;
                    }
                    ((IdleChaseEnemy)parent).Angle1 = (float)(Math.PI / 1.1);
                    ((IdleChaseEnemy)parent).Angle2 = (float)(Math.PI * 1.1);
                    break;
                case GlobalGameConstants.Direction.Up:
                    if ((angle > (Math.PI / 2.25) && angle < (Math.PI / 1.75)) && distance < range_distance)
                    {
                        /*player_in_sight = parentWorld.Map.playerInSight(angle, distance, parent.CenterPoint);
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        if (!player_in_sight)
                        {
                            Console.WriteLine("player Up in view");
                            parent.State = Enemy.EnemyState.Chase;
                        }*/
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        parent.State = Enemy.EnemyState.Chase;
                    }
                    ((IdleChaseEnemy)parent).Angle1 = (float)(-1 * Math.PI / 2.25);
                    ((IdleChaseEnemy)parent).Angle2 = (float)(-1 *Math.PI / 1.75);
                    break;
                default:
                    if ((angle > 1.098 && angle < 2.02 ) && distance < range_distance)
                    {
                        /*player_in_sight = parentWorld.Map.playerInSight(angle, distance, parent.CenterPoint);
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        if (!player_in_sight)
                        {
                            Console.WriteLine("player Down in view");
                            parent.State = Enemy.EnemyState.Chase;
                        }*/
                        Console.WriteLine("player direction: " + parent.Direction_Facing);
                        parent.State = Enemy.EnemyState.Chase;
                    }
                    ((IdleChaseEnemy)parent).Angle1 = (float)(1.098);
                    ((IdleChaseEnemy)parent).Angle2 = (float)(2.02);
                    
                    break;
            }
            ((IdleChaseEnemy)parent).Angle = angle;
            return;
        }

    }
}
