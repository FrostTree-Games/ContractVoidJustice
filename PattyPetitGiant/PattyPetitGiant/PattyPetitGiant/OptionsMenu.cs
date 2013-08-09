using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class OptionsMenu : ScreenState
    {

        private class optionsMenuOptions
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

        private List<optionsMenuOptions> options_list;

        private bool down_pressed;
        private bool up_pressed;
        private bool confirm_pressed;

        private float button_pressed_timer = 0.0f;
        private const float max_button_pressed_timer = 200.0f;
        
        private int menu_item_select = 0;

        private Vector2 text_position = new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - 128, GlobalGameConstants.GameResolutionHeight / 2 - 128);

        public OptionsMenu()
        {
            options_list = new List<optionsMenuOptions>(3);
            options_list.Add(new optionsMenuOptions("HIGH SCORE"));
            options_list.Add(new optionsMenuOptions("ERASE HIGH SCORE"));
            options_list.Add(new optionsMenuOptions("BACK"));
        }

        protected override void doUpdate(GameTime currentTime)
        {
            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection))
            {
                if (!down_pressed)
                {
                    button_pressed_timer = 0.0f;
                }
                down_pressed = true;
            }

            if((down_pressed && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection)) || (down_pressed && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection) && button_pressed_timer > max_button_pressed_timer))
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

            if(InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection))
            {
                if(!up_pressed)
                    button_pressed_timer = 0.0f;

                up_pressed = true;
            }

            if((up_pressed && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection)) || (up_pressed && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection) && button_pressed_timer > max_button_pressed_timer))
            {
                button_pressed_timer = 0.0f;
                up_pressed = false;

                menu_item_select--;

                if( menu_item_select >- 0)
                {
                    menu_item_select = menu_item_select % options_list.Count();
                }
                else if(menu_item_select < 0)
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
                        break;
                    case "BACK":
                        isComplete = true;
                        break;
                    default:
                        break;
                }
            }

            for (int i = 0; i<options_list.Count(); i++)
            {
                if (i == menu_item_select)
                    options_list[menu_item_select].select = true;
                else
                    options_list[i].select = false;

                options_list[i].update(currentTime);
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
