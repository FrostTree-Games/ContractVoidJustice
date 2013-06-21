using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class BroodLord : Enemy
    {
        public BroodLord(LevelState parentWorld, Vector2 position)
        {
            //
        }

        public override void update(GameTime currentTime)
        {
            throw new NotImplementedException();
        }

        public override void draw(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            throw new NotImplementedException();
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            throw new NotImplementedException();
        }
    }
}
