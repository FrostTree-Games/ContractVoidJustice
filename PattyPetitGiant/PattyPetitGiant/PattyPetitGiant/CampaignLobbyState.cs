#define SHOW_TITLE_SAFE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Spine;

namespace PattyPetitGiant
{
    class CampaignLobbyState : ScreenState
    {
        private struct CampaignLoadout
        {
            public InputDevice2.PlayerPad InputDevice;
            public string Name;
            public int Color;

            public const int NumberOfColors = 6;

            public CampaignLoadout(InputDevice2.PlayerPad InputDevice, string Name, int Color)
            {
                this.InputDevice = InputDevice;
                this.Name = Name;
                this.Color = Color % NumberOfColors;
            }
        }

        private CampaignLoadout slot1;
        private CampaignLoadout slot2;

        private ScreenState.ScreenStateType nextState;

        private bool player1RightPressed = false;
        private bool player1LeftPressed = false;
        private bool player1DownPressed = false;
        private bool player1UpPressed = false;
        private bool player1StartPressed = false;
        private bool player1CancelPressed = false;

        private bool player2RightPressed = false;
        private bool player2LeftPressed = false;
        private bool player2DownPressed = false;
        private bool player2UpPressed = false;
        private bool player2CancelPressed = false;

        public static float lineOffset;
        public const float lineMoveSpeed = 0.01f;

        private Texture2D controllerIndexArt = null;

        private float timePassed;

        private string addAPlayerMessage = "One or two players may join the game.";
        private string pressStartToPlayMessage = "Press Player 1 start when all players are ready.";
        private string pressEnterToPlayMessage = "When all players are ready press ";
        private string joinMessage =
#if WINDOWS
            "Press Confirm to Join";
#elif XBOX
        "Press (A) to Join";
#endif

        public CampaignLobbyState()
        {
            slot1.InputDevice = InputDevice2.PlayerPad.NoPad;
            slot2.InputDevice = InputDevice2.PlayerPad.NoPad;
            InputDevice2.UnlockAllControllers();

            controllerIndexArt = TextureLib.getLoadedTexture("controllerIndexArt.png");

            nextState = ScreenStateType.TitleScreen;

            lineOffset = 0;

            timePassed = 0.0f;
        }

        protected override void doUpdate(GameTime currentTime)
        {
            timePassed += currentTime.ElapsedGameTime.Milliseconds;

            lineOffset += (currentTime.ElapsedGameTime.Milliseconds * lineMoveSpeed);
            if (lineOffset > 16.0f) { lineOffset -= 16.0f; }

            // handle players leaving
            {
                if (slot1.InputDevice != InputDevice2.PlayerPad.NoPad)
                {
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.Cancel))
                    {
                        player1CancelPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.Cancel) && player1CancelPressed)
                    {
                        player1CancelPressed = false;

                        InputDevice2.UnlockController(InputDevice2.PPG_Player.Player_1);
                        slot1.InputDevice = InputDevice2.PlayerPad.NoPad;
                    }
                }

