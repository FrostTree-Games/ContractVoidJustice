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

        private int damage;
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
        public IdleChaseEnemy(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = new Vector2(0.0f, 0.0f);
            damage = 10;
            distance = 0.0f;
            angle = 0.0f;
            angle1 = 0.0f;
            angle2 = 0.0f;

            direction_facing = GlobalGameConstants.Direction.Right;
            change_direction_time = 0.0f;

            state = EnemyState.Idle;

            component = new IdleSearch();

            this.parentWorld = parentWorld;

            walk_down = AnimationLib.getSkeleton("chaseDown");
            walk_right = AnimationLib.getSkeleton("chaseRight");
            walk_up = AnimationLib.getSkeleton("chaseUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
            current_skeleton.Skeleton.FlipX = false;
            chaseAnim = AnimationLib.getFrameAnimationSet("chasePic");
            animation_time = 0.0f;
        }

        public override void update(GameTime currentTime)
        {
            change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
            switch (state)
            {
                case EnemyState.Idle:
                    current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                    foreach(Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                            continue;
                        else if (en is Player)
                        {
                            component.update(this, en, currentTime, parentWorld);
                            break;
                        }
                    }
                    if (state == Enemy.EnemyState.Chase)
                    {
                        component = new Chase();
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
                                en.knockBack(this, position, dimensions, damage);
                            }
                            else if (distance > 300)
                            {
                                state = EnemyState.Idle;
                                component = new IdleSearch();
                                velocity = Vector2.Zero;
                                animation_time = 0.0f;
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
            /*
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle1, Vector2.Zero, new Vector2(600.0f, 10.0f),SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle2, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
            */        
        }

        public override void knockBack(Entity other, Vector2 position, Vector2 dimensions, int damage)
        {

            if (disable_movement_time == 0.0)
            {
                disable_movement = true;
                float direction_x = CenterPoint.X - other.CenterPoint.X;
                float direction_y = CenterPoint.Y - other.CenterPoint.Y;

                if (Math.Abs(direction_x) > (Math.Abs(direction_y)))
                {
                    if (direction_x < 0)
                    {
                        velocity = new Vector2(-5.51f, direction_y / 100);
                    }
                    else
                    {
                        velocity = new Vector2(5.51f, direction_y / 100);
                    }
                }
                else
                {
                    if (direction_y < 0)
                    {
                        velocity = new Vector2(direction_x / 100f, -5.51f);
                    }
                    else
                    {
                        velocity = new Vector2(direction_x / 100f, 5.51f);
                    }
                }
                GlobalGameConstants.Player_Health = GlobalGameConstants.Player_Health - damage;
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
