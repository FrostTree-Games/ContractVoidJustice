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
    class GuardSquadLeader : Enemy, SpineEntity
    {
        private enum SquadLeaderState
        {
            Patrol,
            Direct,
            Fight,
            Reset,
            Dying,
        }

        private Vector2 follow_point_1 = Vector2.Zero;
        private Vector2 follow_point_2 = Vector2.Zero;
        
        private GuardSquadSoldiers[] squad_mates = new GuardSquadSoldiers[2];
        private SquadLeaderState state = SquadLeaderState.Patrol;
        private Entity entity_found = null;

        private const int max_number_bullets = 3;
        private const float time_between_shots_threshold = 500.0f;

        private Bullet[] bullets = new Bullet[max_number_bullets];
        private EnemyComponents component = new MoveSearch();
        private int bullet_number = 0;
        private float time_between_shots = 0.0f;
        private int bullet_inactive_number = 0;

        private bool loop = true;
        private bool death = false;
        private string[] deathAnims = { "die", "die2", "die3" };

        public GuardSquadLeader(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = new Vector2(1.0f, 0f);

            component = new MoveSearch();
            this.parentWorld = parentWorld;

            populateSquadMates();

            direction_facing = GlobalGameConstants.Direction.Right;
            state = SquadLeaderState.Patrol;

            follow_point_1 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(-3 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(-3 * Math.PI / 4)));
            if (squad_mates[0] != null)
            {
                squad_mates[0].Follow_Point = follow_point_1;
            }
            if (squad_mates[1] != null)
            {
                squad_mates[1].Follow_Point = follow_point_2;
            }

            enemy_life = 30;
            enemy_damage = 5;
            disable_movement = false;
            disable_movement_time = 0.0f;
            enemy_found = false;
            change_direction_time = 0.0f;
            range_distance = 300.0f;
            change_direction_time_threshold = 5000.0f;
            enemy_type = EnemyType.Guard;
            death = false;

            walk_down = AnimationLib.loadNewAnimationSet("squadLeaderDown");
            walk_right = AnimationLib.loadNewAnimationSet("squadLeaderRight");
            walk_up = AnimationLib.loadNewAnimationSet("squadLeaderUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
            current_skeleton.Skeleton.FlipX = false;
            animation_time = 0.0f;
        }

        public override void update(GameTime currentTime)
        {
            if (bullet_number > 0)
            {
                bullet_inactive_number = 0;
                for (int i = 0; i < bullet_number; i++)
                {
                    if (bullets[i].active)
                    {
                        bullets[i].update(parentWorld, this, currentTime);
                    }
                    else
                    {
                        bullet_inactive_number++;
                    }
                }

                if (bullet_inactive_number == max_number_bullets)
                {
                    bullet_number = 0;
                    bullet_inactive_number = 0;
                }
            }

            if (death == false)
            {
                if (disable_movement == true)
                {
                    disable_movement_time += currentTime.ElapsedGameTime.Milliseconds;
                    if (disable_movement_time > 300)
                    {
                        disable_movement = false;
                        disable_movement_time = 0.0f;
                        velocity = Vector2.Zero;
                    }
                }
                else
                {
                    switch (state)
                    {
                        case SquadLeaderState.Patrol:
                            change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");

                            if (enemy_found == false)
                            {
                                for (int i = 0; i < parentWorld.EntityList.Count; i++)
                                {
                                    if (parentWorld.EntityList[i] == this)
                                    {
                                        continue;
                                    }
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
                            switch (direction_facing)
                            {
                                case GlobalGameConstants.Direction.Right:
                                    follow_point_1 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(-3 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(-3 * Math.PI / 4)));
                                    follow_point_2 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(3 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(3 * Math.PI / 4)));
                                    current_skeleton = walk_right;
                                    break;
                                case GlobalGameConstants.Direction.Left:
                                    follow_point_1 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(Math.PI / 4)));
                                    follow_point_2 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(-1 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(-1 * Math.PI / 4)));
                                    current_skeleton = walk_right;
                                    break;
                                case GlobalGameConstants.Direction.Up:
                                    follow_point_1 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(3 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(3 * Math.PI / 4)));
                                    follow_point_2 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(Math.PI / 4)));
                                    current_skeleton = walk_up;
                                    break;
                                default:
                                    follow_point_1 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(-1 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(-1 * Math.PI / 4)));
                                    follow_point_2 = new Vector2((float)(CenterPoint.X + 64 * Math.Cos(-3 * Math.PI / 4)), (float)(CenterPoint.Y + 64 * Math.Sin(-3 * Math.PI / 4)));
                                    current_skeleton = walk_down;
                                    break;
                            }
                            if (squad_mates[0] != null)
                            {
                                squad_mates[0].Follow_Point = follow_point_1;
                            }
                            if (squad_mates[1] != null)
                            {
                                squad_mates[1].Follow_Point = follow_point_2;
                            }

                            if (enemy_found)
                            {
                                state = SquadLeaderState.Direct;

                                switch (direction_facing)
                                {
                                    case GlobalGameConstants.Direction.Right:
                                        follow_point_1.X = 128 + follow_point_1.X;
                                        follow_point_2.X = 128 + follow_point_2.X;
                                        break;
                                    case GlobalGameConstants.Direction.Left:
                                        follow_point_1.X = follow_point_1.X - 128;
                                        follow_point_2.X = follow_point_2.X - 128;
                                        break;
                                    case GlobalGameConstants.Direction.Up:
                                        follow_point_1.Y = follow_point_1.Y - 128;
                                        follow_point_2.Y = follow_point_2.Y - 128;
                                        break;
                                    default:
                                        follow_point_1.Y = follow_point_1.Y + 128;
                                        follow_point_2.Y = follow_point_2.Y + 128;
                                        break;
                                }

                                squad_mates[0].Enemy_Found = true;
                                squad_mates[1].Enemy_Found = true;

                                velocity = Vector2.Zero;
                            }
                            break;
                        case SquadLeaderState.Direct:

                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");

                            if (squad_mates[0] != null)
                            {
                                squad_mates[0].Follow_Point = follow_point_1;
                            }
                            if (squad_mates[1] != null)
                            {
                                squad_mates[1].Follow_Point = follow_point_2;
                            }

                            if ((squad_mates[0] != null && squad_mates[1] != null && squad_mates[0].Reset_State_Flag == true && squad_mates[1].Reset_State_Flag == true) || squad_mates[0] != null && squad_mates[0].Reset_State_Flag == true || squad_mates[1] != null && squad_mates[1].Reset_State_Flag == true)
                            {
                                state = SquadLeaderState.Patrol;
                                enemy_found = false;
                            }

                            float distance_from_enemy = Vector2.Distance(position, entity_found.Position);
                            /*if (distance_from_enemy > 96)
                            {
                                state = SquadLeaderState.Fight;
                            }*/
                            break;
                        case SquadLeaderState.Fight:
                            //firing bullet
                            time_between_shots += currentTime.ElapsedGameTime.Milliseconds;

                            if (bullet_number < max_number_bullets && time_between_shots > time_between_shots_threshold)
                            {
                                bullets[bullet_number] = new Bullet(new Vector2(current_skeleton.Skeleton.FindBone("muzzle").WorldX, current_skeleton.Skeleton.FindBone("muzzle").WorldY)); 
                                bullet_number++;
                                time_between_shots = 0.0f;
                            }
                            break;
                        case SquadLeaderState.Dying:
                            break;
                        default:
                            break;
                    }
                }
            }

            if (squad_mates[0] != null && squad_mates[0].Remove_From_List == true)
            {
                squad_mates[0].Follow_Point = follow_point_1;
            }
            if (squad_mates[1] != null && squad_mates[1].Remove_From_List == true)
            {
                squad_mates[1].Follow_Point = follow_point_2;
            }

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
            Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
            position.X = finalPos.X;
            position.Y = finalPos.Y;

            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, loop);

            for (int i = 0; i < max_number_bullets; i++)
            {
                if (bullets[i].active == true)
                {
                    bullets[i].update(parentWorld, this, currentTime);
                }
            }

            if(enemy_life <=0 && death == false)
            {
                current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation(deathAnims[Game1.rand.Next() % 3]);
                state = SquadLeaderState.Dying;
                animation_time = 0.0f;
                loop = false;
                death = true;
                //remove_from_list = true;
            }

        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            /*
            sb.Draw(Game1.whitePixel, position, null, Color.Blue, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
            sb.Draw(Game1.whitePixel, follow_point_1, null, Color.Orange, 0.0f, Vector2.Zero, new Vector2(16, 16), SpriteEffects.None, 1.0f);
            sb.Draw(Game1.whitePixel, follow_point_2, null, Color.Green, 0.0f, Vector2.Zero, new Vector2(16, 16), SpriteEffects.None, 1.0f);
             */
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (disable_movement_time == 0.0)
            {
                disable_movement = true;
                current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("hurt");

                if (Math.Abs(direction.X) > (Math.Abs(direction.Y)))
                {
                    if (direction.X < 0)
                    {
                        velocity = new Vector2(-2.0f * magnitude, direction.Y / 100 * magnitude);
                        direction_facing = GlobalGameConstants.Direction.Right;
                    }
                    else
                    {
                        velocity = new Vector2(2.0f * magnitude, direction.Y / 100 * magnitude);
                        direction_facing = GlobalGameConstants.Direction.Left;
                    }
                }
                else
                {
                    if (direction.Y < 0)
                    {
                        velocity = new Vector2(direction.X / 100f * magnitude, -2.0f * magnitude);
                        direction_facing = GlobalGameConstants.Direction.Down;
                    }
                    else
                    {
                        velocity = new Vector2((direction.X / 100f) * magnitude, 2.0f * magnitude);
                        direction_facing = GlobalGameConstants.Direction.Up;
                    }
                }
                enemy_life = enemy_life - damage;
                state = SquadLeaderState.Patrol;
            }

            if (attacker == null)
            {
                return;
            }
            else if (attacker.Enemy_Type != enemy_type && attacker.Enemy_Type != EnemyType.NoType)
            {
                enemy_found = true;
                entity_found = attacker;

                if (attacker != null & attacker is Player)
                {
                    GameCampaign.AlterAllegiance(-0.005f);
                }

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

        public void populateSquadMates()
        {
            GuardSquadSoldiers en = new GuardSquadSoldiers(parentWorld, position.X, position.Y);
            parentWorld.EntityList.Add(en);
            squad_mates[0] = en;
            squad_mates[0].Leader = this;

            en = new GuardSquadSoldiers(parentWorld, position.X, position.Y);
            parentWorld.EntityList.Add(en);
            squad_mates[1] = en;
            squad_mates[1].Leader = this;
        }

        public override void spinerender(SkeletonRenderer renderer)
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

        public struct Bullet
        {
            public bool active;
            private Vector2 velocity;
            private Vector2 position;
            private Vector2 dimensions;
            private Vector2 nextStep_temp;

            private const float max_time_alive = 2000.0f;

            private float knockback_magnitude;
            private float time_alive;
            private int bullet_damage;

            public Bullet(Vector2 position)
            {
                this.position = position;
                nextStep_temp = Vector2.Zero;
                velocity = Vector2.Zero;
                position = Vector2.Zero;
                dimensions = new Vector2(10, 10);
                knockback_magnitude = 3.0f;
                time_alive = 0.0f;
                bullet_damage = 3;
                active = true;
            }

            public bool hitTestBullet(Entity other)
            {
                if ((position.X - (dimensions.X / 2)) > other.Position.X + other.Dimensions.X || (position.X + (dimensions.X / 2)) < other.Position.X || (position.Y - (dimensions.Y / 2)) > other.Position.Y + other.Dimensions.Y || (position.Y + (dimensions.Y / 2)) < other.Position.Y)
                {
                    return false;
                }
                return true;
            }

            public void update(LevelState parentWorld, Entity parent, GameTime currentTime)
            {
                time_alive += currentTime.ElapsedGameTime.Milliseconds;

                for (int i = 0; i < parentWorld.EntityList.Count; i++)
                {
                    if (parentWorld.EntityList[i] == parent)
                        continue;
                    if (hitTestBullet(parentWorld.EntityList[i]))
                    {
                        Vector2 direction = parentWorld.EntityList[i].Position - position;
                        parentWorld.EntityList[i].knockBack(direction, knockback_magnitude, bullet_damage, parent);
                        active = false;
                        return;
                    }
                }

                if (time_alive > max_time_alive)
                {
                    active = false;
                }
                else
                {
                    nextStep_temp = new Vector2(position.X - (dimensions.X / 2) + velocity.X, (position.Y + velocity.X));
                }

                bool on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                int check_corners = 0;
                while (check_corners != 4)
                {
                    if (on_wall == false)
                    {
                        if (check_corners == 0)
                        {
                            nextStep_temp = new Vector2(position.X + (dimensions.X / 2) + velocity.X, position.Y + velocity.Y);
                        }
                        else if (check_corners == 1)
                        {
                            nextStep_temp = new Vector2(position.X + velocity.X, position.Y - (dimensions.Y / 2) + velocity.Y);
                        }
                        else if (check_corners == 2)
                        {
                            nextStep_temp = new Vector2(position.X + velocity.X, position.Y + dimensions.Y + velocity.Y);
                        }
                        else
                        {
                            position += velocity;
                        }
                        on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                    }
                    else
                    {
                        active = false;
                        time_alive = 0.0f;
                        break;
                    }
                    check_corners++;
                }
            }
        }
    }
}
