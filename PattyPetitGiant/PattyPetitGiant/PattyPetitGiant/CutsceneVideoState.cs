using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class CutsceneVideoState : ScreenState
    {
        private bool confirmPressed = false;

        private Texture2D videoTexture = null;

        private Video video;

        private bool playedThrough;
        private ScreenStateType nextScreen;

        public CutsceneVideoState(Video video, ScreenStateType nextScreen)
        {
            this.video = video;
            this.nextScreen = nextScreen;

            playedThrough = false;
        }

        protected override void doUpdate(GameTime currentTime)
        {
            if (Game1.videoPlayer.State == MediaState.Stopped)
            {
                if (playedThrough == false)
                {
                    Game1.videoPlayer.IsLooped = false;
                    Game1.videoPlayer.Play(video);

                    playedThrough = true;
                }
                else
                {
                    isComplete = true;
                }

            }
            
            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm) && !confirmPressed)
            {
                confirmPressed = true;
            }
            else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm) && confirmPressed)
            {
                Game1.videoPlayer.Stop();
                confirmPressed = false;

                isComplete = true;
            }
        }

        public override void render(SpriteBatch sb)
        {
            if (Game1.videoPlayer.State != MediaState.Stopped)
            {
                videoTexture = Game1.videoPlayer.GetTexture();
            }

            Rectangle screen = new Rectangle(AnimationLib.GraphicsDevice.Viewport.X, AnimationLib.GraphicsDevice.Viewport.Y, AnimationLib.GraphicsDevice.Viewport.Width, AnimationLib.GraphicsDevice.Viewport.Height);

            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.Identity);

            //sb.Draw(Game1.whitePixel, new Vector2(-400), null, Color.Black, 0.0f, Vector2.Zero, new Vector2(9999), SpriteEffects.None, 0.5f);

            if (videoTexture != null)
            {
                sb.Draw(videoTexture, screen, Color.White);
            }

            sb.End();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            return nextScreen;
        }
    }
}
