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

            if (Math.Abs(direction_x) > (Math.Abs(direction_y) + 100))
            {
                if (direction_x < 0)
                {
                    parent.Velocity = new Vector2(-1.51f, direction_y/100);
                    if (parent.Change_Direction_Time > 300)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Left;
                        parent.Change_Direction_Time = 0.0f;
                    }
                }
                else
                {
                    parent.Velocity = new Vector2(1.51f, direction_y/100);
                    if (parent.Change_Direction_Time > 300)
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
                    parent.Velocity = new Vector2(direction_x / 100f, -1.51f);
                    if (parent.Change_Direction_Time > 300)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Up;
                        parent.Change_Direction_Time = 0.0f;
                    }
                }
                else
                {
                    parent.Velocity = new Vector2(direction_x / 100f, 1.51f);
                    if (parent.Change_Direction_Time > 300)
                    {
                        parent.Direction_Facing = GlobalGameConstants.Direction.Down;
                        parent.Change_Direction_Time = 0.0f;
                    }
                }
            }
        }
    }
}
