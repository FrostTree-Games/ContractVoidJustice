﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    public class LevelSelectState : ScreenState
    {
        private struct LevelData
        {
            /// <summary>
            /// If this is false, then the array cell is not actually a level and should not be considered.
            /// </summary>
            public bool visible;

            public double prisonerRates;
            public double guardRates;
            public double alienRates;
            public double lootRates;

            public LevelData(double prisonerRates, double guardRates, double alienRates, double lootRates)
            {
                visible = true;

                this.prisonerRates = prisonerRates;
                this.guardRates = guardRates;
                this.alienRates = alienRates;
                this.lootRates = lootRates;
            }
        }

        private LevelData[,] levelMap = null;
        int selectedLevelX, selectedLevelY;

        private Vector2 drawMapTestOffset = new Vector2(100, 100);

        private bool upPressed = false;
        private bool downPressed = false;
        private bool confirmPressed = false;

        public LevelSelectState()
        {
            levelMap = new LevelData[6, 3];

            for (int i = 0; i < levelMap.GetLength(0); i++)
            {
                for (int j = 0; j < levelMap.GetLength(1); j++)
                {
                    levelMap[i, j] = new LevelData(Game1.rand.NextDouble(), Game1.rand.NextDouble(), Game1.rand.NextDouble(), Game1.rand.NextDouble());
                }
            }

            levelMap[0, 0].visible = false;
            levelMap[0, 2].visible = false;
            levelMap[levelMap.GetLength(0) - 1, 0].visible = false;
            levelMap[levelMap.GetLength(0) - 1, 2].visible = false;

            selectedLevelX = GameCampaign.PlayerLevelProgress + 1;
            selectedLevelY = GameCampaign.PlayerFloorHeight;
        }

        protected override void doUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection) && !downPressed)
            {
                downPressed = true;
            }
            else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection) && downPressed)
            {
                downPressed = false;

                if (selectedLevelY < levelMap.GetLength(1) - 1 && levelMap[selectedLevelX, selectedLevelY + 1].visible && selectedLevelY - GameCampaign.PlayerFloorHeight < 1)
                {
                    selectedLevelY++;
                }
            }

            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection) && !upPressed)
            {
                upPressed = true;
            }
            else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection) && upPressed)
            {
                upPressed = false;

                if (selectedLevelY > 0 && levelMap[selectedLevelX, selectedLevelY - 1].visible && selectedLevelY - GameCampaign.PlayerFloorHeight > -1)
                {
                    selectedLevelY--;
                }
            }

            selectedLevelX = GameCampaign.PlayerLevelProgress + 1;

            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm) && !confirmPressed)
            {
                confirmPressed = true;
            }
            else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm) && confirmPressed)
            {
                confirmPressed = false;

                GameCampaign.CurrentAlienRate = levelMap[selectedLevelX, selectedLevelY].alienRates;
                GameCampaign.CurrentGuardRate = levelMap[selectedLevelX, selectedLevelY].guardRates;
                GameCampaign.CurrentPrisonerRate = levelMap[selectedLevelX, selectedLevelY].prisonerRates;

                GameCampaign.PlayerLevelProgress = GameCampaign.PlayerLevelProgress + 1;
                GameCampaign.PlayerFloorHeight = selectedLevelY;

                isComplete = true;
            }
        }

        private void drawLine(SpriteBatch sb, Vector2 origin, float length, float rotation, Color color)
        {
            sb.Draw(Game1.whitePixel, origin, null, color, rotation, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0.5f);
        }

        public override void render(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            Texture2D tex = TextureLib.getLoadedTexture("derek.png");

            sb.Begin();

            for (int i = 0; i < levelMap.GetLength(0); i++)
            {
                for (int j = 0; j < levelMap.GetLength(1); j++)
                {
                    if (!levelMap[i, j].visible)
                    {
                        continue;
                    }

                    sb.Draw(tex, new Vector2(i * 32, j * 32) + drawMapTestOffset, new Rectangle(16, 0, 16, 16), Color.White);

                    if (i == GameCampaign.PlayerLevelProgress && j == GameCampaign.PlayerFloorHeight)
                    {
                        sb.Draw(Game1.whitePixel, new Vector2(i * 32 + 4, j * 32 + 4) + drawMapTestOffset, null, Color.Red, 0.0f, Vector2.Zero, 8.0f, SpriteEffects.None, 0.5f);

                        if (levelMap[GameCampaign.PlayerLevelProgress + 1, GameCampaign.PlayerFloorHeight].visible)
                        {
                            drawLine(sb, new Vector2(i * 32 + 8, j * 32 + 8) + drawMapTestOffset, 32, 0.0f, Color.White);
                        }
                        if (GameCampaign.PlayerFloorHeight < 2 && levelMap[GameCampaign.PlayerLevelProgress + 1, GameCampaign.PlayerFloorHeight + 1].visible)
                        {
                            drawLine(sb, new Vector2(i * 32 + 8, j * 32 + 8) + drawMapTestOffset, 32, (float)(Math.PI / 4), Color.White);
                        }
                        if (GameCampaign.PlayerFloorHeight > 0 && levelMap[GameCampaign.PlayerLevelProgress + 1, GameCampaign.PlayerFloorHeight - 1].visible)
                        {
                            drawLine(sb, new Vector2(i * 32 + 8, j * 32 + 8) + drawMapTestOffset, 32, (float)(Math.PI / -4), Color.White);
                        }
                    }
                    if (i == selectedLevelX && j == selectedLevelY)
                    {
                        sb.Draw(Game1.whitePixel, new Vector2(i * 32 + 4, j * 32 + 4) + drawMapTestOffset, null, Color.Green, 0.0f, Vector2.Zero, 8.0f, SpriteEffects.None, 0.5f);
                    }
                }
            }

            sb.DrawString(Game1.font, "\nPrisoner Rates: " + levelMap[selectedLevelX, selectedLevelY].prisonerRates, 2 * drawMapTestOffset, Color.Orange);
            sb.DrawString(Game1.font, "\n\nAlien Rates: " + levelMap[selectedLevelX, selectedLevelY].alienRates, 2 * drawMapTestOffset, Color.Red);
            sb.DrawString(Game1.font, "\n\n\nGuard Rates: " + levelMap[selectedLevelX, selectedLevelY].guardRates, 2 * drawMapTestOffset, Color.LightBlue);
            sb.DrawString(Game1.font, "\n\n\n\nLoot Rates: " + levelMap[selectedLevelX, selectedLevelY].lootRates, 2 * drawMapTestOffset, Color.Black);

            sb.Draw(Game1.whitePixel, 2 * drawMapTestOffset - new Vector2(1, 1), null, Color.Black, 0.0f, Vector2.Zero, new Vector2(52, 16), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, 2 * drawMapTestOffset, null, Color.Orange, 0.0f, Vector2.Zero, new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].prisonerRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 14), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, 2 * drawMapTestOffset + new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].prisonerRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 0), null, Color.Red, 0.0f, Vector2.Zero, new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].alienRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 14), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, 2 * drawMapTestOffset + new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].prisonerRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 0) + new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].alienRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 0), null, Color.LightBlue, 0.0f, Vector2.Zero, new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].guardRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 14), SpriteEffects.None, 0.5f);

            sb.End();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            return ScreenStateType.LevelState;
        }
    }
}
