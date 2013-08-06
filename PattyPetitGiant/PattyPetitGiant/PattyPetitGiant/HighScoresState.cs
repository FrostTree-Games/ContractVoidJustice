using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class HighScoresState : ScreenState
    {
        // gotta have at least one high verbosity enum name in this project
        private enum HighScoreStateScreenAnimationState
        {
            Start,
            SlideNewHighScoreIn,
            IdleWait,
        }

        public class HighScoreValue
        {
            public string playerName;
            public int coinCollected;
            public float secondsElapsed;
            public int levelDiedAt;
            public int floorDiedOn;
            public bool completedGame;

            public HighScoreValue(string playerName, int coinCollected, float secondsElapsed, int levelDiedAt, int floorDiedOn)
            {
                this.playerName = playerName;
                this.coinCollected = coinCollected;
                this.secondsElapsed = secondsElapsed;
                this.levelDiedAt = levelDiedAt;
                this.floorDiedOn = floorDiedOn % 3;
                completedGame = false;
            }

            public HighScoreValue(string playerName, int coinCollected, float secondsElapsed, int levelDiedAt, int floorDiedOn, bool completedGame)
            {
                this.playerName = playerName;
                this.coinCollected = coinCollected;
                this.secondsElapsed = secondsElapsed;
                this.levelDiedAt = levelDiedAt;
                this.floorDiedOn = floorDiedOn % 3;
                this.completedGame = completedGame;
            }
        }

        private class CompareHighScores : Comparer<HighScoreValue>
        {
            /// <summary>
            /// Compares two high sores to determine which is the higher of the two. Used for quick sorting in C#.
            /// </summary>
            /// <param name="x">First score to compare.</param>
            /// <param name="y">Second scoare to compare.</param>
            /// <returns>
            ///           -1 if x less than y;
            ///           0 if x and y are equal;
            ///           1 if x is greater than y
            /// </returns>
            public override int Compare(HighScoreValue x, HighScoreValue y)
            {
#if DEBUG
                // I'll just leave this here.
                if (x.playerName == "FatalKabuki" && y.playerName == "Wilso326")
                {
                    return 1;
                }
                else if (x.playerName == "Wilso326" && y.playerName == "FatalKabuki")
                {
                    return -1;
                }
#endif

                if (x.levelDiedAt < y.levelDiedAt)
                {
                    return -1;
                }
                else if (x.levelDiedAt > y.levelDiedAt)
                {
                    return 1;
                }
                else
                {
                    if (x.coinCollected < y.coinCollected)
                    {
                        return -1;
                    }
                    else if (x.coinCollected > y.coinCollected)
                    {
                        return 1;
                    }
                    else
                    {
                        if (x.secondsElapsed > y.secondsElapsed)
                        {
                            return -1;
                        }
                        else if (x.secondsElapsed < y.secondsElapsed)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }

        private static List<HighScoreValue> highScores = null;
        private static HighScoreValue mostCoin = null;

        private int newlyAddedScoreIndex = -2;

        private HighScoreStateScreenAnimationState state;
        private float stateTimer;
        private const float preWaitDuration = 600f;
        private const float slideInDuration = 300f;

        public static void InitalizeHighScores()
        {
            if (highScores == null)
            {
                highScores = new List<HighScoreValue>();

                //test values to punch in; load from card later
                highScores.Add(new HighScoreValue("Wilson", 6969, 83939, 2, 2));
                highScores.Add(new HighScoreValue("Eric", 999999, 123, 3, 0));
                highScores.Add(new HighScoreValue("Daniel", 123456, 123, 1, 1));
                highScores.Add(new HighScoreValue("Ivan", 1000, 345, 0, 1));
                highScores.Add(new HighScoreValue("Alyosha", 1000, 345, 0, 1));
                highScores.Add(new HighScoreValue("Dmitri", 1500, 43567, 4, 2, true));
                highScores.Add(new HighScoreValue("Gunther", 2233, 2332, 2, 1));
                highScores.Add(new HighScoreValue("lolololol", 2020, 333, 1, 0));
                highScores.Add(new HighScoreValue("quinten", 20202, 9981, 1, 1));
                highScores.Add(new HighScoreValue("Zippy", 200, 000, 5, 1));

                //find the high score value with the most coin collected
                mostCoin = highScores[0];
                for (int i = 1; i < highScores.Count; i++)
                {
                    if (highScores[i].coinCollected > mostCoin.coinCollected)
                    {
                        mostCoin = highScores[i];
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new high scores screen state.
        /// </summary>
        /// <param name="inGame">If the player has died or completed the game, set this to true. If simply checking the scores from the menu, set this to false</param>
        public HighScoresState(bool inGame)
        {
            InitalizeHighScores();

            HighScoreValue newScore = new HighScoreValue(GameCampaign.PlayerName, GameCampaign.Player_Coin_Amount, GameCampaign.ElapsedCampaignTime, GameCampaign.PlayerLevelProgress, GameCampaign.PlayerFloorHeight);

            if (inGame)
            {
                highScores.Add(newScore);

                // if there are 11 high scores now, remove the lowest score
                if (highScores.Count > 10)
                {
                    HighScoreValue lowestScore = highScores[0];
                    CompareHighScores comparer = new CompareHighScores();

                    for (int i = 1; i < highScores.Count; i++)
                    {
                        if (comparer.Compare(highScores[i], lowestScore) < 0)
                        {
                            lowestScore = highScores[i];
                        }
                    }

                    highScores.Remove(lowestScore);
                }

                highScores.Sort(new CompareHighScores());
                highScores.Reverse();

                for (int i = 0; i < highScores.Count; i++)
                {
                    if (highScores[i] == newScore)
                    {
                        newlyAddedScoreIndex = i;
                    }
                }
            }
            else
            {
                highScores.Sort(new CompareHighScores());
                highScores.Reverse();
            }

            state = HighScoreStateScreenAnimationState.Start;
            stateTimer = 0;
        }

        protected override void doUpdate(GameTime currentTime)
        {
            stateTimer += currentTime.ElapsedGameTime.Milliseconds;

            CampaignLobbyState.lineOffset += (currentTime.ElapsedGameTime.Milliseconds * CampaignLobbyState.lineMoveSpeed);
            if (CampaignLobbyState.lineOffset > 16.0f) { CampaignLobbyState.lineOffset -= 16.0f; }
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
            AnimationLib.GraphicsDevice.Clear(Color.Black);

            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.Identity);

            sb.DrawString(Game1.tenbyFive24, "Postcareer Statistics Report", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 100) - Game1.tenbyFive24.MeasureString("Postcareer Statistics Report") / 2, Color.White);

            Vector2 linesOffset = new Vector2(CampaignLobbyState.lineOffset);

            for (int i = -6; i < GlobalGameConstants.GameResolutionWidth / 16 + 8; i ++)
            {
                drawLine(sb, new Vector2(i * 16, -16) + linesOffset, GlobalGameConstants.GameResolutionHeight + 32, (float)Math.PI / 2, new Color(1, 0, 1, 0.1f), 1.0f);
            }

            for (int i = -6; i < GlobalGameConstants.GameResolutionHeight / 16 + 8; i++)
            {
                drawLine(sb, new Vector2(-16, i * 16) + linesOffset, GlobalGameConstants.GameResolutionWidth + 32, 0, new Color(1, 0, 1, 0.1f), 1.0f);
            }

            sb.Draw(Game1.whitePixel, XboxTools.GetTitleSafeArea(AnimationLib.GraphicsDevice, 0.8f), new Color(0.0f, 0.75f, 1.0f, 0.1f));

            drawBox(sb, new Rectangle(128 + 32, 128, 448, (32 * (highScores.Count + 1))), Color.White, 2);
            drawLine(sb, new Vector2(128 + 32 + 150, 128), (32 * (highScores.Count + 1)), (float)Math.PI / 2, Color.White, 2);
            drawLine(sb, new Vector2(128 + 32 + 150 + 100, 128), (32 * (highScores.Count + 1)), (float)Math.PI / 2, Color.White, 2);
            drawLine(sb, new Vector2(128 + 32 + 150 + 100 + 75, 128), (32 * (highScores.Count + 1)), (float)Math.PI / 2, Color.White, 2);
            drawLine(sb, new Vector2(128 + 32 + 150 + 100 + 75 + 75, 128), (32 * (highScores.Count + 1)), (float)Math.PI / 2, Color.White, 2);
            sb.DrawString(Game1.tenbyFive14, "NAME", new Vector2(128 + 32, 128), Color.White);
            sb.DrawString(Game1.tenbyFive14, "FATE", new Vector2(128 + 32 + 150, 128), Color.White);
            sb.DrawString(Game1.tenbyFive14, "FUND", new Vector2(128 + 32 + 150 + 100, 128), Color.White);
            sb.DrawString(Game1.tenbyFive14, "TIME", new Vector2(128 + 32 + 150 + 100 + 75, 128), Color.White);
            sb.DrawString(Game1.tenbyFive14, "RANK", new Vector2(128 + 32 + 150 + 100 + 75 + 75, 128), Color.White);

            for (int i = 0; i < highScores.Count; i++)
            {
                Color textColor = (i == newlyAddedScoreIndex ? Color.Lerp(Color.Cyan, Color.Orange, (float)((1 + Math.Sin(stateTimer / 300)) / 2)) : Color.White);

                drawLine(sb, new Vector2(128 + 32, 128 + 32 + (i * 32)), 448, 0.0f, Color.White, 2);
                sb.DrawString(Game1.tenbyFive14, highScores[i].playerName, new Vector2(128 + 32, 128 + ((i + 1) * 32)) + new Vector2(4), textColor);
                sb.DrawString(Game1.tenbyFive14, (highScores[i].completedGame ? "ESCAPED" : ("DIED AT " + highScores[i].levelDiedAt + (highScores[i].floorDiedOn == 0 ? 'A' : ((highScores[i].floorDiedOn == 1) ? 'B' : 'C')))), new Vector2(128 + 32 + 150, 128 + ((i + 1) * 32)) + new Vector2(4), (highScores[i].completedGame && i != newlyAddedScoreIndex) ? Color.Red : textColor);
                sb.DrawString(Game1.tenbyFive14, highScores[i].coinCollected.ToString(), new Vector2(128 + 32 + 150 + 100, 128 + ((i + 1) * 32)) + new Vector2(4), textColor);
                sb.DrawString(Game1.tenbyFive14, highScores[i].secondsElapsed.ToString(), new Vector2(128 + 32 + 150 + 100 + 75, 128 + ((i + 1) * 32)) + new Vector2(4), textColor);
                sb.DrawString(Game1.tenbyFive14, (i + 1).ToString(), new Vector2(128 + 32 + 150 + 100 + 75 + 75, 128 + ((i + 1) * 32)) + new Vector2(4), textColor);
            }

            /*
            for (int i = 0; i < highScores.Count; i++)
            {
                if (state == HighScoreStateScreenAnimationState.Start)
                {
                    if (i == newlyAddedScoreIndex) { continue; }

                    Vector2 drawListPosition = new Vector2(128) + new Vector2(0, i * 28);
                    if (i > newlyAddedScoreIndex) { drawListPosition.Y -= 28; }

                    sb.DrawString(Game1.font, highScores[i].playerName + " died on " + highScores[i].levelDiedAt + " with " + highScores[i].coinCollected + " after " + highScores[i].secondsElapsed, drawListPosition, Color.White);
                }
                else if (state == HighScoreStateScreenAnimationState.SlideNewHighScoreIn)
                {
                    if (i == newlyAddedScoreIndex)
                    {
                        Vector2 drawListPosition = new Vector2(128) + new Vector2(-200, i * 28);
                        drawListPosition.X += 200 * (stateTimer / slideInDuration);

                        sb.DrawString(Game1.font, highScores[i].playerName + " died on " + highScores[i].levelDiedAt + " with " + highScores[i].coinCollected + " after " + highScores[i].secondsElapsed, drawListPosition, Color.White);
                    }
                    else
                    {
                        Vector2 drawListPosition = new Vector2(128) + new Vector2(0, i * 28);
                        if (i > newlyAddedScoreIndex) { drawListPosition.Y -= 28 * (1 - (stateTimer / slideInDuration)); }

                        sb.DrawString(Game1.font, highScores[i].playerName + " died on " + highScores[i].levelDiedAt + " with " + highScores[i].coinCollected + " after " + highScores[i].secondsElapsed, drawListPosition, Color.White);
                    }
                }
                else
                {
                    Vector2 drawListPosition = new Vector2(128) + new Vector2(0, i * 28);

                    sb.DrawString(Game1.font, highScores[i].playerName + " died on " + highScores[i].levelDiedAt + " with " + highScores[i].coinCollected + " after " + highScores[i].secondsElapsed, drawListPosition, Color.White);
                }
            } */

            sb.End();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
 	        return ScreenStateType.TitleScreen;
        }
    }
}
