using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    /// <summary>
    /// An alternative input device manager for PattyPetitGiant. Should allow for state management with multiple controller indexes effortlessly.
    /// </summary>
    class InputDevice2
    {
        /// <summary>
        /// Identifier for different ingame "player slots". Does not necessarily coorelate to specific gamepads.
        /// </summary>
        public enum PPG_Player
        {
            /// <summary>
            /// First ingame player.
            /// </summary>
            Player_1 = 0,
            /// <summary>
            /// Second ingame player.
            /// </summary>
            Player_2 = 1,
            /// <summary>
            /// Third ingame player.
            /// </summary>
            Player_3 = 2,
            /// <summary>
            /// Fourth ingame player.
            /// </summary>
            Player_4 = 3,
        }

        /// <summary>
        /// Used to identify which gamepads are in use.
        /// </summary>
        public enum PlayerPad
        {
            /// <summary>
            /// No gamepad specified. This may throw exceptions if passed into InputDevice2.
            /// </summary>
            NoPad = -1,
            /// <summary>
            /// Gamepad at index one.
            /// </summary>
            GamePad1 = 0,
            /// <summary>
            /// Gamepad at index two.
            /// </summary>
            GamePad2 = 1,
            /// <summary>
            /// Gamepad at index three.
            /// </summary>
            GamePad3 = 2,
            /// <summary>
            /// Gamepad at index four.
            /// </summary>
            GamePad4 = 3,
            /// <summary>
            /// Represents the computer keyboard, or a USB keyboard plugged into the Xbox 360.
            /// </summary>
            Keyboard = 4,
        }

        /// <summary>
        /// Represents various buttons pressed on a gamepad.
        /// </summary>
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
            AnyButton,
            PauseButton,
            BackButton,
        }

        private static GamePadState[] xInputControllers = null;
        private static KeyboardState keyboardController = new KeyboardState();

        private static KeyboardControllerConfiguration keyConfig = null;
        /// <summary>
        /// The current key configuration for the Keyboard.
        /// </summary>
        public static KeyboardControllerConfiguration KeyConfig { get { return keyConfig; } set { keyConfig = value; } }

        private static PlayerPad[] bindings = null;

        /// <summary>
        /// Initalizes input device variables.
        /// </summary>
        public static void Initalize()
        {
            if (xInputControllers == null)
            {
                xInputControllers = new GamePadState[4];
            }

            if (keyConfig == null)
            {
                keyConfig = KeyboardControllerConfiguration.DefaultKeyConfig();
            }

            if (bindings == null)
            {
                bindings = new PlayerPad[4];
                for (int i = 0; i < 4; i++) { bindings[i] = PlayerPad.NoPad; }
            }
        }

        private static PlayerPad GamePadIndexToPlayerPad(PlayerIndex index)
        {
            return (PlayerPad)((int)index);
        }

        /// <summary>
        /// Polls the hardware for any input press. Useful for getting the player's controller index.
        /// </summary>
        /// <param name="button">The button you would like to poll.</param>
        /// <returns>The PlayerPad corresponding with a connected input device that pressed the specified button. Will return PlayerPad.NoPad if no devices are pressing the specified button.</returns>
        /// <remarks>If two input devices are pressing the same button simeltaneously, this method will return the device with the higher index. The keyboard's index is considered to be in between 1 and 2.</remarks>
        public static PlayerPad IsAnyControllerButtonDown(PlayerButton button)
        {
            for (int i = 0; i < 4; i++)
            {
                GamePadState state = xInputControllers[i];

                switch (button)
                {
                    case PlayerButton.Confirm:
                        if (xInputControllers[i].Buttons.A == ButtonState.Pressed) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.Confirm)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.Cancel:
                        if (xInputControllers[i].Buttons.B == ButtonState.Pressed) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.Cancel)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.PauseButton:
                        if (xInputControllers[i].Buttons.Start == ButtonState.Pressed) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.PauseButton)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.BackButton:
                        if (xInputControllers[i].Buttons.Back == ButtonState.Pressed) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.BackButton)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.UseItem2:
                        if (xInputControllers[i].Buttons.A == ButtonState.Pressed || xInputControllers[i].Triggers.Left > 0.01f) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.UseLeftItem)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.UseItem1:
                        if (xInputControllers[i].Buttons.B == ButtonState.Pressed || xInputControllers[i].Triggers.Right > 0.01f) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.UseRightItem)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.SwitchItem1:
                        if (xInputControllers[i].Buttons.X == ButtonState.Pressed || xInputControllers[i].Buttons.RightShoulder == ButtonState.Pressed) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.SwitchItem1)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.SwitchItem2:
                        if (xInputControllers[i].Buttons.Y == ButtonState.Pressed || xInputControllers[i].Buttons.LeftShoulder == ButtonState.Pressed) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.SwitchItem2)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.DownDirection:
                        if (xInputControllers[i].DPad.Down == ButtonState.Pressed || xInputControllers[i].ThumbSticks.Left.Y < 0) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.DownDirection)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.UpDirection:
                        if (xInputControllers[i].DPad.Up == ButtonState.Pressed || xInputControllers[i].ThumbSticks.Left.Y > 0) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.UpDirection)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.LeftDirection:
                        if (xInputControllers[i].DPad.Left == ButtonState.Pressed || xInputControllers[i].ThumbSticks.Left.X < 0) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.LeftDirection)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.RightDirection:
                        if (xInputControllers[i].DPad.Right == ButtonState.Pressed || xInputControllers[i].ThumbSticks.Left.X > 0) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.RightDirection)) { return PlayerPad.Keyboard; }
                        break;
                }
            }

            return PlayerPad.NoPad;
        }

        /// <summary>
        /// Assigns a specified player to a certain pad.
        /// </summary>
        /// <param name="player">The player to assign to a device.</param>
        /// <param name="pad">The input device to be assigned a player.</param>
        /// <remarks>Allows the programmer to check input on a device without knowing what it's type/index is.</remarks>
        public static void LockController(PPG_Player player, PlayerPad pad)
        {
            bindings[(int)player] = pad;
        }

        /// <summary>
        /// If a player is assigned to a certain input device, its assignment is removed. Otherwise does nothing.
        /// </summary>
        /// <param name="player">The player to unassign.</param>
        public static void UnlockController(PPG_Player player)
        {
            bindings[(int)player] = PlayerPad.NoPad;
        }

        /// <summary>
        /// Removes assignment for all players tied to input devices.
        /// </summary>
        /// <remarks>This would be good to use for restarting the game's state.</remarks>
        public static void UnlockAllControllers()
        {
            for (int i = 0; i < 4; i++)
            {
                bindings[i] = PlayerPad.NoPad;
            }
        }

        /// <summary>
        /// Checks if a button is pressed for a specified Player.
        /// </summary>
        /// <param name="player">The assigned player index to check.</param>
        /// <param name="button">The button to poll.</param>
        /// <returns>True if the button for the player's device is pressed.</returns>
        public static bool IsPlayerButtonDown(PPG_Player player, PlayerButton button)
        {
            if (bindings[(int)player] == PlayerPad.Keyboard)
            {
                switch (button)
                {
                    case PlayerButton.Confirm:
                        if (keyboardController.IsKeyDown(keyConfig.Confirm)) { return true; }
                        break;
                    case PlayerButton.Cancel:
                        if (keyboardController.IsKeyDown(keyConfig.Cancel)) { return true; }
                        break;
                    case PlayerButton.PauseButton:
                        if (keyboardController.IsKeyDown(keyConfig.PauseButton)) { return true; }
                        break;
                    case PlayerButton.BackButton:
                        if (keyboardController.IsKeyDown(keyConfig.BackButton)) { return true; }
                        break;
                    case PlayerButton.UseItem2:
                        if (keyboardController.IsKeyDown(keyConfig.UseLeftItem)) { return true; }
                        break;
                    case PlayerButton.UseItem1:
                        if (keyboardController.IsKeyDown(keyConfig.UseRightItem)) { return true; }
                        break;
                    case PlayerButton.SwitchItem1:
                        if (keyboardController.IsKeyDown(keyConfig.SwitchItem1)) { return true; }
                        break;
                    case PlayerButton.SwitchItem2:
                        if (keyboardController.IsKeyDown(keyConfig.SwitchItem2)) { return true; }
                        break;
                    case PlayerButton.DownDirection:
                        if (keyboardController.IsKeyDown(keyConfig.DownDirection)) { return true; }
                        break;
                    case PlayerButton.UpDirection:
                        if (keyboardController.IsKeyDown(keyConfig.UpDirection)) { return true; }
                        break;
                    case PlayerButton.LeftDirection:
                        if (keyboardController.IsKeyDown(keyConfig.LeftDirection)) { return true; }
                        break;
                    case PlayerButton.RightDirection:
                        if (keyboardController.IsKeyDown(keyConfig.RightDirection)) { return true; }
                        break;
                }
            }
            else if ((int)bindings[(int)player] > -1)
            {
                GamePadState state = xInputControllers[(int)bindings[(int)player]];

                switch (button)
                {
                    case PlayerButton.Confirm:
                        if (state.Buttons.A == ButtonState.Pressed) { return true; }
                        break;
                    case PlayerButton.Cancel:
                        if (state.Buttons.B == ButtonState.Pressed) { return true; }
                        break;
                    case PlayerButton.PauseButton:
                        if (state.Buttons.Start == ButtonState.Pressed) { return true; }
                        break;
                    case PlayerButton.BackButton:
                        if (state.Buttons.Back == ButtonState.Pressed) { return true; }
                        break;
                    case PlayerButton.UseItem2:
                        if (state.Buttons.A == ButtonState.Pressed || state.Triggers.Left > 0.01f) { return true; }
                        break;
                    case PlayerButton.UseItem1:
                        if (state.Buttons.B == ButtonState.Pressed || state.Triggers.Right > 0.01f) { return true; }
                        break;
                    case PlayerButton.SwitchItem1:
                        if (state.Buttons.X == ButtonState.Pressed || state.Buttons.RightShoulder == ButtonState.Pressed) { return true; }
                        break;
                    case PlayerButton.SwitchItem2:
                        if (state.Buttons.Y == ButtonState.Pressed || state.Buttons.LeftShoulder == ButtonState.Pressed) { return true; }
                        break;
                    case PlayerButton.DownDirection:
                        if (state.DPad.Down == ButtonState.Pressed || state.ThumbSticks.Left.Y < 0) { return true; }
                        break;
                    case PlayerButton.UpDirection:
                        if (state.DPad.Up == ButtonState.Pressed || state.ThumbSticks.Left.Y > 0) { return true; }
                        break;
                    case PlayerButton.LeftDirection:
                        if (state.DPad.Left == ButtonState.Pressed || state.ThumbSticks.Left.X < 0) { return true; }
                        break;
                    case PlayerButton.RightDirection:
                        if (state.DPad.Right == ButtonState.Pressed || state.ThumbSticks.Left.X > 0) { return true; }
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the GamePad index for system calls.
        /// </summary>
        /// <param name="player">Player requested.</param>
        /// <returns>PlayerIndex for specified player. Throws an exception for unindexed pads and the Keyboard.</returns>
        public static PlayerIndex GetPlayerGamePadIndex(PPG_Player player)
        {
            switch (bindings[(int)player])
            {
                case PlayerPad.GamePad1:
                case PlayerPad.GamePad2:
                case PlayerPad.GamePad3:
                case PlayerPad.GamePad4:
                    return (PlayerIndex)bindings[(int)player];
                    break;
                case PlayerPad.Keyboard:
                case PlayerPad.NoPad:
                default:
                    throw new InvalidOperationException("No Xbox Gamepad index for " + bindings[(int)player].ToString());
            }
        }

        public static GlobalGameConstants.Direction PlayerAnalogStickDirection(PPG_Player player)
        {
            if (bindings[(int)player] == PlayerPad.Keyboard)
            {
                return GlobalGameConstants.Direction.NoDirection;
            }
            else if ((int)bindings[(int)player] > -1)
            {
                GamePadState state = xInputControllers[(int)bindings[(int)player]];

                // some third-party gamepads don't help with this
                if (float.IsNaN(state.ThumbSticks.Left.X) || float.IsNaN(state.ThumbSticks.Left.Y))
                {
                    return GlobalGameConstants.Direction.NoDirection;
                }

                if (state.ThumbSticks.Left.Length() < 0.05f)
                {
                    return GlobalGameConstants.Direction.NoDirection;
                }

                if (Math.Abs(state.ThumbSticks.Left.X) > Math.Abs(state.ThumbSticks.Left.Y))
                {
                    if (state.ThumbSticks.Left.X < 0)
                    {
                        return GlobalGameConstants.Direction.Left;
                    }
                    else
                    {
                        return GlobalGameConstants.Direction.Right;
                    }
                }
                else
                {
                    if (state.ThumbSticks.Left.Y < 0)
                    {
                        return GlobalGameConstants.Direction.Down;
                    }
                    else
                    {
                        return GlobalGameConstants.Direction.Up;
                    }
                }
            }

            return GlobalGameConstants.Direction.NoDirection;
        }

        /// <summary>
        /// Update the current input devices.
        /// </summary>
        /// <param name="currentTime">Current GameTime. Not used at the moment.</param>
        public static void Update(GameTime currentTime)
        {
            keyboardController = Keyboard.GetState();

            for (int i = 0; i < 4; i++)
            {
                xInputControllers[i] = GamePad.GetState((PlayerIndex)i);
            }
        }
    }

    /// <summary>
    /// Used to configure desired keys in Keyboad input.
    /// </summary>
    public class KeyboardControllerConfiguration
    {
        public Keys Confirm;
        public Keys Cancel;
        public Keys UseLeftItem;
        public Keys UseRightItem;
        public Keys SwitchItem1;
        public Keys SwitchItem2;
        public Keys UpDirection;
        public Keys DownDirection;
        public Keys LeftDirection;
        public Keys RightDirection;
        public Keys BackButton;
        public Keys PauseButton;

        /// <summary>
        /// Creates a default configuration we thought up at the beginning of development.
        /// </summary>
        /// <returns></returns>
        public static KeyboardControllerConfiguration DefaultKeyConfig()
        {
            KeyboardControllerConfiguration def = new KeyboardControllerConfiguration();

            def.Confirm = Keys.Enter;
            def.Cancel = Keys.Back;
            def.UseLeftItem = Keys.A;
            def.UseRightItem = Keys.S;
            def.SwitchItem1 = Keys.W;
            def.SwitchItem2 = Keys.Q;
            def.UpDirection = Keys.Up;
            def.DownDirection = Keys.Down;
            def.LeftDirection = Keys.Left;
            def.RightDirection = Keys.Right;
            def.BackButton = Keys.Back;
            def.PauseButton = Keys.Escape;

            return def;
        }
    }
}