                if (slot2.InputDevice != InputDevice2.PlayerPad.NoPad)
                {
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.Cancel))
                    {
                        player2CancelPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.Cancel) && player2CancelPressed)
                    {
                        player2CancelPressed = false;

                        InputDevice2.UnlockController(InputDevice2.PPG_Player.Player_2);
                        slot2.InputDevice = InputDevice2.PlayerPad.NoPad;
                    }
                }
            }

            // handle input devices joining for players
            {
                InputDevice2.PlayerPad pressed = InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.Confirm);
                if (pressed != InputDevice2.PlayerPad.NoPad)
                {
                    if (slot1.InputDevice == InputDevice2.PlayerPad.NoPad)
                    {
                        slot1 = new CampaignLoadout(pressed, randomNames[Game1.rand.Next() % randomNames.Length], Game1.rand.Next() % CampaignLoadout.NumberOfColors);

                        InputDevice2.LockController(InputDevice2.PPG_Player.Player_1, pressed);
                    }
                    else if (slot2.InputDevice == InputDevice2.PlayerPad.NoPad)
                    {
                        if (pressed != slot1.InputDevice)
                        {
                            slot2 = new CampaignLoadout(pressed, randomNames[Game1.rand.Next() % randomNames.Length], Game1.rand.Next() % CampaignLoadout.NumberOfColors);

                            InputDevice2.LockController(InputDevice2.PPG_Player.Player_2, pressed);
                        }
                    }
                }

                //shuffle the second player to the player 1 slot if player 1 leaves
                if (slot1.InputDevice == InputDevice2.PlayerPad.NoPad && slot2.InputDevice != InputDevice2.PlayerPad.NoPad)
                {
                    InputDevice2.LockController(InputDevice2.PPG_Player.Player_1, slot2.InputDevice);
                    InputDevice2.UnlockController(InputDevice2.PPG_Player.Player_2);

                    slot1 = slot2;
                    slot2.InputDevice = InputDevice2.PlayerPad.NoPad;
                }
            }

            // adjust player values
            {
                if (slot1.InputDevice != InputDevice2.PlayerPad.NoPad)
                {
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.RightDirection))
                    {
                        player1RightPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.RightDirection) && player1RightPressed)
                    {
                        player1RightPressed = false;

                        slot1.Color = ((slot1.Color + 1) % CampaignLoadout.NumberOfColors);

                        if (slot1.Color == 3) { slot1.Color--; }
                        if (slot1.Color == 0) { slot1.Color = 5; }
                    }

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.LeftDirection))
                    {
                        player1LeftPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.LeftDirection) && player1LeftPressed)
                    {
                        player1LeftPressed = false;

                        slot1.Color -= 1;

                        if (slot1.Color < 0) { slot1.Color = 0; }
                        if (slot1.Color == 2) { slot1.Color = 3; }
                    }

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.DownDirection))
                    {
                        player1DownPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.DownDirection) && player1DownPressed)
                    {
                        player1DownPressed = false;

                        if (slot1.Color < 3)
                        {
                            slot1.Color += 3;
                        }
                    }

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.UpDirection))
                    {
                        player1UpPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.UpDirection) && player1UpPressed)
                    {
                        player1UpPressed = false;

                        if (slot1.Color > 2)
                        {
                            slot1.Color -= 3;
                        }
                    }
                }

                if (slot2.InputDevice != InputDevice2.PlayerPad.NoPad)
                {
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.RightDirection))
                    {
                        player2RightPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.RightDirection) && player2RightPressed)
                    {
                        player2RightPressed = false;

                        slot2.Color = ((slot2.Color + 1) % CampaignLoadout.NumberOfColors);

                        if (slot2.Color == 3) { slot2.Color--; }
                        if (slot2.Color == 0) { slot2.Color = 5; }
                    }

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.LeftDirection))
                    {
                        player2LeftPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.LeftDirection) && player2LeftPressed)
                    {
                        player2LeftPressed = false;

                        slot2.Color -= 1;

                        if (slot2.Color < 0) { slot2.Color = 0; }
                        if (slot2.Color == 2) { slot2.Color = 3; }
                    }

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.DownDirection))
                    {
                        player2DownPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.DownDirection) && player2DownPressed)
                    {
                        player2DownPressed = false;

                        if (slot2.Color < 3)
                        {
                            slot2.Color += 3;
                        }
                    }

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.UpDirection))
                    {
                        player2UpPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.UpDirection) && player2UpPressed)
                    {
                        player2UpPressed = false;

                        if (slot2.Color > 2)
                        {
                            slot2.Color -= 3;
                        }
                    }
                }
            }

            // pressing cancel to quit
            {
                if (slot1.InputDevice == InputDevice2.PlayerPad.NoPad)
                {
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.Cancel))
                    {
                        player1CancelPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.Cancel) && player1CancelPressed)
                    {
                        player1CancelPressed = false;

                        nextState = ScreenStateType.TitleScreen;

                        isComplete = true;
                    }
                }
            }

            // pressing start to continue
            {
                if (slot1.InputDevice != InputDevice2.PlayerPad.NoPad)
                {
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.PauseButton))
                    {
                        player1StartPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.PauseButton) && player1StartPressed)
                    {
                        player1StartPressed = false;

                        nextState = ScreenStateType.LevelSelectState;

                        if (slot2.InputDevice == InputDevice2.PlayerPad.NoPad)
                        {
                            GameCampaign.ResetPlayerValues(slot1.Name, slot1.Color);
                        }
                        else
                        {
                            GameCampaign.ResetPlayerValues(slot1.Name, slot1.Color, slot2.Name, slot2.Color);
                        }

                        isComplete = true;
                    }
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
            AnimationLib.GraphicsDevice.Clear(Color.Black);

            Vector2 linesOffset = new Vector2(lineOffset);
            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

