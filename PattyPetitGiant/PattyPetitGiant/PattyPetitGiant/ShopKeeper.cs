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

        public ShopKeeper(LevelState parentWorld, Vector2 position)
        {
            this.parentWorld = parentWorld;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

            throw new NotImplementedException("shopkeeper yet to come");
        }

        public override void update(GameTime currentTime)
        {
            throw new NotImplementedException();
        }

        public override void draw(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }

        public override void knockBack(Entity other, Vector2 position, Vector2 dimensions, int damage)
        {
            throw new NotImplementedException();
        }
    }
}
