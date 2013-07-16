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
    class BallMutant : Enemy, SpineEntity
    {
        private EnemyComponents component = null;
        private float radius;
        private float angle;
        private float agressive_timer;
        private float radius_max;
        private float distance;
        private Vector2 ball_coordinate;

        private AnimationLib.FrameAnimationSet chain_ball;

        private enum mutantBallState
        {
            Search,
            Agressive,
            KnockBack,
            Reset
        }
        private mutantBallState state;

        public BallMutant(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48f, 48f);
            velocity = Vector2.Zero;
            ball_coordinate = Vector2.Zero;

            state = mutantBallState.Search;
            component = new IdleSearch();
            direction_facing = GlobalGameConstants.Direction.Right;

            radius_max = 200.0f;
            radius = 0.0f;
            angle = 0.0f;
            change_direction_time = 0.0f;
            agressive_timer = 0.0f;
            distance = 0.0f;
            knockback_magnitude = 5.0f;

            this.parentWorld = parentWorld;

            enemy_damage = 5;
            enemy_life = 25;
            enemy_type = EnemyType.Prisoner;

            walk_down = AnimationLib.getSkeleton("chaseDown");
            walk_right = AnimationLib.getSkeleton("chaseRight");
            walk_up = AnimationLib.getSkeleton("chaseUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
            current_skeleton.Skeleton.FlipX = false;
            //chaseAnim = AnimationLib.getFrameAnimationSet("chasePic");
            animation_time = 0.0f;

            chain_ball = AnimationLib.getFrameAnimationSet("hook");
        }

        public override void update(GameTime currentTime)
        {
            switch (state)
            {
                case mutantBallState.Search:
                    change_direction_time += currentTime.ElapsedGameTime.Milliseconds;

                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                            continue;
                        else if (en.Enemy_Type != enemy_type && en.Enemy_Type != EnemyType.NoType)
                        {
                            component.update(this, en, currentTime, parentWorld);
                        }
                    }

                    if (enemy_found)
                    {
                        state = mutantBallState.Agressive;
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
                case mutantBallState.Agressive:
                    agressive_timer += currentTime.ElapsedGameTime.Milliseconds;
                    
                    angle += 0.1f;
                    if (angle >= (float)(2 * Math.PI))
                    {
                        angle = 0;
                    }

                    float temp_radius = 0.0f;
                        
                    while (temp_radius <= radius)
                    {
                        ball_coordinate.X = CenterPoint.X + temp_radius * (float)(Math.Cos(angle));
                        ball_coordinate.Y = CenterPoint.Y + temp_radius * (float)(Math.Sin(angle));

                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en == this)
                                continue;
                            else if (hitTestBall(en, ball_coordinate.X, ball_coordinate.Y))
                            {
                                float distance = Vector2.Distance(ball_coordinate, CenterPoint);
                                Vector2 direction = new Vector2(distance * (float)(Math.Cos(angle)), distance * (float)(Math.Sin(angle)));

                                float temp_knockback_magnitude = knockback_magnitude / (radius / temp_radius);
                                en.knockBack(direction, temp_knockback_magnitude, enemy_damage, this);
                            }
                        }
                        temp_radius++;

                        if (agressive_timer > 4000)
                        {
                            state = mutantBallState.Search;
                            agressive_timer = 0.0f;
                            enemy_found = false;
                        }
                    }
                    break;
                case mutantBallState.KnockBack:
                    disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;

                    if (disable_movement_time > 300)
                    {
                        state = mutantBallState.Search;
                        disable_movement_time = 0.0f;
                        velocity = Vector2.Zero;
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
        }
        public override void draw(Spine.SkeletonRenderer sb)
        {
            sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), position, Color.Black, 0.0f, new Vector2(48));
                //sb.Draw(Game1.whitePixel, position, null, Color.Black, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 0.5f);
            //sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle, Vector2.Zero, new Vector2(radius, 10.0f), SpriteEffects.None, 0.5f);

            if (state == mutantBallState.Agressive)
            {
                float interpolate = Vector2.Distance(ball_coordinate, CenterPoint);

                for (int i = 0; i <= (int)(interpolate); i += 16)
                {
                    chain_ball.drawAnimationFrame(0.0f, sb, CenterPoint + new Vector2(i * (float)(Math.Cos(angle)), i * (float)(Math.Sin(angle))), new Vector2(1.0f), 0.5f, angle, CenterPoint, Color.White);
                }
            }
            if (state == mutantBallState.Agressive)
            {
                if (radius < radius_max)
                    radius += 2.0f;
            }
            else
            {
                if (radius > 0)
                {
                    radius -= 2.0f;
                    angle += 0.1f;
                }
            }
        }
        
        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (disable_movement == false)
            {
                state = mutantBallState.KnockBack;
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

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            base.spinerender(renderer);
        }
        public bool hitTestBall(Entity other, float x, float y)
        {
            if (x > other.Position.X + other.Dimensions.X || x < other.Position.X || y > other.Position.Y + other.Dimensions.Y || y < other.Position.Y)
            {
                return false;
            }
            return true;
        }
    }
}
