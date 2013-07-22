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

        private static float allegiance = 0.5f;
        /// <summary>
        /// Used to determine where the player stands with the guards and/or prisoners.
        /// </summary>
        /// <remarks>Ranged between 0.0f and 1.0f. 0.0f represents the player siding with the prisoners. 1.0f represents the player siding with the guards.</remarks>
        public static float PlayerAllegiance { get { return allegiance; } }

        /// <summary>
        /// Increment or decrement the player's standing between the prisoners and guards.
        /// </summary>
        /// <param name="value">Value to alter PlayerAllegiance by.</param>
        public static void AlterAllegiance(float value)
        {
            allegiance += value;

            if (allegiance < 0.0f) { allegiance = 0.0f; }
            if (allegiance > 1.0f) { allegiance = 1.0f; }
        }

        public static GlobalGameConstants.itemType Player_Item_1;
        public static GlobalGameConstants.itemType Player_Item_2;

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

        private static double currentGuardRate = 1;
        private static double currentPrisonerRate = 1;
        private static double currentAlienRate = 1;
        public static double CurrentGuardRate { get { return currentGuardRate; } set { currentGuardRate = value; } }
        public static double CurrentPrisonerRate { get { return currentPrisonerRate; } set { currentPrisonerRate = value; } }
        public static double CurrentAlienRate { get { return currentAlienRate; } set { currentAlienRate = value; } }

        private static string playerName = null;
        public static string PlayerName { get { return playerName; } }

        private static float elapsedCampaignTime;
        public static float ElapsedCampaignTime { get { return elapsedCampaignTime; } set { elapsedCampaignTime = value; } }

        public static void ResetPlayerValues()
        {
            PlayerLevelProgress = 0;
            PlayerFloorHeight = 1;

            Player_Item_1 = GlobalGameConstants.itemType.Gun;
            Player_Item_2 = GlobalGameConstants.itemType.DungeonMap;

            allegiance = 0.5f;

            playerName = "Jensen";

            player_health = 100;
            player_ammunition = 100;
            player_coin_amount = 200;

            currentGuardRate = 1;
            currentPrisonerRate = 1;
            currentAlienRate = 1;

            elapsedCampaignTime = 0.0f;
        }
    }
}
