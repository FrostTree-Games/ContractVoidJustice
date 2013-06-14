using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class SnakeWorm : Enemy
    {
        private enum SnakeWormState
        {
            InvalidState = -1,
            Moving = 0,
            KnockedBack = 2,
            Dying = 3, // may not use
        }

        private float direction;

        private AnimationLib.FrameAnimationSet testAnim = null;

        public SnakeWorm(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;

            direction = (float)(Game1.rand.NextDouble() * Math.PI * 2);

            testAnim = AnimationLib.getFrameAnimationSet("antiFairy");
            animation_time = 0;
        }

        public override void update(GameTime currentTime)
        {
            direction += 0.1f;
        }

        public override void draw(SpriteBatch sb)
        {
            testAnim.drawAnimationFrame(0.0f, sb, position, new Vector2(1), 0.5f, direction, new Vector2(8));
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            base.knockBack(direction, magnitude, damage, attacker);
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            //spine animations may not be used here
        }
    }
}
