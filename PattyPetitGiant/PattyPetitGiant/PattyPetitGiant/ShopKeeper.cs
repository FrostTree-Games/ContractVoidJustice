using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class ShopKeeper : Entity
    {
        /// <summary>
        /// State enum used to determine shopkeeper's behaviour.
        /// </summary>
        private enum ShopKeeperState
        {
            InvalidState = -1,
            Normal = 0,
            Enraged = 1,
        }

        private ShopKeeperState state = ShopKeeperState.Normal;
        private GlobalGameConstants.itemType[] itemsForSale = new GlobalGameConstants.itemType[3];
        private int[] itemPrices = new int[3];
        private AnimationLib.FrameAnimationSet[] itemIcons = new AnimationLib.FrameAnimationSet[3];

        private AnimationLib.FrameAnimationSet shopKeeperFrameAnimation = null;

        public ShopKeeper(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

            state = ShopKeeperState.Normal;

            shopKeeperFrameAnimation = AnimationLib.getFrameAnimationSet("shopKeeper");

            //test shop data for now
            {
                itemPrices[0] = 30;
                itemPrices[1] = 100;
                itemPrices[2] = 70;

                itemsForSale[0] = GlobalGameConstants.itemType.Bomb;
                itemsForSale[1] = GlobalGameConstants.itemType.Sword;
                itemsForSale[2] = GlobalGameConstants.itemType.Gun;
            }

            getItemIcons();
        }

        private void getItemIcons()
        {
            for (int i = 0; i < 3; i++)
            {
                switch (itemsForSale[i])
                {
                    case GlobalGameConstants.itemType.Gun:
                        itemIcons[i] = AnimationLib.getFrameAnimationSet("gunPic");
                        break;
                    case GlobalGameConstants.itemType.Sword:
                        itemIcons[i] = AnimationLib.getFrameAnimationSet("swordPic");
                        break;
                    case GlobalGameConstants.itemType.Bomb:
                        itemIcons[i] = AnimationLib.getFrameAnimationSet("bombPic");
                        break;
                    case GlobalGameConstants.itemType.NoItem:
                    default:
                        itemIcons[i] = AnimationLib.getFrameAnimationSet("flagPic");
                        break;
                }
            }
        }

        public override void update(GameTime currentTime)
        {
            return;
        }

        public override void draw(SpriteBatch sb)
        {
            shopKeeperFrameAnimation.drawAnimationFrame(0.0f, sb, position, new Vector2(3, 3), 0.5f);

            for (int i = 0; i < 3; i++)
            {
                Vector2 drawItemPos = position + new Vector2((-2 * GlobalGameConstants.TileSize.X) + (i * 2f * GlobalGameConstants.TileSize.X), (2.5f * GlobalGameConstants.TileSize.Y));

                itemIcons[i].drawAnimationFrame(0.0f, sb, drawItemPos, new Vector2(1.0f, 1.0f), 0.5f);
                sb.DrawString(Game1.font, itemPrices[i].ToString(), drawItemPos + new Vector2(0f, GlobalGameConstants.TileSize.Y), Color.Yellow, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
            }
        }

        public override void knockBack(Entity other, Vector2 position, Vector2 dimensions, int damage)
        {
            return;
        }
    }
}
