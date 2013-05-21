using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
namespace PattyPetitGiant
{
    class InputDeviceManager
    {
        public enum PlayerButton
        {
            UpDirection,
            DownDirection,
            LeftDirection,
            RightDirection,
            Confirm,
            Cancel,
            UseItem1,
            UseItem2,
            SwitchItem1,
            SwitchItem2,
            AnyButton
        }

        private static Vector2[] analog_directions = null;

        private static GamePadState[] game_pad_state = null;
        private static int[] game_pad_previous_tick = null;
        private static KeyboardState key_board_state;
        
        private static int current_game_pad = 0;
        private static bool game_controllers_locked = true;
        private static bool button_pushed = false;

        public InputDeviceManager(GraphicsDevice device)
        {
            if (game_pad_state == null)
            {
                game_pad_state = new GamePadState[4];
                game_pad_previous_tick = new int[4];
                analog_directions = new Vector2[4];
            }
        }

        private static bool isButtonDownConfirm(PlayerButton button, int control_number)
        {
            switch (button)
            {
                case PlayerButton.UpDirection:
                    if (game_pad_state[control_number].DPad.Up == ButtonState.Pressed || game_pad_state[control_number].ThumbSticks.Left.Y > 0.0 || key_board_state.IsKeyDown(Keys.Up))
                    {
                        return true;
                    }
                    break;
                case PlayerButton.DownDirection:
                    if (game_pad_state[control_number].DPad.Down == ButtonState.Pressed || game_pad_state[control_number].ThumbSticks.Left.Y < 0.0 || key_board_state.IsKeyDown(Keys.Down))
                    {
                        return true;
                    }
                    break;
                case PlayerButton.LeftDirection:
                    if (game_pad_state[control_number].DPad.Left == ButtonState.Pressed || game_pad_state[control_number].ThumbSticks.Left.X < 0.0 || key_board_state.IsKeyDown(Keys.Left))
                    {
                        return true;
                    }
                    break;
                case PlayerButton.RightDirection:
                    if (game_pad_state[control_number].DPad.Right == ButtonState.Pressed || game_pad_state[control_number].ThumbSticks.Left.X > 0.0 || key_board_state.IsKeyDown(Keys.Right))
                    {
                        return true;
                    }
                    break;
                case PlayerButton.Confirm:
                    if (game_pad_state[control_number].Buttons.A == ButtonState.Pressed || key_board_state.IsKeyDown(Keys.Enter))
                    {
                        return true;
                    }
                    break;
                case PlayerButton.Cancel:
                    if (game_pad_state[control_number].Buttons.B == ButtonState.Pressed || key_board_state.IsKeyDown(Keys.Escape) || key_board_state.IsKeyDown(Keys.Back))
                    {
                        return true;
                    }
                    break;
                case PlayerButton.UseItem1:
                    if(game_pad_state[control_number].Triggers.Left > 0.0 || key_board_state.IsKeyDown(Keys.A))
                    {
                        return true;
                    }
                    break;
                case PlayerButton.UseItem2:
                    if (game_pad_state[control_number].Triggers.Right > 0.0 || key_board_state.IsKeyDown(Keys.S))
                    {
                        return true;
                    }
                    break;
                case PlayerButton.SwitchItem1:
                    if (game_pad_state[control_number].Buttons.LeftShoulder == ButtonState.Pressed || key_board_state.IsKeyDown(Keys.Q))
                    {
                        return true;
                    }
                    break;
                case PlayerButton.SwitchItem2:
                    if (game_pad_state[control_number].Buttons.RightShoulder == ButtonState.Pressed || key_board_state.IsKeyDown(Keys.W))
                    {
                        return true;
                    }
                    break;
                case PlayerButton.AnyButton:
                    if (game_pad_state[control_number].Buttons.A == ButtonState.Pressed || game_pad_state[control_number].Buttons.A == ButtonState.Pressed || game_pad_state[control_number].Buttons.B == ButtonState.Pressed || game_pad_state[control_number].Buttons.Y == ButtonState.Pressed || game_pad_state[control_number].Buttons.X == ButtonState.Pressed || game_pad_state[control_number].Buttons.Back == ButtonState.Pressed || game_pad_state[control_number].Buttons.Start == ButtonState.Pressed || game_pad_state[control_number].Buttons.RightShoulder == ButtonState.Pressed || game_pad_state[control_number].Buttons.LeftShoulder == ButtonState.Pressed || game_pad_state[control_number].Buttons.RightStick == ButtonState.Pressed || game_pad_state[control_number].Buttons.LeftStick == ButtonState.Pressed || key_board_state.IsKeyDown(Keys.Enter))
                    {
                        return true;
                    }
                    break;
                default:
                    return false;
            }
            return false;
        }

        public static bool isButtonDown(PlayerButton button)
        {
            if (game_controllers_locked)
            {
                return isButtonDownConfirm(button, current_game_pad);
            }
            else
            {
                for (int i = 0; i < game_pad_state.Length; i++)
                {
                    button_pushed = isButtonDownConfirm(button, i);
                    if (button_pushed)
                    {
                        Console.WriteLine("Button was pushed: " + button_pushed);
                        return true;
                    }
                }
                return false;
            }
        }

        public void update()
        {
            game_pad_state[0] = GamePad.GetState(PlayerIndex.One);
            game_pad_state[1] = GamePad.GetState(PlayerIndex.Two);
            game_pad_state[2] = GamePad.GetState(PlayerIndex.Three);
            game_pad_state[3] = GamePad.GetState(PlayerIndex.Four);

            //if the game has not started, check each controller to be sure that their states have not changed and assign the one that has changed states to be the current game pad to update
            for (int i = 0; i < game_pad_state.Length; i++)
            {
                if (game_pad_state[i].IsConnected && !game_controllers_locked)
                {
                    if (game_pad_previous_tick[i] != game_pad_state[i].PacketNumber)
                    {
                        current_game_pad = i;
                    }
                    game_pad_previous_tick[i] = game_pad_state[i].PacketNumber;
                }
            }
            key_board_state = Keyboard.GetState();
        }
    }
}
