using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    class CreditsScreen : ScreenState
    {
        private double credits_time_passed;
        private const double credit_duration_time = 20000;

        private const float empty_space_top = 780f;
        private const float empty_space_bot = 300f;

        private bool end_game = false;

        public CreditsScreen(bool end_game)
        {
            credits_time_passed = 0.0f;

            BackGroundAudio.playSong("Menu", false);

            this.end_game = end_game;
        }

        protected override void doUpdate(GameTime currentTime)
        {
            credits_time_passed += currentTime.ElapsedGameTime.Milliseconds;

            if (credits_time_passed > (credit_duration_time+5000))
            {
                isComplete = true;
            }
        }

        public override void render(SpriteBatch sb)
        {
            AnimationLib.GraphicsDevice.Clear(Color.Black);
            sb.Begin();
            sb.Draw(Game1.creditImage, new Vector2(360-Game1.creditImage.Bounds.Width / 4, -1 * ((Game1.creditImage.Bounds.Height / 2)+empty_space_bot+empty_space_top) * (float)(credits_time_passed / credit_duration_time) + empty_space_top), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1.0f), SpriteEffects.None, 0.5f);
            sb.End();
        }

        public override ScreenStateType nextLevelState()
        {
            BackGroundAudio.stopAllSongs();
            if (end_game)
                return ScreenStateType.HighScoresStateComplete;
            else
                return ScreenStateType.OptionsMenu;
        }
    }
}
