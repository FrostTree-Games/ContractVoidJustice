using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class LevelReviewState : ScreenState
    {
        private bool confirmPressed = false;

        private float screenTimePassed;
        private const float numberTickingDuration = 500f;

        private string view_timePassed;
        private string view_timeElapsed;
        private string view_levelCoins;
        private string view_totalCoins;

        private string message_timePassed = "Level Time";
        private string message_totalTimePassed = "Total Game Time";
        private string message_levelCoins = "Coins Collected";
        private string message_totalCoins = "Net Coins";

        public LevelReviewState()
        {
            GameCampaign.ElapsedCampaignTime += LevelState.ElapsedLevelTime;

            screenTimePassed = 0;

            view_timePassed = "0";
            view_timeElapsed = "0";
            view_levelCoins = "0";
            view_totalCoins = "0";
        }

        protected override void doUpdate(GameTime currentTime)
        {
            screenTimePassed += currentTime.ElapsedGameTime.Milliseconds;

            if (screenTimePassed < numberTickingDuration)
            {
                view_timePassed = ((int)((screenTimePassed / numberTickingDuration) * LevelState.ElapsedLevelTime / 1000)).ToString();
                view_timeElapsed = ((int)((screenTimePassed / numberTickingDuration) * GameCampaign.ElapsedCampaignTime / 1000)).ToString();

                view_levelCoins = ((int)((screenTimePassed / numberTickingDuration) * LevelState.ElapsedCoinAmount)).ToString();
                view_totalCoins = ((int)((screenTimePassed / numberTickingDuration) * GameCampaign.Player_Coin_Amount)).ToString();
            }
            else
            {
                view_timePassed = ((int)(LevelState.ElapsedLevelTime / 1000)).ToString();
                view_timeElapsed = ((int)(GameCampaign.ElapsedCampaignTime / 1000)).ToString();

                view_levelCoins = (LevelState.ElapsedCoinAmount).ToString();
                view_totalCoins = (GameCampaign.Player_Coin_Amount).ToString();
            }

            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm) && !confirmPressed)
            {
                confirmPressed = true;
            }
            else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm) && confirmPressed)
            {
                confirmPressed = false;

                if (screenTimePassed < 600f)
                {
                    screenTimePassed += 600;
                }
                else
                {
                    isComplete = true;
                }
            }
        }

        private void drawLine(SpriteBatch sb, Vector2 origin, float length, float rotation, Color color, float width)
        {
            sb.Draw(Game1.whitePixel, origin, null, color, rotation, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0.5f);
        }

        private void drawBox(SpriteBatch sb, Rectangle rect, Color clr, float lineWidth)
        {
            drawLine(sb, new Vector2(rect.X, rect.Y), rect.Width, 0.0f, clr, lineWidth);
            drawLine(sb, new Vector2(rect.X, rect.Y), rect.Height, (float)(Math.PI / 2), clr, lineWidth);
            drawLine(sb, new Vector2(rect.X - lineWidth, rect.Y + rect.Height), rect.Width + lineWidth, 0.0f, clr, lineWidth);
            drawLine(sb, new Vector2(rect.X + rect.Width, rect.Y), rect.Height, (float)(Math.PI / 2), clr, lineWidth);
        }

        public override void render(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.Identity);

            sb.Draw(Game1.whitePixel, new Vector2(-400), null, Color.Black, 0.0f, Vector2.Zero, new Vector2(9999), SpriteEffects.None, 0.5f);

            sb.Draw(Game1.whitePixel, XboxTools.GetTitleSafeArea(AnimationLib.GraphicsDevice, 0.8f), new Color(0.0f, 0.75f, 1.0f, 0.1f));

            sb.DrawString(Game1.tenbyFive24, "Level Complete", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 100) - (Game1.tenbyFive24.MeasureString("Level Complete") / 2), Color.White);

            // time
            sb.DrawString(Game1.tenbyFive14, message_timePassed, new Vector2(GlobalGameConstants.GameResolutionWidth / 3, 200) - (Game1.tenbyFive14.MeasureString(message_timePassed) / 2), Color.White);
            sb.DrawString(Game1.tenbyFive14, message_totalTimePassed, new Vector2((GlobalGameConstants.GameResolutionWidth / 3) * 2, 200) - (Game1.tenbyFive14.MeasureString(message_totalTimePassed) / 2), Color.White);
            sb.DrawString(Game1.tenbyFive24, view_timePassed, new Vector2(GlobalGameConstants.GameResolutionWidth / 3, 200) - (Game1.tenbyFive14.MeasureString(view_timePassed) / 2) + new Vector2(0, 24), Color.White);
            sb.DrawString(Game1.tenbyFive24, view_timeElapsed, new Vector2((GlobalGameConstants.GameResolutionWidth / 3) * 2, 200) - (Game1.tenbyFive14.MeasureString(view_timeElapsed) / 2) + new Vector2(0, 24), Color.White);

            // coins
            sb.DrawString(Game1.tenbyFive14, message_levelCoins, new Vector2(GlobalGameConstants.GameResolutionWidth / 3, 265) - (Game1.tenbyFive14.MeasureString(message_levelCoins) / 2), Color.White);
            sb.DrawString(Game1.tenbyFive14, message_totalCoins, new Vector2((GlobalGameConstants.GameResolutionWidth / 3) * 2, 265) - (Game1.tenbyFive14.MeasureString(message_totalCoins) / 2), Color.White);
            sb.DrawString(Game1.tenbyFive24, view_levelCoins, new Vector2(GlobalGameConstants.GameResolutionWidth / 3, 265) - (Game1.tenbyFive14.MeasureString(view_levelCoins) / 2) + new Vector2(0, 24), Color.White);
            sb.DrawString(Game1.tenbyFive24, view_totalCoins, new Vector2((GlobalGameConstants.GameResolutionWidth / 3) * 2, 265) - (Game1.tenbyFive14.MeasureString(view_totalCoins) / 2) + new Vector2(0, 24), Color.White);

            drawBox(sb, new Rectangle((GlobalGameConstants.GameResolutionWidth / 8), 350, 550, 200), Color.White, 2);
            sb.DrawString(Game1.tenbyFive24, "Contract", new Vector2((GlobalGameConstants.GameResolutionWidth / 8) + 350/2 + 30, 350), Color.White);
            sb.DrawString(Game1.tenbyFive14, GameCampaign.levelMap[GameCampaign.PlayerLevelProgress, GameCampaign.PlayerFloorHeight].contract.contractMessage, new Vector2((GlobalGameConstants.GameResolutionWidth / 8) + 15, 385), Color.White);
            if (GameCampaign.currentContract.type == GameCampaign.GameContract.ContractType.KillQuest)
            {
                sb.DrawString(Game1.tenbyFive24, GameCampaign.currentContract.killCount + " kills made", new Vector2((GlobalGameConstants.GameResolutionWidth / 8) + 350 / 2 + 20, 500), Color.White);
            }

            for (int i = 0; i < GameCampaign.levelMap.GetLength(0); i++)
            {
                for (int j = 0; j < GameCampaign.levelMap.GetLength(1); j++)
                {
                    if (!GameCampaign.levelMap[i, j].visible)
                    {
                        continue;
                    }

                    if (j == GameCampaign.floorProgress[i])
                    {
                        sb.Draw(Game1.whitePixel, new Vector2(755, 390) + new Vector2(64 * i, 64 * j), null, Color.White, 0.0f, Vector2.Zero, new Vector2(32), SpriteEffects.None, 0.5f);
                    }
                    else
                    {
                        sb.Draw(Game1.whitePixel, new Vector2(755, 390) + new Vector2(64 * i, 64 * j), null, new Color(1, 1, 1, 0.5f), 0.0f, Vector2.Zero, new Vector2(32), SpriteEffects.None, 0.5f);
                    }
                }
            }

            for (int i = 0; i < GameCampaign.PlayerLevelProgress; i++)
            {
                float length = GameCampaign.floorProgress[i + 1] == GameCampaign.floorProgress[i] ? 64 : 90.5f;

                drawLine(sb, new Vector2(755, 390) + new Vector2(64 * i, 64 * GameCampaign.floorProgress[i]) + new Vector2(16), length, (float)(Math.Atan2(GameCampaign.floorProgress[i+1] - GameCampaign.floorProgress[i], 1)), Color.Red, 2.0f);
            }

            if (screenTimePassed > numberTickingDuration)
            {
                sb.DrawString(Game1.tenbyFive24, "Press A to continue", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 596) - (Game1.tenbyFive24.MeasureString("Press A to continue") / 2), Color.Lerp(Color.White, Color.Transparent, (float)(Math.Sin(screenTimePassed / 500) * 0.5f) + 0.5f));
            }

            sb.End();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            return ScreenStateType.LevelSelectState;
        }
    }
}
