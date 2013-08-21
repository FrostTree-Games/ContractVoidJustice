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
        private  const float radius_max = 200.0f;
        private float distance;
        private float alert_timer;
        private Vector2 ball_coordinate;
        private Entity entity_found;

        private float sound_timer = 0.0f;
        private bool play_sound = true;
        
        private AnimationLib.FrameAnimationSet chain_ball;

        private string[] deathAnim = { "die", "die2", "die3" };

        private enum mutantBallState
        {
            Search,
            Alert,
            Agressive,
            KnockBack,
            Reset,
            Death
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

            radius = 0.0f;
            angle = 0.0f;
            change_direction_time = 0.0f;
            agressive_timer = 0.0f;
            distance = 0.0f;
            alert_timer = 0.0f;
            knockback_magnitude = 5.0f;
            range_distance = 250.0f;

            this.parentWorld = parentWorld;

            death = false;

            enemy_damage = 5;
            enemy_life = 5;
            enemy_type = EnemyType.Alien;

            prob_item_drop = 0.4;
            number_drop_items = 4;

            walk_down = AnimationLib.loadNewAnimationSet("ballMutantUp");
            walk_right = AnimationLib.loadNewAnimationSet("ballMutantRight");
            walk_up = AnimationLib.loadNewAnimationSet("ballMutantUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
            current_skeleton.Skeleton.FlipX = false;
            //chaseAnim = AnimationLib.getFrameAnimationSet("chasePic");
            animation_time = 0.0f;

            entity_found = null;

            chain_ball = AnimationLib.getFrameAnimationSet("snakeB");
        }

        public override void update(GameTime currentTime)
        {
            if (sound_alert && state == mutantBallState.Search && entity_found == null && !death)
            {
                state = mutantBallState.Alert;
                alert_timer = 0.0f;
            }

            if (enemy_life <= 0 && !death)
            {
                velocity = Vector2.Zero;
                death = true;
                AudioLib.playSoundEffect("alienChaserDie");
                current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation(deathAnim[Game1.rand.Next() % 3]);
                state = mutantBallState.Death;
                animation_time = 0.0f;
                parentWorld.pushCoin(this);
            }
            switch (state)
            {
                case mutantBallState.Search:
                    current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                    change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                    if (!enemy_found)
                    {
                        for (int i = 0; i < parentWorld.EntityList.Count; i++)
                        {
                            if (parentWorld.EntityList[i] == this)
                                continue;
                            else if (parentWorld.EntityList[i].Enemy_Type != enemy_type && parentWorld.EntityList[i].Enemy_Type != EnemyType.NoType)
                            {
                                component.update(this, parentWorld.EntityList[i], currentTime, parentWorld);
                                if (enemy_found)
                                {
                                    entity_found = parentWorld.EntityList[i];
                                    break;
                                }
                            }
                        }
                    }
                    if (enemy_found)
                    {
                        state = mutantBallState.Agressive;
                        velocity = Vector2.Zero;
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

                    switch (direction_facing)
                    {
                        case GlobalGameConstants.Direction.Right:
                            current_skeleton = walk_right;
                            break;
                        case GlobalGameConstants.Direction.Left:
                            current_skeleton = walk_right;
                            break;
                        case GlobalGameConstants.Direction.Up:
                            current_skeleton = walk_up;
                            break;
                        default:
                            current_skeleton = walk_down;
                            break;
                    }
                    break;
                /*case mutantBallState.Alert:
                    if (sound_alert && entity_found == null)
                    {
                        //if false then sound didn't hit a wall
                        if (!parentWorld.Map.soundInSight(this, sound_position))
                        {
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
                            alert_timer += currentTime.ElapsedGameTime.Milliseconds;
                            for (int i = 0; i < parentWorld.EntityList.Count; i++)
                            {
                                if (parentWorld.EntityList[i].Enemy_Type != enemy_type && parentWorld.EntityList[i].Enemy_Type != EnemyType.NoType && parentWorld.EntityList[i].Death == false)
                                {
                                    float distance = Vector2.Distance(CenterPoint, parentWorld.EntityList[i].CenterPoint);
                                    if (distance <= range_distance)
                                    {
                                        enemy_found = true;
                                        entity_found = parentWorld.EntityList[i];
                                        state = mutantBallState.Search;
                                        animation_time = 0.0f;
                                        sound_alert = false;
                                        animation_time = 0.0f;
                                        velocity = Vector2.Zero;
                                        agressive_timer = 0.0f;
                                        break;
                                    }
                                }
                            }

                            if (alert_timer > 3000 || ((int)CenterPoint.X == (int)sound_position.X && (int)CenterPoint.Y == (int)sound_position.Y))
                            {
                                entity_found = null;
                                enemy_found = false;
                                sound_alert = false;
                                state = mutantBallState.Search;
                                velocity = Vector2.Zero;
                                animation_time = 0.0f;
                                agressive_timer = 0.0f;
                                animation_time = 0.0f;
                            }
                        }
                        else
                        {
                            entity_found = null;
                            enemy_found = false;
                            sound_alert = false;
                            state = mutantBallState.Search;
                            velocity = Vector2.Zero;
                            animation_time = 0.0f;
                            agressive_timer = 0.0f;
                            animation_time = 0.0f;
                        }
                    }
                    else if (entity_found != null)
                    {
                        sound_alert = false;
                        float distance = Vector2.Distance(CenterPoint, entity_found.CenterPoint);
                        if (parentWorld.Map.enemyWithinRange(entity_found, this, range_distance) && distance < range_distance)
                        {
                            state = mutantBallState.Search;
                            animation_time = 0.0f;
                        }
                        else
                        {
                            entity_found = null;
                            enemy_found = false;
                            state = mutantBallState.Search;
                            velocity = Vector2.Zero;
                            animation_time = 0.0f;
                            agressive_timer = 0.0f;
                            animation_time = 0.0f;
                        }
                    }
                    break;*/
                case mutantBallState.Agressive:
                    agressive_timer += currentTime.ElapsedGameTime.Milliseconds;
                    current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("attack");
                    sound_timer += currentTime.ElapsedGameTime.Milliseconds;

                    if (sound_timer > 30)
                    {
                        play_sound = true;
                    }

                    float angle_from_entity = (float)Math.Atan2(entity_found.CenterPoint.Y - CenterPoint.Y, entity_found.CenterPoint.X - CenterPoint.X);

                    switch (direction_facing)
                    {
                        case GlobalGameConstants.Direction.Right:
                            if (angle_from_entity > Math.PI / 4)
                            {
                                direction_facing = GlobalGameConstants.Direction.Down;
                                current_skeleton = walk_down;
                            }
                            else if (angle_from_entity < -1 * Math.PI / 4)
                            {
                                direction_facing = GlobalGameConstants.Direction.Up;
                                current_skeleton = walk_up;
                            }
                            break;
                        case GlobalGameConstants.Direction.Left:
                            if (angle_from_entity > -3 * Math.PI / 4 && angle_from_entity < 0)
                            {
                                direction_facing = GlobalGameConstants.Direction.Up;
                                current_skeleton = walk_up;
                            }
                            else if (angle_from_entity < 3 * Math.PI / 4 && angle_from_entity > 0)
                            {
                                direction_facing = GlobalGameConstants.Direction.Down;
                                current_skeleton = walk_down;
                            }
                            break;
                        case GlobalGameConstants.Direction.Up:
                            if (angle_from_entity > -1 * Math.PI / 4)
                            {
                                direction_facing = GlobalGameConstants.Direction.Right;
                                current_skeleton = walk_right;
                            }
                            else if (angle_from_entity < -3 * Math.PI / 4)
                            {
                                direction_facing = GlobalGameConstants.Direction.Left;
                                current_skeleton = walk_right;
                            }
                            break;
                        default:
                            if (angle_from_entity > 3 * Math.PI / 4)
                            {
                                direction_facing = GlobalGameConstants.Direction.Left;
                                current_skeleton = walk_right;
                            }
                            else if (angle_from_entity < Math.PI / 4)
                            {
                                direction_facing = GlobalGameConstants.Direction.Right;
                                current_skeleton = walk_right;
                            }
                            break;
                    }
                    
                    velocity = new Vector2((float)Math.Cos(angle_from_entity), (float)Math.Sin(angle_from_entity));

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

                        for (int i = 0; i < parentWorld.EntityList.Count; i++)
                        {
                            if (parentWorld.EntityList[i] == this)
                                continue;
                            else if (parentWorld.EntityList[i].Death == false && hitTestBall(parentWorld.EntityList[i], ball_coordinate.X, ball_coordinate.Y))
                            {
                                if (play_sound)
                                {
                                    AudioLib.playSoundEffect("chargerImpact");
                                    play_sound = false;
                                    sound_timer = 0.0f;
                                }
                                float distance = Vector2.Distance(ball_coordinate, CenterPoint);
                                Vector2 direction = new Vector2(distance * (float)(Math.Cos(angle)), distance * (float)(Math.Sin(angle)));

                                float temp_knockback_magnitude = knockback_magnitude / (radius / temp_radius);
                                parentWorld.EntityList[i].knockBack(direction, temp_knockback_magnitude, enemy_damage, this);
                            }
                        }
                        temp_radius++;
                    }

                    if (agressive_timer > 4000)
                    {
                        state = mutantBallState.Search;
                        velocity = Vector2.Zero;
                        agressive_timer = 0.0f;
                        enemy_found = false;
                        entity_found = null;
                        animation_time = 0.0f;
                    }
                    break;
                case mutantBallState.KnockBack:
                    disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                    current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("hurt");
                    if (disable_movement_time > 300)
                    {
                        state = mutantBallState.Search;
                        disable_movement_time = 0.0f;
                        velocity = Vector2.Zero;
                        animation_time = 0.0f;
                    }
                    break;
                case mutantBallState.Death:
                    velocity = Vector2.Zero;
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
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, (state == mutantBallState.Death)? false:true);
        }
        public override void draw(Spine.SkeletonRenderer sb)
        {
            //sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), position, Color.Black, 0.0f, new Vector2(48));
            //sb.Draw(Game1.whitePixel, position, null, Color.Black, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 0.5f);
            //sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle, Vector2.Zero, new Vector2(radius, 10.0f), SpriteEffects.None, 0.5f);

            if (state == mutantBallState.Agressive)
            {
                float interpolate = Vector2.Distance(ball_coordinate, CenterPoint);

                for (int i = 0; i <= (int)(interpolate); i += 3)
                {
                    chain_ball.drawAnimationFrame(0.0f, sb, new Vector2(current_skeleton.Skeleton.FindBone("head").WorldX, current_skeleton.Skeleton.FindBone("head").WorldY) +  new Vector2(i * (float)(Math.Cos(angle)), i * (float)(Math.Sin(angle))), new Vector2(1.0f), 0.5f, angle, CenterPoint, Color.White);
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
            if (!death)
            {
                if (disable_movement == false)
                {
                    AudioLib.playSoundEffect("fleshyKnockBack");
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
                animation_time = 0.0f;
            }
            parentWorld.Particles.pushBloodParticle(CenterPoint);
            parentWorld.Particles.pushBloodParticle(CenterPoint);
            parentWorld.Particles.pushBloodParticle(CenterPoint);
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
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