#if SHOW_TITLE_SAFE

            for (int i = -6; i < GlobalGameConstants.GameResolutionWidth / 16 + 8; i ++)
            {
                drawLine(sb, new Vector2(i * 16, -16) + linesOffset, GlobalGameConstants.GameResolutionHeight + 32, (float)Math.PI / 2, new Color(1, 0, 1, 0.1f), 1.0f);
            }

            for (int i = -6; i < GlobalGameConstants.GameResolutionHeight / 16 + 8; i++)
            {
                drawLine(sb, new Vector2(-16, i * 16) + linesOffset, GlobalGameConstants.GameResolutionWidth + 32, 0, new Color(1, 0, 1, 0.1f), 1.0f);
            }

            sb.Draw(Game1.whitePixel, XboxTools.GetTitleSafeArea(AnimationLib.GraphicsDevice, 0.8f), new Color(0.0f, 0.75f, 1.0f, 0.1f));
#endif

            if (slot1.InputDevice == InputDevice2.PlayerPad.NoPad)
            {
                sb.DrawString(Game1.tenbyFive14, addAPlayerMessage, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 500) - Game1.tenbyFive14.MeasureString(addAPlayerMessage) / 2, new Color(1, 1, 1, (float)Math.Abs(Math.Sin(timePassed / 600f))));
            }
            else
            {
                if (slot1.InputDevice != InputDevice2.PlayerPad.Keyboard)
                {
                    sb.DrawString(Game1.tenbyFive14, pressStartToPlayMessage, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 500) - Game1.tenbyFive14.MeasureString(pressStartToPlayMessage) / 2, Color.White);
                }
                else
                {
                    sb.DrawString(Game1.tenbyFive14, pressEnterToPlayMessage + InputDevice2.KeyConfig.PauseButton.ToString() + ".", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 500) - Game1.tenbyFive14.MeasureString(pressEnterToPlayMessage + InputDevice2.KeyConfig.PauseButton.ToString() + ".") / 2, Color.White);
                }
            }

            drawBox(sb, new Rectangle(GlobalGameConstants.GameResolutionWidth / 2 - 306, 96, 288, 256), slot1.InputDevice != InputDevice2.PlayerPad.NoPad ? Color.LightBlue : new Color(173, 216, 230, 80), 2.0f);
            sb.DrawString(Game1.tenbyFive14, "Player 1", new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 306, 352), slot1.InputDevice != InputDevice2.PlayerPad.NoPad ? Color.LightBlue : new Color(173, 216, 230, 80));
            drawBox(sb, new Rectangle(GlobalGameConstants.GameResolutionWidth / 2 + 16, 96, 288, 256), slot2.InputDevice != InputDevice2.PlayerPad.NoPad ? Color.LightBlue : new Color(173, 216, 230, 80), 2.0f);
            sb.DrawString(Game1.tenbyFive14, "Player 2", new Vector2(GlobalGameConstants.GameResolutionWidth / 2 + 16, 352), slot2.InputDevice != InputDevice2.PlayerPad.NoPad ? Color.LightBlue : new Color(173, 216, 230, 80));

            if (slot1.InputDevice == InputDevice2.PlayerPad.NoPad)
            {
                sb.DrawString(Game1.tenbyFive14, joinMessage, new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 306, 96) + new Vector2(288 / 2, 256 / 2) - (Game1.tenbyFive14.MeasureString(joinMessage) / 2.0f), Color.LightBlue);
            }
            else
            {
                sb.DrawString(Game1.tenbyFive14, "Name: " + slot1.Name, new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 306, 96) + new Vector2(16), Color.LightBlue);

                sb.Draw(controllerIndexArt, new Rectangle(GlobalGameConstants.GameResolutionWidth / 2 - 306 + 288 - 40, 104, 32, 32), new Rectangle(128 * (int)(slot1.InputDevice), 0, 128, 128), Color.LightBlue);

                int drawY = 96 + 64;
                for (int i = 0; i < CampaignLoadout.NumberOfColors; i++)
                {
                    if (i == 3) { drawY += 80 + 8; }
                    drawBox(sb, new Rectangle((GlobalGameConstants.GameResolutionWidth / 2 - 306 + 16) + ((80 + 8) * (i % 3)), drawY, 80, 80), i == slot1.Color ? Color.LightBlue : new Color(173, 216, 230, 80), 2.0f);
                }
            }

            if (slot2.InputDevice == InputDevice2.PlayerPad.NoPad)
            {
                sb.DrawString(Game1.tenbyFive14, joinMessage, new Vector2(GlobalGameConstants.GameResolutionWidth / 2 + 16, 96) + new Vector2(288 / 2, 256 / 2) - (Game1.tenbyFive14.MeasureString(joinMessage) / 2.0f), Color.LightBlue);
            }
            else
            {
                sb.DrawString(Game1.tenbyFive14, "Name: " + slot2.Name, new Vector2(GlobalGameConstants.GameResolutionWidth / 2 + 16, 96) + new Vector2(16), Color.LightBlue);

                sb.Draw(controllerIndexArt, new Rectangle(GlobalGameConstants.GameResolutionWidth / 2 + 16 + 288 - 40, 104, 32, 32), new Rectangle(128 * (int)(slot2.InputDevice), 0, 128, 128), Color.LightBlue);

                int drawY = 96 + 64;
                for (int i = 0; i < CampaignLoadout.NumberOfColors; i++)
                {
                    if (i == 3) { drawY += 80 + 8; }
                    drawBox(sb, new Rectangle((GlobalGameConstants.GameResolutionWidth / 2 + 16 + 16) + ((80 + 8) * (i % 3)), drawY, 80, 80), i == slot2.Color ? Color.LightBlue : new Color(173, 216, 230, 80), 2.0f);
                }
            }

            sb.End();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            return nextState;
        }

        public static string[] randomNames = {"Isiah",
                                                "Eric",
                                                "Wilson",
                                                "Daniel",
                                                "Marget",
                                                "Rueben",
                                                "Lovella",
                                                "Felisha",
                                                "Kandace",
                                                "Wopley",
                                                "Zippy",
                                                "Fish",
                                                "Belmont",
                                                "Jensen",
                                                "Louise",
                                                "Merrilee",
                                                "Faustina",
                                                "Kenda",
                                                "Jamison",
                                                "Monserrate",
                                                "Jacquelyn",
                                                "Nicky",
                                                "Luigi",
                                                "Alyosha",
                                                "Denton",
                                                "Jensen",
                                                "Bethanie",
                                                "Kecia",
                                                "Darline",
                                                "Roselle",
                                                "Hiram",
                                                "Marin",
                                                "John",
                                                "Chief",
                                                "Agent",
                                                "Commando",
                                                "Patrick",
                                                "Petit",
                                                "Giant",
                                                "Quixote",
                                                "Security",
                                                "Mitt",
                                                "Barack",
                                                "Gregory",
                                                "Karin",
                                                "Robo",
                                                "Diskun",
                                                "Takamaru",
                                                "Gary",
                                                "Red",
                                                "Levin",
                                                "Velda",
                                                "Roselia",
                                                "Lizbeth",
                                                "Roselyn",
                                                "Mamie",
                                                "Isabella",
                                                "Willa",
                                                "Berry",
                                                "Lindsay",
                                                "Kimberley",
                                                "Galen",
                                                "Shea",
                                                "Bennett",
                                                "Adelaida",
                                                "Toya",
                                                "Devon",
                                                "Maye",
                                                "Georgette",
                                                "Zofia",
                                                "Enrique",
                                                "Germaine",
                                                "Seth",
                                                "Gabrielle",
                                                "Sari",
                                                "Joline",
                                                "Carolynn",
                                                "Tova",
                                                "Deborah",
                                                "Dacia",
                                                "Erin",
                                                "Commander",
                                                "Shepard",
                                                "Daphne",
                                                "Lona"};
    }
}
