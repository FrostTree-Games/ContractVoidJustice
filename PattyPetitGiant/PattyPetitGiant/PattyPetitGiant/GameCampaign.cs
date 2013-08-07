using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PattyPetitGiant
{
    public class GameCampaign
    {
        public struct GameContract
        {
            public enum ContractType
            {
                NoContract,
                KillQuest,
            }

            public ContractType type;
            public Entity.EnemyType killTarget;
            public int goldPerKill;

            public string contractMessage;

            public int killCount;

            public GameContract(ContractType type, Entity.EnemyType killTarget, int goldPerKill)
            {
                this.type = type;
                this.killTarget = killTarget;
                this.goldPerKill = goldPerKill;

                killCount = 0;

                if (type == ContractType.NoContract)
                {
                    contractMessage = "No contracts available at this time.";
                    return;
                }

                StringBuilder builder = new StringBuilder();

                if (killTarget == Entity.EnemyType.Prisoner)
                {
                    switch (Game1.rand.Next() % 3)
                    {
                        case 0:
                            builder.Append("The ship's Warden wants to quell a prisoner uprising in the sector. ");
                            break;
                        case 1:
                            builder.Append("The guards in this sector need escaped prisoners killed and are posting a bounty. ");
                            break;
                        default:
                            builder.Append("A prisoner riot is building in this sector and the Warden wants it shut down. ");
                            break;
                    }
                }
                else if (killTarget == Entity.EnemyType.Guard)
                {
                    switch (Game1.rand.Next() % 3)
                    {
                        case 0:
                            builder.Append("Escaped prisoners are trying to take over the sector and have requested aid. ");
                            break;
                        case 1:
                            builder.Append("Prisoners want guards dead. ");
                            break;
                        default:
                            builder.Append("The prisoners are preparing a riot in one of the sectors and posted a bounty. ");
                            break;
                    }
                }

                builder.Append(Game1.rand.Next() % 2 == 0 ? "Take out " : "Terminate ");

                if (killTarget == Entity.EnemyType.Prisoner)
                {
                    builder.Append(Game1.rand.Next() % 2 == 0 ? "escaped prisoners " : "prisoners ");
                }
                else if (killTarget == Entity.EnemyType.Guard)
                {
                    builder.Append(Game1.rand.Next() % 2 == 0 ? "prison security " : "guards ");
                }

                builder.Append("for ");

                builder.Append(goldPerKill);

                builder.Append(" credits per kill. ");

                int linesOut = 0;
                contractMessage = InGameGUI.WrapText(Game1.tenbyFive14, builder.ToString(), 525, ref linesOut);
            }
        }

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

        private static float player2_health = 100.00f;
        public static float Player2_Health
        {
            set { player2_health = value; }
            get { return player2_health; }
        }
        private static float player2_ammunition = 100;
        public static float Player2_Ammunition
        {
            set { player2_ammunition = value; }
            get { return player2_ammunition; }
        }

        private static float allegiance = 0.5f;
        /// <summary>
        /// Used to determine where the player stands with the guards and/or prisoners.
        /// </summary>
        /// <remarks>Ranged between 0.0f and 1.0f. 0.0f represents the player siding with the prisoners. 1.0f represents the player siding with the guards.</remarks>
        public static float PlayerAllegiance { get { return allegiance; } }

        public const int numberOfLevels = 6;
        public static int[] floorProgress = null;

        /// <summary>
        /// Increment or decrement the player's standing between the prisoners and guards.
        /// </summary>
        /// <param name="value">Value to alter PlayerAllegiance by. Make it negative for when a guard is killed. Positive when a prisoner is killed.</param>
        public static void AlterAllegiance(float value)
        {
            allegiance += value;

            if (allegiance < 0.0f) { allegiance = 0.0f; }
            if (allegiance > 1.0f) { allegiance = 1.0f; }

            if (currentContract.type == GameContract.ContractType.KillQuest)
            {
                if (value < 0 && currentContract.killTarget == Entity.EnemyType.Guard)
                {
                    currentContract.killCount++;
                }
                else if (value > 0 && currentContract.killTarget == Entity.EnemyType.Prisoner)
                {
                    currentContract.killCount++;
                }
            }
        }

        public static GlobalGameConstants.itemType Player_Item_1;
        public static GlobalGameConstants.itemType Player_Item_2;

        public static GlobalGameConstants.itemType Player2_Item_1;
        public static GlobalGameConstants.itemType Player2_Item_2;

        public static GameContract currentContract;

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
        private static int playerColor = 0;
        public static int PlayerColor { get { return playerColor; } }

        private static string player2Name = null;
        public static string Player2Name { get { return player2Name; } }
        private static int player2Color = 0;
        public static int Player2Color { get { return player2Color; } } 

        private static float elapsedCampaignTime;
        public static float ElapsedCampaignTime { get { return elapsedCampaignTime; } set { elapsedCampaignTime = value; } }

        public static LevelSelectState.LevelData[,] levelMap = null;

        private static bool isATwoPlayerGame;
        public static bool IsATwoPlayerGame { get { return isATwoPlayerGame; } set { isATwoPlayerGame = value; } }

        public static void ResetPlayerValues(string player1Name, int player1Color)
        {
            if (floorProgress == null)
            {
                floorProgress = new int[numberOfLevels];
            }

            for (int i = 0; i < numberOfLevels; i++) { floorProgress[i] = -1; }

            PlayerLevelProgress = -1;
            PlayerFloorHeight = 1;

            Player_Item_1 = GlobalGameConstants.itemType.MachineGun;
            Player_Item_2 = GlobalGameConstants.itemType.ShotGun;

            allegiance = 0.5f;

            playerName = player1Name;
            playerColor = player1Color;

            player_health = 100;

            player_ammunition = 100;
            player_coin_amount = 200;

            currentGuardRate = 1;
            currentPrisonerRate = 1;
            currentAlienRate = 1;

            elapsedCampaignTime = 0.0f;

            currentContract = new GameContract();
            currentContract.type = GameContract.ContractType.NoContract;

            levelMap = new LevelSelectState.LevelData[6, 3];

            for (int i = 0; i < levelMap.GetLength(0); i++)
            {
                for (int j = 0; j < levelMap.GetLength(1); j++)
                {
                    levelMap[i, j] = new LevelSelectState.LevelData(Game1.rand.NextDouble(), Game1.rand.NextDouble(), Game1.rand.NextDouble(), Game1.rand.NextDouble());
                }
            }

            levelMap[0, 0].visible = false;
            levelMap[0, 2].visible = false;
            levelMap[levelMap.GetLength(0) - 1, 0].visible = false;
            levelMap[levelMap.GetLength(0) - 1, 2].visible = false;

            isATwoPlayerGame = false;
        }

        public static void ResetPlayerValues(string player1Name, int player1Color, string Player2Name, int Player2Color)
        {
            ResetPlayerValues(player1Name, player1Color);

            Player2_Item_1 = GlobalGameConstants.itemType.Sword;
            Player2_Item_2 = GlobalGameConstants.itemType.Gun;

            player2Name = Player2Name;
            player2Color = Player2Color;

            player2_health = 100;
            player2_ammunition = 100;

            isATwoPlayerGame = true;
        }
    }
}