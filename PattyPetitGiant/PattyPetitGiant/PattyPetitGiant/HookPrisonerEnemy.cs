﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spine;

namespace PattyPetitGiant
{
    class HookPrisonerEnemy : Enemy, SpineEntity
    {
        private enum ChainState
        {
            Moving,
            Throw,
            Pull,
            Neutral
        }
        private ChainState state;
        private EnemyComponents component;
        private float angle;
        private float distance;
        private float max_chain_distance = 300.0f;
        private float chain_distance = 0.0f;
        private float chain_speed = 0.0f;
        private Vector2 chain_velocity;
        private Vector2 chain_position;
        private Vector2 chain_dimensions;
        private Entity en_chained;
        private Entity target;
        private AnimationLib.SpineAnimationSet[] directionAnims = null;

        public HookPrisonerEnemy(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = Vector2.Zero;
            chain_velocity = Vector2.Zero;
            chain_dimensions = new Vector2(10.0f, 10.0f);
            chain_position = position;
            
            disable_movement = false;
            disable_movement_time = 0.0f;
            knockback_magnitude = 10.0f;
            enemy_damage = 20;
            enemy_life = 10;
            enemy_found = false;
            change_direction_time = 0.0f;
            range_distance = 300.0f;
            angle = 0.0f;
            animation_time = 0.0f;

            state = ChainState.Moving;
            enemy_type = EnemyType.Prisoner;
            component = new MoveSearch();
            en_chained = null;

            directionAnims = new AnimationLib.SpineAnimationSet[4];
            directionAnims[(int)GlobalGameConstants.Direction.Up] = AnimationLib.loadNewAnimationSet("chargerUp");
            directionAnims[(int)GlobalGameConstants.Direction.Down] = AnimationLib.loadNewAnimationSet("chargerDown");
            directionAnims[(int)GlobalGameConstants.Direction.Left] = AnimationLib.loadNewAnimationSet("chargerRight");
            directionAnims[(int)GlobalGameConstants.Direction.Left].Skeleton.FlipX = true;
            directionAnims[(int)GlobalGameConstants.Direction.Right] = AnimationLib.loadNewAnimationSet("chargerRight");

            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
            switch (state)
            {
                case ChainState.Moving:
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("run");
                    change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                    if(enemy_found == false)
                    {
                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en == this)
                                continue;
                            else if (en.Enemy_Type != enemy_type && en.Enemy_Type != EnemyType.NoType)
                            {
                                    component.update(this, en, currentTime, parentWorld);
                                    if (enemy_found)
                                    {
                                        target = en;
                                        break;
                                    }
                            }
                        }
                    }

                    if (enemy_found)
                    {
                        state = ChainState.Throw;
                        velocity = Vector2.Zero;

                        angle = (float)Math.Atan2(target.CenterPoint.Y - CenterPoint.Y, target.CenterPoint.X - CenterPoint.X);
                        distance = Vector2.Distance(CenterPoint, target.CenterPoint);

                        switch (direction_facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                chain_velocity = new Vector2(8.0f, (float)(distance * Math.Sin(angle)) * 0.04f);
                                chain_position = CenterPoint + new Vector2(dimensions.X / 2, 0);
                                chain_speed = chain_velocity.X;
                                break;
                            case GlobalGameConstants.Direction.Left:
                                chain_velocity = new Vector2(-8.0f, (float)(distance * Math.Sin(angle)) * 0.04f);
                                chain_position = CenterPoint - new Vector2(dimensions.X / 2, 0);
                                chain_speed = Math.Abs(chain_velocity.Y);
                                break;
                            case GlobalGameConstants.Direction.Up:
                                chain_velocity = new Vector2((float)(distance * Math.Cos(angle) * 0.04f), -8.0f);
                                chain_position = CenterPoint - new Vector2(0, dimensions.X / 2);
                                chain_speed = Math.Abs(chain_velocity.Y);
                                break;
                            default:
                                chain_velocity = new Vector2((float)(distance * Math.Cos(angle) * 0.04f), 8.0f);
                                chain_position = CenterPoint + new Vector2(0, dimensions.X / 2);
                                chain_speed = Math.Abs(chain_velocity.Y);
                                break;
                        }
                    }

                    Vector2 pos = new Vector2(position.X, position.Y);
                    Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                    Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
                    position.X = finalPos.X;
                    position.Y = finalPos.Y;
                    break;
                    case ChainState.Throw:
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("idle");
                        chain_distance += chain_speed;
                        chain_position += chain_velocity;
                        if (chain_distance >= max_chain_distance || parentWorld.Map.hitTestWall(chain_position))
                        {
                            state = ChainState.Pull;
                            chain_velocity = -1 * chain_velocity;
                        }

