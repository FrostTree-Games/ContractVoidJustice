using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class SlowChaser : Enemy
    {
        private enum SlowChaserState
        {
            Idle = 0,
            WindUp = 1,
            Sprint = 2,
            Cooldown = 3,
            KnockedBack = 4,
            Dying = 5,
        }

        public SlowChaser(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

            enemy_type = EnemyType.Alien;
        }

        public override void update(GameTime currentTime)
        {
            //base.update(currentTime);
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Red, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            //base.knockBack(direction, magnitude, damage, attacker);
        }

        public override void spinerender(Spine.SkeletonRenderer renderer)
        {
            //base.spinerender(renderer);
        }
    }
}
