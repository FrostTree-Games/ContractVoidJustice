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
    class GuardSquadSoldiers : Enemy
    {
        private enum SquadSoldierState
        {
            Patrol,
            Fire,
            Follow
        }

        private SquadSoldierState state = SquadSoldierState.Patrol;
        private Vector2 follow_point = Vector2.Zero;
        public Vector2 Follow_Point
        {
            set { follow_point = value; }
            get { return follow_point; }
        }

        private GuardSquadLeader leader = null;
        public GuardSquadLeader Leader
        {
            set { leader = value; }
            get { return leader; }
        }

        public GuardSquadSoldiers(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = new Vector2(0.0f, 0.0f);

            enemy_damage = 20;
            enemy_life = 30;
            knockback_magnitude = 2.0f;
            disable_movement = false;
            disable_movement_time = 0.0f;
            player_found = false;
            change_direction_time = 0.0f;
            range_distance = 300.0f;
            
            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
            if (disable_movement == true)
            {
                disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                if (disable_movement_time > 300)
                {
                    disable_movement = false;
                    disable_movement_time = 0.0f;
                    velocity = Vector2.Zero;
                }
            }
            else
            {
                switch (state)
                {
                    case SquadSoldierState.Patrol:
                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                        float distance = Vector2.Distance(Leader.CenterPoint, CenterPoint);

                        if (distance > 96)
                        {
                            float distance_from_follow_pt = Vector2.Distance(follow_point, CenterPoint);
                            float angle = (float)(Math.Atan2(follow_point.Y - CenterPoint.Y, follow_point.X - CenterPoint.X));
                            velocity = new Vector2(distance_from_follow_pt * (float)(Math.Cos(angle))/ 100.0f, distance_from_follow_pt * (float)(Math.Sin(angle)) / 100.0f);

                            if (Math.Abs(velocity.X) > (Math.Abs(velocity.Y)))
                            {
                                //enemy facing left
                                if (velocity.X < 0)
                                {
                                    velocity = new Vector2(-1.5f, velocity.Y);
                                    /*if (parent.Direction_Facing == GlobalGameConstants.Direction.Up && angle < -3 * Math.PI / 4 - 0.10 || parent.Direction_Facing == GlobalGameConstants.Direction.Down && angle > 3 * Math.PI / 4 + 0.10 || parent.Direction_Facing == GlobalGameConstants.Direction.Right)
                                    {
                                        parent.Direction_Facing = GlobalGameConstants.Direction.Left;
                                        parent.Change_Direction_Time = 0.0f;
                                    }*/
                                }
                                //enemy facing right
                                else
                                {
                                    velocity = new Vector2(1.5f, velocity.Y);
                                    /*if (parent.Direction_Facing == GlobalGameConstants.Direction.Up && angle > -1 * Math.PI / 4 + 0.10 || parent.Direction_Facing == GlobalGameConstants.Direction.Down && angle < Math.PI / 4 - 0.10 || parent.Direction_Facing == GlobalGameConstants.Direction.Left)
                                    {
                                        parent.Direction_Facing = GlobalGameConstants.Direction.Right;
                                        parent.Change_Direction_Time = 0.0f;
                                    }*/
                                }
                            }
                            else
                            {
                                //enemy facing up
                                if (velocity.Y < 0)
                                {
                                    velocity = new Vector2(velocity.X, -1.5f);
                                    /*if (parent.Direction_Facing == GlobalGameConstants.Direction.Left && angle > -3 * Math.PI / 4 + 0.10 || parent.Direction_Facing == GlobalGameConstants.Direction.Right && angle < -1 * Math.PI / 4 - 0.10 || parent.Direction_Facing == GlobalGameConstants.Direction.Down)
                                    {
                                        parent.Direction_Facing = GlobalGameConstants.Direction.Up;
                                        parent.Change_Direction_Time = 0.0f;
                                    }*/
                                }
                                //enemy facing down
                                else
                                {
                                    velocity = new Vector2(velocity.X, 1.5f);
                                    /*if (parent.Direction_Facing == GlobalGameConstants.Direction.Left && angle < -3 * Math.PI / 4 - 0.10 || parent.Direction_Facing == GlobalGameConstants.Direction.Right && angle > -1 * Math.PI / 4 + 0.10 || parent.Direction_Facing == GlobalGameConstants.Direction.Up)
                                    {
                                        parent.Direction_Facing = GlobalGameConstants.Direction.Down;
                                        parent.Change_Direction_Time = 0.0f;
                                    }*/
                                }
                            }
                            Console.WriteLine(velocity);
                        }
                        else
                        {
                            velocity = Vector2.Zero;
                        }
                        break;
                    case SquadSoldierState.Fire:
                        break;
                    default:
                        break;
                }
            }
            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position.X = finalPos.X;
            position.Y = finalPos.Y;
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Red, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);

        }

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
        }
    }
}
