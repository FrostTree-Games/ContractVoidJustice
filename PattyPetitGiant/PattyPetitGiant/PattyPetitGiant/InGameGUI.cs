using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    /// <summary>
    /// Used for managing/rendering the code on the Player's GUI. Generally managed and rendered by a LevelState.
    /// </summary>
    public class InGameGUI
    {
        private LevelState parent = null;
        public LevelState Parent { get { return parent; } }

        private Color boxColor = Color.Green;
        private Color textColor = Color.LimeGreen;

        private BoxWindow[] boxWindows = new BoxWindow[10];
        private bool[] windowIsActive = new bool[10];

        private BoxWindow testWin;

        /// <summary>
        /// Data representing a simple text box.
        /// </summary>
        public struct BoxWindow
        {
            /// <summary>
            /// Used for opening/closing animations.
            /// </summary>
            public enum State
            {
                Hide = -1,
                Show = 0,
                Opening = 1,
                Closing = 0,
            }

            public BoxWindow(string name, int x, int y, int width, string text)
            {
                int lineCount = 0;

                this.name = name;
                this.x = x;
                this.y = y;
                this.width = width;
                this.text = WrapText(Game1.font, text, width, ref lineCount);
                this.height = lineCount * Game1.font.LineSpacing;
                this.animationState = State.Show;
                this.animationLocation = 0.0f;
            }

            /// <summary>
            /// Name of box for programatic purposes.
            /// </summary>
            public string name;

            /// <summary>
            /// Onscreen horizontal position.
            /// </summary>
            public int x;

            /// <summary>
            /// Onscreen vertical position.
            /// </summary>
            public int y;

            /// <summary>
            /// Box width.
            /// </summary>
            public int width;

            /// <summary>
            /// Box height.
            /// </summary>
            public int height;

            /// <summary>
            /// Current state in animation.
            /// </summary>
            public State animationState;

            /// <summary>
            /// Location in animation between [0, 1].
            /// </summary>
            public float animationLocation;

            /// <summary>
            /// Duration of animation when opening/closing.
            /// </summary>
            public const float animationDuration = 100;

            /// <summary>
            /// Text to display.
            /// </summary>
            public string text;
        }

        public InGameGUI(LevelState parent)
        {
            this.parent = parent;

            for (int i = 0; i < windowIsActive.Length; i++)
            {
                windowIsActive[i] = false;
            }

            testWin = new BoxWindow("foo", 100, 100, 200, "GamePad code overflowing with madness");
        }

        public static string WrapText(SpriteFont spriteFont, string text, float maxLineWidth, ref int lineCountOut)
        {
            string[] words = text.Split(' ');

            StringBuilder sb = new StringBuilder();

            float lineWidth = 0f;

            float spaceWidth = spriteFont.MeasureString(" ").X;

            int lineCount = 1;

            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    lineCount++;
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            lineCountOut = lineCount;

            return sb.ToString();
        }

        public void pushBox(BoxWindow box)
        {
            for (int i = 0; i < windowIsActive.Length; i++)
            {
                if (windowIsActive[i])
                {
                    continue;
                }

                windowIsActive[i] = true;
                boxWindows[i] = box;
                boxWindows[i].animationState = BoxWindow.State.Opening;
                boxWindows[i].animationLocation = 0.0f;
                break;
            }
        }

        public void popBox(string boxName)
        {
            for (int i = 0; i < windowIsActive.Length; i++)
            {
                if (windowIsActive[i])
                {
                    if (boxWindows[i].name == boxName)
                    {
                        boxWindows[i].animationLocation = 0.0f;
                        boxWindows[i].animationState = BoxWindow.State.Closing;
                        return;
                    }
                }
            }
        }

        public bool peepBox(string boxName)
        {
            for (int i = 0; i < windowIsActive.Length; i++)
            {
                if (windowIsActive[i])
                {
                    if (boxWindows[i].name == boxName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void update(GameTime currentTime)
        {
            for (int i = 0; i < windowIsActive.Length; i++)
            {
                if (!windowIsActive[i])
                {
                    continue;
                }

                if (boxWindows[i].animationState == BoxWindow.State.Opening)
                {
                    boxWindows[i].animationLocation += (currentTime.ElapsedGameTime.Milliseconds / BoxWindow.animationDuration);

                    if (boxWindows[i].animationLocation > 1.0f)
                    {
                        boxWindows[i].animationState = BoxWindow.State.Show;
                    }
                }
                else if (boxWindows[i].animationState == BoxWindow.State.Closing)
                {
                    boxWindows[i].animationLocation += (currentTime.ElapsedGameTime.Milliseconds / BoxWindow.animationDuration);

                    if (boxWindows[i].animationLocation > 1.0f)
                    {
                        windowIsActive[i] = false;
                    }
                }
            }
        }

        private void drawBox(ref BoxWindow box, SpriteBatch sb)
        {
            if (box.animationState == BoxWindow.State.Show)
            {
                sb.Draw(Game1.whitePixel, new Vector2(box.x, box.y), null, boxColor, 0.0f, Vector2.Zero, new Vector2(box.width, box.height), SpriteEffects.None, 0.5f);
                sb.DrawString(Game1.font, box.text, new Vector2(box.x, box.y), textColor);
            }
            else if (box.animationState == BoxWindow.State.Opening)
            {
                sb.Draw(Game1.whitePixel, new Vector2(box.x, box.y), null, boxColor, 0.0f, Vector2.Zero, new Vector2(box.width, box.height * box.animationLocation), SpriteEffects.None, 0.5f);
            }
            else if (box.animationState == BoxWindow.State.Closing)
            {
                sb.Draw(Game1.whitePixel, new Vector2(box.x, box.y), null, boxColor, 0.0f, Vector2.Zero, new Vector2(box.width, box.height * (1.0f - box.animationLocation)), SpriteEffects.None, 0.5f);
            }
        }

        public void render(SpriteBatch sb)
        {
            sb.Begin();

            string player_health_display = "Health: " + GlobalGameConstants.Player_Health;
            sb.DrawString(Game1.font, player_health_display, new Vector2(10, 10), Color.Black);

            string ammunition_amount_display = "Ammunition: " + GlobalGameConstants.Player_Ammunition;
            sb.DrawString(Game1.font, ammunition_amount_display, new Vector2(10, 42), Color.Black);

            string coin_amount_display = "Coin: " + GlobalGameConstants.Player_Coin_Amount;
            sb.DrawString(Game1.font, coin_amount_display, new Vector2(10, 74), Color.Black);

            string player_item_1 = "Item 1: " + GlobalGameConstants.Player_Item_1;
            sb.DrawString(Game1.font, player_item_1, new Vector2(320, 10), Color.Black);

            string player_item_2 = "Item 2: " + GlobalGameConstants.Player_Item_2;
            sb.DrawString(Game1.font, player_item_2, new Vector2(320, 42), Color.Black);

            for (int i = 0; i < windowIsActive.Length; i++)
            {
                if (!windowIsActive[i])
                {
                    continue;
                }

                drawBox(ref boxWindows[i], sb);
            }

            if (parent.RenderNodeMap)
            {
                Vector2 renderMapPosition = new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - (parent.NodeMap.GetLength(0) * 64 / 2), GlobalGameConstants.GameResolutionHeight / 4);
                int renderFocusX = (int)((parent.CameraFocus.CenterPoint.X / GlobalGameConstants.TileSize.X) / GlobalGameConstants.TilesPerRoomWide);
                int renderFocusY = (int)((parent.CameraFocus.CenterPoint.Y / GlobalGameConstants.TileSize.Y) / GlobalGameConstants.TilesPerRoomHigh);

                for (int i = 0; i < parent.NodeMap.GetLength(0); i++)
                {
                    for (int j = 0; j < parent.NodeMap.GetLength(1); j++)
                    {
                        if (parent.NodeMap[i, j].east || parent.NodeMap[i, j].west || parent.NodeMap[i, j].south || parent.NodeMap[i, j].north)
                        {
                            sb.Draw(Game1.whitePixel, renderMapPosition + new Vector2(i * 64, j * 64), null, Color.Black, 0.0f, Vector2.Zero, 32.0f, SpriteEffects.None, 0.6f);
                        }
                        if (i == renderFocusX && j == renderFocusY)
                        {
                            sb.Draw(Game1.whitePixel, renderMapPosition + new Vector2(i * 64 + 8, j * 64 + 8), null, Color.Red, 0.0f, Vector2.Zero, 16.0f, SpriteEffects.None, 0.61f);
                        }

                        if (parent.NodeMap[i, j].east)
                        {
                            sb.Draw(Game1.whitePixel, renderMapPosition + new Vector2(i * 64 + 32, j * 64 + 8), null, Color.Black, 0.0f, Vector2.Zero, 16.0f, SpriteEffects.None, 0.6f);
                        }
                        if (parent.NodeMap[i, j].west)
                        {
                            sb.Draw(Game1.whitePixel, renderMapPosition + new Vector2(i * 64 - 16, j * 64 + 8), null, Color.Black, 0.0f, Vector2.Zero, 16.0f, SpriteEffects.None, 0.6f);
                        }
                        if (parent.NodeMap[i, j].south)
                        {
                            sb.Draw(Game1.whitePixel, renderMapPosition + new Vector2(i * 64 + 8, j * 64 + 32), null, Color.Black, 0.0f, Vector2.Zero, 16.0f, SpriteEffects.None, 0.6f);
                        }
                        if (parent.NodeMap[i, j].north)
                        {
                            sb.Draw(Game1.whitePixel, renderMapPosition + new Vector2(i * 64 + 8, j * 64 - 16), null, Color.Black, 0.0f, Vector2.Zero, 16.0f, SpriteEffects.None, 0.6f);
                        }
                    }
                }
            }

            sb.End();
        }
    }
}
