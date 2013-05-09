using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    struct Bullet
    {
        public Vector2 hitbox;
        public Vector2 position;
        public Vector2 velocity;
        public int bullet_damage;
        public GlobalGameConstants.Direction bullet_direction;
    }
    class Gun : Item
    {
        private Vector2 hitbox = Vector2.Zero;
        private Vector2 position = Vector2.Zero;
        private GlobalGameConstants.Direction item_direction = GlobalGameConstants.Direction.Right;
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.Gun;
        private bool bullet_alive = false;
        private float bullet_alive_time = 0.0f;

        private Bullet bullet = new Bullet();

        public Gun(Vector2 initial_position)
        {
            hitbox = new Vector2(48.0f, 48.0f);
            position = initial_position;
            bullet_alive_time = 0.0f;
            bullet_alive = false;
            item_type = GlobalGameConstants.itemType.Gun;
        }
        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            position = parent.CenterPoint;
            item_direction = parent.Direction_Facing;
            
            if (bullet_alive == false)
            {
                bullet.hitbox = new Vector2(10.0f, 10.0f);
                bullet.velocity = new Vector2(0.0f, 0.0f);
                bullet.bullet_damage = 2;
                bullet_alive = true;

                GlobalGameConstants.Player_Ammunition = GlobalGameConstants.Player_Ammunition - 1;

                bullet_alive_time = 0.0f;

                if (item_direction == GlobalGameConstants.Direction.Right)
                {
                    bullet.position.X = parent.CenterPoint.X + (parent.Dimensions.X / 2) + (bullet.hitbox.X / 2);
                    bullet.position.Y = parent.CenterPoint.Y;
                    bullet.velocity.X = 3.0f;
                    bullet.bullet_direction = item_direction;
                }
                else if (item_direction == GlobalGameConstants.Direction.Left)
                {
                    bullet.position.X = parent.CenterPoint.X - (parent.Dimensions.X / 2) - (bullet.hitbox.X / 2);
                    bullet.position.Y = parent.CenterPoint.Y;
                    bullet.velocity.X = -3.0f;
                    bullet.bullet_direction = item_direction;
                    // position.Y = parent.Position.Y;
                }
                else if (item_direction == GlobalGameConstants.Direction.Up)
                {
                    bullet.position.Y = parent.CenterPoint.Y - (parent.Dimensions.Y / 2) - (bullet.hitbox.Y / 2);
                    bullet.position.X = parent.CenterPoint.X;
                    bullet.velocity.Y = -3.0f;
                    bullet.bullet_direction = item_direction;
                    //position.X = parent.Position.X;
                }
                else
                {
                    bullet.position.Y = parent.CenterPoint.Y + (parent.Dimensions.Y / 2) + (bullet.hitbox.Y / 2);
                    bullet.position.X = parent.CenterPoint.X;
                    bullet.velocity.Y = 3.0f;
                    bullet.bullet_direction = item_direction;
                    //position.X = parent.Position.X;
                }
            }
            
            parent.State = Player.playerState.Moving;
        }

        public void daemonupdate(GameTime currentTime, LevelState parentWorld)
        {
            bullet_alive_time += currentTime.ElapsedGameTime.Milliseconds;
            Vector2 nextStep_temp = Vector2.Zero;

            if (bullet_alive == true)
            {
                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is Enemy)
                    {
                        if (hitTest(en))
                        {
                            en.knockBack(en, bullet.position, bullet.hitbox, bullet.bullet_damage);
                            bullet_alive = false;
                        }
                    }
                }

                if (bullet_alive_time > 1000)
                {
                    bullet_alive = false;
                    bullet_alive_time = 0.0f;
                }
                else
                {
                    nextStep_temp = new Vector2(bullet.position.X - (bullet.hitbox.X/2) + bullet.velocity.X, (bullet.position.Y + bullet.velocity.X));
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

        public GlobalGameConstants.itemType ItemType()
        {
            return item_type;
        }

        public string getEnumType()
        {
            return item_type.ToString();
        }

        public void draw(SpriteBatch sb)
        {
            if (bullet_alive)
            {
                sb.Draw(Game1.whitePixel, bullet.position, null, Color.Pink, 0.0f, Vector2.Zero, bullet.hitbox, SpriteEffects.None, 0.5f);
            }
        }

        public bool hitTest(Entity other)
        {
            if ((bullet.position.X - (bullet.hitbox.X / 2)) > other.Position.X + other.Dimensions.X || (bullet.position.X + (bullet.hitbox.X / 2)) < other.Position.X || (bullet.position.Y - (bullet.hitbox.Y / 2)) > other.Position.Y + other.Dimensions.Y || (bullet.position.Y + (bullet.hitbox.Y / 2)) < other.Position.Y)
            {
                return false;
            }
            return true;
        }
    }
}
