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
    class Pickup : Entity
    {
        GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.Bomb;

        public Pickup(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
        }

        public override void update(GameTime currentTime)
        {
            return;
        }

        public Item assignItem(Item player_item, GameTime currentTime)
        {
            switch (item_type)
            {
                case GlobalGameConstants.itemType.Bomb:
                    item_type = player_item.ItemType();
                    return new Bomb(position);
                case GlobalGameConstants.itemType.Gun:
                    item_type = player_item.ItemType();
                    return new Gun(position);
                default:
                    item_type = player_item.ItemType();
                    return new Sword(position);
            }
        }

        public override void draw(SpriteBatch sb)
        {
            sb.Draw(Game1.whitePixel, position, null, Color.Orange, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
        }
    }
}
