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
    class ShotGun : Item
    {
        private GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.ShotGun;
        private const int max_pellets = 5;
        private Pellets[] shotgun_pellets = new Pellets[max_pellets];

        private int inactive_pellets = 0;
        private int pellet_count = 0;
        private Vector2 position;
        private bool shotgun_active;

        public ShotGun()
        {
            inactive_pellets = 0;
            pellet_count = 0;
            shotgun_active = false;
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            position = parent.CenterPoint;

            if (pellet_count == max_pellets)
            {
                inactive_pellets = 0;
                for (int i = 0; i < pellet_count; i++)
                {
                    if (shotgun_pellets[i].active == false)
                        inactive_pellets++;
                }

                if (inactive_pellets == max_pellets)
                {
                    pellet_count = 0;
                    shotgun_active = false;
                }
                else
                {
                    shotgun_active = true;
                }
            }

            if (shotgun_active == false)
            {
                if (pellet_count < max_pellets)
                {
                    for (int i = 0; i < max_pellets; i++)
                    {
                        shotgun_pellets[i] = new Pellets(parent.CenterPoint);
                        pellet_count++;
                    }
                }
            }
            parent.State = Player.playerState.Moving;
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            for (int i = 0; i < pellet_count; i++)
            {
                if (shotgun_pellets[i].active == true)
                {
                    shotgun_pellets[i].update(parentWorld, currentTime, parent);
                }
            }
        }

        public GlobalGameConstants.itemType ItemType()
        { return item_type; }

        public string getEnumType()
        { return item_type.ToString(); }

        public void draw(SpriteBatch sb)
        {
            if (pellet_count > 0)
            {
                for (int i = 0; i < pellet_count; i++)
                {
                    if (shotgun_pellets[i].active)
                        sb.Draw(Game1.whitePixel, shotgun_pellets[i].position, null, Color.White, 0.0f, Vector2.Zero, new Vector2(10.0f, 10.0f), SpriteEffects.None, 0.5f);
                }
            }
        }

        private struct Pellets
        {
            public Vector2 dimensions;
            public Vector2 position;
            public Vector2 velocity;
            public int pellet_damage;
            public bool active;
            public Vector2 nextStep_temp;
            public Vector2 CenterPoint
            {
                get { return position + dimensions / 2; }
            }

            public float time_alive;
            private const float max_time_alive = 500f;
            private const float knockback_magnitude = 3.0f;
            public Pellets(Vector2 position)
            {
                this.position = position;
                active = true;
                this.dimensions = new Vector2(10, 10);
                this.velocity = new Vector2(3.0f, 0f);
                nextStep_temp = Vector2.Zero;
                pellet_damage = 3;
                time_alive = 0.0f;
            }

            public void update(LevelState parentWorld, GameTime currentTime, Entity parent)
            {
                time_alive += currentTime.ElapsedGameTime.Milliseconds;
                Vector2 nextStep_temp = Vector2.Zero;

                foreach (Entity en in parentWorld.EntityList)
                {
                    if (en is Enemy || en is ShopKeeper)
                    {
                        if (hitTest(en))
                        {
                            active = false;
                        }
                    }
                }

                if(time_alive > max_time_alive)
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

            public bool hitTest(Entity other)
            {
                if ((position.X - (dimensions.X / 2)) > other.Position.X + other.Dimensions.X || (position.X + (dimensions.X / 2)) < other.Position.X || (position.Y - (dimensions.Y / 2)) > other.Position.Y + other.Dimensions.Y || (position.Y + (dimensions.Y / 2)) < other.Position.Y)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
