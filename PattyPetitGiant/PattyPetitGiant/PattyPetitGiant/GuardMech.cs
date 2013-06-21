using System;
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

        public struct Grenades
        {
        }

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
            float distanceSCOPE = float.MaxValue;

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
                            float distance = Vector2.Distance(position, entity_found.Position);
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
                        
                        distanceSCOPE = Vector2.Distance(position, entity_found.Position);

                        Console.WriteLine("Range");

                        velocity = Vector2.Zero;
                        if (windup_timer > 5000)
                        {
                            windup_timer = 0.0f;
                            enemy_found = false;
                            mech_state = MechState.Moving;
                        }
                        else if (distanceSCOPE < 192)
                        {
                            windup_timer = 0.0f;
                            mech_state = MechState.Melee;
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
                        else if (distanceSCOPE > 192)
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
    }
}
