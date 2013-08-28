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
            LazerGun = 12,
            FlameThrower = 13,
            MachineGun = 14,
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

        public static int LevelAliensAppearAt = 3;

        public class WeaponDictionary
        {
            public struct WeaponInformation
            {
                public int price;
                public string name;
                public string message;
                public AnimationLib.FrameAnimationSet pickupImage;
                public int ammo_consumption;
                public string priceString; // for rendering purposes to save on garbage

                public WeaponInformation(int price, string name, string message, string pickupFrameImage, int ammo_consumption)
                {
                    this.price = price;
                    this.name = name;
                    this.message = message;
                    this.pickupImage = AnimationLib.getFrameAnimationSet(pickupFrameImage);
                    this.ammo_consumption = ammo_consumption;
                    this.priceString = this.price.ToString();
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

                weaponInfo[(int)itemType.Sword] = new WeaponInformation(20, "Sword", "A simple sword. It's dangerous to go alone without one.", "itemSword", 0);
                weaponInfo[(int)itemType.WandOfGyges] = new WeaponInformation(200, "Gyges Gem", "A magical wand that seems to let its weilder bend space and location.", "itemWand", 5);
                weaponInfo[(int)itemType.Gun] = new WeaponInformation(120, "Pistol", "A sidearm issued to trainee guards on the ship. Hokey religions and ancient weapons are no match for this.", "itemPistol", 1);
                weaponInfo[(int)itemType.Bomb] = new WeaponInformation(70, "Bomb", "Will destroy enemies in a radius, but watch out! They're dangerous!", "itemBomb", 10);
                weaponInfo[(int)itemType.Compass] = new WeaponInformation(55, "Solar Compass", "A magical tool that always seems to direct you toward the exit on this floor of the ship.", "itemCompass", 0);
                weaponInfo[(int)itemType.HermesSandals] = new WeaponInformation(130, "Velocity Distortion Gem", "Enables you to run at extreme speeds, but at the price of vulnerability.", "itemHermes", 1);
                weaponInfo[(int)itemType.WaveMotionGun] = new WeaponInformation(80, "Sine Motion Gun", "A strange weapon that fires in an ecclectic motion. Somewhat impractical.", "itemRayGun", 5);
                weaponInfo[(int)itemType.BushidoBlade] = new WeaponInformation(90, "Bushido Blade", "This weapon will allow you to slay almost any foe perfectly, but brings an honorable death to the imperfect warrior.", "itemSword", 0);
                weaponInfo[(int)itemType.RocketLauncher] = new WeaponInformation(120, "Rocket Launcher", "A one-handed device that fires rockets that combust on explosion. Avoid shooting it in close quarters.", "itemRocket", 20);
                weaponInfo[(int)itemType.ShotGun] = new WeaponInformation(170, "Shotgun", "Formely used by space mobsters when performing bank robbery. It's signed!", "itemShotgun", 5);
                weaponInfo[(int)itemType.DungeonMap] = new WeaponInformation(45, "Level Map", "Allows you to view the corridors of the current floor of the ship.", "itemMap", 0);
                weaponInfo[(int)itemType.FlameThrower] = new WeaponInformation(200, "Flamethrower", "Badassery. Enough said.", "itemFlamethrower", 1);
                weaponInfo[(int)itemType.LazerGun] = new WeaponInformation(159, "Laser Gun", "A hard laser that can burn flesh in an instant.", "itemLaser", 15);
                weaponInfo[(int)itemType.MachineGun] = new WeaponInformation(100, "Machine Gun", "Rapid-fire gunplay has never been so satisfying!", "itemMachineGun", 1);
            }
        }
    }
}
