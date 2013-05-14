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
    class RangeEnemy : Enemy
    {
        private int change_direction;
        private AnimationLib.FrameAnimationSet enemyAnim;
        private Random rand;
        private Item weapon;
        private float angle;
        private float distance;
        private float range_distance;

        public RangeEnemy(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            this.parentWorld = parentWorld;
            dimensions = new Vector2(48.0f, 48.0f);

            state = EnemyState.Moving;

            direction_facing = GlobalGameConstants.Direction.Down;

            velocity = new Vector2(0.0f, -1.0f);

            change_direction = 0;
            change_direction_time = 0.0f;

            enemy_life = 10;
            enemy_damage = 2;
            damage_player_time = 0.0f;

            rand = new Random(DateTime.Now.Millisecond);
            enemyAnim = AnimationLib.getFrameAnimationSet("rangeenemyPic");
            weapon = new Gun(position);
            angle = 0.0f;
            distance = 0.0f;
            range_distance = 325;
        }

        //needs to walk randomly and then shoot in a direction
        public override void update(GameTime currentTime)
        {
            switch (state)
            {
                case EnemyState.Moving:
                    change_direction_time += currentTime.ElapsedGameTime.Milliseconds;

                    if (disable_movement == true)
                    {
                        disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                        if (disable_movement_time > 300)
                        {
                            velocity = Vector2.Zero;
                            disable_movement = false;
                            disable_movement_time = 0;
                        }
                    }

                    //checks where the player is
                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                        {
                            continue;
                        }

                        if (en is Player)
                        {
                            angle = (float)(Math.Atan2(CenterPoint.X - en.CenterPoint.X, CenterPoint.Y - en.CenterPoint.Y));
                            angle = angle + (float)(Math.PI / 2);

                            distance = (float)Math.Sqrt(Math.Pow((double)(en.Position.X - position.X), 2.0) + Math.Pow((double)(en.Position.Y - position.Y), 2.0));
                        }
                    }

                    Console.WriteLine(angle);
                    Console.WriteLine(distance);
                    switch (direction_facing)
                    {
                        case GlobalGameConstants.Direction.Right:
                            if ((angle > (-1 * Math.PI / 7) && angle < Math.PI / 7) && distance < range_distance)
                                Console.WriteLine("In enemy's range");
                            break;
                        case GlobalGameConstants.Direction.Left:
                            if ((angle > (Math.PI / 1.1) && angle < Math.PI * 1.1) && distance < range_distance)
                                Console.WriteLine("In enemy's range");
                            break;
                        case GlobalGameConstants.Direction.Up:
                            if ((angle > (Math.PI / 2.25) && angle < (Math.PI / 1.75)) && distance < range_distance)
                                Console.WriteLine("In enemy's range");
                            break;
                        default:
                            if ((angle > (Math.PI * 1.35) || angle < (-1 * Math.PI / 2.5)) && distance < range_distance)
                                   Console.WriteLine("In enemy's range");
                            break;

                    }
                    /*
                    int check_corners = 0;
                    Vector2 nextStep_temp = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                    bool on_wall = parentWorld.Map.hitTestWall(nextStep_temp);

                    
                    while (check_corners != 4)
                    {
                        if (on_wall != true)
                        {
                            if (check_corners == 0)
                            {
                                nextStep_temp = new Vector2(position.X + dimensions.X + velocity.X, position.Y + velocity.Y);
                            }
                            else if (check_corners == 1)
                            {
                                nextStep_temp = new Vector2(position.X + velocity.X, position.Y + dimensions.Y + velocity.Y);
                            }
                            else if (check_corners == 2)
                            {
                                nextStep_temp = new Vector2(position.X + dimensions.X + velocity.X, position.Y + dimensions.Y + velocity.Y);
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {

                            switch (direction_facing)
                            {
                                case GlobalGameConstants.Direction.Right:
                                    direction_facing = GlobalGameConstants.Direction.Left;
                                    velocity.X = -1.0f;
                                    velocity.Y = 0.0f;
                                    break;
                                case GlobalGameConstants.Direction.Left:
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    velocity.X = 1.0f;
                                    velocity.Y = 0.0f;
                                    break;
                                case GlobalGameConstants.Direction.Up:
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    velocity.Y = 1.0f;
                                    velocity.X = 0.0f;
                                    break;
                                default:
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    velocity.Y = -1.0f;
                                    velocity.X = 0.0f;
                                    break;
                            }
                            break;
                        }
                        on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                        check_corners++;
                    }

                    if (change_direction_time > 2000)
                    {
                        change_direction = rand.Next(4);
                        //change_direction_time = 0.0f;
                        if (change_direction_time > 2300)
                        {
                            switch (change_direction)
                            {
                                case 0:
                                    velocity.X = 1.0f;
                                    velocity.Y = 0.0f;
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    break;
                                case 1:
                                    velocity.X = -1.0f;
                                    velocity.Y = 0.0f;
                                    direction_facing = GlobalGameConstants.Direction.Left;
                                    break;
                                case 2:
                                    velocity.X = 0.0f;
                                    velocity.Y = -1.0f;
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    break;
                                default:
                                    velocity.X = 0.0f;
                                    velocity.Y = 1.0f;
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    break;
                            }
                            change_direction_time = 0.0f;
                        }
                        else
                        {
                            velocity = Vector2.Zero;
                        }
                    }
                
                    Vector2 pos = new Vector2(position.X, position.Y);

                    Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                    Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
                    position.X = finalPos.X;
                    position.Y = finalPos.Y;
                    */
                    break;
                case EnemyState.Pause:
                    //state = EnemyState.Firing;
                    break;
                default:
                    //state = EnemyState.Moving;
                    break;
            }
            if (enemy_life <= 0)
            {
                remove_from_list = true;
            }
        }
        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Black, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 0.5f);

        }
    }
}
