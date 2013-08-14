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
        private enum ChaseState
        {
            search,
            chase,
            windUp,
            attack,
            knockback,
            none,
            death
        }

        private EnemyComponents component = null;
        private AnimationLib.FrameAnimationSet chaseAnim;
        private float wind_anim = 0.0f;
        private Vector2 sword_hitbox;
        private Vector2 sword_position;
        private bool player_in_range;

        private ChaseState state = ChaseState.search;

        private string[] death_anim = { "die", "die2", "die3" };
        
        private EnemyComponents chaseComponent = null;
        private EnemyComponents searchComponent = null;

        private bool play_sound;

        private Entity chase_target;

        public ChaseEnemy(LevelState parentWorld, float initial_x, float initial_y)
        {
            this.position = new Vector2(initial_x, initial_y);
            enemy_speed = 2.0f;
            velocity = new Vector2(0.0f, -1.0f*enemy_speed);
            dimensions = new Vector2(48f, 48f);
            sword_hitbox = new Vector2(48f, 48f);
            sword_position = position;

            state = ChaseState.search;
            chaseComponent = new Chase();
            searchComponent = new MoveSearch();
            component = searchComponent;
            direction_facing = GlobalGameConstants.Direction.Up;
            change_direction_time = 0.0f;
            this.parentWorld = parentWorld;
            enemy_found = false;
            player_in_range = false;
            chase_target = null;
            play_sound = true;

            enemy_type = EnemyType.Prisoner;
            enemy_damage = 1;
            enemy_life = 15;
            knockback_magnitude = 5.0f;
            wind_anim = 0.0f;
            number_drop_items = 3;
            prob_item_drop = 0.3;

            walk_down = AnimationLib.loadNewAnimationSet("chaseDown");
            walk_right = AnimationLib.loadNewAnimationSet("chaseRight");
            walk_up = AnimationLib.loadNewAnimationSet("chaseUp");
            current_skeleton = walk_up;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
            current_skeleton.Skeleton.FlipX = false;
            chaseAnim = AnimationLib.getFrameAnimationSet("chasePic");
            animation_time = 0.0f;
        }

        public override void update(GameTime currentTime)
        {
            if (enemy_life <= 0 && death == false)
            {
                death = true;
                state = ChaseState.death;
                animation_time = 0.0f;
                wind_anim = 1;
                current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation(death_anim[(int)(Game1.rand.Next()) % 3]);

                parentWorld.pushCoin(this);
            }

            /*if (Math.Abs(velocity.X) > 7 || Math.Abs(velocity.Y) > 7)
            {
                Console.WriteLine(velocity);
                Console.WriteLine(state);
            }*/

            switch (state)
            {
                case ChaseState.search:
                    change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                    current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
                    for (int i = 0; i < parentWorld.EntityList.Count; i++)
                    {
                        if (parentWorld.EntityList[i] == this || (parentWorld.EntityList[i] is Player && GameCampaign.PlayerAllegiance < 0.3))
                        {
                            continue;
                        }

                        if (parentWorld.EntityList[i].Enemy_Type != enemy_type && parentWorld.EntityList[i].Enemy_Type != EnemyType.NoType && !parentWorld.EntityList[i].Death)
                        {
                            component.update(this, parentWorld.EntityList[i], currentTime, parentWorld);
                            if (enemy_found)
                            {
                                component = chaseComponent;
                                state = ChaseState.chase;
                                animation_time = 0.0f;
                                current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("chase");
                                chase_target = parentWorld.EntityList[i];
                                break;
                            }
                        }
                    }
                    break;
                case ChaseState.chase:
                    //checks to see if player was hit
                    //wind up
                        
                    //component won't update when the swing is in effect
                    float distance = Vector2.Distance(chase_target.CenterPoint, CenterPoint);
                    current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("chase");
                    if (distance > 300.0f || chase_target.Death)
                    {
                        state = ChaseState.search;
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
                        component = searchComponent;
                        enemy_found = false;
                        wind_anim = 0.0f;
                        animation_time = 0.0f;
                    }
                    else
                    {
                        component.update(this, chase_target, currentTime, parentWorld);
                        if (distance < 64.0f)
                        {
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("windUp");
                            state = ChaseState.windUp;
                            wind_anim = 0.0f;
                            animation_time = 0.0f;
                        }
                    }
                    break;
                case ChaseState.windUp:
                    wind_anim += currentTime.ElapsedGameTime.Milliseconds;
                    //animation_time = 0.0f;
                                        
                    velocity = Vector2.Zero;
                    switch (direction_facing)
                    {
                        case GlobalGameConstants.Direction.Right:
                            sword_position.X = position.X + dimensions.X;
                            sword_position.Y = position.Y;
                            break;
                        case GlobalGameConstants.Direction.Left:
                            sword_position.X = position.X - sword_hitbox.X;
                            sword_position.Y = position.Y;
                            break;
                        case GlobalGameConstants.Direction.Up:
                            sword_position.Y = position.Y - sword_hitbox.Y;
                            sword_position.X = CenterPoint.X - sword_hitbox.X / 2;
                            break;
                        default:
                            sword_position.Y = CenterPoint.Y + dimensions.Y / 2;
                            sword_position.X = CenterPoint.X - sword_hitbox.X / 2;
                            break;
                    }
                    if(wind_anim > 300)
                    {
                        state = ChaseState.attack;
                        wind_anim = 0.0f;
                        animation_time = 0.0f;
                    }
                    break;
                case ChaseState.attack:
                    wind_anim += currentTime.ElapsedGameTime.Milliseconds;
                    current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("attack");
                    //animation_time = 0.0f;
                    if (swordSlashHitTest(chase_target))
                    {
                        Vector2 direction = chase_target.CenterPoint - CenterPoint;

                        if (play_sound)
                        {
                            AudioLib.playSoundEffect("swordHit");
                            play_sound = false;
                        }
                        chase_target.knockBack(direction, knockback_magnitude, enemy_damage);
                    }
                    else
                    {
                        if (play_sound)
                        {
                            AudioLib.playSoundEffect("testSword");
                            play_sound = false;
                        }
                    }
                    if (wind_anim > 500)
                    {
                        play_sound = true;
                        dimensions = new Vector2(48f, 48f);
                        wind_anim = 0.0f;
                        animation_time = 0.0f;
                        state = ChaseState.chase;
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("chase");

                    }
                    break;
                case ChaseState.knockback:
                    if (death == false)
                    {
                        disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("hurt");
                        if (disable_movement_time > 300)
                        {
                                state = ChaseState.search;
                                velocity = Vector2.Zero;
                                component = searchComponent;
                            disable_movement = false;
                            disable_movement_time = 0;
                            animation_time = 0.0f;
                        }
                    }
                    break;
                case ChaseState.death:
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

            //decides if the animation loops or not
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, (wind_anim == 0) ? true : false);

        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            //sb.Draw(Game1.whitePixel, sword_position, null, Color.Pink, 0.0f, Vector2.Zero, sword_hitbox, SpriteEffects.None, 0.5f);
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

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (death == false)
            {
                if (disable_movement == false)
                {
                    AudioLib.playSoundEffect("fleshyKnockBack");
                    disable_movement = true;
                    disable_movement_time = 0.0f;
                    state = ChaseState.knockback;

                    if (Math.Abs(direction.X) > (Math.Abs(direction.Y)))
                    {
                        if (direction.X < 0)
                        {
                            velocity = new Vector2(-2.0f * magnitude, direction.Y / 100 * magnitude);
                        }
                        else
                        {
                            velocity = new Vector2(2.0f * magnitude, direction.Y / 100 * magnitude);
                        }
                    }
                    else
                    {
                        if (direction.Y < 0)
                        {
                            velocity = new Vector2(direction.X / 100f * magnitude, -2.0f * magnitude);
                        }
                        else
                        {
                            velocity = new Vector2((direction.X / 100f) * magnitude, 2.0f * magnitude);
                        }
                    }
                    enemy_life = enemy_life - damage;

                    if (enemy_life < 1 && !death)
                    {
                        if (attacker != null & attacker is Player)
                        {
                            GameCampaign.AlterAllegiance(0.015f);
                        }
                    }
                }

                if (attacker == null)
                {
                    return;
                }
                else if (attacker.Enemy_Type != enemy_type && attacker.Enemy_Type != EnemyType.NoType)
                {
                    enemy_found = true;

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

        public bool swordSlashHitTest(Entity other)
        {
            if (sword_position.X > other.Position.X + other.Dimensions.X || sword_position.X + sword_hitbox.X < other.Position.X || sword_position.Y > other.Position.Y + other.Dimensions.Y || sword_position.Y + sword_hitbox.Y < other.Position.Y)
            {
                return false;
            }

            return true;
        }
    }
}
