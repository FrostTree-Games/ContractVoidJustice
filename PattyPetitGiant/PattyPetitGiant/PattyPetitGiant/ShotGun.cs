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

        public ShotGun()
        {
            
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
                }
            }

            if (pellet_count < max_pellets)
            {
                for (int i = 0; i < max_pellets; i++)
                {
                    shotgun_pellets[i] = new Pellets(parent.CenterPoint);
                }
            }
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        { }

        public GlobalGameConstants.itemType ItemType()
        { return item_type; }

        public string getEnumType()
        { return item_type.ToString(); }

        public void draw(SpriteBatch sb)
        { }

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
                this.velocity = Vector2.Zero;
                nextStep_temp = Vector2.Zero;
                pellet_damage = 3;
                time_alive = 0.0f;
            }

            public void update(LevelState parentWorld, GameTime currentTime, Entity parent)
            {
            }
        }
    }
}
