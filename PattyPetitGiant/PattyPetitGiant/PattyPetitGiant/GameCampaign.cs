using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PattyPetitGiant
{
    class GameCampaign
    {
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

        /// <summary>
        /// Indicates how many levels the player has completed so far in the campaign.
        /// </summary>
        public static int PlayerLevelProgress
        {
            get { return playerLevelProgress; }
            set { playerLevelProgress = value % 6; }
        }
        private static int playerLevelProgress = 0;

        /// <summary>
        /// Indicates which of the three floors of the ship the player is on in the campaign.
        /// </summary>
        public static int PlayerFloorHeight
        {
            get { return playerFloorHeight; }
            set { playerFloorHeight = value % 3; }
        }
        private static int playerFloorHeight = 1;

        public static void ResetPlayerValues()
        {
            PlayerLevelProgress = 0;
            PlayerFloorHeight = 1;

            player_health = 100;
            player_ammunition = 100;
            player_coin_amount = 200;
        }
    }
}
