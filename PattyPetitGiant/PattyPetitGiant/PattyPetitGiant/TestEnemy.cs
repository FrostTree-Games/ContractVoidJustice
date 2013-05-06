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
        public TestEnemy()
        {
        }
        public TestEnemy(LevelState parentWorld, float initialx, float initialy)
        {
            position.X = initialx;
            position.Y = initialy;

            dimensions = new Vector2(48f, 48f);

            state = EnemyState.Moving;

            velocity = new Vector2(1.0f, 3.0f);

            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
            if(state == EnemyState.Moving)
            {

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

                Vector2 pos = new Vector2(position.X, position.Y);

                Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);

                bool on_wall = parentWorld.Map.hitTestWall(nextStep);

                Console.WriteLine("on_wall: " + on_wall);

                int check_corners = 0;

                while (check_corners != 4)
                {
                    if (on_wall)
                    {
                        Random rand = new Random();
                        Random temp = new Random();

                        neg_direction = temp.Next(2);

                        Console.WriteLine("neg_direction: " + neg_direction);
                        if (neg_direction % 2 == 0)
                        {
                            neg_direction = 1.0f;
                        }
                        else
                        {
                            neg_direction = -1.0f;
                        }

                        float new_horz_velocity = (float)(rand.NextDouble() * 5);
                        velocity.X = (neg_direction) * (new_horz_velocity);

                        neg_direction = temp.Next(2);
                        if (neg_direction % 2 == 0)
                        {
                            neg_direction = 1.0f;
                        }
                        else
                        {
                            neg_direction = -1.0f;
                        }

                        float new_vert_velocity = (float)(rand.NextDouble() * 5);
                        velocity.X = (neg_direction) * (new_vert_velocity);
                        break;
                    }
                    else
                    {
                        if (check_corners == 0)
                        {
                            nextStep = new Vector2(position.X + dimensions.X + velocity.X, position.Y + velocity.Y);
                        }
                        else if (check_corners == 1)
                        {
                            nextStep = new Vector2(position.X + velocity.X, position.Y + dimensions.Y + velocity.Y);
                        }
                        else if (check_corners == 2)
                        {
                            nextStep = new Vector2(position.X + dimensions.X + velocity.X, position.Y + dimensions.Y + velocity.Y);
                        }
                        else
                        {
                            break;
                        }
                    }
                    on_wall = parentWorld.Map.hitTestWall(nextStep);

                    check_corners++;
                }
               
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
