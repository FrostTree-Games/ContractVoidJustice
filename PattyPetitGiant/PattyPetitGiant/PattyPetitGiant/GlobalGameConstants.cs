﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PattyPetitGiant
{
    public class GlobalGameConstants
    {
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        public enum itemType
        {
            NoItem = -1,
            Sword = 0,
            Gun = 1,
            Bomb = 2,
        }

        private static int gameResolutionWidth = 1280;
        private static int gameResolutionHeight = 720;
        public static int GameResolutionWidth { get { return gameResolutionWidth; } }
        public static int GameResolutionHeight { get { return gameResolutionHeight; } }


        private static Vector2 tileSize = new Vector2(48, 48);
        public static Vector2 TileSize { get { return tileSize; } }

        
        private static TileMap.TileDimensions standardMapSize = new TileMap.TileDimensions(5, 5);
        public static TileMap.TileDimensions StandardMapSize { get { return standardMapSize; } }
        private static int tilesPerRoomWide = 16;
        private static int tilesPerRoomHigh = 16;
        public static int TilesPerRoomWide { get { return tilesPerRoomWide; } }
        public static int TilesPerRoomHigh { get { return tilesPerRoomHigh; } }

        private static float player_health = 100.00f;
        public static float Player_Health 
        {
            set { player_health = value; }
            get { return player_health; } 
        }
        private static float player_ammunition = 100;
        public static float Player_Ammunition
        {
            set { player_ammunition = value; }
            get { return player_ammunition; }
        }
        private static int player_coin_amount = 0;
        public static int Player_Coin_Amount
        {
            set { player_coin_amount = value; }
            get { return player_coin_amount; }
        }

        public static string Player_Item_1 = null;
        public static string Player_Item_2 = null;
    }
}
