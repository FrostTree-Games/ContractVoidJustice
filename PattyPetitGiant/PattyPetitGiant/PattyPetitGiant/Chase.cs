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
            //distance in the X direction
            float direction_x = player.CenterPoint.X - parent.CenterPoint.X;
            float direction_y = player.CenterPoint.Y - parent.CenterPoint.Y;
            float angle = (float)Math.Atan2(player.CenterPoint.Y - parent.CenterPoint.Y, player.CenterPoint.X - parent.CenterPoint.X);

            Console.WriteLine(angle);
            if (Math.Abs(direction_x) > (Math.Abs(direction_y)))
            {
                if (direction_x < 0)
                {
                    parent.Velocity = new Vector2(-1.0f * parent.Enemy_Speed, direction_y / 100);
                    if (parent.Direction_Facing == GlobalGameConstants.Direction.Up && angle < -3 * Math.PI / 4 - 0.25 || parent.Direction_Facing == GlobalGameConstants.Direction.Down && angle > 3 * Math.PI / 4 + 0.75 || parent.Direction_Facing == GlobalGameConstants.Direction.Right)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Left;
                        parent.Change_Direction_Time = 0.0f;
                    }
                }
                else
                {
                    parent.Velocity = new Vector2(parent.Enemy_Speed, direction_y / 100);
                    if (parent.Direction_Facing == GlobalGameConstants.Direction.Up && angle > -1 * Math.PI / 4 + 0.25 || parent.Direction_Facing == GlobalGameConstants.Direction.Down && angle < Math.PI / 4 - 0.25 || parent.Direction_Facing == GlobalGameConstants.Direction.Left)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Right;
                        parent.Change_Direction_Time = 0.0f;
                    }
                }
            }
            else
            {
                if (direction_y < 0)
                {
                    parent.Velocity = new Vector2(direction_x / 100f, -1.0f*parent.Enemy_Speed);
                    if (parent.Direction_Facing == GlobalGameConstants.Direction.Left && angle > -3 * Math.PI / 4 + 0.25 || parent.Direction_Facing == GlobalGameConstants.Direction.Left && angle < -1 * Math.PI / 4 - 0.75 || parent.Direction_Facing == GlobalGameConstants.Direction.Down)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Up;
                        parent.Change_Direction_Time = 0.0f;
                    }
                }
                else
                {
                    parent.Velocity = new Vector2(direction_x / 100f, parent.Enemy_Speed);
                    if (parent.Direction_Facing == GlobalGameConstants.Direction.Left && angle < -3 * Math.PI / 4 - 0.25 || parent.Direction_Facing == GlobalGameConstants.Direction.Right && angle > -1 * Math.PI / 4 + 0.25 || parent.Direction_Facing == GlobalGameConstants.Direction.Up)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Down;
                        parent.Change_Direction_Time = 0.0f;
                    }
                }
            }
        }
    }
}
