using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class PatrolGuard : Enemy
    {
        public PatrolGuard(LevelState parentWorld, Vector2 position)
        {
            //
        }

        public override void update(GameTime currentTime)
        {
 	         base.update(currentTime);
        }

        public override void draw(SpriteBatch sb)
        {
 	         base.draw(sb);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            //base.knockBack(direction, magnitude, damage);
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            //base.spinerender(renderer);
        }
    }
}
