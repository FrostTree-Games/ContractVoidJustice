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
    class GuardSquadSoldiers : Entity
    {
        private enum SquadSoldierState
        {
            Patrol,
            Fire,
            Follow
        }

        public GuardSquadSoldiers(LevelState parentWorld, float intial_x, float initial_y)
        {
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
    }
}
