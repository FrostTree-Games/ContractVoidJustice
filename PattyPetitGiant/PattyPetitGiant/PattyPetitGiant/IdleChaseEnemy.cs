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
    class IdleChaseEnemy : Enemy, SpineEntity
    {
        private EnemyComponents component = null;
        private AnimationLib.FrameAnimationSet chaseAnim;

        private float distance = 0.0f;
        private float angle = 0.0f;
        public float Angle
        {
            set { angle = value; }
        }

        private float angle1 = 0.0f;
        public float Angle1
        {
            set { angle1 = value; }
        }
        private float angle2 = 0.0f;
        public float Angle2
        {
            set { angle2 = value; }
        }
        public IdleChaseEnemy(LevelState parentWorld, Vector2 position)
        {
            this.position = position;
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = new Vector2(0.0f, 0.0f);
            enemy_damage = 10;
            enemy_life = 15;
            distance = 0.0f;
            angle = 0.0f;
            angle1 = 0.0f;
            angle2 = 0.0f;
            knockback_magnitude = 2.0f;
            disable_movement_time = 0.0f;

            direction_facing = GlobalGameConstants.Direction.Right;
            change_direction_time = 0.0f;
            enemy_found = false;
            state = EnemyState.Idle;

            component = new IdleSearch();

            this.parentWorld = parentWorld;

            walk_down = AnimationLib.loadNewAnimationSet("chaseDown");
            walk_right = AnimationLib.loadNewAnimationSet("chaseRight");
            walk_up = AnimationLib.loadNewAnimationSet("chaseUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
            current_skeleton.Skeleton.FlipX = false;
            chaseAnim = AnimationLib.getFrameAnimationSet("chasePic");
            animation_time = 0.0f;
        }

        public override void update(GameTime currentTime)
        {
            change_direction_time += currentTime.ElapsedGameTime.Milliseconds;

            if (disable_movement == true)
            {
                disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                if (disable_movement_time > 300)
                {
                    velocity = Vector2.Zero;
                    disable_movement = false;
                    disable_movement_time = 0;
                }
            }
            else
            {
                switch (state)
                {
                    case EnemyState.Idle:
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en == this)
                                continue;
                            else if (en is Player)
                            {
                                //component.update(this, en, currentTime, parentWorld);
                                break;
                            }
                        }
                        if (enemy_found)
                        {
                            component = new Chase();
                            state = EnemyState.Chase;
                            change_direction_time = 0.0f;
                            animation_time = 0.0f;
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
                    case EnemyState.Chase:
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en == this)
                                continue;
                            else if (en is Player)
                            {
                                distance = (float)Math.Sqrt(Math.Pow((double)(en.Position.X - position.X), 2.0) + Math.Pow((double)(en.Position.Y - position.Y), 2.0));
                                if (hitTest(en))
                                {
                                    Vector2 direction = en.CenterPoint - CenterPoint;
                                    en.knockBack(direction, knockback_magnitude, enemy_damage);
                                }
                                else if (distance > 300)
                                {
                                    state = EnemyState.Idle;
                                    component = new IdleSearch();
                                    velocity = Vector2.Zero;
                                    animation_time = 0.0f;
                                    enemy_found = false;
                                }
                                else
                                {
                                    component.update(this, en, currentTime, parentWorld);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            if (enemy_life <= 0)
            {
                remove_from_list = true;
            }

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position.X = finalPos.X;
            position.Y = finalPos.Y;
            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, true);
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            /*
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle1, Vector2.Zero, new Vector2(600.0f, 10.0f),SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle2, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
            */
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (attacker == null)
            {
                return;
            }

            if (disable_movement_time == 0.0)
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
                enemy_life = enemy_life - damage;
            }
        }

        public override void spinerender(SkeletonRenderer renderer)
        {
            switch (direction_facing)
            {
                case GlobalGameConstants.Direction.Left:
                    current_skeleton = walk_right;
                    current_skeleton.Skeleton.FlipX = true;
                    break;
                case GlobalGameConstants.Direction.Right:
                    current_skeleton = walk_right;
                    current_skeleton.Skeleton.FlipX = false;
                    break;
                case GlobalGameConstants.Direction.Up:
                    current_skeleton = walk_up;
                    current_skeleton.Skeleton.FlipX = false;
                    break;
                default:
                    current_skeleton = walk_down;
                    current_skeleton.Skeleton.FlipX = false;
                    break;
            }
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");

            current_skeleton.Skeleton.RootBone.X = CenterPoint.X * (current_skeleton.Skeleton.FlipX ? -1 : 1);
            current_skeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2f);

            current_skeleton.Skeleton.RootBone.ScaleX = 1.0f;
            current_skeleton.Skeleton.RootBone.ScaleY = 1.0f;

            current_skeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(current_skeleton.Skeleton);
        }
    }
}
