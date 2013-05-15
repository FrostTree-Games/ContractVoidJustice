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

        public Pickup(LevelState parentWorld, float initial_x, float initial_y, int item_choice)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            switch (item_choice)
            {
                case 0:
                    item_type = GlobalGameConstants.itemType.Bomb;
                    break;
                case 1:
                    item_type = GlobalGameConstants.itemType.Sword;
                    break;
                default:
                    item_type = GlobalGameConstants.itemType.Gun;
                    break;
            }
        }

        public override void update(GameTime currentTime)
        {
            return;
        }

        //after the item gets picked up, it gets replaced with the item the player dropped
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
            AnimationLib.FrameAnimationSet dropAnim = null;

            switch (item_type)
            {
                case GlobalGameConstants.itemType.Sword:
                    dropAnim = AnimationLib.getFrameAnimationSet("swordPic");
                    break;
                case GlobalGameConstants.itemType.Gun:
                    dropAnim = AnimationLib.getFrameAnimationSet("gunPic");
                    break;
                case GlobalGameConstants.itemType.Bomb:
                    dropAnim = AnimationLib.getFrameAnimationSet("bombPic");
                    break;
                default:
                    sb.Draw(Game1.whitePixel, position, null, Color.Black, 0.0f, Vector2.Zero, dimensions, SpriteEffects.None, 0.5f);
                    break;
            }

            if (dropAnim != null)
            {
                dropAnim.drawAnimationFrame(0.0f, sb, Position, new Vector2(1.0f, 1.0f), 0.5f);
            }
        }
    }
}
