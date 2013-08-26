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
    class HookPrisonerEnemy : Enemy, SpineEntity
    {
        private enum ChainState
        {
            Moving,
            WindUp,
            Throw,
            Pull,
            Neutral,
            knockBack,
            Death
        }
        private ChainState state;
        private EnemyComponents component;
        private float angle;
        private float distance;
        private const float max_chain_distance = 300.0f;
        private float chain_distance = 0.0f;
        private float chain_speed = 0.0f;
        private float windup_timer = 0.0f;
        private Vector2 chain_velocity;
        private Vector2 chain_position;
        private Vector2 chain_position_start;
        private Vector2 chain_dimensions;
        private Entity en_chained;
        private Entity target;
        private AnimationLib.SpineAnimationSet[] directionAnims = null;
        private AnimationLib.FrameAnimationSet hook;

        private string[] die_animations = { "die", "die2", "die3" };

        public HookPrisonerEnemy(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(32f, 90.0f);
            velocity = Vector2.Zero;
            chain_velocity = Vector2.Zero;
            chain_dimensions = new Vector2(10.0f, 10.0f);
            chain_position = position;
            
            disable_movement = false;
            disable_movement_time = 0.0f;
            knockback_magnitude = 10.0f;
            enemy_damage = 20;
            enemy_life = 25;
            enemy_found = false;
            change_direction_time = 0.0f;
            range_distance = 300.0f;
            angle = 0.0f;
            animation_time = 0.0f;

            prob_item_drop = 0.5;
            number_drop_items = 5;
            
            state = ChainState.Moving;
            enemy_type = EnemyType.Prisoner;
            component = new MoveSearch();
            en_chained = null;
            death = false;

            directionAnims = new AnimationLib.SpineAnimationSet[4];
            directionAnims[(int)GlobalGameConstants.Direction.Up] = AnimationLib.loadNewAnimationSet("hookUp");
            directionAnims[(int)GlobalGameConstants.Direction.Down] = AnimationLib.loadNewAnimationSet("hookDown");
            directionAnims[(int)GlobalGameConstants.Direction.Left] = AnimationLib.loadNewAnimationSet("hookRight");
            directionAnims[(int)GlobalGameConstants.Direction.Left].Skeleton.FlipX = true;
            directionAnims[(int)GlobalGameConstants.Direction.Right] = AnimationLib.loadNewAnimationSet("hookRight");

            hook = AnimationLib.getFrameAnimationSet("hook");

            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
            if ((!death && enemy_life <= 0))
            {
                death = true;
                state = ChainState.Death;
                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation(die_animations[Game1.rand.Next() % 3]);
                animation_time = 0.0f;
                parentWorld.pushCoin(this);
            }

            switch (state)
            {
                case ChainState.Moving:
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("run");
                    change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                    if(enemy_found == false)
                    {
                        for(int i = 0; i < parentWorld.EntityList.Count(); i++)
                        {
                            if (parentWorld.EntityList[i] == this || (parentWorld.EntityList[i] is Player && GameCampaign.PlayerAllegiance < 0.3))
                                continue;
                            else if (parentWorld.EntityList[i].Enemy_Type != enemy_type && parentWorld.EntityList[i].Enemy_Type != EnemyType.NoType && parentWorld.EntityList[i].Death == false)
                            {
                                component.update(this, parentWorld.EntityList[i], currentTime, parentWorld);
                                if (enemy_found)
                                {
                                    target = parentWorld.EntityList[i];
                                    break;
                                }
                            }
                        }
                    }

                    if (enemy_found)
                    {
                        state = ChainState.WindUp;
                        windup_timer = 0.0f;
                        velocity = Vector2.Zero;
                        animation_time = 0.0f;
                    }

                    Vector2 pos = new Vector2(position.X, position.Y);
                    Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                    Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
                    position.X = finalPos.X;
                    position.Y = finalPos.Y;
                    break;
                case ChainState.WindUp:
                    windup_timer += currentTime.ElapsedGameTime.Milliseconds;
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("windUp");
                    if (windup_timer > 670)
                    {
                        state = ChainState.Throw;
                        angle = (float)Math.Atan2(target.CenterPoint.Y - directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldY, target.CenterPoint.X - directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldX);
                        distance = Vector2.Distance(new Vector2(directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldX, directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldY), target.CenterPoint);

                        chain_position_start = new Vector2(directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldX, directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldY);

                        switch (direction_facing)
                        {
                            case GlobalGameConstants.Direction.Right:
                                chain_velocity = new Vector2((float)(distance * Math.Cos(angle) * 0.08f), (float)(distance * Math.Sin(angle)) * 0.08f);
                                Console.WriteLine("Chain Velocity " + chain_velocity);
                                chain_position = new Vector2(directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldX, directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldY);
                                chain_speed = chain_velocity.X;
                                break;
                            case GlobalGameConstants.Direction.Left:
                                chain_velocity = new Vector2((float)(distance * Math.Cos(angle)) * 0.08f, (float)(distance * Math.Sin(angle)) * 0.08f);
                                chain_position = new Vector2(directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldX, directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldY);
                                chain_speed = Math.Abs(chain_velocity.X);
                                break;
                            case GlobalGameConstants.Direction.Up:
                                chain_velocity = new Vector2((float)(distance * Math.Cos(angle) * 0.08f), (float)(distance * Math.Sin(angle)) * 0.08f);
                                chain_position = new Vector2(directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldX, directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldY);
                                chain_speed = Math.Abs(chain_velocity.Y);
                                break;
                            default:
                                chain_velocity = new Vector2((float)(distance * Math.Cos(angle) * 0.08f), (float)(distance * Math.Sin(angle)) * 0.08f);
                                chain_position = new Vector2(directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldX, directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldY);
                                chain_speed = Math.Abs(chain_velocity.Y);
                                break;
                        }
                        windup_timer = 0.0f;
                        animation_time = 0.0f;
                    }
                    break;
                case ChainState.Throw:
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("throw");
                    chain_distance += chain_speed;
                    chain_position += chain_velocity;
                    if (chain_distance >= max_chain_distance || parentWorld.Map.hitTestWall(chain_position))
                    {
                        state = ChainState.Pull;
                        chain_velocity = -1 * chain_velocity;
                    }

                    //checks to see if chain hits the player
                    if (hitTestChain(target, chain_position.X, chain_position.Y))
                    {
                        en_chained = target;
                        target.Disable_Movement = true;
                        target.Disable_Movement_Time = 0.0f;
                        target.Velocity = Vector2.Zero;
                        state = ChainState.Pull;
                        chain_velocity = -1 * chain_velocity;
                        animation_time = 0.0f;
                    }
                    break;
                //need to work on pull
                case ChainState.Pull:
                    if (chain_distance > 5)
                    {
                        directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("pull");
                        chain_position += chain_velocity;
                        chain_distance -= chain_speed;
                        if (en_chained != null)
                        {
                            en_chained.Position += chain_velocity;
                            en_chained.Disable_Movement = true;
                        }
                    }
                    else
                    {
                        directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("attack");
                        if (en_chained != null)
                        {
                            AudioLib.playSoundEffect("chargerImpact");
                            Vector2 direction = en_chained.CenterPoint - CenterPoint;
                            en_chained.Disable_Movement_Time = 0.0f;
                            en_chained.Disable_Movement = false;
                            en_chained.knockBack(direction, knockback_magnitude, enemy_damage, this);
                        }
                        state = ChainState.Neutral;
                        en_chained = null;
                        enemy_found = false;
                    }
                    break;
                case ChainState.knockBack:
                    disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                    directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("hurt");
                    if (disable_movement_time > 300)
                    {
                        disable_movement = false;
                        state = ChainState.Moving;
                        chain_distance = 0.0f;
                    }
                    break;
                case ChainState.Death:
                    velocity = Vector2.Zero;
                    break;
                default:
                    state = ChainState.Moving;
                    break;
            }
            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            directionAnims[(int)direction_facing].Animation.Apply(directionAnims[(int)direction_facing].Skeleton, animation_time, (state==ChainState.Moving||state == ChainState.Pull || state == ChainState.knockBack)?true:false);
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            //sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), position, Color.Green, 0.0f, new Vector2(48));

            if (state == ChainState.Throw || state == ChainState.Pull)
            {

                float interpolate = Vector2.Distance(chain_position, new Vector2(directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldX, directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldY));

                for (int i = 0; i <= (int)(interpolate); i += 16)
                {
                    hook.drawAnimationFrame(0.0f, sb, new Vector2(directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldX, directionAnims[(int)direction_facing].Skeleton.FindBone("rHand").WorldY) + new Vector2(i * (float)(Math.Cos(angle)), i * (float)(Math.Sin(angle))), new Vector2(1.0f), 0.5f, angle, Vector2.Zero, Color.White);
                }
            } 
        }
        
        public bool hitTestChain(Entity other, float x, float y)
        {
            if (x > other.Position.X + other.Dimensions.X || x < other.Position.X  || y > other.Position.Y + other.Dimensions.Y || y < other.Position.Y)
            {
                return false;
            }
            return true;
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (!death)
            {
                if (disable_movement == false)
                {
                    AudioLib.playSoundEffect("fleshyKnockBack");
                    if (state == ChainState.Neutral || state == ChainState.Neutral)

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
                    disable_movement_time = 0.0f;
                    disable_movement = true;
                    state = ChainState.knockBack;

                    if (enemy_life < 1 && !death && attacker != null & attacker is Player)
                    {
                        GameCampaign.AlterAllegiance(0.015f);
                    }
                }

                if (attacker == null)
                {
                    return;
                }
                else if (attacker.Enemy_Type != enemy_type && attacker.Enemy_Type != EnemyType.NoType)
                {
                    enemy_found = true;
                    target = attacker;

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
