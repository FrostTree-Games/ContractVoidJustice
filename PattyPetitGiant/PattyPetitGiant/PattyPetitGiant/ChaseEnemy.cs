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
    class ChaseEnemy : Enemy
    {
        private EnemyComponents component = null;

        public ChaseEnemy(LevelState parentWorld, float initialx, float initialy)
        {
            position.X = initialx;
            position.Y = initialy;

            dimensions = new Vector2(48f, 48f);

            state = EnemyState.Moving;

            component = new Walk();

            direction_facing = GlobalGameConstants.Direction.Up;

            velocity = new Vector2(0.0f, -1.0f);

            change_direction_time = 0.0f;

            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
            if (state == EnemyState.Moving)
            {

                change_direction_time += currentTime.ElapsedGameTime.Milliseconds;

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en == this)
                    {
                        continue;
                    }

                    if (en is Player)
                    {
                        float distance = (float)Math.Sqrt(Math.Pow((double)(en.Position.X - position.X), 2.0) + Math.Pow((double)(en.Position.Y - position.Y),2.0));
                        if(distance < 300)
                        {
                            state = EnemyState.Chase;
                        }
                    }
                }

                component.update(this, currentTime, parentWorld);

                Vector2 pos = new Vector2(position.X, position.Y);

                Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
                position.X = finalPos.X;
                position.Y = finalPos.Y;

                if (state == EnemyState.Chase)
                {
                    component = new Chase();
                }
            }
            else if (state == EnemyState.Chase)
            {
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
                else
                {
                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                            continue;

                        if (en is Player)
                        {
                            float distance = (float)Math.Sqrt(Math.Pow((double)(en.Position.X - position.X), 2.0) + Math.Pow((double)(en.Position.Y - position.Y), 2.0));
                            if (hitTest(en))
                            {
                                this.knockBack(en, this.position, this.dimensions);
                            }
                            else if (distance > 300)
                            {
                                state = EnemyState.Moving;
                            }
                            else
                            {
                                component.update(this, en, currentTime, parentWorld);
                            }
                        }
                    }
                }
                Vector2 pos = new Vector2(position.X, position.Y);

                Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
                position.X = finalPos.X;
                position.Y = finalPos.Y;

                if (state == EnemyState.Moving)
                {
                    component = new Walk();
                }

            }
        }
    }
}
