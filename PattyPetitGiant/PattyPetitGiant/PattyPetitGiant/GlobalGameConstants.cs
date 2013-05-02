using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PattyPetitGiant
{
    public class GlobalGameConstants
    {
        private static Vector2 tileSize = new Vector2(48, 48);
        public static Vector2 TileSize { get { return tileSize; } }

        private static TileMap.TileDimensions standardMapSize = new TileMap.TileDimensions(128, 128);
        public static TileMap.TileDimensions StandardMapSize { get { return standardMapSize; } }
    }
}
