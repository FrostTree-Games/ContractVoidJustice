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
    class TestEnemy : Enemy, SpineEntity    
    {
        private int change_direction;
        private AnimationLib.FrameAnimationSet enemyAnim;

        private Random rand;
       
        public TestEnemy()
        {
        }
        public TestEnemy(LevelState parentWorld, float initialx, float initialy)
        {
            position.X = initialx;
            position.Y = initialy;

            dimensions = new Vector2(48f, 48f);

            state = EnemyState.Moving;

            direction_facing = GlobalGameConstants.Direction.Right;

            velocity = new Vector2(1.0f, 0.0f);

            change_direction_time = 0.0f;
            change_direction = 0;

            this.parentWorld = parentWorld;

            enemy_life = 10;
            enemy_damage = 1;
            damage_player_time = 0.0f;

            rand = new Random(DateTime.Now.Millisecond);

            walk_down = AnimationLib.getSkeleton("zippyDown");
            walk_right = AnimationLib.getSkeleton("zippyRight");
            walk_up = AnimationLib.getSkeleton("zippyUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
            current_skeleton.Skeleton.FlipX = false;
            enemyAnim = AnimationLib.getFrameAnimationSet("enemyPic");
            animation_time = 0.0f;
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
                            this.knockBack(en, this.position, this.dimensions, enemy_damage);
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
                else
                {

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
                                    current_skeleton = walk_right;
                                    velocity.X = -1.0f;
                                    velocity.Y = 0.0f;
                                    break;
                                case GlobalGameConstants.Direction.Left:
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    current_skeleton = walk_right;
                                    velocity.X = 1.0f;
                                    velocity.Y = 0.0f;
                                    break;
                                case GlobalGameConstants.Direction.Up:
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    current_skeleton = walk_down;
                                    velocity.Y = 1.0f;
                                    velocity.X = 0.0f;
                                    break;
                                default:
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    current_skeleton = walk_up;
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
                        animation_time = 0.0f;
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                        if (change_direction_time > 3000)
                        {
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
                            switch (change_direction)
                            {
                                case 0:
                                    velocity.X = 1.0f;
                                    velocity.Y = 0.0f;
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    current_skeleton = walk_right;
                                    break;
                                case 1:
                                    velocity.X = -1.0f;
                                    velocity.Y = 0.0f;
                                    direction_facing = GlobalGameConstants.Direction.Left;
                                    current_skeleton = walk_right;
                                    break;
                                case 2:
                                    velocity.X = 0.0f;
                                    velocity.Y = -1.0f;
                                    current_skeleton = walk_up;
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    break;
                                default:
                                    velocity.X = 0.0f;
                                    velocity.Y = 1.0f;
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    current_skeleton = walk_down;
                                    break;
                            }
                            change_direction_time = 0.0f;
                        }
                        else
                        {
                            velocity = Vector2.Zero;
                        }
                    }
                }
                
                Vector2 pos = new Vector2(position.X, position.Y);

                Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
                position.X = finalPos.X;
                position.Y = finalPos.Y;
            }

            if (enemy_life <= 0)
            {
                remove_from_list = true;
            }
            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, true);
        }

        public override void draw(SpriteBatch sb)
        {
        }
        public override void spinerender(SkeletonRenderer renderer)
        {
            if (direction_facing == GlobalGameConstants.Direction.Right || direction_facing == GlobalGameConstants.Direction.Up || direction_facing == GlobalGameConstants.Direction.Down)
            {
                current_skeleton.Skeleton.FlipX = false;
            }
            if (direction_facing == GlobalGameConstants.Direction.Left)
            {
                current_skeleton.Skeleton.FlipX = true;
            }

            current_skeleton.Skeleton.RootBone.X = CenterPoint.X * (current_skeleton.Skeleton.FlipX ? -1 : 1);
            current_skeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2f);

            current_skeleton.Skeleton.RootBone.ScaleX = 1.0f;
            current_skeleton.Skeleton.RootBone.ScaleY = 1.0f;

            current_skeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(current_skeleton.Skeleton);
        }
    }
}
