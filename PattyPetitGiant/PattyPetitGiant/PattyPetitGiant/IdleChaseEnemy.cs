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
        public IdleChaseEnemy(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = new Vector2(0.0f, 0.0f);
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
                    foreach(Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                            continue;
                        else if (en is Player)
                        {
                            component.update(this, en, currentTime, parentWorld);

                            angle = (float)(Math.Atan2(CenterPoint.X - en.CenterPoint.X, CenterPoint.Y - en.CenterPoint.Y));
                            angle = angle + (float)(Math.PI / 2);

                            distance = Vector2.Distance(CenterPoint, en.CenterPoint);

                            break;
                        }
                    }
                    if (change_direction_time > 2000)
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
                    break;
                case EnemyState.Chase:
                    if (change_direction_time > 5000)
                    {
                        state = EnemyState.Idle;
                        change_direction_time = 0.0f;
                    }
    
                /*foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                            continue;
                        else if (en is Player)
                        {
                            distance = (float)Math.Sqrt(Math.Pow((double)(en.Position.X - position.X), 2.0) + Math.Pow((double)(en.Position.Y - position.Y), 2.0));
                            if(distance > 100)
                            {
                                state = EnemyState.Idle;
                            }
                        }
                    }*/
                    break;
                default:
                    break;
            }

            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, true);
        }

        public override void draw(SpriteBatch sb)
        {
            Console.WriteLine(angle*180/Math.PI);

            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle1, Vector2.Zero, new Vector2(600.0f, 10.0f),SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle2, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
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
