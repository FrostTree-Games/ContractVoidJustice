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
    class TestEnemy : Enemy
    {

        private float neg_direction = -1;
        private float change_direction_time = 0.0f;
        private int change_direction;

        public TestEnemy()
        {
        }
        public TestEnemy(LevelState parentWorld, float initialx, float initialy)
        {
            position.X = initialx;
            position.Y = initialy;

            dimensions = new Vector2(48f, 48f);

            state = EnemyState.Moving;

            direction_facing = GlobalGameConstants.Direction.Up;

            velocity = new Vector2(0.0f, -1.0f);

            change_direction_time = 0.0f;
            change_direction = 0;

            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
            if(state == EnemyState.Moving)
            {

                change_direction_time += currentTime.ElapsedGameTime.Milliseconds;

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en == this)
                    {
                        continue;
                    }

                    if (hitTest(en))
                    {
                        if (en is Player)
                        {
                            this.knockBack(en, this.position, this.dimensions);
                        }
                    }
                }

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
                        if (direction_facing == GlobalGameConstants.Direction.Right)
                        {
                            direction_facing = GlobalGameConstants.Direction.Left;
                            velocity.X = -1.0f;
                            velocity.Y = 0.0f;
                            break;
                        }
                        else if (direction_facing == GlobalGameConstants.Direction.Left)
                        {
                            direction_facing = GlobalGameConstants.Direction.Right;
                            velocity.X = 1.0f;
                            velocity.Y = 0.0f;
                            break;
                        }
                        else if (direction_facing == GlobalGameConstants.Direction.Up)
                        {
                            direction_facing = GlobalGameConstants.Direction.Down;
                            velocity.Y = 1.0f;
                            velocity.X = 0.0f;
                            break;
                        }
                        else if(direction_facing == GlobalGameConstants.Direction.Down)
                        {
                            direction_facing = GlobalGameConstants.Direction.Up;
                            velocity.Y = -1.0f;
                            velocity.X = 0.0f;
                            break;
                        }

                    }
                    on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                    check_corners++;
                }

                if (change_direction_time > 1000)
                {
                    Random rand = new Random();
                    change_direction = rand.Next(4);
                    change_direction_time = 0.0f;
                    if (change_direction == 0)
                    {
                        velocity.X = 1.0f;
                        velocity.Y = 0.0f;
                        direction_facing = GlobalGameConstants.Direction.Right;
                    }
                    else if (change_direction == 1)
                    {
                        velocity.X = -1.0f;
                        velocity.Y = 0.0f;
                        direction_facing = GlobalGameConstants.Direction.Left;
                    }
                    else if (change_direction == 2)
                    {
                        velocity.X = 0.0f;
                        velocity.Y = -1.0f;
                        direction_facing = GlobalGameConstants.Direction.Up;
                    }
                    else if (change_direction == 3)
                    {
                        velocity.X = 0.0f;
                        velocity.Y = 1.0f;
                        direction_facing = GlobalGameConstants.Direction.Down;
                    }
                }

                Vector2 pos = new Vector2(position.X, position.Y);

                Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
                position.X = finalPos.X;
                position.Y = finalPos.Y;
            }
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Black, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 0.5f);
        }
    }
}
