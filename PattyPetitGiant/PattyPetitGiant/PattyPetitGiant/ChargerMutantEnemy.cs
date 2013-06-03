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
    class ChargerMutantEnemy : Enemy
    {
        private EnemyComponents component;
        
        public ChargerMutantEnemy(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = Vector2.Zero;

            disable_movement = false;
            disable_movement_time = 0.0f;
            knockback_magnitude = 6.0f;
            enemy_damage = 20;
            enemy_life = 10;
            player_found = false;
            change_direction_time = 0.0f;
            range_distance = 300.0f;

            state = EnemyState.Idle;
            enemy_type = EnemyType.Alien;
            component = new IdleSearch();

            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
            switch (state)
            {
                case EnemyState.Idle:
                    change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                            continue;
                        else if (en is Player)
                            component.update(this, en, currentTime, parentWorld);
                    }

                    if (player_found)
                    {
                        state = EnemyState.Agressive;
                        velocity = Vector2.Zero;
                    }
                    else
                    {
                        if (change_direction_time > 5000)
                        {
                            switch (direction_facing)
                            {
                                case GlobalGameConstants.Direction.Right:
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    break;
                                case GlobalGameConstants.Direction.Left:
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    break;
                                case GlobalGameConstants.Direction.Up:
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    break;
                                default:
                                    direction_facing = GlobalGameConstants.Direction.Left;
                                    break;
                            }
                            change_direction_time = 0.0f;
                        }
                    }
                    break;
                case EnemyState.Agressive:
                    float distance = 0.0f;
                    switch (direction_facing)
                    {
                        case GlobalGameConstants.Direction.Right:
                            velocity = new Vector2(8.0f, 0.0f);
                            break;
                        case GlobalGameConstants.Direction.Left:
                            velocity = new Vector2(-8.0f, 0.0f);
                            break;
                        case GlobalGameConstants.Direction.Up:
                            velocity = new Vector2(0.0f, -8.0f);
                            break;
                        default:
                            velocity = new Vector2(0.0f, 8.0f);
                            break;
                    }

                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                            continue;
                        else if (en is Player)
                        {
                            distance = Vector2.Distance(en.CenterPoint, CenterPoint);
                            if (distance > 300)
                            {
                                state = EnemyState.Idle;
                                component = new IdleSearch();
                                velocity = Vector2.Zero;
                                animation_time = 0.0f;
                                player_found = false;
                            }
                        }

                        if (hitTest(en))
                        {
                            Vector2 direction = en.CenterPoint - CenterPoint;
                            en.knockBack(direction, knockback_magnitude, enemy_damage);
                            break;
                        }
                    }

                    Vector2 pos = new Vector2(position.X, position.Y);
                    Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                    Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
                    position.X = finalPos.X;
                    position.Y = finalPos.Y;
                    break;
                default:
                    break;
            }
        }
        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Green, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
        }
        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            if (disable_movement_time == 0.0)
            {
                if (state != EnemyState.Agressive)
                {
                    disable_movement = true;
                    if (Math.Abs(direction.X) > (Math.Abs(direction.Y)))
                    {
                        if (direction.X < 0)
                        {
                            velocity = new Vector2(-5.51f * magnitude, direction.Y / 100 * magnitude);
                        }
                        else
                        {
                            velocity = new Vector2(5.51f * magnitude, direction.Y / 100 * magnitude);
                        }
                    }
                    else
                    {
                        if (direction.Y < 0)
                        {
                            velocity = new Vector2(direction.X / 100f * magnitude, -5.51f * magnitude);
                        }
                        else
                        {
                            velocity = new Vector2((direction.X / 100f) * magnitude, 5.51f * magnitude);
                        }
                    }
                }

                enemy_life = enemy_life - damage;
            }
        }
    }
}