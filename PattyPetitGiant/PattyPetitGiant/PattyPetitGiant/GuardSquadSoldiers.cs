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
    class GuardSquadSoldiers : Enemy
    {
        private enum SquadSoldierState
        {
            Patrol,
            MoveIntoPosition,
            WindUp,
            Fire,
            Follow,
            Dying,
            IndividualPatrol
        }

        private SquadSoldierState state = SquadSoldierState.Patrol;
        private Vector2 follow_point = Vector2.Zero;
        public Vector2 Follow_Point
        {
            set { follow_point = value; }
            get { return follow_point; }
        }

        private GuardSquadLeader leader = null;
        public GuardSquadLeader Leader
        {
            set { leader = value; }
            get { return leader; }
        }

        private bool reset_state_flag = false;
        public bool Reset_State_Flag
        {
            get { return reset_state_flag; }
        }

        private float distance_from_follow_pt = 0.0f;
        private float angle = 0.0f;
        private float wind_up_timer = 0.0f;

        private int bullet_count;
        private int bullet_inactive_count;
        private float bullet_timer = 0.0f;
        private float firing_timer = 0.0f;
        private float move_timer = 0.0f;
        private bool loop = true;

        public static AnimationLib.FrameAnimationSet bulletPic = AnimationLib.getFrameAnimationSet("testBullet");

        private Entity entity_found = null;
        public Entity Entity_Found
        {
            set { entity_found = value; }
            get { return entity_found; }
        }

        private const int max_bullet_count = 10;
        private SquadBullet[] bullets = new SquadBullet[max_bullet_count];
        private EnemyComponents component = new MoveSearch();

        private string[] deathAnims = { "die", "die2", "die3" };

        public GuardSquadSoldiers(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = new Vector2(0.0f, 0.0f);
            
            enemy_damage = 20;
            enemy_life = 30;
            knockback_magnitude = 2.0f;
            disable_movement = false;
            disable_movement_time = 0.0f;
            enemy_found = false;
            change_direction_time = 0.0f;
            range_distance = 300.0f;
            wind_up_timer = 0.0f;
            bullet_count = 0;
            bullet_timer = 0.0f;
            firing_timer = 0.0f;
            sight_angle1 = 1.047f;
            sight_angle2 = 2.094f;

            reset_state_flag = false;
            this.parentWorld = parentWorld;
            change_direction_time_threshold = 4000.0f;
            enemy_type = EnemyType.Guard;

            prob_item_drop = 0.6;
            number_drop_items = 5;

            walk_down = AnimationLib.loadNewAnimationSet("squadSoldierDown");
            walk_right = AnimationLib.loadNewAnimationSet("squadSoldierRight");
            walk_up = AnimationLib.loadNewAnimationSet("squadSoldierUp");
            current_skeleton = walk_right;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
            current_skeleton.Skeleton.FlipX = false;
            animation_time = 0.0f;
        }

        public override void update(GameTime currentTime)
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
                if (leader.Death == true && (state == SquadSoldierState.Patrol || state == SquadSoldierState.MoveIntoPosition))
                {
                    state = SquadSoldierState.IndividualPatrol;
                }


                switch (state)
                {
                    case SquadSoldierState.Patrol:
                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                        reset_state_flag = false;
                        float distance = Vector2.Distance(Leader.CenterPoint, CenterPoint);
                        if (leader != null && death == false)
                        {
                            direction_facing = Leader.Direction_Facing;
                        }
                                                
                        switch (direction_facing)
                        {
                            case GlobalGameConstants.Direction.Up:
                                current_skeleton = walk_up;
                                break;
                            case GlobalGameConstants.Direction.Down:
                                current_skeleton = walk_down;
                                break;
                            default:
                                current_skeleton = walk_right;
                                break;
                        }

                        if (distance > 96)
                        {
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
                            distance_from_follow_pt = Vector2.Distance(follow_point, CenterPoint);
                            angle = (float)(Math.Atan2(follow_point.Y - CenterPoint.Y, follow_point.X - CenterPoint.X));
                            velocity = new Vector2(distance_from_follow_pt * (float)(Math.Cos(angle))/ 100.0f, distance_from_follow_pt * (float)(Math.Sin(angle)) / 100.0f);

                            if (Math.Abs(velocity.X) > (Math.Abs(velocity.Y)))
                            {
                                //enemy facing left
                                if (velocity.X < 0)
                                {
                                    velocity = new Vector2(-1.5f, velocity.Y);
                                }
                                //enemy facing right
                                else
                                {
                                    velocity = new Vector2(1.5f, velocity.Y);
                                }
                            }
                            else
                            {
                                //enemy facing up
                                if (velocity.Y < 0)
                                {
                                    velocity = new Vector2(velocity.X, -1.5f);
                                }
                                //enemy facing down
                                else
                                {
                                    velocity = new Vector2(velocity.X, 1.5f);
                                }
                            }
                        }
                        else
                        {
                            //current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                            velocity = Vector2.Zero;
                        }

                        if (enemy_found == true)
                        {
                            state = SquadSoldierState.MoveIntoPosition;
                            move_timer = 0.0f;
                            switch (direction_facing)
                            {
                                case GlobalGameConstants.Direction.Up:
                                    current_skeleton = walk_up;
                                    break;
                                case GlobalGameConstants.Direction.Down:
                                    current_skeleton = walk_down;
                                    break;
                                default:
                                    current_skeleton = walk_right;
                                    break;
                            }
                            animation_time = 0.0f;
                        }
                        break;
                    case SquadSoldierState.MoveIntoPosition:
                        distance_from_follow_pt = Vector2.Distance(follow_point, CenterPoint);
                        angle = (float)(Math.Atan2(follow_point.Y - CenterPoint.Y, follow_point.X - CenterPoint.X));
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
                        move_timer += currentTime.ElapsedGameTime.Milliseconds;
                        
                        if ((int)distance_from_follow_pt > 5 || move_timer < 1000)
                        {
                            velocity = new Vector2(distance_from_follow_pt * (float)(Math.Cos(angle)) / 100.0f, distance_from_follow_pt * (float)(Math.Sin(angle)) / 100.0f);

                            if (Math.Abs(velocity.X) > (Math.Abs(velocity.Y)))
                            {
                                //enemy facing left
                                if (velocity.X < 0)
                                {
                                    velocity = new Vector2(-2.5f, velocity.Y);
                                }
                                //enemy facing right
                                else
                                {
                                    velocity = new Vector2(2.5f, velocity.Y);
                                }
                            }
                            else
                            {
                                //enemy facing up
                                if (velocity.Y < 0)
                                {
                                    velocity = new Vector2(velocity.X, -2.5f);
                                }
                                //enemy facing down
                                else
                                {
                                    velocity = new Vector2(velocity.X, 2.5f);
                                }
                            }
                        }
                        else
                        {
                            velocity = Vector2.Zero;
                            state = SquadSoldierState.WindUp;
                            direction_facing = Leader.Direction_Facing;
                            animation_time = 0.0f;
                            move_timer = 0.0f;
                            wind_up_timer = 0.0f;
                        }
                        break;
                    case SquadSoldierState.WindUp:
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("windUp");
                        wind_up_timer += currentTime.ElapsedGameTime.Milliseconds;
                        if (wind_up_timer > 300)
                        {
                            state = SquadSoldierState.Fire;
                            animation_time = 0.0f;
                            firing_timer = 0.0f;
                            bullet_timer = 0.0f;
                            wind_up_timer = 0.0f;
                        }
                        break;
                    case SquadSoldierState.Fire:
                        switch (direction_facing)
                        {
                            case GlobalGameConstants.Direction.Up:
                                current_skeleton = walk_up;
                                break;
                            case GlobalGameConstants.Direction.Down:
                                current_skeleton = walk_down;
                                break;
                            default:
                                current_skeleton = walk_right;
                                break;
                        }
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("attack");
                        bullet_timer += currentTime.ElapsedGameTime.Milliseconds;
                        firing_timer += currentTime.ElapsedGameTime.Milliseconds;
                        angle = (float)Math.Atan2(current_skeleton.Skeleton.FindBone("muzzle").WorldY - current_skeleton.Skeleton.FindBone("gun").WorldY, current_skeleton.Skeleton.FindBone("muzzle").WorldX - current_skeleton.Skeleton.FindBone("gun").WorldX);
                        
                        if (bullet_timer > 100)
                        {
                            for (int i = 0; i < max_bullet_count; i++)
                            {
                                if (!bullets[i].active)
                                {
                                    AudioLib.playSoundEffect("machineGun");
                                    parentWorld.Particles.pushBulletCasing(new Vector2(current_skeleton.Skeleton.FindBone("gun").WorldX, current_skeleton.Skeleton.FindBone("gun").WorldY));
                                    bullets[i] = new SquadBullet(new Vector2(current_skeleton.Skeleton.FindBone("muzzle").WorldX, current_skeleton.Skeleton.FindBone("muzzle").WorldY), angle);

                                    bullets[i].velocity = new Vector2((float)(12.0 * Math.Cos(angle)), (float)(12.0 * Math.Sin(angle)));
                                    bullet_timer = 0.0f;
                                    break;
                                }
                            }
                        }

                        if (firing_timer > 3000)
                        {
                            reset_state_flag = true;
                            enemy_found = false;
                            firing_timer = 0.0f;
                            bullet_timer = 0.0f;
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                            if (leader != null)
                            {
                                state = SquadSoldierState.Patrol;
                            }
                            else
                            {
                                state = SquadSoldierState.IndividualPatrol;
                            }
                        }
                        break;

                    case SquadSoldierState.IndividualPatrol:
                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");

                        if (enemy_found == true)
                        {
                            state = SquadSoldierState.WindUp;
                            velocity = Vector2.Zero;
                            animation_time = 0.0f;
                        }
                        else
                        {
                            //checks where the player is
                            for (int i = 0; i < parentWorld.EntityList.Count; i++)
                            {
                                if (parentWorld.EntityList[i] == this || (parentWorld.EntityList[i] is Player && GameCampaign.PlayerAllegiance > 0.7))
                                {
                                    continue;
                                }

                                if (parentWorld.EntityList[i].Enemy_Type != enemy_type && parentWorld.EntityList[i].Enemy_Type != EnemyType.NoType)
                                {
                                    component.update(this, parentWorld.EntityList[i], currentTime, parentWorld);

                                }
                            }
                        }

                        switch (direction_facing)
                        {
                            case GlobalGameConstants.Direction.Up:
                                current_skeleton = walk_up;
                                break;
                            case GlobalGameConstants.Direction.Down:
                                current_skeleton = walk_down;
                                break;
                            default:
                                current_skeleton = walk_right;
                                break;
                        }
                        break;
                    case SquadSoldierState.Dying:
                        velocity = Vector2.Zero;
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

            animation_time += currentTime.ElapsedGameTime.Milliseconds / 1000f;
            current_skeleton.Animation.Apply(current_skeleton.Skeleton, animation_time, loop);
            
            for (int i = 0; i < max_bullet_count; i++)
            {
                if (bullets[i].active)
                {
                    bullets[i].update(parentWorld, currentTime, this);
                }
            }

            if( enemy_life <= 0 && death == false)
            {
                //remove_from_list = true;
                death = true;
                current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation(deathAnims[Game1.rand.Next() % 3]);
                loop = false;
                animation_time = 0.0f;
                state = SquadSoldierState.Dying;
                parentWorld.pushCoin(this);
            } 
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            for (int i = 0; i < max_bullet_count; i++)
            {
                if (bullets[i].active)
                {
                    //sb.DrawSpriteToSpineVertexArray(Game1.whitePixel, new Rectangle(0, 0, 1, 1), bullets[i].position, Color.White, 0.0f, new Vector2(10.0f, 10.0f));
                    bulletPic.drawAnimationFrame(0.0f, sb, bullets[i].position - (bulletPic.FrameDimensions / 2), new Vector2(1f), 0.5f, bullets[i].Bullet_Angle, bullets[i].CenterPoint, Color.White);
                }
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (death == false)
            {
                if (disable_movement_time == 0.0)
                {
                    disable_movement = true;

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
                }

                if (attacker == null)
                {
                    return;
                }
                else if (attacker.Enemy_Type != enemy_type && attacker.Enemy_Type != EnemyType.NoType)
                {
                    enemy_found = true;

                    if (enemy_life < 1 && !death && attacker != null & attacker is Player)
                    {
                        GameCampaign.AlterAllegiance(-0.08f);
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

        private struct SquadBullet
        {
            public bool active;
            public Vector2 position;
            public Vector2 dimensions;
            public Vector2 velocity;
            public Vector2 nextStep_temp;
            public Vector2 CenterPoint
            {
                get { return position + (dimensions / 2); }
            }

            private float bullet_angle;
            public float Bullet_Angle
            {
                get { return bullet_angle; }
            }

            public float time_alive;
            private const float max_time_alive = 1000f;
            private int bullet_damage;
            private float knockback_magnitude;
            
            public SquadBullet(Vector2 position, float angle)
            {
                this.position = position;
                active = true;
                this.dimensions = new Vector2(10, 10);
                this.velocity = Vector2.Zero;

                nextStep_temp = Vector2.Zero;

                bullet_angle = angle;

                bullet_damage = 5;
                knockback_magnitude = 2.0f;

                time_alive=0.0f;
            }

            public bool hitTestBullet(Entity other)
            {
                if ((position.X - (dimensions.X / 2)) > other.Position.X + other.Dimensions.X || (position.X + (dimensions.X / 2)) < other.Position.X || (position.Y - (dimensions.Y / 2)) > other.Position.Y + other.Dimensions.Y || (position.Y + (dimensions.Y / 2)) < other.Position.Y)
                {
                    return false;
                }
                return true;
            }

            public void update(LevelState parentWorld, GameTime currentTime, Entity parent)
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
                        parentWorld.Particles.pushImpactEffect(position - new Vector2(24), Color.White);

                        return;
                    }
                }

                if (time_alive > max_time_alive)
                {
                    parentWorld.Particles.pushImpactEffect(position - new Vector2(24), Color.White);

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
                        parentWorld.Particles.pushImpactEffect(position - new Vector2(24), Color.White);

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
