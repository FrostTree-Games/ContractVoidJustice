using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class HighScoresState : ScreenState
    {
        public class HighScoreValue
        {
            public string playerName;
            public int coinCollected;
            public float secondsElapsed;
            public int levelDiedAt;
            public int floorDiedOn;

            public HighScoreValue(string playerName, int coinCollected, float secondsElapsed, int levelDiedAt, int floorDiedOn)
            {
                this.playerName = playerName;
                this.coinCollected = coinCollected;
                this.secondsElapsed = secondsElapsed;
                this.levelDiedAt = levelDiedAt;
                this.floorDiedOn = floorDiedOn % 3;
            }
        }

        private static List<HighScoreValue> highScores = null;

        public static void InitalizeHighScores()
        {
            if (highScores == null)
            {
                highScores = new List<HighScoreValue>();
            }
        }

        public HighScoresState()
        {
            InitalizeHighScores();

            //test values to punch in
            highScores.Add(new HighScoreValue("Wilson", 6969, 83939, 2, 2));
            highScores.Add(new HighScoreValue("Eric", 999999, 123, 3, 0));
            highScores.Add(new HighScoreValue("Daniel", 123456, 123, 1, 1));
            highScores.Add(new HighScoreValue("Ivan", 1000, 345, 0, 1));
            highScores.Add(new HighScoreValue("Alyosha", 1000, 345, 0, 1));
            highScores.Add(new HighScoreValue("Dmitri", 1500, 43567, 4, 2));
        }

        protected override void doUpdate(GameTime currentTime)
        {
        }

        public override void render(SpriteBatch sb)
        {
            AnimationLib.GraphicsDevice.Clear(Color.Black);

            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.Identity);

            for (int i = 0; i < highScores.Count; i++)
            {
                Vector2 drawListPosition = new Vector2(100) + new Vector2(0, i * 28);

                sb.DrawString(Game1.font, highScores[i].playerName, drawListPosition, Color.Black);
            }

            sb.End();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
 	        return ScreenStateType.TitleScreen;
        }
    }
}
