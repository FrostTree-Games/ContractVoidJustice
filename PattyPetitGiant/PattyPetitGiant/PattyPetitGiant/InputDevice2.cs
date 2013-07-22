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
            Player_4 = 4,
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
        public static KeyboardControllerConfiguration KeyConfig { get { return keyConfig; } set { keyConfig = value; } }

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
        }

        private static PlayerPad GamePadIndexToPlayerPad(PlayerIndex index)
        {
            return (PlayerPad)((int)index);
        }

        public static PlayerPad isAnyControllerButtonDown(PlayerButton button)
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
                    case PlayerButton.UseItem1:
                        if (xInputControllers[i].Buttons.A == ButtonState.Pressed || xInputControllers[i].Triggers.Left > 0.01f) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.UseItem1)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.UseItem2:
                        if (xInputControllers[i].Buttons.B == ButtonState.Pressed || xInputControllers[i].Triggers.Right > 0.01f) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.UseItem2)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.SwitchItem1:
                        if (xInputControllers[i].Buttons.X == ButtonState.Pressed || xInputControllers[i].Buttons.LeftShoulder == ButtonState.Pressed) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.SwitchItem1)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.SwitchItem2:
                        if (xInputControllers[i].Buttons.Y == ButtonState.Pressed || xInputControllers[i].Buttons.RightShoulder == ButtonState.Pressed) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.SwitchItem2)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.DownDirection:
                        if (xInputControllers[i].DPad.Down == ButtonState.Pressed || xInputControllers[i].ThumbSticks.Left.Y < 0) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
                        if (keyboardController.IsKeyDown(keyConfig.DownDirection)) { return PlayerPad.Keyboard; }
                        break;
                    case PlayerButton.UpDirection:
                        if (xInputControllers[i].DPad.Down == ButtonState.Pressed || xInputControllers[i].ThumbSticks.Left.Y > 0) { return GamePadIndexToPlayerPad((PlayerIndex)i); }
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
        /// Update the current input devices.
        /// </summary>
        /// <param name="currentTime">Current GameTime. Not used.</param>
        public static void update(GameTime currentTime)
        {
            keyboardController = Keyboard.GetState();

            for (int i = 0; i < 4; i++)
            {
                xInputControllers[i] = GamePad.GetState((PlayerIndex)i);
            }
        }
    }

    public class KeyboardControllerConfiguration
    {
        public Keys Confirm;
        public Keys Cancel;
        public Keys UseItem1;
        public Keys UseItem2;
        public Keys SwitchItem1;
        public Keys SwitchItem2;
        public Keys UpDirection;
        public Keys DownDirection;
        public Keys LeftDirection;
        public Keys RightDirection;
        public Keys BackButton;
        public Keys PauseButton;

        public static KeyboardControllerConfiguration DefaultKeyConfig()
        {
            KeyboardControllerConfiguration def = new KeyboardControllerConfiguration();

            def.Confirm = Keys.Enter;
            def.Cancel = Keys.Back;
            def.UseItem1 = Keys.A;
            def.UseItem2 = Keys.B;
            def.SwitchItem1 = Keys.Q;
            def.SwitchItem2 = Keys.W;
            def.UpDirection = Keys.Up;
            def.DownDirection = Keys.Down;
            def.LeftDirection = Keys.Left;
            def.RightDirection = Keys.Right;
            def.BackButton = Keys.Back;
            def.PauseButton = Keys.Pause;

            return def;
        }
    }
}
