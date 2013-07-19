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

        private static GamePadState[] xInputControllers = null;
        private static KeyboardState keyboardController = new KeyboardState();

        /// <summary>
        /// Initalizes input device variables.
        /// </summary>
        public static void Initalize()
        {
            if (xInputControllers != null)
            {
                xInputControllers = new GamePadState[4];
            }
        }

        public static void update(GameTime currentTime)
        {
            keyboardController = Keyboard.GetState();

            for (int i = 0; i < 4; i++)
            {
                xInputControllers[i] = GamePad.GetState((PlayerIndex)i);
            }
        }
    }
}
