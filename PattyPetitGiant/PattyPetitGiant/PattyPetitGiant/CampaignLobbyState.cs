#define SHOW_TITLE_SAFE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

namespace PattyPetitGiant
{
    class CampaignLobbyState : ScreenState
    {
        private struct CampaignLoadout
        {
            public InputDevice2.PlayerPad InputDevice;
            public string Name;
            public float Hue;

            public CampaignLoadout(InputDevice2.PlayerPad InputDevice, string Name, float Hue)
            {
                this.InputDevice = InputDevice;
                this.Name = Name;
                this.Hue = Hue % 1.0f;
            }
        }

        private CampaignLoadout slot1;
        private CampaignLoadout slot2;

        private ScreenState.ScreenStateType nextState;

        private bool player1CancelPressed = false;
        private bool player2CancelPressed = false;

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

            nextState = ScreenStateType.TitleScreen; 
        }

        protected override void doUpdate(GameTime currentTime)
        {
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
                        slot1 = new CampaignLoadout(pressed, randomNames[Game1.rand.Next() % randomNames.Length], 0.5f);

                        InputDevice2.LockController(InputDevice2.PPG_Player.Player_1, pressed);
                    }
                    else if (slot2.InputDevice == InputDevice2.PlayerPad.NoPad)
                    {
                        if (pressed != slot1.InputDevice)
                        {
                            slot2 = new CampaignLoadout(pressed, randomNames[Game1.rand.Next() % randomNames.Length], 0.5f);

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

            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

#if SHOW_TITLE_SAFE
            sb.Draw(Game1.whitePixel, XboxTools.GetTitleSafeArea(AnimationLib.GraphicsDevice, 0.8f), new Color(0.0f, 0.75f, 1.0f, 0.3f));

            for (int i = 0; i < GlobalGameConstants.GameResolutionWidth; i += 16)
            {
                drawLine(sb, new Vector2(i, 0), GlobalGameConstants.GameResolutionHeight, (float)Math.PI / 2, new Color(1, 0, 1, 0.25f), 1.0f);
            }

            for (int i = 0; i < GlobalGameConstants.GameResolutionHeight; i += 16)
            {
                drawLine(sb, new Vector2(0, i), GlobalGameConstants.GameResolutionWidth, 0, new Color(1, 0, 1, 0.25f), 1.0f);
            }
#endif

            sb.DrawString(Game1.tenbyFive14, "Foo no way that's cool man.", new Vector2(500), Color.White);

            drawBox(sb, new Rectangle(GlobalGameConstants.GameResolutionWidth / 2 - 306, 96, 288, 192), Color.LightBlue, 2.0f);
            sb.DrawString(Game1.tenbyFive14, "Player 1", new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 306, 288), Color.LightBlue);
            drawBox(sb, new Rectangle(GlobalGameConstants.GameResolutionWidth / 2 + 16, 96, 288, 192), Color.LightBlue, 2.0f);
            sb.DrawString(Game1.tenbyFive14, "Player 2", new Vector2(GlobalGameConstants.GameResolutionWidth / 2 + 16, 288), Color.LightBlue);

            if (slot1.InputDevice == InputDevice2.PlayerPad.NoPad)
            {
                sb.DrawString(Game1.tenbyFive14, joinMessage, new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 306, 96) + new Vector2(288 / 2, 192 / 2) - (Game1.tenbyFive14.MeasureString(joinMessage) / 2.0f), Color.LightBlue);
            }
            else
            {
                sb.DrawString(Game1.tenbyFive14, "Name: " + slot1.Name, new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 306, 96) + new Vector2(16), Color.LightBlue);
            }

            if (slot2.InputDevice == InputDevice2.PlayerPad.NoPad)
            {
                sb.DrawString(Game1.tenbyFive14, joinMessage, new Vector2(GlobalGameConstants.GameResolutionWidth / 2 + 16, 96) + new Vector2(288 / 2, 192 / 2) - (Game1.tenbyFive14.MeasureString(joinMessage) / 2.0f), Color.LightBlue);
            }
            else
            {
                sb.DrawString(Game1.tenbyFive14, "Name: " + slot2.Name, new Vector2(GlobalGameConstants.GameResolutionWidth / 2 + 16, 96) + new Vector2(16), Color.LightBlue);
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
