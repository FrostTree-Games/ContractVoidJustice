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

            public HighScoreValue(string playerName, int coinCollected, float secondsElapsed, int levelDiedAt, int floorDiedOn)
            {
                this.playerName = playerName;
                this.coinCollected = coinCollected;
                this.secondsElapsed = secondsElapsed;
                this.levelDiedAt = levelDiedAt;
                this.floorDiedOn = floorDiedOn % 3;
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
                highScores.Add(new HighScoreValue("Dmitri", 1500, 43567, 4, 2));
                highScores.Add(new HighScoreValue("Gunther", 2233, 2332, 2, 1));
                highScores.Add(new HighScoreValue("lolololol", 2020, 333, 1, 0));
                highScores.Add(new HighScoreValue("quinten", 20202, 9981, 1, 1));
                highScores.Add(new HighScoreValue("Zippy", 200, 000, 5, 1));
            }
        }

        public HighScoresState()
        {
            InitalizeHighScores();

            HighScoreValue newScore = new HighScoreValue(GameCampaign.PlayerName, GameCampaign.Player_Coin_Amount, GameCampaign.ElapsedCampaignTime, GameCampaign.PlayerLevelProgress, GameCampaign.PlayerFloorHeight);

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

            state = HighScoreStateScreenAnimationState.Start;
            stateTimer = 0;
        }

        protected override void doUpdate(GameTime currentTime)
        {
            stateTimer += currentTime.ElapsedGameTime.Milliseconds;

            if (state == HighScoreStateScreenAnimationState.Start)
            {
                if (newlyAddedScoreIndex == -2)
                {
                    stateTimer = -300;
                    state = HighScoreStateScreenAnimationState.IdleWait;
                }

                if (stateTimer > preWaitDuration)
                {
                    stateTimer = 0;
                    state = HighScoreStateScreenAnimationState.SlideNewHighScoreIn;
                }
            }
            else if (state == HighScoreStateScreenAnimationState.SlideNewHighScoreIn)
            {
                if (stateTimer > slideInDuration)
                {
                    stateTimer = 0;
                    state = HighScoreStateScreenAnimationState.IdleWait;
                }
            }
            else
            {
                //
            }
        }

        public override void render(SpriteBatch sb)
        {
            AnimationLib.GraphicsDevice.Clear(Color.Black);

            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.Identity);

            for (int i = 0; i < highScores.Count; i++)
            {
                if (state == HighScoreStateScreenAnimationState.Start)
                {
                    if (i == newlyAddedScoreIndex) { continue; }

                    Vector2 drawListPosition = new Vector2(100) + new Vector2(0, i * 28);
                    if (i > newlyAddedScoreIndex) { drawListPosition.Y -= 28; }

                    sb.DrawString(Game1.font, highScores[i].playerName + " died on " + highScores[i].levelDiedAt + " with " + highScores[i].coinCollected + " after " + highScores[i].secondsElapsed, drawListPosition, Color.White);
                }
                else if (state == HighScoreStateScreenAnimationState.SlideNewHighScoreIn)
                {
                    if (i == newlyAddedScoreIndex)
                    {
                        Vector2 drawListPosition = new Vector2(100) + new Vector2(-200, i * 28);
                        drawListPosition.X += 200 * (stateTimer / slideInDuration);

                        sb.DrawString(Game1.font, highScores[i].playerName + " died on " + highScores[i].levelDiedAt + " with " + highScores[i].coinCollected + " after " + highScores[i].secondsElapsed, drawListPosition, Color.White);
                    }
                    else
                    {
                        Vector2 drawListPosition = new Vector2(100) + new Vector2(0, i * 28);
                        if (i > newlyAddedScoreIndex) { drawListPosition.Y -= 28 * (1 - (stateTimer / slideInDuration)); }

                        sb.DrawString(Game1.font, highScores[i].playerName + " died on " + highScores[i].levelDiedAt + " with " + highScores[i].coinCollected + " after " + highScores[i].secondsElapsed, drawListPosition, Color.White);
                    }
                }
                else
                {
                    Vector2 drawListPosition = new Vector2(100) + new Vector2(0, i * 28);

                    sb.DrawString(Game1.font, highScores[i].playerName + " died on " + highScores[i].levelDiedAt + " with " + highScores[i].coinCollected + " after " + highScores[i].secondsElapsed, drawListPosition, Color.White);
                }
            }

            sb.End();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
 	        return ScreenStateType.TitleScreen;
        }
    }
}
