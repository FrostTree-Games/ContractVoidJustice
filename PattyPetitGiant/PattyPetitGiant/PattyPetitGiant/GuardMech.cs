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
    class GuardMech : Enemy, SpineEntity
    {
        public enum MechState
        {
            Moving,
            Firing,
            Melee,
            Reset,
            Death,
        }

        private const float enemy_range_damage = 10.0f;
        private const float enemy_melee_damage = 20.0f;
        private const float knockback_magnitude = 2.0f;

        private float windup_timer = 0.0f;
        private float firing_timer = 0.0f;
        private MechState mech_state = MechState.Moving;
        private EnemyComponents component = null;
        private Entity entity_found = null;
        private Grenades grenade;
        
        public GuardMech(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(72, 72);
            velocity = new Vector2(0.8f, 0.0f);

            windup_timer = 0.0f;
            firing_timer = 0.0f;

            enemy_life = 50;
            disable_movement = false;
            disable_movement_time = 0.0f;
            enemy_found = false;
            change_direction_time = 0.0f;
            this.parentWorld = parentWorld;
            enemy_type = EnemyType.Guard;
            change_direction_time_threshold = 3000.0f;
            direction_facing = GlobalGameConstants.Direction.Right;

            component = new MoveSearch();
            mech_state = MechState.Moving;
            enemy_type = EnemyType.Guard;
            velocity_speed = 3.0f;
            entity_found = null;

            grenade = new Grenades(Vector2.Zero, 0.0f);

            walk_down = AnimationLib.loadNewAnimationSet("squadSoldierDown");
            walk_right = AnimationLib.loadNewAnimationSet("squadSoldierRight");
            walk_up = AnimationLib.loadNewAnimationSet("squadSoldierUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
            current_skeleton.Skeleton.FlipX = false;
            animation_time = 0.0f;
        }

        public override void update(GameTime currentTime)
        {
            float distance = float.MaxValue;

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
                switch(mech_state)
                {
                    case MechState.Moving:
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                        foreach(Entity en in parentWorld.EntityList)
                        {
                            if (en == this)
                                continue;
                            else if (en.Enemy_Type != enemy_type && en.Enemy_Type != EnemyType.NoType)
                            {
                                component.update(this, en, currentTime, parentWorld);
                                if(enemy_found)
                                {
                                    entity_found = en;
                                    break;
                                }
                            }
                        }

                        switch (direction_facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                velocity = new Vector2(0.5f, 0f);
                                current_skeleton = walk_right;
                                break;
                            case GlobalGameConstants.Direction.Left:
                                velocity = new Vector2(-0.5f, 0f);
                                current_skeleton = walk_right;
                                break;
                            case GlobalGameConstants.Direction.Up:
                                velocity = new Vector2(0f, -0.5f);
                                current_skeleton = walk_up;
                                break;
                            default:
                                velocity = new Vector2(0.0f, 0.5f);
                                current_skeleton = walk_down;
                                break;
                        }

                        if (enemy_found)
                        {
                            distance = Vector2.Distance(position, entity_found.Position);
                            if (distance > 192 && distance < 300)
                            {
                                mech_state = MechState.Firing;
                            }
                            else
                            {
                                mech_state = MechState.Melee;
                            }
                        }
                        break;
                    case MechState.Firing:
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                        windup_timer += currentTime.ElapsedGameTime.Milliseconds;
                        float angle = (float)Math.Atan2(entity_found.CenterPoint.Y - CenterPoint.Y, entity_found.CenterPoint.X - CenterPoint.X);

                        distance = Vector2.Distance(position, entity_found.Position);

                        velocity = Vector2.Zero;
                        if (windup_timer > 2000)
                        {
                            windup_timer = 0.0f;
                            if (grenade.active == false)
                            {
                                grenade = new Grenades(position, angle);
                                grenade.active = true;
                            }
                                                        
                            //enemy_found = false;
                            //mech_state = MechState.Moving;
                        }
                        /*else if (distance < 192)
                        {
                            windup_timer = 0.0f;
                            mech_state = MechState.Melee;
                        }*/

                        Console.WriteLine(angle);
                        Console.WriteLine(direction_facing);
                        switch(direction_facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                if(angle < -1 * Math.PI/3.27)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    current_skeleton = walk_up;
                                }
                                else if (angle > Math.PI / 3.27)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    current_skeleton = walk_down;
                                }
                                break;
                            case GlobalGameConstants.Direction.Left:
                                if (angle < Math.PI / 1.44 && angle > Math.PI/1.5)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Down;
                                    current_skeleton = walk_down;
                                }
                                else if (angle > -1*Math.PI / 1.44 && angle< -1*Math.PI/1.5)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Up;
                                    current_skeleton = walk_up;
                                }
                                break;
                            case GlobalGameConstants.Direction.Up:
                                if (angle < -1 * Math.PI / 1.24)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Left;
                                    current_skeleton = walk_right;
                                }
                                else if (angle > -1 * Math.PI / 5.14)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    current_skeleton = walk_right;
                                }
                                break;
                            default:
                                if (angle < Math.PI / 5.14)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Right;
                                    current_skeleton = walk_right;
                                }
                                else if (angle > Math.PI / 1.24)
                                {
                                    direction_facing = GlobalGameConstants.Direction.Left;
                                    current_skeleton = walk_right;
                                }
                                break;
                        }

                        break;
                    case MechState.Melee:
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                        windup_timer += currentTime.ElapsedGameTime.Milliseconds;

                        Console.WriteLine("Melee");

                        velocity = Vector2.Zero;
                        if (windup_timer > 5000)
                        {
                            windup_timer = 0.0f;
                            enemy_found = false;
                            mech_state = MechState.Moving;
                        }
                        else if (distance > 192)
                        {
                            windup_timer = 0.0f;
                            mech_state = MechState.Firing;
                        }
                        break;
                    case MechState.Reset:
                        break;
                    case MechState.Death:
                        break;
                    default:
                        break;
                }
            }

            if (grenade.active)
            {
                grenade.update(parentWorld, currentTime, this);
            }

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position.X = finalPos.X;
            position.Y = finalPos.Y;

            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, true);
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.White, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
            if(grenade.active)
            {
                sb.Draw(Game1.whitePixel, grenade.Position, null, Color.Blue, 0.0f, Vector2.Zero, grenade.Dimensions, SpriteEffects.None, 0.5f);
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
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

        public struct Grenades
        {
            private Vector2 position;
            public Vector2 Position { get { return position; } }

            private Vector2 dimensions;
            public Vector2 Dimensions { get { return dimensions; } }

            private Vector2 velocity;
            public bool active;

            private enum GrenadeState
            {
                Travel,
                Explosion,
                Reset
            }
            private GrenadeState state;

            private float active_timer;
            private const float max_active_time = 2000.0f;

            public Grenades(Vector2 parent_position, float angle)
            {
                this.position = parent_position;
                dimensions = new Vector2(16, 16);
                velocity = new Vector2((float)(4*Math.Cos(angle)), (float)(4*Math.Sin(angle)));

                state = GrenadeState.Travel;

                active = false;
                active_timer = 0.0f;
            }

            public void update(LevelState parentWorld, GameTime currentTime, Entity parent)
            {
                active_timer += currentTime.ElapsedGameTime.Milliseconds;
                if (active)
                {
                    switch(state)
                    {
                        case GrenadeState.Travel:
                        if (active_timer > max_active_time)
                        {
                            active = false;
                            position = Vector2.Zero;
                        }
                        else
                        {
                            foreach (Entity en in parentWorld.EntityList)
                            {
                                if (en == parent)
                                    continue;
                                else if (grenadeHitTest(en))
                                {
                                    state = GrenadeState.Explosion;
                                    active_timer = 0.0f;
                                }
                            }
                            position += velocity;
                        }
                        break;
                        case GrenadeState.Explosion:
                        dimensions = new Vector2(96, 96);
                        position = position - (dimensions / 2);
                        if (active_timer > max_active_time)
                        {
                            active_timer = 0.0f;
                            state = GrenadeState.Travel;
                        }
                        break;
                        case GrenadeState.Reset:
                        break;
                    }
                }
            }

            public bool grenadeHitTest(Entity other)
            {
                if (position.X > other.Position.X + other.Dimensions.X || position.X + dimensions.X < other.Position.X || position.Y > other.Position.Y + other.Dimensions.Y || position.Y + dimensions.Y < other.Position.Y)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
