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
    class Enemy : Entity
    {
        public Enemy()
        {
        }

        public Enemy(float initialx, float initialy)
        {
            horizontal_pos = initialx;
            vertical_pos = initialy;

            dimensions.X = 47.9f;
            dimensions.Y = 47.9f;
        }

        public override void update(GameTime currentTime)
        {
            return;
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, new Vector2(horizontal_pos, vertical_pos), null, Color.Green, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);
        }
    }
}
