﻿#define SHOW_TITLE_SAFE

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

        private enum fadeState
        {
            fadeIn,
            fadeStay,
            fadeOut,
        }
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

        private float alpha = 0.0f;
        private fadeState fade_state;

        private float fade_duration = 0.0f;
        private const float max_fade_time = 1500.0f;

        private float Alpha
        {
            get
            {
                return (fade_state == fadeState.fadeIn) ? (1 - (fade_duration / max_fade_time)) : (fade_state == fadeState.fadeOut) ? (fade_duration / max_fade_time) : 0.0f;
            }
        }

        private bool player1RightPressed = false;
        private bool player1LeftPressed = false;
        private bool player1DownPressed = false;
        private bool player1UpPressed = false;
        private bool player1StartPressed = false;
        private bool player1CancelPressed = false;
        private bool player1RenamePressed = false;

        private bool player2RightPressed = false;
        private bool player2LeftPressed = false;
        private bool player2DownPressed = false;
        private bool player2UpPressed = false;
        private bool player2CancelPressed = false;
        private bool player2RenamePressed = false;

        private bool playerCancelPressed = false;

        private IAsyncResult keyboardResult = null;
        private IAsyncResult keyboard2Result = null;

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

            fade_state = fadeState.fadeIn;
            fade_duration = 0.0f;

            timePassed = 0.0f;
        }

        protected override void doUpdate(GameTime currentTime)
        {
            timePassed += currentTime.ElapsedGameTime.Milliseconds;

            if (fade_state == fadeState.fadeIn)
            {
                fade_duration += currentTime.ElapsedGameTime.Milliseconds;
                if (fade_duration > max_fade_time)
                {
                    fade_state = fadeState.fadeStay;
                    fade_duration = 0.0f;
                }
            }

            if (fade_state == fadeState.fadeOut)
            {
                fade_duration += currentTime.ElapsedGameTime.Milliseconds;
                if (fade_duration > max_fade_time)
                {
                    nextState = ScreenStateType.TitleScreen;
                    isComplete = true;
                }
            }

            lineOffset += (currentTime.ElapsedGameTime.Milliseconds * lineMoveSpeed);
            if (lineOffset > 16.0f) { lineOffset -= 16.0f; }

            // handle players leaving
            {
                if (slot1.InputDevice != InputDevice2.PlayerPad.NoPad)
                {
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.Cancel) && (keyboardResult == null && keyboard2Result == null))
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
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.Cancel) && (keyboardResult == null && keyboard2Result == null))
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
                        slot1 = new CampaignLoadout(pressed, randomNames[Game1.rand.Next() % randomNames.Length], 0);

                        InputDevice2.LockController(InputDevice2.PPG_Player.Player_1, pressed);
                    }
                    else if (slot2.InputDevice == InputDevice2.PlayerPad.NoPad && (keyboardResult == null && keyboard2Result == null))
                    {
                        if (pressed != slot1.InputDevice)
                        {
                            slot2 = new CampaignLoadout(pressed, randomNames[Game1.rand.Next() % randomNames.Length], 0);

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
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.RightDirection) && (keyboardResult == null && keyboard2Result == null))
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

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.LeftDirection) && (keyboardResult == null && keyboard2Result == null))
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

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.DownDirection) && (keyboardResult == null && keyboard2Result == null))
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

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.UpDirection) && (keyboardResult == null && keyboard2Result == null))
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

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.SwitchItem1) && (keyboardResult == null && keyboard2Result == null))
                    {
                        player1RenamePressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.SwitchItem1) && player1RenamePressed)
                    {
                        player1RenamePressed = false;

                        if (keyboardResult == null)
                        {
                            bool noIndex = false;
                            PlayerIndex p = PlayerIndex.One;

                            try
                            {
                                p = InputDevice2.GetPlayerGamePadIndex(InputDevice2.PPG_Player.Player_1);
                            }
                            catch (Exception)
                            {
                                slot1.Name = randomNames[Game1.rand.Next() % randomNames.Length];
                                noIndex = true;
                            }

                            if (!noIndex)
                            {
#if XBOX
                                keyboardResult = Guide.BeginShowKeyboardInput(p, "Rename " + slot1.Name, "Enter a new name to reassign to the current character.", slot1.Name, null, null);
# elif WINDOWS
                                keyboardResult = Guide.BeginShowKeyboardInput(PlayerIndex.One, "Rename " + slot1.Name, "Enter a new name to reassign to the current character.", slot1.Name, null, null);
#endif
                            }
                        }
                    }

                    if (keyboardResult != null && keyboardResult.IsCompleted)
                    {
                        string output = Guide.EndShowKeyboardInput(keyboardResult);
                        if (output != null && output.Length > 0)
                        {
                            slot1.Name = output;
                        }
                        keyboardResult = null;
                    }
                }

                if (slot2.InputDevice != InputDevice2.PlayerPad.NoPad)
                {
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.RightDirection) && (keyboardResult == null && keyboard2Result == null))
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

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.LeftDirection) && (keyboardResult == null && keyboard2Result == null))
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

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.DownDirection) && (keyboardResult == null && keyboard2Result == null))
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

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.UpDirection) && (keyboardResult == null && keyboard2Result == null))
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

                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.SwitchItem1) && (keyboardResult == null && keyboard2Result == null))
                    {
                        player2RenamePressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_2, InputDevice2.PlayerButton.SwitchItem1) && player2RenamePressed)
                    {
                        player2RenamePressed = false;

                        if (keyboardResult == null)
                        {
                            bool noIndex = false;
                            PlayerIndex p = PlayerIndex.Two;

                            try
                            {
                                p = InputDevice2.GetPlayerGamePadIndex(InputDevice2.PPG_Player.Player_2);
                            }
                            catch (Exception)
                            {
                                slot1.Name = randomNames[Game1.rand.Next() % randomNames.Length];
                                noIndex = true;
                            }

                            if (!noIndex)
                            {
#if XBOX
                                keyboard2Result = Guide.BeginShowKeyboardInput(p, "Rename " + slot2.Name, "Enter a new name to reassign to the current character.", slot2.Name, null, null);
# elif WINDOWS
                                keyboard2Result = Guide.BeginShowKeyboardInput(PlayerIndex.One, "Rename " + slot2.Name, "Enter a new name to reassign to the current character.", slot2.Name, null, null);
#endif
                            }
                        }
                    }

                    if (keyboard2Result != null && keyboard2Result.IsCompleted)
                    {
                        string output = Guide.EndShowKeyboardInput(keyboard2Result);
                        if (output != null && output.Length > 0)
                        {
                            slot2.Name = output;
                        }
                        keyboard2Result = null;
                    }
                }
            }

            // pressing cancel to quit
            {
                if (slot1.InputDevice == InputDevice2.PlayerPad.NoPad)
                {
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.Cancel) && (keyboardResult == null && keyboard2Result == null))
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
                    if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.PauseButton) && (keyboardResult == null && keyboard2Result == null))
                    {
                        player1StartPressed = true;
                    }
                    else if (!InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.PauseButton) && player1StartPressed)
                    {
                        player1StartPressed = false;

                        if (slot2.InputDevice == InputDevice2.PlayerPad.NoPad)
                        {
                            GameCampaign.ResetPlayerValues(slot1.Name, slot1.Color);
                            nextState = ScreenStateType.IntroCutScene;
                        }
                        else
                        {
                            GameCampaign.ResetPlayerValues(slot1.Name, slot1.Color, slot2.Name, slot2.Color);
                            nextState = ScreenStateType.IntroCutSceneCoop;
                        }

                        isComplete = true;
                    }
                }
            }

            // pressing B will go back to title screen
            {
                if (slot1.InputDevice == InputDevice2.PlayerPad.NoPad && slot2.InputDevice == InputDevice2.PlayerPad.NoPad)
                {
                    InputDevice2.PlayerPad pressed = InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.Cancel);
                    if (pressed != InputDevice2.PlayerPad.NoPad && (keyboardResult == null && keyboard2Result == null) && !playerCancelPressed)
                    {
                        playerCancelPressed = true;
                    }
                    else if (pressed == InputDevice2.PlayerPad.NoPad && playerCancelPressed)
                    {
                        playerCancelPressed = false;
                        fade_state = fadeState.fadeOut;
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

        public static Color getSquareColor(int colorNumber)
        {
            Color squareColor = Color.White;

            switch (colorNumber)
            {
                case 0:
                    squareColor = Color.Green;
                    break;
                case 1:
                    squareColor = Color.Red;
                    break;
                case 2:
                    squareColor = Color.Purple;
                    break;
                case 3:
                    squareColor = Color.Blue;
                    break;
                case 4:
                    squareColor = Color.Cyan;
                    break;
                case 5:
                    squareColor = Color.Brown;
                    break;
                default:
                    break;
            }

            return squareColor;
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
                sb.DrawString(Game1.tenbyFive14, addAPlayerMessage, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 550) - Game1.tenbyFive14.MeasureString(addAPlayerMessage) / 2, new Color(1, 1, 1, (float)Math.Abs(Math.Sin(timePassed / 600f))));
            }
            else
            {
                if (slot1.InputDevice != InputDevice2.PlayerPad.Keyboard)
                {
                    sb.DrawString(Game1.tenbyFive14, pressStartToPlayMessage, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 550) - Game1.tenbyFive14.MeasureString(pressStartToPlayMessage) / 2, Color.White);
                }
                else
                {
                    sb.DrawString(Game1.tenbyFive14, pressEnterToPlayMessage + InputDevice2.KeyConfig.PauseButton.ToString() + ".", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 550) - Game1.tenbyFive14.MeasureString(pressEnterToPlayMessage + InputDevice2.KeyConfig.PauseButton.ToString() + ".") / 2, Color.White);
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
                    Color squareColor = Color.White;
                    //

                    if (i == 3) { drawY += 80 + 8; }
                    drawBox(sb, new Rectangle((GlobalGameConstants.GameResolutionWidth / 2 - 306 + 16) + ((80 + 8) * (i % 3)), drawY, 80, 80), i == slot1.Color ? Color.LightBlue : new Color(173, 216, 230, 80), 2.0f);
                    sb.Draw(Game1.whitePixel, new Rectangle((GlobalGameConstants.GameResolutionWidth / 2 - 306 + 16) + ((80 + 8) * (i % 3)) + 6, drawY + 8, 66, 66), i == slot1.Color ? getSquareColor(i) : Color.Lerp(getSquareColor(i), Color.Transparent, 0.75f));
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
                    sb.Draw(Game1.whitePixel, new Rectangle((GlobalGameConstants.GameResolutionWidth / 2 + 16 + 16) + ((80 + 8) * (i % 3)) + 6, drawY + 8, 66, 66), i == slot2.Color ? getSquareColor(i) : Color.Lerp(getSquareColor(i), Color.Transparent, 0.75f));
                }
            }

#if XBOX
            AnimationLib.getFrameAnimationSet("gamepadX").drawAnimationFrame(0, sb, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 442) - (Game1.tenbyFive14.MeasureString("Rename Character") * new Vector2(0.5f, 0)) - new Vector2(38, 6), new Vector2(0.75f), 0);
            sb.DrawString(Game1.tenbyFive14, "Rename Character", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 442) - (Game1.tenbyFive14.MeasureString("Rename Character") * new Vector2(0.5f, 0)), Color.White);
#endif
            //fade square
            sb.Draw(Game1.whitePixel, Vector2.Zero, new Rectangle(0, 0, GlobalGameConstants.GameResolutionWidth, GlobalGameConstants.GameResolutionHeight), new Color(0.0f, 0.0f, 0.0f, Alpha), 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.6f);
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
