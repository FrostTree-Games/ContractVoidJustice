//#define SHOW_MAP_COLORS

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
        private Color guardColor = Color.White;
        private Color prisonerColor = Color.White;

        private float guardColorBlink = 0.0f;
        private float prisonerColorBlink = 0.0f;
        private float guardColorBlinkDuration = 500.0f;
        private float prisonColorBlinkDuration = 500.0f;
        private const float maxColorBlink = 2000.0f;

        private float prisonerBarPosition = 0.0f;
        private bool prisonerBarPositionReset = false;

        private float guardBarPosition = 0.0f;
        private bool guardBarPositionReset = false;

        private bool guardAllegianceChange = false;
        private bool prisonerAllegianceChange = false;

        private BoxWindow[] boxWindows = new BoxWindow[10];
        private bool[] windowIsActive = new bool[10];

        private AnimationLib.FrameAnimationSet keyFoundPic = null;
        private AnimationLib.FrameAnimationSet keyNotFoundPic = null;

        private BoxWindow testWin;

        private string deathMessage = "MISSION FAILED";

        private float blackFadeOverlay;
        public float BlackFadeOverlay { get { return blackFadeOverlay; } set { blackFadeOverlay = value; } }

        private Texture2D mapIcon = null;

        private float flickerTime;

        private AnimationLib.FrameAnimationSet coinAnim = null;
        private AnimationLib.FrameAnimationSet medAnim = null;
        private AnimationLib.FrameAnimationSet ammoAnim = null;

        public struct PriceDisplay
        {
            public bool active;
            public string price;
            public string description;
            public Vector2 position;
        }

        public static PriceDisplay[] prices;

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
                Closing = 2,
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

            if (prices == null)
            {
                prices = new PriceDisplay[3];
            }

            for (int i = 0; i < windowIsActive.Length; i++)
            {
                windowIsActive[i] = false;
            }

            blackFadeOverlay = 0.0f;

            keyFoundPic = AnimationLib.getFrameAnimationSet("keyPic");
            keyNotFoundPic = AnimationLib.getFrameAnimationSet("keyEmptyPic");

            coinAnim = AnimationLib.getFrameAnimationSet("testCoin");
            medAnim = AnimationLib.getFrameAnimationSet("itemHealth");
            ammoAnim = AnimationLib.getFrameAnimationSet("itemBattery");

            mapIcon = TextureLib.getLoadedTexture("mapPda.png");

            testWin = new BoxWindow("foo", 100, 100, 200, "GamePad code overflowing with madness");

            flickerTime = 0.0f;
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
                        windowIsActive[i] = false;
                        return;
                    }
                }
            }
        }

        public bool peekBox(string boxName)
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
            flickerTime += currentTime.ElapsedGameTime.Milliseconds;

            if (GameCampaign.IsATwoPlayerGame)
            {
                //if(Vector2.Distance()
            }

            if (GameCampaign.change_guard_icon_color)
            {
                guardColorBlink += currentTime.ElapsedGameTime.Milliseconds;
                if (guardColorBlink > maxColorBlink)
                {
                    guardColorBlink = 0.0f;
                    guardColorBlinkDuration = 500f;
                    guardColor = Color.White;
                    GameCampaign.change_guard_icon_color = false;
                }
                else
                {
                    if (guardColorBlink > guardColorBlinkDuration)
                    {
                        guardColor = (guardColor == Color.White) ? Color.Blue : Color.White;
                        guardColorBlinkDuration += 500f;
                    }
                }
            }

            if (GameCampaign.change_prisoner_icon_color)
            {
                prisonerColorBlink += currentTime.ElapsedGameTime.Milliseconds;
                if (prisonerColorBlink > maxColorBlink)
                {
                    prisonerColorBlink = 0.0f;
                    prisonColorBlinkDuration = 500f;
                    prisonerColor = Color.White;
                    GameCampaign.change_prisoner_icon_color = false;
                }
                else
                {
                    if (prisonerColorBlink > prisonColorBlinkDuration)
                    {
                        prisonerColor = (prisonerColor == Color.White) ? Color.Red : Color.White;
                        prisonColorBlinkDuration += 500f;
                    }
                }
            }

            for (int i = 0; i < windowIsActive.Length; i++)
            {
                if (!windowIsActive[i])
                {
                    continue;
                }

                if (boxWindows[i].animationState == BoxWindow.State.Show)
                {
                    //
                }
                else if (boxWindows[i].animationState == BoxWindow.State.Opening)
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

        private void renderText(SpriteBatch sb, Vector2 plus, Color clr)
        {
            //
        }

        public void render(SpriteBatch sb)
        {
            Color textColor = Color.Lerp(CampaignLobbyState.getSquareColor(GameCampaign.PlayerColor), Color.White, (float)(Math.Sin(flickerTime / 500f) + 1.0f) / 2);
            Color text2Color = Color.Lerp(CampaignLobbyState.getSquareColor(GameCampaign.Player2Color), Color.White, (float)(Math.Sin(flickerTime / 500f) + 1.0f) / 2);

            AnimationLib.FrameAnimationSet player_first_weapon = GlobalGameConstants.WeaponDictionary.weaponInfo[(int)GameCampaign.Player_Right_Item].pickupImage;
            AnimationLib.FrameAnimationSet player_second_weapon = GlobalGameConstants.WeaponDictionary.weaponInfo[(int)GameCampaign.Player_Left_Item].pickupImage;

            AnimationLib.FrameAnimationSet player2_first_weapon = GlobalGameConstants.WeaponDictionary.weaponInfo[(int)GameCampaign.Player2_Item_1].pickupImage;
            AnimationLib.FrameAnimationSet player2_second_weapon = GlobalGameConstants.WeaponDictionary.weaponInfo[(int)GameCampaign.Player2_Item_2].pickupImage;

            //sb.Draw(Game1.whitePixel, XboxTools.GetTitleSafeArea(AnimationLib.GraphicsDevice, 0.8f), new Color(0.0f, 0.75f, 1.0f, 0.6f));

            string player_health_display = "H: ";
            string ammunition_amount_display = "A: ";
            string coin_amount_display = GameCampaign.Player_Coin_Amount.ToString();
            string Player_Right_Item = "R: ";
            string Player_Left_Item = "L: ";

            for (int i = 0; i < prices.Length; i++)
            {
                if (prices[i].active)
                {
                    sb.DrawString(Game1.tenbyFive14, prices[i].price.ToString(), prices[i].position - parent.CameraFocus.Position + new Vector2(GlobalGameConstants.GameResolutionWidth / 2, GlobalGameConstants.GameResolutionHeight / 2), Color.LightBlue);
                }
            }

            //draw a black 1px outline over the GUI font

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 || j == 0)
                    {
                        continue;
                    }

                    Vector2 offset = new Vector2(i, j);
                    //sb.DrawString(Game1.font, player_health_display, , Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                    medAnim.drawAnimationFrame(0.0f, sb, new Vector2(140, 80) + offset, new Vector2(0.75f), 0.5f, 0.0f, Vector2.Zero, Color.White);
                    //sb.DrawString(Game1.font, ammunition_amount_display, new Vector2(140, 110) + offset, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                    ammoAnim.drawAnimationFrame(0.0f, sb, new Vector2(140, 110) + offset, new Vector2(0.75f), 0.5f, 0.0f, Vector2.Zero, Color.White);
                    coinAnim.drawAnimationFrame(0.0f, sb, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 70) - new Vector2(Game1.font.MeasureString(coin_amount_display).X, 0) / 2 + offset, new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.White);
                    sb.DrawString(Game1.font, coin_amount_display, new Vector2(GlobalGameConstants.GameResolutionWidth / 2 + 32, 70) - new Vector2(Game1.font.MeasureString(coin_amount_display).X, 0) / 2 + offset, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                    sb.DrawString(Game1.font, Player_Right_Item, new Vector2(245, 140) + offset, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                    sb.DrawString(Game1.font, Player_Left_Item, new Vector2(160, 140) + offset, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);

                    player_second_weapon.drawAnimationFrame(0.0f, sb, new Vector2(180, 140), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.Black);
                    player_first_weapon.drawAnimationFrame(0.0f, sb, new Vector2(265, 140), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.Black);

                    if (GameCampaign.IsATwoPlayerGame)
                    {
                        //sb.DrawString(Game1.font, player_health_display, new Vector2(955, 80) + offset, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                        medAnim.drawAnimationFrame(0.0f, sb, new Vector2(945, 80) + offset, new Vector2(0.75f), 0.5f, 0.0f, Vector2.Zero, Color.White);
                        //sb.DrawString(Game1.font, ammunition_amount_display, new Vector2(955, 110) + offset, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                        ammoAnim.drawAnimationFrame(0.0f, sb, new Vector2(945, 110) + offset, new Vector2(0.75f), 0.5f, 0.0f, Vector2.Zero, Color.White);
                        sb.DrawString(Game1.font, Player_Right_Item, new Vector2(1057, 140) + offset, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                        sb.DrawString(Game1.font, Player_Left_Item, new Vector2(972, 140) + offset, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);

                        player2_second_weapon.drawAnimationFrame(0.0f, sb, new Vector2(1070 + 16, 140), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.Black);
                        player2_first_weapon.drawAnimationFrame(0.0f, sb, new Vector2(980 + 16, 140), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.Black);
                    }
                }
            }

            // allegiance meter
            sb.Draw(Game1.healthBar, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 100) - new Vector2(Game1.healthBar.Width / 2, 0), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.greyBar, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 100) - new Vector2(Game1.healthBar.Width / 2, 0) + new Vector2(5, 0), null, Color.Orange, 0.0f, Vector2.Zero, new Vector2((1 - GameCampaign.PlayerAllegiance) * 160f, 1), SpriteEffects.None, 0.51f);
            sb.Draw(Game1.greyBar, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 100) - new Vector2(Game1.healthBar.Width / 2, 0) + new Vector2(5 + ((1 - GameCampaign.PlayerAllegiance) * 160f), 0), null, Color.Cyan, 0.0f, Vector2.Zero, new Vector2(GameCampaign.PlayerAllegiance * 160f, 1), SpriteEffects.None, 0.51f);
            
            sb.Draw(Game1.whitePixel, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 100) - new Vector2(Game1.healthBar.Width / 2, 0) + new Vector2((float)(5 + ((1 - guardBarPosition) * 160.0)), 0), null, Color.White, 0.0f, Vector2.Zero, new Vector2(5, 16), SpriteEffects.None, 0.53f);
            sb.Draw(Game1.whitePixel, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 100) - new Vector2(Game1.healthBar.Width / 2, 0) + new Vector2((float)(5 + ((1 - prisonerBarPosition) * 160.0)), 0), null, Color.White, 0.0f, Vector2.Zero, new Vector2(5, 16), SpriteEffects.None, 0.53f);
            
            sb.Draw(Game1.energyOverlay, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 100) - new Vector2(Game1.healthBar.Width / 2, 0), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.52f);
            sb.Draw(Game1.prisonerIcon, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 100) + new Vector2(Game1.healthBar.Width / 2 + 12, -12), null, prisonerColor, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.guardIcon, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 100) - new Vector2(Game1.healthBar.Width / 2 + 32, 20), null, guardColor, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.5f);

            if (GameCampaign.change_guard_icon_color)
            {
                if (guardBarPosition >= GameCampaign.PlayerAllegiance)
                {
                    guardBarPosition = GameCampaign.PlayerAllegiance;
                    guardBarPositionReset = false;
                }
                else
                {
                    if (!guardBarPositionReset)
                    {
                        guardBarPosition = 0.0f;
                        guardBarPositionReset = true;
                    }
                    else
                    {
                        guardBarPosition += 0.01f;
                    }
                }
            }
            else
            {
                guardBarPosition = GameCampaign.PlayerAllegiance;
            }

            if (GameCampaign.change_prisoner_icon_color)
            {
                if (prisonerBarPosition <= GameCampaign.PlayerAllegiance)
                {
                    prisonerBarPosition = GameCampaign.PlayerAllegiance;
                    prisonerBarPositionReset = false;
                }
                else
                {
                    if (!prisonerBarPositionReset)
                    {
                        prisonerBarPosition = 1.0f;
                        prisonerBarPositionReset = true;
                    }
                    else
                    {
                        prisonerBarPosition -= 0.01f;
                    }
                }
            }
            else
            {
                prisonerBarPosition = GameCampaign.PlayerAllegiance;
            }
            //player 1 GUI
            medAnim.drawAnimationFrame(0.0f, sb, new Vector2(140, 80), new Vector2(0.75f), 0.51f, 0.0f, Vector2.Zero, Color.White); 
            sb.Draw(Game1.healthBar, new Vector2(167, 85), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.healthColor, new Vector2(172, 85), null, Color.White, 0.0f, Vector2.Zero, new Vector2(GameCampaign.Player_Health * 1.6f, 1), SpriteEffects.None, 0.51f);
            sb.Draw(Game1.energyOverlay, new Vector2(167, 85), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.52f);

            ammoAnim.drawAnimationFrame(0.0f, sb, new Vector2(140, 110), new Vector2(0.75f), 0.5f, 0.0f, Vector2.Zero, Color.White);
            sb.Draw(Game1.healthBar, new Vector2(167, 113), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.5f);
            sb.Draw(Game1.energyColor, new Vector2(172, 113), null, Color.White, 0.0f, Vector2.Zero, new Vector2(GameCampaign.Player_Ammunition * 1.6f, 1), SpriteEffects.None, 0.51f);
            sb.Draw(Game1.energyOverlay, new Vector2(167, 113), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.52f);

            coinAnim.drawAnimationFrame(0.0f, sb, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 70) - new Vector2(Game1.font.MeasureString(coin_amount_display).X, 0) / 2, new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.White);
            sb.DrawString(Game1.font, coin_amount_display, new Vector2(GlobalGameConstants.GameResolutionWidth / 2 + 32, 70) - new Vector2(Game1.font.MeasureString(coin_amount_display).X, 0) / 2, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                    
            sb.DrawString(Game1.font, Player_Right_Item, new Vector2(245, 140), textColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.51f);
            player_first_weapon.drawAnimationFrame(0.0f, sb, new Vector2(265, 140), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.White);
            
            sb.DrawString(Game1.font, Player_Left_Item, new Vector2(160, 140), textColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.51f);
            player_second_weapon.drawAnimationFrame(0.0f, sb, new Vector2(180, 140), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.White);

            if (GameCampaign.IsATwoPlayerGame)
            {
                medAnim.drawAnimationFrame(0.0f, sb, new Vector2(945, 80), new Vector2(0.75f), 0.5f, 0.0f, Vector2.Zero, Color.White);
                sb.Draw(Game1.healthBar, new Vector2(972, 83), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.5f);
                sb.Draw(Game1.healthColor, new Vector2(977, 83), null, Color.White, 0.0f, Vector2.Zero, new Vector2(GameCampaign.Player2_Health * 1.6f, 1), SpriteEffects.None, 0.51f);
                sb.Draw(Game1.energyOverlay, new Vector2(972, 83), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.52f);

                ammoAnim.drawAnimationFrame(0.0f, sb, new Vector2(945, 110), new Vector2(0.75f), 0.5f, 0.0f, Vector2.Zero, Color.White);
                sb.Draw(Game1.healthBar, new Vector2(972, 113), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.5f);
                sb.Draw(Game1.energyColor, new Vector2(977, 113), null, Color.White, 0.0f, Vector2.Zero, new Vector2(GameCampaign.Player2_Ammunition * 1.6f, 1), SpriteEffects.None, 0.51f);
                sb.Draw(Game1.energyOverlay, new Vector2(972, 113), null, Color.White, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0.52f);

                sb.DrawString(Game1.font, Player_Left_Item, new Vector2(972, 140), text2Color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.51f);
                player2_first_weapon.drawAnimationFrame(0.0f, sb, new Vector2(980 + 16, 140), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.White);

                sb.DrawString(Game1.font, Player_Right_Item, new Vector2(1057, 140), text2Color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.51f);
                player2_second_weapon.drawAnimationFrame(0.0f, sb, new Vector2(1070 + 16, 140), new Vector2(1.0f), 0.5f, 0.0f, Vector2.Zero, Color.White);
            }
            
            /*sb.Draw(Game1.whitePixel, new Vector2(499, 31), null, Color.Black, 0.0f, Vector2.Zero, new Vector2(66, 26), SpriteEffects.None, 0.0f);
            sb.Draw(Game1.whitePixel, new Vector2(500, 32), null, Color.Orange, 0.0f, Vector2.Zero, new Vector2(64 * (1 - GameCampaign.PlayerAllegiance), 24), SpriteEffects.None, 0.0f);
            sb.Draw(Game1.whitePixel, new Vector2(500 + (64 * (1 - GameCampaign.PlayerAllegiance)), 32), null, Color.LightBlue, 0.0f, Vector2.Zero, new Vector2(64 * GameCampaign.PlayerAllegiance, 24), SpriteEffects.None, 0.0f);

            for (int i = 0; i < windowIsActive.Length; i++)
            {
                if (!windowIsActive[i])
                {
                    continue;
                }

                drawBox(ref boxWindows[i], sb);
            }

            for (int i = 0; i < parent.KeyModule.NumberOfKeys; i++)
            {
                if (parent.KeyModule.isKeyFound((LevelKeyModule.KeyColor)i))
                {
                    keyFoundPic.drawAnimationFrame(0.0f, sb, new Vector2(600 + (i * 49), 10), new Vector2(3.0f, 3.0f), 0.5f, parent.KeyModule.KeyColorSet[i]);
                }
                else
                {
                    keyNotFoundPic.drawAnimationFrame(0.0f, sb, new Vector2(600 + (i * 49), 10), new Vector2(3.0f, 3.0f), 0.5f, parent.KeyModule.KeyColorSet[i]);
                }
            }*/

            if (parent.RenderNodeMap && !parent.Player1Dead)
            {
                Vector2 renderMapPosition = new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - (parent.NodeMap.GetLength(0) * 64 / 2), GlobalGameConstants.GameResolutionHeight / 4);
                int renderFocusX = (int)((parent.CameraFocus.CenterPoint.X / GlobalGameConstants.TileSize.X) / GlobalGameConstants.TilesPerRoomWide);
                int renderFocusY = (int)((parent.CameraFocus.CenterPoint.Y / GlobalGameConstants.TileSize.Y) / GlobalGameConstants.TilesPerRoomHigh);

                sb.Draw(mapIcon, renderMapPosition - (new Vector2(100)) / 2, Color.White);

                int mapResolution = 2;
                float mapScale = 2.5f;
                for (int i = 0; i < parent.Map.Map.GetLength(0); i += mapResolution)
                {
                    for (int j = 0; j < parent.Map.Map.GetLength(1); j += mapResolution)
                    {
                        if (parent.Map.Map[i, j] == TileMap.TileType.NoWall)
                        {
                            sb.Draw(Game1.whitePixel, renderMapPosition + new Vector2(i, j) * mapScale, null, Color.Lerp(Color.Cyan, Color.LightCyan, (float)(Math.Sin(flickerTime / 1000f))), 0.0f, Vector2.Zero, new Vector2(mapResolution) * mapScale, SpriteEffects.None, 0.5f);
                        }
                    }
                }

                Vector2 playerPos = parent.CameraFocus.Position / parent.Map.SizeInPixels;
                sb.Draw(Game1.whitePixel, renderMapPosition + new Vector2(300) * playerPos, null, Color.Red, 0.0f, Vector2.Zero, new Vector2(mapResolution) * mapScale, SpriteEffects.None, 0.5f);
            }

            sb.Draw(Game1.whitePixel, Vector2.Zero, null, Color.Lerp(Color.Transparent, Color.Black, blackFadeOverlay), 0.0f, Vector2.Zero, new Vector2(GlobalGameConstants.GameResolutionWidth, GlobalGameConstants.GameResolutionHeight), SpriteEffects.None, 0.5f);

            if (parent.Player1Dead)
            {
                sb.DrawString(Game1.tenbyFive72, deathMessage, (new Vector2(GlobalGameConstants.GameResolutionWidth, GlobalGameConstants.GameResolutionHeight) - Game1.tenbyFive72.MeasureString(deathMessage)) / 2, Color.LightGray);
            }
        }
    }
}
