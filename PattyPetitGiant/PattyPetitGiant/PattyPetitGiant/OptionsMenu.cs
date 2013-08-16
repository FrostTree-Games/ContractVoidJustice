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
            private const float max_z_distance = 1.5f;
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

        private List<optionsMenuOptions> options_list;
        private List<popUpMenu> popup_options;

        private bool down_pressed;
        private bool up_pressed;
        private bool confirm_pressed;

        private float button_pressed_timer = 0.0f;
        private const float max_button_pressed_timer = 200.0f;
        
        private int menu_item_select = 0;
        private int popup_item_selected = 0;

        private bool pop_up_menu = false;

        private Vector2 text_position = new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 128, GlobalGameConstants.GameResolutionHeight / 2 - 128);
        private Vector2 popup_position = new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 64, GlobalGameConstants.GameResolutionHeight / 2 - 96);

        public OptionsMenu()
        {
            options_list = new List<optionsMenuOptions>(3);
            options_list.Add(new optionsMenuOptions("HIGH SCORE"));
            options_list.Add(new optionsMenuOptions("ERASE HIGH SCORE"));
            options_list.Add(new optionsMenuOptions("BACK"));

            popup_options = new List<popUpMenu>(2);
            popup_options.Add(new popUpMenu("YES"));
            popup_options.Add(new popUpMenu("NO"));
        }

        protected override void doUpdate(GameTime currentTime)
        {
            button_pressed_timer += currentTime.ElapsedGameTime.Milliseconds;
            if (!pop_up_menu)
            {
                if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection))
                {
                    if (!down_pressed)
                    {
                        button_pressed_timer = 0.0f;
                    }
                    down_pressed = true;
                }

                if ((down_pressed && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection)) || (down_pressed && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection) && button_pressed_timer > max_button_pressed_timer))
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

                if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection))
                {
                    if (!up_pressed)
                        button_pressed_timer = 0.0f;

                    up_pressed = true;
                }

                if ((up_pressed && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection)) || (up_pressed && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection) && button_pressed_timer > max_button_pressed_timer))
                {
                    button_pressed_timer = 0.0f;
                    up_pressed = false;

                    menu_item_select--;

                    if (menu_item_select > -0)
                    {
                        menu_item_select = menu_item_select % options_list.Count();
                    }
                    else if (menu_item_select < 0)
                    {
                        menu_item_select += options_list.Count();
                    }
                }

                if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm))
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
                            pop_up_menu = true;
                            popup_item_selected = 0;
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
                if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.LeftDirection))
                {
                    if (!down_pressed)
                    {
                        button_pressed_timer = 0.0f;
                    }
                    down_pressed = true;
                }

                if ((down_pressed && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.LeftDirection)) || (down_pressed && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.LeftDirection) && button_pressed_timer > max_button_pressed_timer))
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

                if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.RightDirection))
                {
                    if (!up_pressed)
                        button_pressed_timer = 0.0f;

                    up_pressed = true;
                }

                if ((up_pressed && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.RightDirection)) || (up_pressed && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.RightDirection) && button_pressed_timer > max_button_pressed_timer))
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

                if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm))
                {
                    confirm_pressed = true;
                }
                else if (confirm_pressed)
                {
                    confirm_pressed = false;

                    switch (popup_options[popup_item_selected].text)
                    {
                        case "NO":
                            pop_up_menu = false;
                            break;
                        case "YES":
                            pop_up_menu = false;
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
                sb.DrawString(Game1.font, options_list[i].text, text_position + new Vector2((25 * options_list[i].zDistance), 32 * i), Color.White, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.5f);
            }
            if (pop_up_menu)
            {
                sb.Draw(Game1.popUpBackground, new Vector2(GlobalGameConstants.GameResolutionWidth * 0.30f, GlobalGameConstants.GameResolutionHeight * 0.2f), new Rectangle(0,0, 640, 360), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.51f);
                sb.DrawString(Game1.font, "ARE YOU SURE YOU WANT TO ERASE YOUR HIGHSCORE?", text_position - new Vector2(96, 0), Color.White, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.51f);
                for (int i = 0; i < popup_options.Count(); i++)
                {
                    sb.DrawString(Game1.font, popup_options[i].text, popup_position + new Vector2(128 * i, 32), Color.White, 0.0f, Vector2.Zero, popup_options[i].zDistance + 1.0f, SpriteEffects.None, 0.51f);
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
            else
            {
                return ScreenStateType.TitleScreen;
            }
        }
    }
}
