using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class LevelReviewState : ScreenState
    {
        private bool confirmPressed = false;

        public LevelReviewState()
        {
            GameCampaign.ElapsedCampaignTime += LevelState.ElapsedLevelTime;
        }

        protected override void doUpdate(GameTime currentTime)
        {
            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm) && !confirmPressed)
            {
                confirmPressed = true;
            }
            else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm) && confirmPressed)
            {
                confirmPressed = false;

                isComplete = true;
            }
        }

        public override void render(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.Identity);

            sb.Draw(Game1.whitePixel, new Vector2(-400), null, Color.Black, 0.0f, Vector2.Zero, new Vector2(9999), SpriteEffects.None, 0.5f);

            sb.DrawString(Game1.tenbyFive24, "Level Time: " + Math.Round(LevelState.ElapsedLevelTime / 1000f) + " seconds", new Vector2(100, 200), Color.White);
            sb.DrawString(Game1.tenbyFive24, "Total Game Time: " + Math.Round(GameCampaign.ElapsedCampaignTime / 1000f) + " seconds", new Vector2(100, 224), Color.White);

            sb.DrawString(Game1.tenbyFive24, "Press A to continue", new Vector2(100, 300), Color.White);

            sb.End();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            return ScreenStateType.LevelSelectState;
        }
    }
}
