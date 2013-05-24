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

        public RangeEnemy(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            this.parentWorld = parentWorld;
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = new Vector2(0.0f, 1.0f);
            player_found = false;

            state = EnemyState.Moving;
            component = new MoveSearch();
            direction_facing = GlobalGameConstants.Direction.Down;

            change_direction = 0;
            change_direction_time = 0.0f;

            enemy_life = 10;
            enemy_damage = 2;
            damage_player_time = 0.0f;

            rand = new Random(DateTime.Now.Millisecond);
            enemyAnim = AnimationLib.getFrameAnimationSet("rangeenemyPic");
            bullet_alive = false;

            bullet.bullet_damage = 5;
        }

        //needs to walk randomly and then shoot in a direction
        public override void update(GameTime currentTime)
        {
            if (bullet_alive)
            {
                daemonupdate(currentTime, parentWorld);
            }

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
                    foreach (Entity en in parentWorld.EntityList)
                    {
                        if (en == this)
                        {
                            continue;
                        }

                        if (en is Player)
                        {
                            component.update(this, en, currentTime, parentWorld);
                        }
                    }

                    if (player_found == true)
                    {
                        state = EnemyState.Firing;
                        velocity = Vector2.Zero;
                    }

                    Vector2 pos = new Vector2(position.X, position.Y);
                    Vector2 nextStep = new Vector2(position.X + velocity.X, position.Y + velocity.Y);
                    Vector2 finalPos = parentWorld.Map.reloactePosition(pos, nextStep, dimensions);
                    position.X = finalPos.X;
                    position.Y = finalPos.Y;
                    break;
                case EnemyState.Firing: 
                    pause_time += currentTime.ElapsedGameTime.Milliseconds;
                    if (pause_time > 500)
                    {
                        if (pause_time > 1500)
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
                        else
                        {
                            if (bullet_alive == false)
                            {
                                bullet.hitbox = new Vector2(10.0f, 10.0f);
                                bullet.velocity = new Vector2(0.0f, 0.0f);
                                bullet_alive = true;
                                bullet_alive_time = 0.0f;
                                bullet.bullet_direction = direction_facing;
                                switch (direction_facing)
                                {
                                    case GlobalGameConstants.Direction.Right:
                                        bullet.position.X = CenterPoint.X + (Dimensions.X / 2);
                                        bullet.position.Y = CenterPoint.Y;
                                        bullet.velocity.X = 20.0f;
                                        break;
                                    case GlobalGameConstants.Direction.Left:
                                        bullet.position.X = CenterPoint.X - (Dimensions.X / 2);
                                        bullet.position.Y = CenterPoint.Y;
                                        bullet.velocity.X = -20.0f;
                                        break;
                                    case GlobalGameConstants.Direction.Up:
                                        bullet.position.Y = CenterPoint.Y - (Dimensions.Y / 2);
                                        bullet.position.X = CenterPoint.X;
                                        bullet.velocity.Y = -20.0f;

                                        break;
                                    default:
                                        bullet.position.Y = CenterPoint.Y + (Dimensions.Y / 2);
                                        bullet.position.X = CenterPoint.X;
                                        bullet.velocity.Y = 20.0f;
                                        break;
                                }
                            }
                            else
                            {
                                state = EnemyState.Moving;
                                player_found = false;
                            }
                        }
                    }
                    break;
                default:
                    //this is where enemy animation is reloading
                    pause_time += currentTime.ElapsedGameTime.Milliseconds;
                    if (pause_time > 500)
                    {
                        state = EnemyState.Moving;
                        pause_time = 0.0f;
                    }
                    break;
            }
            if (enemy_life <= 0)
            {
                remove_from_list = true;
            }
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
                            en.knockBack(this, bullet.bullet_damage);
                            bullet_alive = false;
                        }
                    }
                }

                if (bullet_alive_time > 2000)
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
                    if (on_wall != true)
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

        public override void knockBack(Entity other, int damage)
        {

            if (disable_movement_time == 0.0)
            {
                disable_movement = true;
                float direction_x = CenterPoint.X - other.CenterPoint.X;
                float direction_y = CenterPoint.Y - other.CenterPoint.Y;

                if (Math.Abs(direction_x) > (Math.Abs(direction_y)))
                {
                    if (direction_x < 0)
                    {
                        velocity = new Vector2(-5.51f, direction_y / 100);
                    }
                    else
                    {
                        velocity = new Vector2(5.51f, direction_y / 100);
                    }
                }
                else
                {
                    if (direction_y < 0)
                    {
                        velocity = new Vector2(direction_x / 100f, -5.51f);
                    }
                    else
                    {
                        velocity = new Vector2(direction_x / 100f, 5.51f);
                    }
                }
                enemy_life = enemy_life - damage;
            }
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle1, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, CenterPoint, null, Color.White, angle2, Vector2.Zero, new Vector2(600.0f, 10.0f), SpriteEffects.None, 0.5f);
            
            sb.Draw(Game1.whitePixel, position, null, Color.Black, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 0.5f);
            if (bullet_alive)
            {
                sb.Draw(Game1.whitePixel, bullet.position, null, Color.Pink, 0.0f, Vector2.Zero, bullet.hitbox, SpriteEffects.None, 0.5f);
            }
        }

        public bool hitTestBullet(Entity other)
        {
            if ((bullet.position.X - (bullet.hitbox.X / 2)) > other.Position.X + other.Dimensions.X || (bullet.position.X + (bullet.hitbox.X / 2)) < other.Position.X || (bullet.position.Y - (bullet.hitbox.Y / 2)) > other.Position.Y + other.Dimensions.Y || (bullet.position.Y + (bullet.hitbox.Y / 2)) < other.Position.Y)
            {
                return false;
            }
            return true;
        }
    }
}