                        //checks to see if chain hits the player
                        if (hitTestChain(target, chain_position.X, chain_position.Y))
                        {
                            en_chained = target;
                            target.Disable_Movement = true;
                            target.Disable_Movement_Time = 0.0f;
                            target.Velocity = Vector2.Zero;
                            state = ChainState.Pull;
                            chain_velocity = -1 * chain_velocity;
                        }
                        break;
                    //need to work on pull
                    case ChainState.Pull:
                        if (chain_distance > 0)
                        {
                            chain_position += chain_velocity;
                            chain_distance -= chain_speed;
                            if (en_chained != null)
                            {
                                en_chained.Position += chain_velocity;
                                en_chained.Disable_Movement = true;
                            }
                        }
                        else
                        {
                            if (en_chained != null)
                            {
                                Vector2 direction = en_chained.CenterPoint - CenterPoint;
                                en_chained.Disable_Movement_Time = 0.0f;
                                en_chained.knockBack(direction, knockback_magnitude, enemy_damage, this);
                            }
                            state = ChainState.Neutral;
                            en_chained = null;
                            enemy_found = false;
                        }
                        break;
                    default:
                        state = ChainState.Moving;
                        break;
            }
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Green, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
            //sb.Draw(Game1.whitePixel, chain_position, null, Color.Black, 0.0f, Vector2.Zero, new Vector2(48.0f, 48.0f), SpriteEffects.None, 0.5f);

            if (state != ChainState.Neutral || state != ChainState.Moving)
            {
                switch (direction_facing)
                {
                    case GlobalGameConstants.Direction.Right:
                        sb.Draw(Game1.whitePixel, CenterPoint + new Vector2(dimensions.X/2, 0), null, Color.Black, angle, Vector2.Zero, new Vector2(chain_distance, 10.0f), SpriteEffects.None, 0.5f);
                        break;
                    case GlobalGameConstants.Direction.Left:
                        sb.Draw(Game1.whitePixel, CenterPoint + new Vector2(dimensions.X/2, 0), null, Color.Black, angle, Vector2.Zero, new Vector2(chain_distance, 10.0f), SpriteEffects.None, 0.5f);
                        break;
                    case GlobalGameConstants.Direction.Up:
                        sb.Draw(Game1.whitePixel, CenterPoint + new Vector2(0, dimensions.Y/2), null, Color.Black, angle, Vector2.Zero, new Vector2(chain_distance, 10.0f), SpriteEffects.None, 0.5f);
                        break;
                    default:
                        sb.Draw(Game1.whitePixel, CenterPoint + new Vector2(0.0f, dimensions.Y / 2), null, Color.Black, angle, Vector2.Zero, new Vector2(chain_distance, 10.0f), SpriteEffects.None, 0.5f);
                        break;
                }
            }
        }
        
        public bool hitTestChain(Entity other, float x, float y)
        {
            if (x > other.Position.X + other.Dimensions.X || x < other.Position.X || y > other.Position.Y + other.Dimensions.Y || y < other.Position.Y)
            {
                return false;
            }
            return true;
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (attacker == null)
            {
                return;
            }
            if (disable_movement_time == 0.0)
            {
                if(state == ChainState.Neutral || state == ChainState.Neutral)
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

                enemy_life = enemy_life - damage;
            }

            if (attacker == null)
            {
                return;
            }
            else if (attacker.Enemy_Type != enemy_type && attacker.Enemy_Type != EnemyType.NoType)
            {
                enemy_found = true;

                switch (attacker.Direction_Facing)
                {
                    case GlobalGameConstants.Direction.Right:
                        direction_facing = GlobalGameConstants.Direction.Left;
                        break;
                    case GlobalGameConstants.Direction.Left:
                        direction_facing = GlobalGameConstants.Direction.Right;
                        break;
                    case GlobalGameConstants.Direction.Up:
                        direction_facing = GlobalGameConstants.Direction.Down;
                        break;
                    default:
                        direction_facing = GlobalGameConstants.Direction.Up;
                        break;
                }
            }
        }

        public override void spinerender(SkeletonRenderer renderer)
        {
            AnimationLib.SpineAnimationSet currentSkeleton = directionAnims[(int)direction_facing];

            currentSkeleton.Skeleton.RootBone.X = CenterPoint.X * (currentSkeleton.Skeleton.FlipX ? -1 : 1);
            currentSkeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2f);

            currentSkeleton.Skeleton.RootBone.ScaleX = 1.0f;
            currentSkeleton.Skeleton.RootBone.ScaleY = 1.0f;

            currentSkeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(currentSkeleton.Skeleton);
        }
    }
}