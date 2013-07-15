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

        private const float pellet_angle_interval = (float)Math.PI / 6;
        private float pellet_angle_direction = 0.0f;
        private int damage_multiplier;

        private float damage_delay_timer = 0.0f;
        private float damage_delay = 100.0f;
        private bool damage_delay_flag = false;

        public ShotGun()
        {
            inactive_pellets = 0;
            pellet_count = 0;
            shotgun_active = false;

            damage_delay_flag = false;
            damage_delay_timer = 0.0f;
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
            }

            if (shotgun_active == false)
            {
                switch (parent.Direction_Facing)
                {
                    case GlobalGameConstants.Direction.Right:
                        pellet_angle_direction = (float)(-1*Math.PI / 12);
                        break;
                    case GlobalGameConstants.Direction.Left:
                        pellet_angle_direction = (float)(Math.PI/1.09);
                        break;
                    case GlobalGameConstants.Direction.Up:
                        pellet_angle_direction = (float)(-1 * Math.PI / 1.74);
                        break;
                    default:
                        pellet_angle_direction = (float)(Math.PI / 2.4);
                        break;
                }
                for (int i = 0; i < max_pellets; i++)
                {
                    float angle = (float)((Game1.rand.Next() % pellet_angle_interval) + pellet_angle_direction);
                    shotgun_pellets[i] = new Pellets(parent.CenterPoint, angle);
                    pellet_count++;
                }
                shotgun_active = true;
            }
            parent.State = Player.playerState.Moving;
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            damage_delay_timer += currentTime.ElapsedGameTime.Milliseconds;

            for (int i = 0; i < pellet_count; i++)
            {
                if (shotgun_pellets[i].active == true)
                {
                    shotgun_pellets[i].update(parentWorld, currentTime, parent);

                    if (shotgun_pellets[i].hit_enemy && !damage_delay_flag)
                    {
                        damage_delay_flag = true;
                        damage_delay_timer = 0.0f;
                    }
                }
            }
            
            if (damage_delay_timer > damage_delay && damage_delay_flag)
            {
                for (int i = 0; i < pellet_count; i++)
                {
                    if (shotgun_pellets[i].hit_enemy && !shotgun_pellets[i].pellet_checked)
                    {
                        Entity en = shotgun_pellets[i].entity_hit;
                        int damage_multiplier = 1;
                        for (int j = i + 1; j < pellet_count; j++)
                        {
                            if (shotgun_pellets[j].hit_enemy && !shotgun_pellets[j].pellet_checked)
                            {
                                if (en == shotgun_pellets[j].entity_hit)
                                {
                                    damage_multiplier++;
                                    shotgun_pellets[j].pellet_checked = true;
                                }
                            }
                        }

                        shotgun_pellets[i].pellet_checked = true;
                        Vector2 direction = en.CenterPoint - shotgun_pellets[i].CenterPoint;
                        en.knockBack(shotgun_pellets[i].velocity, shotgun_pellets[i].knockback_magnitude / (Vector2.Distance(en.CenterPoint, parent.CenterPoint) / 100), shotgun_pellets[i].pellet_damage * damage_multiplier, parent);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        public GlobalGameConstants.itemType ItemType()
        { return item_type; }

        public string getEnumType()
        { return item_type.ToString(); }

        public void draw(Spine.SkeletonRenderer sb)
        {
            if (pellet_count > 0)
            {
                for (int i = 0; i < pellet_count; i++)
                {
                    //if (shotgun_pellets[i].active)
                        //sb.Draw(Game1.whitePixel, shotgun_pellets[i].position, null, Color.White, 0.0f, Vector2.Zero, new Vector2(10.0f, 10.0f), SpriteEffects.None, 0.5f);
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
            public bool hit_enemy;
            public bool pellet_checked;
            public Entity entity_hit;
            public Vector2 nextStep_temp;
            public Vector2 CenterPoint
            {
                get { return position + dimensions / 2; }
            }

            public float time_alive;
            private const float max_time_alive = 500f;
            public float knockback_magnitude;
            
            public Pellets(Vector2 position, float angle)
            {
                this.position = position;
                active = true;
                this.dimensions = new Vector2(10, 10);
                this.velocity = new Vector2((float)(8.0f * Math.Cos(angle)), (float)(8.0f * Math.Sin(angle)));
                nextStep_temp = Vector2.Zero;
                pellet_damage = 3;
                time_alive = 0.0f;
                hit_enemy = false;
                entity_hit = null;
                pellet_checked = false;
                knockback_magnitude = 3.0f;
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
                            hit_enemy = true;
                            entity_hit = en;
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
