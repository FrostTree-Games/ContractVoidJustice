﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

        public struct HighScoreValue
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

        public static List<HighScoreValue> highScores = null;

        private int newlyAddedScoreIndex = -2;

        private float stateTimer;
        private const float preWaitDuration = 600f;
        private const float slideInDuration = 300f;

        private bool player1ConfirmPressed = false;

        private bool inGame = false;
        private bool gameComplete = false;

        private bool isSignedIn = false;

        public static void ResetHighScores()
        {
            highScores = new List<HighScoreValue>(10);

            for (int i = 0; i < 6; i++)
            {
                highScores.Add(new HighScoreValue(CampaignLobbyState.randomNames[Game1.rand.Next() % CampaignLobbyState.randomNames.Length], 200 + 100 * i, (200 + (Game1.rand.Next() % 200) * i), i / 2, (i == 0) ? 1 : Game1.rand.Next() % 3));
            }
        }

        public static void InitalizeHighScores()
        {
            if (highScores == null)
            {
                ResetHighScores();
            }
        }

        /// <summary>
        /// Creates a new high scores screen state.
        /// </summary>
        /// <param name="inGame">If the player has died or completed the game, set this to true. If simply checking the scores from the menu, set this to false</param>
        public HighScoresState(bool inGame, bool gameComplete)
        {
            InitalizeHighScores();

            HighScoreValue newScore = new HighScoreValue(GameCampaign.PlayerName, GameCampaign.Player_Coin_Amount, GameCampaign.ElapsedCampaignTime, GameCampaign.PlayerLevelProgress, GameCampaign.PlayerFloorHeight);

            this.inGame = inGame;
            this.gameComplete = gameComplete;

            try
            {
                SignedInGamer gamer = Gamer.SignedInGamers[InputDevice2.GetPlayerGamePadIndex(InputDevice2.PPG_Player.Player_1)];
                isSignedIn = (gamer != null);
            }
            catch (Exception)
            {
                isSignedIn = false;
            }

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
                    if (highScores[i].playerName == newScore.playerName && highScores[i].secondsElapsed == newScore.secondsElapsed && highScores[i].floorDiedOn == newScore.floorDiedOn)
                    {
                        newlyAddedScoreIndex = i;
                    }
                }

                SaveGameModule.saveGame();
            }
            else
            {
                highScores.Sort(new CompareHighScores());
                highScores.Reverse();
            }

            stateTimer = 0;

            if (isSignedIn)
            {
                SaveGameModule.loadGame();
            }
        }

        protected override void doUpdate(GameTime currentTime)
        {
            stateTimer += currentTime.ElapsedGameTime.Milliseconds;

            if (stateTimer > 1500f)
            {
                if ((InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.Confirm) != InputDevice2.PlayerPad.NoPad) && !player1ConfirmPressed)
                {
                    player1ConfirmPressed = true;
                }
                else if (!(InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.Confirm) != InputDevice2.PlayerPad.NoPad) && player1ConfirmPressed)
                {
                    player1ConfirmPressed = false;

                    isComplete = true;
                }
            }

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

            for (int i = -6; i < GlobalGameConstants.GameResolutionWidth / 16 + 8; i++)
            {
                drawLine(sb, new Vector2(i * 16, -16) + linesOffset, GlobalGameConstants.GameResolutionHeight + 32, (float)Math.PI / 2, new Color(1, 0, 1, 0.1f), 1.0f);
            }

            for (int i = -6; i < GlobalGameConstants.GameResolutionHeight / 16 + 8; i++)
            {
                drawLine(sb, new Vector2(-16, i * 16) + linesOffset, GlobalGameConstants.GameResolutionWidth + 32, 0, new Color(1, 0, 1, 0.1f), 1.0f);
            }

            sb.Draw(Game1.whitePixel, XboxTools.GetTitleSafeArea(AnimationLib.GraphicsDevice, 0.8f), new Color(0.0f, 0.75f, 1.0f, 0.1f));

            if (isSignedIn)
            {
                drawBox(sb, new Rectangle(400, 166, 480, (32 * (10 + 1))), new Color(1, 1, 1, 0.5f), 2);
                drawBox(sb, new Rectangle(400, 166, 480, (32 * (highScores.Count + 1))), Color.White, 2);

                drawLine(sb, new Vector2(400 + 150, 166), (32 * (highScores.Count + 1)), (float)Math.PI / 2, Color.White, 2);
                drawLine(sb, new Vector2(400 + 150 + 132, 166), (32 * (highScores.Count + 1)), (float)Math.PI / 2, Color.White, 2);
                drawLine(sb, new Vector2(400 + 150 + 132 + 75, 166), (32 * (highScores.Count + 1)), (float)Math.PI / 2, Color.White, 2);
                drawLine(sb, new Vector2(400 + 150 + 132 + 75 + 75, 166), (32 * (highScores.Count + 1)), (float)Math.PI / 2, Color.White, 2);
                sb.DrawString(Game1.tenbyFive14, "NAME", new Vector2(400, 166), Color.White);
                sb.DrawString(Game1.tenbyFive14, "FATE", new Vector2(400 + 150, 166), Color.White);
                sb.DrawString(Game1.tenbyFive14, "FUND", new Vector2(400 + 150 + 132, 166), Color.White);
                sb.DrawString(Game1.tenbyFive14, "TIME", new Vector2(400 + 150 + 132 + 75, 166), Color.White);
                sb.DrawString(Game1.tenbyFive14, "RANK", new Vector2(400 + 150 + 132 + 75 + 75, 166), Color.White);

                for (int i = 0; i < 10; i++)
                {
                    Color textColor = (i == newlyAddedScoreIndex ? Color.Lerp(Color.Cyan, Color.Orange, (float)((1 + Math.Sin(stateTimer / 300)) / 2)) : Color.White);

                    if (i < highScores.Count)
                    {
                        drawLine(sb, new Vector2(400, 166 + 32 + (i * 32)), 480, 0.0f, Color.White, 2);
                        sb.DrawString(Game1.tenbyFive14, highScores[i].playerName, new Vector2(400, 166 + ((i + 1) * 32)) + new Vector2(4), textColor);
                        if (highScores[i].levelDiedAt == 0 && highScores[i].floorDiedOn == 1)
                        {
                            sb.DrawString(Game1.tenbyFive14, "DIED IN HANGAR", new Vector2(400 + 150, 166 + ((i + 1) * 32)) + new Vector2(4), textColor);
                        }
                        else if (highScores[i].levelDiedAt == 5 && highScores[i].floorDiedOn == 1)
                        {
                            sb.DrawString(Game1.tenbyFive14, "DIED AT BRIDGE", new Vector2(400 + 150, 166 + ((i + 1) * 32)) + new Vector2(4), textColor);
                        }
                        else
                        {
                            sb.DrawString(Game1.tenbyFive14, (highScores[i].completedGame ? "ESCAPED" : ("DIED AT " + highScores[i].levelDiedAt + (highScores[i].floorDiedOn == 0 ? 'A' : ((highScores[i].floorDiedOn == 1) ? 'B' : 'C')))), new Vector2(400 + 150, 166 + ((i + 1) * 32)) + new Vector2(4), (highScores[i].completedGame && i != newlyAddedScoreIndex) ? Color.Red : textColor);
                        }
                        sb.DrawString(Game1.tenbyFive14, highScores[i].coinCollected.ToString(), new Vector2(400 + 150 + 132, 166 + ((i + 1) * 32)) + new Vector2(4) + new Vector2(65 - Game1.tenbyFive14.MeasureString(highScores[i].coinCollected.ToString()).X, 0), textColor);
                        sb.DrawString(Game1.tenbyFive14, highScores[i].secondsElapsed.ToString(), new Vector2(400 + 150 + 132 + 75, 166 + ((i + 1) * 32)) + new Vector2(4) + new Vector2(65 - Game1.tenbyFive14.MeasureString(highScores[i].secondsElapsed.ToString()).X, 0), textColor);
                        sb.DrawString(Game1.tenbyFive14, (i + 1).ToString(), new Vector2(400 + 150 + 132 + 75 + 75, 166 + ((i + 1) * 32)) + new Vector2(4), textColor);
                    }
                    else
                    {
                        drawLine(sb, new Vector2(400, 166 + 32 + (i * 32)), 480, 0.0f, new Color(1, 1, 1, 0.25f), 2);
                        sb.DrawString(Game1.tenbyFive14, "<NO DATA>", new Vector2(400 + (448 / 2), 166 + ((i + 1) * 32)) + new Vector2(4) - new Vector2(Game1.tenbyFive14.MeasureString("<NO DATA>").X, 0) / 2, new Color(1, 1, 1, 0.25f));
                    }
                }
            }
            else
            {
                sb.DrawString(Game1.tenbyFive14, "Please sign in to view your personal high scores.", (new Vector2(GlobalGameConstants.GameResolutionWidth, GlobalGameConstants.GameResolutionHeight) - Game1.tenbyFive14.MeasureString("Please sign in to view your personal high scores.")) / 2, Color.White);
            }

            if (stateTimer > 1500f)
            {

#if XBOX
                sb.DrawString(Game1.tenbyFive24, "Press (A) to Continue", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 600) - Game1.tenbyFive24.MeasureString("Press (A) to Continue") / 2, new Color(1, 1, 1, (float)(Math.Sin(stateTimer / 300f) / 2) + 0.5f));
