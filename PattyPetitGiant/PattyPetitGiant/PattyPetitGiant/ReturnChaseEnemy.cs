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
    class ReturnChaseEnemy : Enemy
    {
        private EnemyComponents component = null;
        private AnimationLib.FrameAnimationSet chaseAnim;

        private float distance = 0.0f;
        private Vector2 original_position = Vector2.Zero;
        private float distance_from_origin = 0.0f;
        private float return_timer = 0.0f;

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
        public ReturnChaseEnemy(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            original_position = CenterPoint;
            velocity = new Vector2(0.0f, 0.0f);
            enemy_damage = 10;
            enemy_life = 15;
            distance = 0.0f;
            angle = 0.0f;
            angle1 = 0.0f;
            angle2 = 0.0f;
            knockback_magnitude = 2.0f;
            disable_movement_time = 0.0f;
            return_timer = 0.0f;

            direction_facing = GlobalGameConstants.Direction.Right;
            change_direction_time = 0.0f;
            player_found = false;
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

            Console.WriteLine(velocity);

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
                                component.update(this, en, currentTime, parentWorld);
                                break;
                            }
                        }
                        if (player_found)
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
                        distance_from_origin = Vector2.Distance(CenterPoint, original_position);
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
                                else if (distance > 300 || Math.Abs(distance_from_origin) > 1500)
                                {
                                    //returns to original location
                                    state = EnemyState.Moving;
                                }
                                else
                                {
                                    component.update(this, en, currentTime, parentWorld);
                                }
                            }
                        }
                        break;
                    default:
                        return_timer += currentTime.ElapsedGameTime.Milliseconds;
                        if (return_timer > 2500)
                        {
                            position = original_position;
                            velocity = Vector2.Zero;
                            state = EnemyState.Idle;
                            component = new IdleSearch();
                            return_timer = 0.0f;
                        }
                        else
                        {
                            Vector2 direction = original_position - CenterPoint;
                            velocity = new Vector2(direction.X/100, direction.Y/100);
                        }
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

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Black, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 0.5f);
        }
        
    }
}
