using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    public class LevelSelectState : ScreenState
    {
        private struct LevelData
        {
            /// <summary>
            /// If this is false, then the array cell is not actually a level and should not be considered.
            /// </summary>
            public bool visible;

            public double prisonerRates;
            public double guardRates;
            public double alienRates;
            public double lootRates;

            public LevelData(double prisonerRates, double guardRates, double alienRates, double lootRates)
            {
                visible = true;

                this.prisonerRates = prisonerRates;
                this.guardRates = guardRates;
                this.alienRates = alienRates;
                this.lootRates = lootRates;
            }
        }

        private LevelData[,] levelMap = null;
        int selectedLevelX, selectedLevelY;

        private Vector2 cursorPosition;
        private const float cursorVelocity = 0.5f;
        private float cursorAnimationTime;

        private Vector2 drawMapTestOffset = new Vector2(1000, 225);
        private Vector2 testDetailStuff = new Vector2(750, 550);

        private bool upPressed = false;
        private bool downPressed = false;
        private bool confirmPressed = false;

        private Texture2D wireframe = null;

        public LevelSelectState()
        {
            levelMap = new LevelData[6, 3];

            wireframe = TextureLib.getLoadedTexture("shipWireframe.png");

            for (int i = 0; i < levelMap.GetLength(0); i++)
            {
                for (int j = 0; j < levelMap.GetLength(1); j++)
                {
                    levelMap[i, j] = new LevelData(Game1.rand.NextDouble(), Game1.rand.NextDouble(), Game1.rand.NextDouble(), Game1.rand.NextDouble());
                }
            }

            levelMap[0, 0].visible = false;
            levelMap[0, 2].visible = false;
            levelMap[levelMap.GetLength(0) - 1, 0].visible = false;
            levelMap[levelMap.GetLength(0) - 1, 2].visible = false;

            selectedLevelX = GameCampaign.PlayerLevelProgress + 1;
            selectedLevelY = GameCampaign.PlayerFloorHeight;

            cursorPosition = new Vector2(GlobalGameConstants.GameResolutionWidth / 2, GlobalGameConstants.GameResolutionHeight / 2);
            cursorAnimationTime = 0;
        }

        protected override void doUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection) && !downPressed)
            {
                downPressed = true;
            }
            else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection) && downPressed)
            {
                downPressed = false;

                if (selectedLevelY < levelMap.GetLength(1) - 1 && levelMap[selectedLevelX, selectedLevelY + 1].visible && selectedLevelY - GameCampaign.PlayerFloorHeight < 1)
                {
                    selectedLevelY++;
                }
            }

            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection) && !upPressed)
            {
                upPressed = true;
            }
            else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection) && upPressed)
            {
                upPressed = false;

                if (selectedLevelY > 0 && levelMap[selectedLevelX, selectedLevelY - 1].visible && selectedLevelY - GameCampaign.PlayerFloorHeight > -1)
                {
                    selectedLevelY--;
                }
            }

            selectedLevelX = GameCampaign.PlayerLevelProgress + 1;

            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm) && !confirmPressed)
            {
                confirmPressed = true;
            }
            else if (!InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm) && confirmPressed)
            {
                confirmPressed = false;

                GameCampaign.CurrentAlienRate = levelMap[selectedLevelX, selectedLevelY].alienRates;
                GameCampaign.CurrentGuardRate = levelMap[selectedLevelX, selectedLevelY].guardRates;
                GameCampaign.CurrentPrisonerRate = levelMap[selectedLevelX, selectedLevelY].prisonerRates;

                GameCampaign.PlayerLevelProgress = GameCampaign.PlayerLevelProgress + 1;
                GameCampaign.PlayerFloorHeight = selectedLevelY;

                isComplete = true;
            }

            double cursorDir = Math.Atan2(((selectedLevelY * 96) + drawMapTestOffset.Y) - cursorPosition.Y, ((selectedLevelX * 128) + drawMapTestOffset.X) - cursorPosition.X);
            cursorPosition += currentTime.ElapsedGameTime.Milliseconds * cursorVelocity * new Vector2((float)(Math.Cos(cursorDir)), (float)(Math.Sin(cursorDir)));

            if (Vector2.Distance(cursorPosition, new Vector2(((selectedLevelX * 128) + drawMapTestOffset.X), ((selectedLevelY * 96) + drawMapTestOffset.Y))) < 10f)
            {
                cursorPosition = new Vector2(((selectedLevelX * 128) + drawMapTestOffset.X), ((selectedLevelY * 96) + drawMapTestOffset.Y));
            }

            cursorAnimationTime += currentTime.ElapsedGameTime.Milliseconds;
        }

        private void drawLine(SpriteBatch sb, Vector2 origin, float length, float rotation, Color color, float width)
        {
            sb.Draw(Game1.whitePixel, origin, null, color, rotation, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0.5f);
        }

        private void drawBox(SpriteBatch sb, Rectangle rect, Color clr, float lineWidth)
        {
            drawLine(sb, new Vector2(rect.X, rect.Y), rect.Width, 0.0f, clr, lineWidth);
            drawLine(sb, new Vector2(rect.X, rect.Y), rect.Height, (float)(Math.PI / 2), clr, lineWidth);
        }

        public override void render(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            Texture2D tex = TextureLib.getLoadedTexture("wireFramePieces.png");

            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-575, -100, 0));

            sb.Draw(Game1.whitePixel, new Vector2(-99999, -99999) /2, null, Color.Black, 0.0f, Vector2.Zero, new Vector2(99999, 99999), SpriteEffects.None, 0.0f);

            sb.Draw(wireframe, Vector2.Zero, null, Color.DarkOrange, 0.0f, Vector2.Zero, new Vector2(1), SpriteEffects.FlipHorizontally, 0.0f);

            for (int i = 0; i < levelMap.GetLength(0); i++)
            {
                for (int j = 0; j < levelMap.GetLength(1); j++)
                {
                    if (!levelMap[i, j].visible)
                    {
                        continue;
                    }

                    if (i == GameCampaign.PlayerLevelProgress && j == GameCampaign.PlayerFloorHeight)
                    {
                        sb.Draw(tex, new Vector2(i * 128, j * 96) + drawMapTestOffset, new Rectangle(48, 0, 48, 48), Color.Blue);
                    }
                    else if (i == selectedLevelX && j == selectedLevelY)
                    {
                        sb.Draw(tex, new Vector2(i * 128, j * 96) + drawMapTestOffset, new Rectangle(0, 48, 48, 48), Color.Green);
                    }
                    else
                    {
                        sb.Draw(tex, new Vector2(i * 128, j * 96) + drawMapTestOffset, new Rectangle(0, 48, 48, 48), Color.Orange);
                    }
                }
            }

            drawLine(sb, drawMapTestOffset + new Vector2(24) + new Vector2(GameCampaign.PlayerLevelProgress * 128, GameCampaign.PlayerFloorHeight * 96), GameCampaign.PlayerFloorHeight == selectedLevelY ? 128f : 155f, 0.85f * (float)((-Math.PI / 2) + Math.Atan2(selectedLevelX - GameCampaign.PlayerLevelProgress, GameCampaign.PlayerFloorHeight - selectedLevelY)), Color.Gray, 3.5f);

            sb.Draw(tex, cursorPosition + new Vector2(24), new Rectangle(0, 0, 48, 48), Color.Red, 0.0f, new Vector2(24), 1 + (0.2f * (float)Math.Sin(cursorAnimationTime / 250f)), SpriteEffects.None, 0.5f);

            /*
            Rectangle rx = XboxTools.GetTitleSafeArea(AnimationLib.GraphicsDevice, 0.8f);
            rx.X += 575;
            rx.Y += 100;
            drawBox(sb, rx, Color.Orange, 2); */

            sb.DrawString(Game1.font, "\nPrisoner Rates: " + levelMap[selectedLevelX, selectedLevelY].prisonerRates, testDetailStuff, Color.Orange);
            sb.DrawString(Game1.font, "\n\nAlien Rates: " + levelMap[selectedLevelX, selectedLevelY].alienRates, testDetailStuff, Color.Red);
            sb.DrawString(Game1.font, "\n\n\nGuard Rates: " + levelMap[selectedLevelX, selectedLevelY].guardRates, testDetailStuff, Color.LightBlue);
            sb.DrawString(Game1.font, "\n\n\n\nLoot Rates: " + levelMap[selectedLevelX, selectedLevelY].lootRates, testDetailStuff, Color.Black);

            sb.Draw(Game1.whitePixel, testDetailStuff - new Vector2(1, 1), null, Color.Black, 0.0f, Vector2.Zero, new Vector2(52, 16), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, testDetailStuff, null, Color.Orange, 0.0f, Vector2.Zero, new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].prisonerRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 14), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, testDetailStuff + new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].prisonerRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 0), null, Color.Red, 0.0f, Vector2.Zero, new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].alienRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 14), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, testDetailStuff + new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].prisonerRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 0) + new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].alienRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 0), null, Color.LightBlue, 0.0f, Vector2.Zero, new Vector2((float)(levelMap[selectedLevelX, selectedLevelY].guardRates / (levelMap[selectedLevelX, selectedLevelY].prisonerRates + levelMap[selectedLevelX, selectedLevelY].guardRates + levelMap[selectedLevelX, selectedLevelY].alienRates)) * 50, 14), SpriteEffects.None, 0.5f);

            sb.End();
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            return ScreenStateType.LevelState;
        }
    }
}