#elif WINDOWS
                sb.DrawString(Game1.tenbyFive24, "Press " + InputDevice2.KeyConfig.Confirm.ToString() + " to Continue", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 600) - Game1.tenbyFive24.MeasureString("Press " + InputDevice2.KeyConfig.Confirm.ToString() + " to Continue") / 2, new Color(1, 1, 1, (float)(Math.Sin(stateTimer / 300f) / 2) + 0.5f));
#endif
            }
            else if (stateTimer > 1300f)
            {
#if XBOX
                sb.DrawString(Game1.tenbyFive24, "Press (A) to Continue", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 600) - Game1.tenbyFive24.MeasureString("Press (A) to Continue") / 2, Color.Lerp(Color.Transparent, Color.White, (stateTimer - 1300)/200));
#elif WINDOWS
                sb.DrawString(Game1.tenbyFive24, "Press " + InputDevice2.KeyConfig.Confirm.ToString() + " to Continue", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 600) - Game1.tenbyFive24.MeasureString("Press " + InputDevice2.KeyConfig.Confirm.ToString() + " to Continue") / 2, Color.Lerp(Color.Transparent, Color.White, (stateTimer - 1300)/200));
#endif
            }

            sb.End();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            if (inGame && !gameComplete)
            {
                return ScreenStateType.GameSetupMenu;
            }
            else if (inGame && gameComplete)
            {
                return ScreenStateType.TitleScreen;
            }
            else
            {
                return ScreenStateType.OptionsMenu;
            }
        }
    }
}
