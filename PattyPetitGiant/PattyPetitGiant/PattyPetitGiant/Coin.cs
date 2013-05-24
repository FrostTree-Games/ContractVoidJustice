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
    class Coin : Entity
    {
        public Coin(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            this.parentWorld = parentWorld;
            dimensions = new Vector2(20.0f, 20.0f);
        }
        public override void update(GameTime currentTime)
        {
            foreach (Entity en in parentWorld.EntityList)
            {
                if (en is Player)
                {
                    if (hitTest(en))
                    {
                        GlobalGameConstants.Player_Coin_Amount = GlobalGameConstants.Player_Coin_Amount + 1;
                        remove_from_list = true;
                    }
                }
            }

        }
        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Brown, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
        }
        public override void knockBack(Vector2 direction, float magnitude, int damage)
        {
            return;
        }
    }
}
