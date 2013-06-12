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
            windUp,
            charge,
            none
        }
        private EnemyComponents component;
        private ChargerState charger_state;
        private float windup_timer;

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
            enemy_life = 10;
            player_found = false;
            change_direction_time = 0.0f;
            range_distance = 300.0f;

            state = EnemyState.Idle;
            enemy_type = EnemyType.Prisoner;
            component = new IdleSearch();
            charger_state = ChargerState.none;

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

            if (disable_movement == true)
            {
                disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;

                if (disable_movement_time > 300)
                {
                    disable_movement_time = 0.0f;
                    disable_movement = false;
                    velocity = Vector2.Zero;
                }
            }
            else
            {
                switch (state)
                {
                    case EnemyState.Idle:
                        directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("idle");

                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                        foreach (Entity en in parentWorld.EntityList)
                        {
                            if (en == this)
                                continue;
                            else if (en is Player)
                            {
                                if (player_found == true)
                                {
                                    switch (en.Direction_Facing)
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
                                else
                                {
                                    component.update(this, en, currentTime, parentWorld);
                                }
                            }
                        }

                        if (player_found)
                        {
                            state = EnemyState.Agressive;
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
                    case EnemyState.Agressive:
                        float distance = 0.0f;

                        switch (charger_state)
                        {
                            case ChargerState.windUp:
                                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("windUp");
                                windup_timer += currentTime.ElapsedGameTime.Milliseconds;
                                if (windup_timer > 300)
                                {
                                    animation_time = 0.0f;
                                    charger_state = ChargerState.charge;
                                    switch (direction_facing)
                                    {
                                        case GlobalGameConstants.Direction.Right:
                                            velocity = new Vector2(8.0f, 0.0f);
                                            break;
                                        case GlobalGameConstants.Direction.Left:
                                            velocity = new Vector2(-8.0f, 0.0f);
                                            break;
                                        case GlobalGameConstants.Direction.Up:
                                            velocity = new Vector2(0.0f, -8.0f);
                                            break;
                                        default:
                                            velocity = new Vector2(0.0f, 8.0f);
                                            break;
                                    }
                                }
                                break;
                            case ChargerState.charge:
                                directionAnims[(int)direction_facing].Animation = directionAnims[(int)direction_facing].Skeleton.Data.FindAnimation("charge");
                                foreach (Entity en in parentWorld.EntityList)
                                {
                                    if (en == this)
                                        continue;
                                    else if (en is Player)
                                    {
                                        distance = Vector2.Distance(en.CenterPoint, CenterPoint);
                                        if (distance > 300)
                                        {
                                            state = EnemyState.Idle;
                                            component = new IdleSearch();
                                            velocity = Vector2.Zero;
                                            animation_time = 0.0f;
                                            player_found = false;
                                            charger_state = ChargerState.none;
                                        }
                                    }

                                    if (hitTest(en))
                                    {
                                        Vector2 direction = en.CenterPoint - CenterPoint;
                                        en.knockBack(direction, knockback_magnitude, enemy_damage);
                                    }
                                }
                                break;
                            default:
                                charger_state = ChargerState.windUp;
                                windup_timer = 0.0f;
                                animation_time = 0.0f;
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position.X = finalPos.X;
            position.Y = finalPos.Y;

            directionAnims[(int)direction_facing].Animation.Apply(directionAnims[(int)direction_facing].Skeleton, animation_time, true);

            if (enemy_life <= 0)
            {
                remove_from_list = true;
            }
        }
        public override void draw(SpriteBatch sb)
        {
            //sb.Draw(Game1.whitePixel, position, null, Color.Green, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
        }
        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            if (disable_movement_time == 0.0)
            {
                if (state != EnemyState.Agressive)
                {
                    disable_movement = true;
                    player_found = true;

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
            }
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
