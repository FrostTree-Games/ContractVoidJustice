using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

namespace PattyPetitGiant
{
    class OptionsMenu : ScreenState
    {

        private class optionsMenuOptions
        {
            public string text;
            public bool select;

            private const float min_z_distance = 0.0f;
            private const float max_z_distance = 1.0f;
            private const float z_text_speed = 0.1f;

            private float z_distance = 0.0f;

            public float zDistance
            {
                get { return z_distance; }
            }

            public optionsMenuOptions(string text)
            {
                this.text = text;
                select = false;
                z_distance = 0.0f;
            }

            public void update(GameTime currentTime)
            {
                if (select)
                {
                    if (z_distance < max_z_distance)
                    {
                        z_distance += z_text_speed * currentTime.ElapsedGameTime.Milliseconds;
                    }

                    if (z_distance > max_z_distance)
                    {
                        z_distance = max_z_distance;
                    }
                }
                else
                {
                    if (z_distance > min_z_distance)
                    {
                        z_distance -= z_text_speed * currentTime.ElapsedGameTime.Milliseconds;
                    }

                    if (z_distance < min_z_distance)
                    {
                        z_distance = min_z_distance;
                    }
                }
            }
        }

        public enum optionScreenState
        {
            back,
            viewHighScore,
            eraseHighScore,
        }

        private class popUpMenu
        {
            public string text;
            public bool select;

            private const float min_z_distance = 0.0f;
            private const float max_z_distance = 1.0f;
            private const float z_text_speed = 0.1f;

            private float z_distance = 0.0f;

            public float zDistance
            {
                get { return z_distance; }
            }

            public popUpMenu(string pop_up_text)
            {
                text = pop_up_text;
                select = false;
                z_distance = 0.0f;
            }

            public void update(GameTime currentTime)
            {
                if (select)
                {
                    if (z_distance < max_z_distance)
                    {
                        z_distance += z_text_speed * currentTime.ElapsedGameTime.Milliseconds;
                    }

                    if (z_distance > max_z_distance)
                    {
                        z_distance = max_z_distance;
                    }
                }
                else
                {
                    if (z_distance > min_z_distance)
                    {
                        z_distance -= z_text_speed * currentTime.ElapsedGameTime.Milliseconds;
                    }

                    if (z_distance < min_z_distance)
                    {
                        z_distance = min_z_distance;
                    }
                }
            }

        }

        private enum popUpZoomState
        {
            zoomIn = 0,
            zoomStay = 1,
            zoomOut = 2,
        }

        private popUpZoomState zoom_state;

        public float popUpZoom
        {
            get
            {
                return((zoom_state == popUpZoomState.zoomIn)?(float)(zoom/zoom_duration):(zoom_state == popUpZoomState.zoomOut)?(float)(1.0f - zoom/zoom_duration) : 1.0f);
            }
        }

        private List<optionsMenuOptions> options_list;
        private List<popUpMenu> popup_options;

        private string popup_message = "ARE YOU SURE YOU WANT TO ERASE YOUR HIGHSCORE?";

        private bool down_pressed;
        private bool up_pressed;
        private bool confirm_pressed;

        private float button_pressed_timer = 0.0f;
        private const float max_button_pressed_timer = 200.0f;
        
        private float zoom = 0.0f;
        private const float zoom_duration = 500.0f;

        private int menu_item_select = 0;
        private int popup_item_selected = 0;

        private bool pop_up_menu = false;
        private bool pop_up_screen = false;

