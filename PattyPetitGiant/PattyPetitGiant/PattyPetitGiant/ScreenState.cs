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
            LevelSelectState = 4,
            HighScoresState = 5,
            LevelReviewState = 6,
            FMV_ELEVATOR_EXIT = 7,
            FMV_ELEVATOR_ENTER = 8,
            HighScoresStateOptions = 9,
            CreditsOptionsState = 10,
            CreditsEndGameState = 11,
            IntroCutScene = 12,
            EndingCutScene = 13,
            IntroCutSceneCoop = 14,
            EndingCutSceneCoop = 15,
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
                case ScreenStateType.LevelSelectState:
                    return new LevelSelectState();
                case ScreenStateType.TitleScreen:
                    return new TitleScreen(TitleScreen.titleScreens.menuScreen);
                case ScreenStateType.HighScoresState:
                    return new HighScoresState(true);
                case ScreenStateType.OptionsMenu:
                    return new OptionsMenu();
                case ScreenStateType.LevelReviewState:
                    return new LevelReviewState();
                case ScreenStateType.GameSetupMenu:
                    return new CampaignLobbyState();
                case ScreenStateType.FMV_ELEVATOR_EXIT:
                    return new CutsceneVideoState((GameCampaign.IsATwoPlayerGame)? Game1.levelExitVideoCoop:Game1.levelExitVideo, ScreenStateType.LevelReviewState);
                case ScreenStateType.FMV_ELEVATOR_ENTER:
                    return new CutsceneVideoState((GameCampaign.IsATwoPlayerGame)?Game1.levelEnterVideoCoop:Game1.levelEnterVideo, ScreenStateType.LevelState);
                case ScreenStateType.HighScoresStateOptions:
                    return new HighScoresState(false);
                case ScreenStateType.CreditsOptionsState:
                    return new CreditsScreen(false);
                case ScreenStateType.CreditsEndGameState:
                    return new CreditsScreen(true);
                case ScreenStateType.IntroCutScene:
                    return new CutsceneVideoState(Game1.introCutScene, ScreenStateType.LevelSelectState);
                case ScreenState.ScreenStateType.IntroCutSceneCoop:
                    return new CutsceneVideoState(Game1.introCutSceneCoop, ScreenStateType.LevelSelectState);
                case ScreenStateType.EndingCutScene:
                    return new CutsceneVideoState((GameCampaign.PlayerAllegiance > 0.7) ? Game1.guardEndCutScene : (GameCampaign.PlayerAllegiance < 0.3) ? Game1.prisonerEndCutScene : Game1.alienEndCutScene, ScreenStateType.CreditsEndGameState);
                case ScreenStateType.EndingCutSceneCoop:
                    return new CutsceneVideoState((GameCampaign.PlayerAllegiance > 0.7) ? Game1.guardEndCutSceneCoop : (GameCampaign.PlayerAllegiance < 0.3) ? Game1.prisonerEndCutSceneCoop : Game1.alienEndCutSceneCoop, ScreenStateType.CreditsEndGameState);
                default:
                    return null;
            }
        }

        public abstract ScreenStateType nextLevelState();
    }
}
