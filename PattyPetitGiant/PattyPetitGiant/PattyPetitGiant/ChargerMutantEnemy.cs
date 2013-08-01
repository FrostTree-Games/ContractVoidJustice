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
    class ChargerMutantEnemy : Enemy
    {
        private enum ChargerState
        {
            search,
            alert,
            windUp,
            charge,
            none,
            dying
        }
        private EnemyComponents component;
        private ChargerState state;
        private float windup_timer;
        private float charge_timer;
        private float alert_timer;
        private float angle;
        private Entity entity_found = null;

        private AnimationLib.SpineAnimationSet[] directionAnims = null;

        public ChargerMutantEnemy(LevelState parentWorld, Vector2 position)
        {
            this.position = position;
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = Vector2.Zero;

            disable_movement = false;
            disable_movement_time = 0.0f;
            windup_timer = 0.0f;
            knockback_magnitude = 8.0f;
            enemy_damage = 20;
            enemy_life = 15;
            enemy_found = false;
            change_direction_time = 0.0f;
            range_distance = 300.0f;
            charge_timer = 0.0f;
            alert_timer = 0.0f;
            range_distance = 600f;

            entity_found = null;
            state = ChargerState.search;
            enemy_type = EnemyType.Prisoner;
            component = new IdleSearch();

            sound_alert = false;

            direction_facing = (GlobalGameConstants.Direction)(Game1.rand.Next() % 4);

            this.parentWorld = parentWorld;

            directionAnims = new AnimationLib.SpineAnimationSet[4];
            directionAnims[(int)GlobalGameConstants.Direction.Up] = AnimationLib.loadNewAnimationSet("chargerUp");
            directionAnims[(int)GlobalGameConstants.Direction.Down] = AnimationLib.loadNewAnimationSet("chargerDown");
            directionAnims[(int)GlobalGameConstants.Direction.Left] = AnimationLib.loadNewAnimationSet("chargerRight");
            directionAnims[(int)GlobalGameConstants.Direction.Left].Skeleton.FlipX = true;
            directionAnims[(int)GlobalGameConstants.Direction.Right] = AnimationLib.loadNewAnimationSet("chargerRight");
            for (int i = 0; i < 4; i++)
            {
                directionAnims[i].Animation = directionAnims[i].Skeleton.Data.FindAnimation("idle");
            }
        }

        public override void update(GameTime currentTime)
        {
            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;


            if (sound_alert && state == ChargerState.search && entity_found == null)
            {
                state = ChargerState.alert;
                alert_timer = 0.0f;
            }

            if (state == ChargerState.dying)
            {
                velocity = Vector2.Zero;

                death = true;

                // may the programming gods have mercy on me hacking over this complicated state machine
                goto DeadSkipStates;
            }

            if (disable_movement == true)
            {
                disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;

                if (disable_movement_time > 300)
                {
                    disable_movement_time = 0.0f;
                    disable_movement = false;
                    velocity = Vector2.Zero;
                    state = ChargerState.alert;
                }

                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("hurt");
            }
            else
            {
                switch (state)
                {
                    case ChargerState.search:
                        directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("idle");

                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                        //first if state is if enemy was knocked back by a enemy of a different type
                        if (enemy_found == false)
                        {
                            for (int i = 0; i < parentWorld.EntityList.Count; i++)
                            {
                                if (parentWorld.EntityList[i] == this)
                                    continue;
                                else if (parentWorld.EntityList[i].Enemy_Type != enemy_type && parentWorld.EntityList[i].Enemy_Type != EnemyType.NoType && parentWorld.EntityList[i].Death == false)
                                {
                                    component.update(this, parentWorld.EntityList[i], currentTime, parentWorld);
                                    if (enemy_found == true)
                                    {
                                        entity_found = parentWorld.EntityList[i];
                                        break;
                                    }
                                }
                            }
                        }

                        if (enemy_found)
                        {
                            state = ChargerState.windUp;
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
                            entity_found = null;
                            sound_alert = false;
                        }
                        break;
                        case ChargerState.alert:
                        directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("idle");
                        
                        if (sound_alert && entity_found == null)
                        {
                            //if false then sound didn't hit a wall
                            if (!parentWorld.Map.soundInSight(this, sound_position))
                            {
                                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("run");
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
                                            state = ChargerState.windUp;
                                            animation_time = 0.0f;
                                            sound_alert = false;
                                            alert_timer = 0.0f;
                                            windup_timer = 0.0f;
                                            animation_time = 0.0f;
                                            velocity = Vector2.Zero;
                                            charge_timer = 0.0f;
                                            break;
                                        }
                                    }
                                }

                                if (alert_timer > 3000 || ((int)CenterPoint.X == (int)sound_position.X && (int)CenterPoint.Y == (int)sound_position.Y))
                                {
                                    entity_found = null;
                                    enemy_found = false;
                                    sound_alert = false;
                                    state = ChargerState.search;
                                    velocity = Vector2.Zero;
                                    animation_time = 0.0f;
                                    charge_timer = 0.0f;
                                    windup_timer = 0.0f;
                                    animation_time = 0.0f;
                                }
                            }
                            else
                            {
                                entity_found = null;
                                enemy_found = false;
                                sound_alert = false;
                                state = ChargerState.search;
                                velocity = Vector2.Zero;
                                animation_time = 0.0f;
                                charge_timer = 0.0f;
                                windup_timer = 0.0f;
                                animation_time = 0.0f;
                            }
                        }
                        else if(entity_found != null)
                        {
                            sound_alert = false;
                            float distance = Vector2.Distance(CenterPoint, entity_found.CenterPoint);
                            if (parentWorld.Map.enemyWithinRange(entity_found, this, range_distance) && distance < range_distance && entity_found.Death == false)
                            {
                                state = ChargerState.windUp;
                                animation_time = 0.0f;
                            }
                            else
                            {
                                entity_found = null;
                                enemy_found = false;
                                state = ChargerState.search;
                                velocity = Vector2.Zero;
                                animation_time = 0.0f;
                                charge_timer = 0.0f;
                                windup_timer = 0.0f;
                                animation_time = 0.0f;
                            }
                        }
                        break;
                        case ChargerState.windUp:
                            directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("windUp");
                            windup_timer += currentTime.ElapsedGameTime.Milliseconds;
                            angle = (float)Math.Atan2(entity_found.CenterPoint.Y - CenterPoint.Y, entity_found.CenterPoint.X - CenterPoint.X);

                            if (windup_timer > 300)
                            {
                                animation_time = 0.0f;
                                state = ChargerState.charge;
                                velocity = new Vector2(8.0f * (float)(Math.Cos(angle)), 8.0f * (float)(Math.Sin(angle)));

                                charge_timer = 0.0f;
                            }
                            break;
                        case ChargerState.charge:
                            directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("charge");
                           charge_timer += currentTime.ElapsedGameTime.Milliseconds;
                            for (int i = 0; i < parentWorld.EntityList.Count; i++)
                            {
                                if (parentWorld.EntityList[i] == this)
                                    continue;
                                if (hitTest(parentWorld.EntityList[i]))
                                {
                                    Vector2 direction = parentWorld.EntityList[i].CenterPoint - CenterPoint;
                                    parentWorld.EntityList[i].knockBack(direction, knockback_magnitude, enemy_damage);
                                }
                            }

                            if (charge_timer > 800)
                            {
                                if (entity_found.Death == true)
                                {
                                    entity_found = null;
                                }
                                sound_alert = false;
                                state = ChargerState.alert;
                                velocity = Vector2.Zero;
                                animation_time = 0.0f;
                                charge_timer = 0.0f;
                                windup_timer = 0.0f;
                                animation_time = 0.0f;
                                enemy_found = false;
                            }
                            break;
                        default:
                            break;
                }
            }

            DeadSkipStates:
            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position.X = finalPos.X;
            position.Y = finalPos.Y;

            directionAnims[(int)direction_facing].Animation.Apply(directionAnims[(int)direction_facing].Skeleton, animation_time, state == ChargerState.dying ? false : true);

            if (enemy_life <= 0 && state != ChargerState.dying && death == false)
            {
                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation(Game1.rand.Next() % 3 == 0 ? "die" : Game1.rand.Next() % 2 == 0 ? "die2" : "die3");
                death = true;
                state = ChargerState.dying;
                animation_time = 0;
            }
        }
        public override void draw(Spine.SkeletonRenderer sb)
        {
            //sb.Draw(Game1.whitePixel, position, null, Color.Green, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
        }
        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (!death)
            {
                if (disable_movement_time == 0.0)
                {
                    if (state != ChargerState.windUp || state != ChargerState.charge)
                    {
                        disable_movement = true;
                        animation_time = 0;
                        
                        AudioLib.playSoundEffect("fleshyKnockBack");

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
                    }

                    enemy_life = enemy_life - damage;

                    if (enemy_life < 1 && !death)
                    {
                        if (attacker != null & attacker is Player)
                        {
                            GameCampaign.AlterAllegiance(0.005f);
                        }
                    }
                }

                if (attacker == null)
                {
                    return;
                }
                if (attacker.Enemy_Type != enemy_type && attacker.Enemy_Type != EnemyType.NoType)
                {
                    enemy_found = true;
                    entity_found = attacker;
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
            parentWorld.Particles.pushBloodParticle(CenterPoint);
            parentWorld.Particles.pushBloodParticle(CenterPoint);
            parentWorld.Particles.pushBloodParticle(CenterPoint);
        }

        public override void spinerender(SkeletonRenderer renderer)
        {
            AnimationLib.SpineAnimationSet currentSkeleton = directionAnims[(int)direction_facing];

            currentSkeleton.Skeleton.RootBone.X = CenterPoint.X * (currentSkeleton.Skeleton.FlipX ? -1 : 1);
            currentSkeleton.Skeleton.RootBone.Y = CenterPoint.Y + (dimensions.Y / 2f);

            currentSkeleton.Skeleton.RootBone.ScaleX = 1.0f;
            currentSkeleton.Skeleton.RootBone.ScaleY = 1.0f;

            currentSkeleton.Skeleton.UpdateWorldTransform();
            renderer.Draw(currentSkeleton.Skeleton);
        }
    }
}