        private Vector2 text_position = new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 128, GlobalGameConstants.GameResolutionHeight / 2 - 96);
        private Vector2 popup_position = new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 64, GlobalGameConstants.GameResolutionHeight / 2);

        public OptionsMenu()
        {
            options_list = new List<optionsMenuOptions>(3);
            options_list.Add(new optionsMenuOptions("HIGH SCORE"));
            options_list.Add(new optionsMenuOptions("ERASE HIGH SCORE"));
            options_list.Add(new optionsMenuOptions("CREDITS"));
            options_list.Add(new optionsMenuOptions("BACK"));

            popup_options = new List<popUpMenu>(2);
            popup_options.Add(new popUpMenu("YES"));
            popup_options.Add(new popUpMenu("NO"));
            zoom_state = popUpZoomState.zoomStay;
        }

        protected override void doUpdate(GameTime currentTime)
        {
            button_pressed_timer += currentTime.ElapsedGameTime.Milliseconds;
            zoom += currentTime.ElapsedGameTime.Milliseconds;

            switch(zoom_state)
            {
                case popUpZoomState.zoomIn:
                    if (zoom > zoom_duration)
                    {
                        pop_up_menu = true;
                        zoom_state = popUpZoomState.zoomStay;
                        zoom = 0.0f;
                    }
                    break;
                case popUpZoomState.zoomStay:
                    zoom = 0.0f;
                    break;
                case popUpZoomState.zoomOut:
                    if (zoom > zoom_duration)
                    {
                        zoom_state = popUpZoomState.zoomStay;
                        pop_up_menu = false;
                        pop_up_screen = false;
                        zoom = 0.0f;
                    }
                    break;
            }

            if (!pop_up_menu)
            {
                if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.DownDirection))
                {
                    if (!down_pressed)
                    {
                        button_pressed_timer = 0.0f;
                    }
                    down_pressed = true;
                }

                if ((down_pressed && !InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.DownDirection)) || (down_pressed && InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.DownDirection) && button_pressed_timer > max_button_pressed_timer))
                {
                    button_pressed_timer = 0.0f;
                    down_pressed = false;

                    menu_item_select++;

                    if (menu_item_select >= options_list.Count())
                    {
                        menu_item_select = menu_item_select % options_list.Count();
                    }
                    else if (menu_item_select < 0)
                    {
                        menu_item_select += options_list.Count();
                    }
                }

                if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.UpDirection))
                {
                    if (!up_pressed)
                        button_pressed_timer = 0.0f;

                    up_pressed = true;
                }

                if ((up_pressed && !InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.UpDirection)) || (up_pressed && InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.UpDirection) && button_pressed_timer > max_button_pressed_timer))
                {
                    button_pressed_timer = 0.0f;
                    up_pressed = false;

                    menu_item_select--;

                    if (menu_item_select > 0)
                    {
                        menu_item_select = menu_item_select % options_list.Count();
                    }
                    else if (menu_item_select < 0)
                    {
                        menu_item_select += options_list.Count();
                    }
                }

                if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.Confirm))
                {
                    confirm_pressed = true;
                }
                else if (confirm_pressed)
                {
                    confirm_pressed = false;

                    switch (options_list[menu_item_select].text)
                    {
                        case "HIGH SCORE":
                            isComplete = true;
                            break;
                        case "ERASE HIGH SCORE":
                            pop_up_screen = true;
                            popup_item_selected = 0;
                            zoom_state = popUpZoomState.zoomIn;
                            break;
                        case "CREDITS":
                            isComplete = true;
                            break;
                        case "BACK":
                            isComplete = true;
                            break;
                        default:
                            break;
                    }
                }

                for (int i = 0; i < options_list.Count(); i++)
                {
                    if (i == menu_item_select)
                        options_list[menu_item_select].select = true;
                    else
                        options_list[i].select = false;

                    options_list[i].update(currentTime);
                }
            }
            /*************************************************************************************************************/
            else
            {
                if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.LeftDirection))
                {
                    if (!down_pressed)
                    {
                        button_pressed_timer = 0.0f;
                    }
                    down_pressed = true;
                }

                if ((down_pressed && !InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.LeftDirection)) || (down_pressed && InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.LeftDirection) && button_pressed_timer > max_button_pressed_timer))
                {
                    button_pressed_timer = 0.0f;
                    down_pressed = false;

                    popup_item_selected++;

                    if (popup_item_selected >= popup_options.Count())
                    {
                        popup_item_selected = popup_item_selected % popup_options.Count();
                    }
                    else if (menu_item_select < 0)
                    {
                        popup_item_selected += popup_options.Count();
                    }
                }

                if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.RightDirection))
                {
                    if (!up_pressed)
                        button_pressed_timer = 0.0f;

                    up_pressed = true;
                }

                if ((up_pressed && !InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.RightDirection)) || (up_pressed && InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.RightDirection) && button_pressed_timer > max_button_pressed_timer))
                {
                    button_pressed_timer = 0.0f;
                    up_pressed = false;

                    popup_item_selected--;

                    if (popup_item_selected > 0)
                    {
                        popup_item_selected = popup_item_selected % popup_options.Count();
                    }
                    else if (popup_item_selected < 0)
                    {
                        popup_item_selected += popup_options.Count();
                    }
                }

                if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.Confirm))
                {
                    confirm_pressed = true;
                }
                else if (confirm_pressed)
                {
                    confirm_pressed = false;

                    switch (popup_options[popup_item_selected].text)
                    {
                        case "NO":
                            zoom_state = popUpZoomState.zoomOut;
                            break;
                        case "YES":
                            zoom_state = popUpZoomState.zoomOut;
                            break;
                        default:
                            break;
                    }
                }

                for (int i = 0; i < popup_options.Count(); i++)
                {
                    if (i == popup_item_selected)
                        popup_options[popup_item_selected].select = true;
                    else
                        popup_options[i].select = false;

                    popup_options[i].update(currentTime);
                }
            }
        }

        public override void render(SpriteBatch sb)
        {
            AnimationLib.GraphicsDevice.Clear(Color.Black);

            sb.Begin();

            for (int i = 0; i < options_list.Count(); i++)
            {
                sb.DrawString(Game1.tenbyFive24, options_list[i].text, text_position + new Vector2((25 * options_list[i].zDistance), 32 * i), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
            }
            if (pop_up_screen)
            {
                sb.Draw(Game1.popUpBackground, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, GlobalGameConstants.GameResolutionHeight / 2), null, Color.White, 0.0f, new Vector2(Game1.popUpBackground.Width / 2, Game1.popUpBackground.Height / 2), popUpZoom, SpriteEffects.None, 0.51f);
                sb.DrawString(Game1.tenbyFive14, popup_message, new Vector2(GlobalGameConstants.GameResolutionWidth / 2, GlobalGameConstants.GameResolutionHeight / 2), Color.White, 0.0f, Game1.tenbyFive14.MeasureString(popup_message)/2 + new Vector2(0f,96f), popUpZoom, SpriteEffects.None, 0.51f);
                
                for (int i = 0; i < popup_options.Count(); i++)
                {
                    sb.DrawString((popup_options[i].select) ? Game1.tenbyFive24 : Game1.font, popup_options[i].text, popup_position + new Vector2(128*i, 32), Color.White, 0.0f, (popup_options[i].select) ? Game1.tenbyFive24.MeasureString(popup_options[i].text)/2 : Game1.font.MeasureString(popup_options[i].text)/2, popUpZoom, SpriteEffects.None, 0.51f);
                }
            }
            sb.End();
        }

        public override ScreenStateType nextLevelState()
        {
            if (options_list[menu_item_select].text == "HIGH SCORE")
            {
                return ScreenStateType.HighScoresStateOptions;
            }
            if (options_list[menu_item_select].text == "CREDITS")
            {
                return ScreenStateType.CreditsOptionsState;
            }
            else
            {
                return ScreenStateType.TitleScreen;
            }
        }
    }
}
