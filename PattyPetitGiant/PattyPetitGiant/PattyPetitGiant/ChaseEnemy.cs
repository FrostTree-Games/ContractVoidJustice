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
        private enum ChaseAttackStage
        {
            windUp,
            attack,
            none
        }

        private EnemyComponents component = null;
        private AnimationLib.FrameAnimationSet chaseAnim;
        private float wind_anim = 0.0f;
        private Vector2 sword_hitbox;
        private Vector2 sword_position;
        private bool player_in_range;
        private ChaseAttackStage chase_stage;

        public ChaseEnemy(LevelState parentWorld, float initialx, float initialy)
        {
            position.X = initialx;
            position.Y = initialy;
            enemy_speed = 2.0f;
            velocity = new Vector2(0.0f, -1.0f*enemy_speed);
            dimensions = new Vector2(48f, 48f);
            sword_hitbox = new Vector2(48f, 48f);
            sword_position = position;

            state = EnemyState.Moving;
            component = new MoveSearch();
            direction_facing = GlobalGameConstants.Direction.Up;
            change_direction_time = 0.0f;
            this.parentWorld = parentWorld;
            player_found = false;
            player_in_range = false;
            chase_stage = ChaseAttackStage.none;
            
            enemy_damage = 1;
            enemy_life = 15;
            knockback_magnitude = 5.0f;
            wind_anim = 0.0f;

            walk_down = AnimationLib.getSkeleton("chaseDown");
            walk_right = AnimationLib.getSkeleton("chaseRight");
            walk_up = AnimationLib.getSkeleton("chaseUp");
            current_skeleton = walk_up;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
            current_skeleton.Skeleton.FlipX = false;
            chaseAnim = AnimationLib.getFrameAnimationSet("chasePic");
            animation_time = 0.0f;
        }

        public override void update(GameTime currentTime)
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
                switch (state)
                {
                    case EnemyState.Moving:
                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;

                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en == this)
                            {
                                continue;
                            }

                            if (en is Player)
                            {
                                float distance = (float)Math.Sqrt(Math.Pow((double)(en.Position.X - position.X), 2.0) + Math.Pow((double)(en.Position.Y - position.Y), 2.0));
                                component.update(this, en, currentTime, parentWorld);
                            }
                        }

                        if (player_found)
                        {
                            component = new Chase();
                            state = EnemyState.Chase;
                            animation_time = 0.0f;
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
                        }
                        break;
                    case EnemyState.Chase:
                        //checks to see if player was hit
                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                        //wind up
                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en == this)
                                continue;

                            if (en is Player)
                            {
                                //component won't update when the swing is in effect
                                float distance = Vector2.Distance(en.CenterPoint, CenterPoint);
                                switch(chase_stage)
                                {
                                    case ChaseAttackStage.windUp:
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
                                            chase_stage = ChaseAttackStage.attack;
                                            wind_anim = 0.0f;
                                            animation_time = 0.0f;
                                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("attack");
                                        }
                                        break;
                                    case ChaseAttackStage.attack:
                                        wind_anim += currentTime.ElapsedGameTime.Milliseconds;    
                                        //animation_time = 0.0f;
                                        if (swordSlashHitTest(en))
                                        {
                                            Vector2 direction = en.CenterPoint - CenterPoint;
                                            
                                            en.knockBack(direction, knockback_magnitude, enemy_damage);
                                        }
                                        if (wind_anim > 500)
                                        {
                                            dimensions = new Vector2(48f, 48f);
                                            wind_anim = 0.0f;
                                            animation_time = 0.0f;
                                            chase_stage = ChaseAttackStage.none;
                                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");

                                        }
                                        break;
                                    default:
                                        component.update(this, en, currentTime, parentWorld);
                                        if (distance < 64.0f)
                                        {
                                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("windUp");
                                            chase_stage = ChaseAttackStage.windUp;
                                            wind_anim = 0.0f;
                                            animation_time = 0.0f;
                                        }
                                        if (distance > 300.0f)
                                        {
                                            state = EnemyState.Moving;
                                            component = new MoveSearch();
                                            wind_anim = 0.0f;
                                        }
                                        break;
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

                if (enemy_life <= 0)
                {
                    remove_from_list = true;
                }

                animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
                current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, (wind_anim == 0)? true : false);
            }
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, sword_position, null, Color.Pink, 0.0f, Vector2.Zero, sword_hitbox, SpriteEffects.None, 0.5f);
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

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            if (disable_movement_time == 0.0)
            {
                disable_movement = true;
                if (Math.Abs(direction.X) > (Math.Abs(direction.Y)))
                {
                    if (direction.X < 0)
                    {
                        velocity = new Vector2(-1.0f * magnitude, direction.Y / 100 * magnitude);
                    }
                    else
                    {
                        velocity = new Vector2(1.0f * magnitude, direction.Y / 100 * magnitude);
                    }
                }
                else
                {
                    if (direction.Y < 0)
                    {
                        velocity = new Vector2(direction.X / 100f * magnitude, -1.0f * magnitude);
                    }
                    else
                    {
                        velocity = new Vector2((direction.X / 100f) * magnitude, 1.0f * magnitude);
                    }
                }
                enemy_life = enemy_life - damage;
            }
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
