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
    class ChaseEnemy : Enemy, SpineEntity
    {
        private EnemyComponents component = null;
        private AnimationLib.FrameAnimationSet chaseAnim;

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

            enemy_damage = 1;
            enemy_life = 15;

            walk_down = AnimationLib.getSkeleton("chaseDown");
            walk_right = AnimationLib.getSkeleton("chaseRight");
            walk_up = AnimationLib.getSkeleton("chaseUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
            current_skeleton.Skeleton.FlipX = false;
            chaseAnim = AnimationLib.getFrameAnimationSet("chasePic");
            animation_time = 0.0f;
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
                                en.knockBack(this,enemy_damage);
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
            if (enemy_life <= 0)
            {
                remove_from_list = true;
            }

            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, true);
        }

        public override void draw(SpriteBatch sb)
        {
            //sb.Draw(Game1.whitePixel, position, null, Color.Pink, 0.0f, Vector2.Zero, hitbox, SpriteEffects.None, 0.5f);
            //chaseAnim.drawAnimationFrame(0.0f, sb, position, new Vector2(3, 3), 0.5f);
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

            current_skeleton.Skeleton.RootBone.X = CenterPoint.X * (current_skeleton.Skeleton.FlipX ? -1 : 1);
            current_skeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2f);

            current_skeleton.Skeleton.RootBone.ScaleX = 1.0f;
            current_skeleton.Skeleton.RootBone.ScaleY = 1.0f;

            current_skeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(current_skeleton.Skeleton);
        }
    }
}
