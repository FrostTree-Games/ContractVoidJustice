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
    struct EnemyBullet
    {
        public Vector2 hitbox;
        public Vector2 position;
        public Vector2 velocity;
        public int bullet_damage;
        public GlobalGameConstants.Direction bullet_direction;
    }

    class RangeEnemy : Enemy, SpineEntity
    {
        private int change_direction;
        private AnimationLib.FrameAnimationSet enemyAnim;
        private Random rand;
        private EnemyBullet bullet;
        private bool bullet_alive;
        private float bullet_alive_time;
        private float pause_time = 0.0f;

        private EnemyComponents component = null;


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

        public RangeEnemy(LevelState parentWorld, Vector2 position)
        {
            this.position = position;
            this.parentWorld = parentWorld;
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = new Vector2(0.0f, 1.0f);
            enemy_found = false;

            state = EnemyState.Moving;
            component = new MoveSearch();
            direction_facing = GlobalGameConstants.Direction.Down;

            change_direction = 0;
            change_direction_time = 0.0f;
            disable_movement_time = 0.0f;

            enemy_life = 10;
            enemy_damage = 2;
            damage_player_time = 0.0f;
            knockback_magnitude = 1.0f;

            rand = new Random(DateTime.Now.Millisecond);
            enemyAnim = AnimationLib.getFrameAnimationSet("rangeenemyPic");
            bullet_alive = false;
            bullet.bullet_damage = 5;

            walk_down = AnimationLib.loadNewAnimationSet("zippyGunDown");
            walk_right = AnimationLib.loadNewAnimationSet("zippyGunRight");
            walk_up = AnimationLib.loadNewAnimationSet("zippyGunUp");
            current_skeleton = walk_down;
            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
            current_skeleton.Skeleton.FlipX = false;
            //enemyAnim = AnimationLib.getFrameAnimationSet("enemyPic");
            animation_time = 0.0f;
        }

        //needs to walk randomly and then shoot in a direction
        public override void update(GameTime currentTime)
        {
            if (bullet_alive)
            {
                daemonupdate(currentTime, parentWorld);
            }

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
                    case EnemyState.Moving:
                        change_direction_time += currentTime.ElapsedGameTime.Milliseconds;

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
                        //checks where the player is
                        else
                        {
                            foreach (Entity en in parentWorld.EntityList)
                            {
                                if (en == this)
                                {
                                    continue;
                                }

                                if (en is Player)
                                {
                                    if (hitTest(en))
                                    {
                                        Vector2 direction = en.Position - bullet.position;
                                        en.knockBack(direction, knockback_magnitude, enemy_damage);
                                    }
                                    component.update(this, en, currentTime, parentWorld);
                                }
                            }

                            if (enemy_found == true)
                            {
                                state = EnemyState.Firing;
                                velocity = Vector2.Zero;
                                animation_time = 0.0f;
                            }
                            else
                            {
                                current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("run");
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
                            }
                        }
                        break;
                    case EnemyState.Firing:
                        pause_time += currentTime.ElapsedGameTime.Milliseconds;
                        //for winding up && difference between this and the follow if statement is when bullet gets created before next pause
                        if (pause_time > 500)
                        {
                            //if the timer is less then 1500, then the enemy can fire bullets
                            if (pause_time < 1500)
                            {
                                if (bullet_alive_time == 0.0f)
                                {
                                    current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("attack");
                                    animation_time = 0.0f;
                                    bullet.hitbox = new Vector2(10.0f, 10.0f);
                                    bullet.velocity = new Vector2(0.0f, 0.0f);
                                    bullet_alive = true;
                                    bullet_alive_time = 0.0f;
                                    bullet.bullet_direction = direction_facing;
                                    switch (direction_facing)
                                    {
                                        case GlobalGameConstants.Direction.Right:
                                            bullet.position.X = current_skeleton.Skeleton.FindBone("muzzle").WorldX;
                                            bullet.position.Y = current_skeleton.Skeleton.FindBone("muzzle").WorldY;
                                            bullet.velocity.X = 20.0f;
                                            break;
                                        case GlobalGameConstants.Direction.Left:
                                            bullet.position.X = current_skeleton.Skeleton.FindBone("muzzle").WorldX;
                                            bullet.position.Y = current_skeleton.Skeleton.FindBone("muzzle").WorldY;
                                            bullet.velocity.X = -20.0f;
                                            break;
                                        case GlobalGameConstants.Direction.Up:
                                            bullet.position.X = current_skeleton.Skeleton.FindBone("muzzle").WorldX;
                                            bullet.position.Y = current_skeleton.Skeleton.FindBone("muzzle").WorldY;
                                            bullet.velocity.Y = -20.0f;

                                            break;
                                        default:
                                            bullet.position.X = current_skeleton.Skeleton.FindBone("muzzle").WorldX;
                                            bullet.position.Y = current_skeleton.Skeleton.FindBone("muzzle").WorldY;
                                            bullet.velocity.Y = 20.0f;
                                            break;
                                    }
                                }
                                else
                                {
                                    enemy_found = false;
                                    if (animation_time > 0.3)
                                    {
                                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                                        bullet_alive_time = 0.0f;
                                    }
                                }
                            }
                            else
                            {
                                if (bullet_alive == true)
                                {
                                    daemonupdate(currentTime, parentWorld);
                                }
                                else
                                {
                                    state = EnemyState.Idle;
                                    pause_time = 0.0f;
                                    bullet_alive = false;
                                }
                            }
                        }
                        else
                        {
                            current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("windUp");
                        }
                        break;
                    default:
                        //this is where enemy animation is reloading
                        pause_time += currentTime.ElapsedGameTime.Milliseconds;
                        current_skeleton.Animation = current_skeleton.Skeleton.Data.FindAnimation("idle");
                        if (pause_time > 500)
                        {
                            state = EnemyState.Moving;
                            switch (direction_facing)
                            {
                                case GlobalGameConstants.Direction.Right:
                                    velocity = new Vector2(1.0f, 0.0f);
                                    break;
                                case GlobalGameConstants.Direction.Left:
                                    velocity = new Vector2(-1.0f, 0.0f);
                                    break;
                                case GlobalGameConstants.Direction.Up:
                                    velocity = new Vector2(0.0f, -1.0f);
                                    break;
                                default:
                                    velocity = new Vector2(0.0f, 1.0f);
                                    break;
                            }
                            pause_time = 0.0f;
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

        //checks to see if the bullet hit the player
        public void daemonupdate(GameTime currentTime, LevelState parentWorld)
        {
            bullet_alive_time += currentTime.ElapsedGameTime.Milliseconds;
            Vector2 nextStep_temp = Vector2.Zero;

            if (bullet_alive == true)
            {
                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is Player)
                    {
                        if (hitTestBullet(en))
                        {
                            Vector2 direction = en.Position - bullet.position;
                            en.knockBack(direction, knockback_magnitude, bullet.bullet_damage);
                            bullet_alive = false;
                        }
                    }
                }

                if (bullet_alive_time > 300)
                {
                    bullet_alive = false;
                    bullet_alive_time = 0.0f;
                }
                else
                {
                    nextStep_temp = new Vector2(bullet.position.X - (bullet.hitbox.X / 2) + bullet.velocity.X, (bullet.position.Y + bullet.velocity.X));
                }

                bool on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                int check_corners = 0;
                while (check_corners != 4)
                {
                    if (on_wall == false)
                    {
                        if (check_corners == 0)
                        {
                            nextStep_temp = new Vector2(bullet.position.X + (bullet.hitbox.X / 2) + bullet.velocity.X, bullet.position.Y + bullet.velocity.Y);
                        }
                        else if (check_corners == 1)
                        {
                            nextStep_temp = new Vector2(bullet.position.X + bullet.velocity.X, bullet.position.Y - (bullet.hitbox.Y / 2) + bullet.velocity.Y);
                        }
                        else if (check_corners == 2)
                        {
                            nextStep_temp = new Vector2(bullet.position.X + bullet.velocity.X, position.Y + bullet.hitbox.Y + bullet.velocity.Y);
                        }
                        else
                        {
                            bullet.position += bullet.velocity;
                        }
                        on_wall = parentWorld.Map.hitTestWall(nextStep_temp);
                    }
                    else
                    {
                        bullet_alive = false;
                        bullet_alive_time = 0.0f;
                        break;
                    }
                    check_corners++;
                }
            }
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            if (attacker == null)
            {
                return;
            }

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
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            /*sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle1, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle2, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);*/
            
            /*
            sb.Draw(Game1.whitePixel, position, null, Color.Black, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 0.5f);
            if (bullet_alive)
            {
                sb.Draw(Game1.whitePixel, bullet.position, null, Color.Pink, 0.0f, Vector2.Zero, bullet.hitbox, SpriteEffects.None, 0.5f);
            }
             * */
        }

        public bool hitTestBullet(Entity other)
        {
            if ((bullet.position.X - (bullet.hitbox.X / 2)) > other.Position.X + other.Dimensions.X || (bullet.position.X + (bullet.hitbox.X / 2)) < other.Position.X || (bullet.position.Y - (bullet.hitbox.Y / 2)) > other.Position.Y + other.Dimensions.Y || (bullet.position.Y + (bullet.hitbox.Y / 2)) < other.Position.Y)
            {
                return false;
            }
            return true;
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
    }
}
