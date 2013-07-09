using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace PattyPetitGiant
{
    class TitleScreen : ScreenState
    {

       // private List<TitleMenuOption> 

        private class TitleMenuOptions
        {
            public string text;
            public bool selected;

            private const float min_z_distance = 0.0f;
            private const float max_z_distance = 1.5f;
            private const float z_text_speed = 0.1f;
            public float z_distance = 0.0f;

            public TitleMenuOptions(string menu_text)
            {
                text = menu_text;
                selected = false;
                z_distance = 0.0f;
            }

            public void update(GameTime currentTime)
            {
                if (selected)
                {
                    if (z_distance < max_z_distance)
                    {
                        z_distance += z_text_speed;
                    }

                    if (z_distance > max_z_distance)
                    {
                        z_distance = max_z_distance;
                    }
                }
                else
                {
                    if(z_distance > min_z_distance)
                    {
                        z_distance -= z_text_speed;
                    }

                    if (z_distance < min_z_distance)
                    {
                        z_distance = min_z_distance;
                    }
                }
            }
        }

        private List<TitleMenuOptions> menu_list;
        private Vector2 text_position = new Vector2(960, 420);

        private int menu_item_selected = 0;

        public TitleScreen()
        {
            menu_list = new List<TitleMenuOptions>(3);
            menu_list.Add(new TitleMenuOptions("START"));
            menu_list.Add(new TitleMenuOptions("OPTIONS"));
            menu_list.Add(new TitleMenuOptions("QUIT"));

            menu_item_selected = 0;
        }

        private const int height = 32;
        private const int width = 32;
        protected override void doUpdate(GameTime currentTime)
        {
            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection))
            {
                if (menu_item_selected >= menu_list.Count())
                {
                    menu_item_selected = menu_item_selected % menu_list.Count();
                }
                else if (menu_item_selected < 0)
                {
                    menu_item_selected += menu_list.Count();
                }
                else
                {
                    menu_item_selected++;
                }
            }

            for (int i = 0; i < menu_list.Count(); i++)
            {
                if (i == menu_item_selected)
                {
                    menu_list[menu_item_selected].selected = true;
                }
                else 
                {
                    menu_list[i].selected = false;
                }
            }
        }

        public override void render(SpriteBatch sb)
        {
            sb.Begin();

            //sb.Draw(Game1.whitePixel,new Vector2(3 * GlobalGameConstants.GameResolutionWidth / 4.0f, 3 * GlobalGameConstants.GameResolutionHeight / 4), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.5f);
            sb.Draw(Game1.whitePixel, new Vector2(3 * GlobalGameConstants.GameResolutionWidth / 4.0f, 3 * GlobalGameConstants.GameResolutionHeight / 4), null, Color.White, 0.0f, Vector2.Zero, 150.0f, SpriteEffects.None, 0.5f);

            for (int i = 0; i < menu_list.Count(); i++)
            {
                sb.DrawString(Game1.font, menu_list[i].text, text_position + new Vector2((25*menu_list[i].z_distance), 32*i), Color.MistyRose, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.5f);
            }
            sb.End();
        }

        public override ScreenStateType nextLevelState()
        {
            throw new NotImplementedException();
        }
    }
}
