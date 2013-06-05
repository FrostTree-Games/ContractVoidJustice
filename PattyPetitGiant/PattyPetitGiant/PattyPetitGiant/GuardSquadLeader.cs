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
    class GuardSquadLeader : Enemy
    {
        private enum SquadLeaderState
        {
            Patrol,
            Direct,
            Flee
        }

        private Entity[] squad_mates = new Entity[2];

        public GuardSquadLeader(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = Vector2.Zero;
            populateSquadMates();

            this.parentWorld = parentWorld;
        }

        public override void update(GameTime currentTime)
        {
        }

        public override void draw(SpriteBatch sb)
        {
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
        }

        public void populateSquadMates()
        {

        }
    }
}
