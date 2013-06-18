using System;
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
            Up = 3,
            Down = 1,
            Left = 2,
            Right = 0,
            NoDirection = -1,
        }

        public enum itemType
        {
            NoItem = -1,
            Sword = 0,
            Gun = 1,
            Bomb = 2,
            Compass = 3,
            DungeonMap = 4,
            BushidoBlade = 5,
            WandOfGyges = 6,
            ShotGun = 7,
            WaveMotionGun = 8,
            HermesSandals = 10,
            RocketLauncher = 11,
        }

        private static int gameResolutionWidth = 1280;
        private static int gameResolutionHeight = 720;
        public static int GameResolutionWidth { get { return gameResolutionWidth; } }
        public static int GameResolutionHeight { get { return gameResolutionHeight; } }


        private static Vector2 tileSize = new Vector2(48, 48);
        public static Vector2 TileSize { get { return tileSize; } }

        
        private static TileMap.TileDimensions standardMapSize = new TileMap.TileDimensions(5, 5);
        public static TileMap.TileDimensions StandardMapSize { get { return standardMapSize; } }
        private static int tilesPerRoomWide = 24;
        private static int tilesPerRoomHigh = 24;
        public static int TilesPerRoomWide { get { return tilesPerRoomWide; } }
        public static int TilesPerRoomHigh { get { return tilesPerRoomHigh; } }

        public class WeaponDictionary
        {
            public struct WeaponInformation
            {
                public int price;
                public string message;
                public AnimationLib.FrameAnimationSet pickupImage;

                public WeaponInformation(int price, string message, string pickupFrameImage)
                {
                    this.price = price;
                    this.message = message;
                    this.pickupImage = AnimationLib.getFrameAnimationSet(pickupFrameImage);
                }
            }

            public static WeaponInformation[] weaponInfo = null;

            public static void InitalizePriceData()
            {
                if (weaponInfo != null)
                {
                    return;
                }

                weaponInfo = new WeaponInformation[20];

                weaponInfo[(int)itemType.Sword] = new WeaponInformation(20, "A simple sword. It's dangerous to go alone without one.", "swordPic");
                weaponInfo[(int)itemType.WandOfGyges] = new WeaponInformation(200, "A magical wand that seems to let its weilder bend space and location.", "wandPic");
                weaponInfo[(int)itemType.Gun] = new WeaponInformation(120, "A pistol issued to trainee guards on the ship. Hokey religions and ancient weapons are no match for this.", "gunPic");
                weaponInfo[(int)itemType.Bomb] = new WeaponInformation(70, "Bombs will destroy enemies in a radius, but watch out! They're dangerous!", "bombPic");
                weaponInfo[(int)itemType.Compass] = new WeaponInformation(55, "A magical tool that always seems to direct you toward the exit on this floor of the ship.", "compassPic");
                weaponInfo[(int)itemType.HermesSandals] = new WeaponInformation(130, "Enables you to run at extreme speeds, but at the price of vulnerability.", "gunPic");
                weaponInfo[(int)itemType.WaveMotionGun] = new WeaponInformation(80, "A strange weapon that fires in an ecclectic motion. Somewhat impractical.", "gunPic");
                weaponInfo[(int)itemType.BushidoBlade] = new WeaponInformation(90, "This weapon will allow you to slay almost any foe perfectly, but brings an honorable death to the imperfect warrior.", "gunPic");
                weaponInfo[(int)itemType.RocketLauncher] = new WeaponInformation(120, "A one-handed device that fires rockets that combust on explosion. Avoid shooting it in close quarters.", "gunPic");
                weaponInfo[(int)itemType.ShotGun] = new WeaponInformation(170, "A shotgun formely used by space mobsters to rob banks with.", "gunPic");
                weaponInfo[(int)itemType.DungeonMap] = new WeaponInformation(45, "Allows you to see the corridors of the current floor of the ship.", "gunPic");
            }
        }
    }
}
