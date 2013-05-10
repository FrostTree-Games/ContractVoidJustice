using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    public abstract class ScreenState
    {
        public enum ScreenStateType
        {
            InvalidScreenState = -1,
            TitleScreen = 0,
            LevelState = 1,
            OptionsMenu = 2,
            GameSetupMenu = 3,
        }

        protected bool pause = false;
        public bool Pause { get { return pause; } set { pause = value; } }

        protected bool isComplete = false;
        /// <summary>
        /// Used to determine if the state is to be switched or not.
        /// </summary>
        public bool IsComplete { get { return isComplete; } }

        public void update(GameTime currentTime)
        {
            if (pause)
            {
                return;
            }

            doUpdate(currentTime);
        }

        protected abstract void doUpdate(GameTime currentTime);

        /// <summary>
        /// Renders a the state's output to the desired SpriteBatch. Must have it's own Begin/End call.
        /// </summary>
        /// <param name="sb"></param>
        public abstract void render(SpriteBatch sb);

        public static ScreenState SwitchToNewScreen(ScreenStateType type)
        {
            switch (type)
            {
                case ScreenStateType.LevelState:
                    return new LevelState();
                default:
                    return null;
            }
        }

        public abstract ScreenStateType nextLevelState();
    }
}
