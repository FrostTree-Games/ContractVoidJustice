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

        public OptionsMenu()
        {
            options_list = new List<optionsMenuOptions>(3);
            options_list.Add(new optionsMenuOptions("High Score"));
            options_list.Add(new optionsMenuOptions("Erase High Score"));
            options_list.Add(new optionsMenuOptions("Back"));
        }

        public override void doUpdate(GameTime currentTime)
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

            if(InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection)
            {
                if(!up_pressed)
                    button_pressed_timer = 0.0f;

                up_pressed = true;
            }

            if((up_pressed && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection)) || (up_pressed && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection) && button_pressed_timer > max_button_pressed_timer))
            {
                button_pressed_timer = 0.0f;
                down_pressed = false;

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
        }

        public override void render(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }

        public override ScreenStateType nextLevelState()
        {
            throw new NotImplementedException();
        }
    }
}
