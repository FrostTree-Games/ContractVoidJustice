using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace PattyPetitGiant
{
    class TitleScreen : ScreenState
    {
        public enum titleScreens
        {
            introScreen,
            logoScreen,
            menuScreen,
            playScreen,
            optionScreen
        }

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
                        z_distance += z_text_speed * currentTime.ElapsedGameTime.Milliseconds / 7f; //is this a copy-paste job?
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
                        z_distance -= z_text_speed * currentTime.ElapsedGameTime.Milliseconds / 7f;
                    }

                    if (z_distance < min_z_distance)
                    {
                        z_distance = min_z_distance;
                    }
                }
            }
        }

        private List<TitleMenuOptions> menu_list;
        private titleScreens screen;

        private Vector2 text_position = new Vector2(GlobalGameConstants.GameResolutionWidth/2 - 48, 514);
        private Vector3 model_position = Vector3.Zero;
        private Vector3 camera_Position = new Vector3(0.0f, 0, 5000.0f);

        private bool down_pressed = false;
        private bool up_pressed = false;
        private bool confirm_pressed = false;
        private bool music_playing = false;

        private float button_pressed_timer = 0.0f;
        private const float max_button_pressed_timer = 200.0f;
        private float fade = 0.0f;
        private float fade_duration = 0.0f;

        private const float max_fade_timer = 1500f;
        private const float logo_stay_timer = 4000f;
        private const float max_fade_menu_timer = 1000f;

        private Texture2D videoTexture;

        private int menu_item_selected = 0;

        private RasterizerState rasterizer_state = new RasterizerState();

        private const string menuBlipSound = "menuSelect";

        private bool storageDevicePrompted;

        /// <summary>
        /// Used to determine which device pressed the confirm button on the title screen.
        /// </summary>
        private InputDevice2.PlayerPad whoPressedConfirm;

        private enum FadeState
        {
            fadeIn = 0,
            stay = 1,
            fadeOut = 2,
            
        }
        private FadeState fade_state;

        private Color fadeColour
        {
            get
            {
                return (fade_state == FadeState.fadeOut)? Color.Lerp(Color.White, Color.Black, (float)(fade/fade_duration)) : (fade_state == FadeState.fadeIn)? Color.Lerp(Color.White, Color.Black,1.0f - (float)(fade/fade_duration)) : Color.White;
            }
        }
        private Color fadeTextColour
        {
            get
            {
                return (fade_state == FadeState.fadeOut) ? Color.Lerp(Color.LightSkyBlue, Color.Black, (float)(fade / fade_duration)) : (fade_state == FadeState.fadeIn) ? Color.Lerp(Color.LightSkyBlue, Color.Black, 1.0f - (float)(fade / fade_duration)) : Color.LightSkyBlue;
            }
        }

        //public TitleScreen(Model model, float aspectRatio, Texture2D texture)
        public TitleScreen(titleScreens screen_state)
        {
            menu_list = new List<TitleMenuOptions>(3);
            menu_list.Add(new TitleMenuOptions("START"));
            menu_list.Add(new TitleMenuOptions("OPTIONS"));
            menu_list.Add(new TitleMenuOptions("QUIT"));

            menu_item_selected = 0;

            screen = screen_state;

            fade_state = FadeState.fadeIn;

            storageDevicePrompted = false;

            InputDevice2.UnlockAllControllers();
        }

        private const int width = 32;
        private const int height = 32;
        protected override void doUpdate(GameTime currentTime)
        {
            button_pressed_timer += currentTime.ElapsedGameTime.Milliseconds;
            fade += currentTime.ElapsedGameTime.Milliseconds;

            if (screen == titleScreens.menuScreen || screen == titleScreens.playScreen)
            {
                fade_duration = max_fade_menu_timer;
            }
            else
            {
                fade_duration = max_fade_timer;
            }

            switch(screen)
            {
                case titleScreens.introScreen:
                    if (fade > max_fade_timer)
                    {
                        fade = 0.0f;
                        fade_state = (FadeState)(((int)fade_state + 1) % 3);
                        if (fade_state == FadeState.stay)
                        {
                            fade = 0.0f;
                            screen = titleScreens.menuScreen;
                        }
                    }
                    break;
                /**************************************************************************************************************************/
                case titleScreens.logoScreen:
                    if (fade > fade_duration)
                    {
                        fade = 0.0f;
                        fade_state = (FadeState)(((int)fade_state+1)%3);

                        if (fade_state == FadeState.fadeIn)
                        {
                            fade = 0.0f;
                            screen = titleScreens.menuScreen;
                        }
                        else if (fade_state == FadeState.stay)
                        {
                            fade = 0.0f;
                            fade_duration = logo_stay_timer;
                        }
                        else if (fade_state == FadeState.fadeOut)
                        {
                            fade = 0.0f;
                            fade_duration = max_fade_timer;
                        }
                    }
                    break;
                /**************************************************************************************************************************/
                case titleScreens.menuScreen:
                    if (!music_playing)
                    {
                        BackGroundAudio.playSong("Menu", true);
                        music_playing = true;
                    }

                    if (Game1.videoPlayer.State == Microsoft.Xna.Framework.Media.MediaState.Stopped)
                    {
                        Game1.videoPlayer.IsLooped = true;
                        Game1.videoPlayer.Play(Game1.titleScreenVideo);
                    }


                    if (fade > max_fade_menu_timer)
                    {
                        fade = 0.0f;
                        fade_state = FadeState.stay;
                        if (fade_state == FadeState.fadeIn)
                        {
                            fade = 0.0f;
                            screen = titleScreens.menuScreen;
                        }
                    }

                    if (storageDevicePrompted)
                    {
                        if (InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.DownDirection) != InputDevice2.PlayerPad.NoPad)
                        {
                            if (!down_pressed)
                                button_pressed_timer = 0.0f;

                            down_pressed = true;
                        }

                        if ((down_pressed && !(InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.DownDirection) != InputDevice2.PlayerPad.NoPad)) || (down_pressed && (InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.DownDirection) != InputDevice2.PlayerPad.NoPad) && button_pressed_timer > max_button_pressed_timer))
                        {
                            down_pressed = false;
                            button_pressed_timer = 0.0f;

                            menu_item_selected++;
                            AudioLib.playSoundEffect(menuBlipSound);
                            if (menu_item_selected >= menu_list.Count())
                            {
                                menu_item_selected = menu_item_selected % menu_list.Count();
                            }
                            else if (menu_item_selected < 0)
                            {
                                menu_item_selected += menu_list.Count();
                            }
                        }

                        if (InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.UpDirection) != InputDevice2.PlayerPad.NoPad)
                        {
                            if (!up_pressed)
                                button_pressed_timer = 0.0f;
                            up_pressed = true;
                        }

                        if ((up_pressed && !(InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.UpDirection) != InputDevice2.PlayerPad.NoPad)) || (up_pressed && (InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.UpDirection) != InputDevice2.PlayerPad.NoPad) && button_pressed_timer > max_button_pressed_timer))
                        {
                            up_pressed = false;
                            button_pressed_timer = 0.0f;

                            menu_item_selected--;
                            AudioLib.playSoundEffect(menuBlipSound);
                            if (menu_item_selected >= menu_list.Count())
                            {
                                menu_item_selected = menu_item_selected % menu_list.Count();
                            }
                            else if (menu_item_selected < 0)
                            {
                                menu_item_selected += menu_list.Count();
                            }
                        }

                        if (InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.Confirm) != InputDevice2.PlayerPad.NoPad)
                        {
                            confirm_pressed = true;

                            whoPressedConfirm = InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.Confirm);
                        }
                        else if (confirm_pressed)
                        {
                            confirm_pressed = false;

                            switch (menu_list[menu_item_selected].text)
                            {
                                case "START":
                                    screen = titleScreens.playScreen;
                                    fade_state = FadeState.fadeOut;
                                    fade = 0.0f;
                                    break;
                                case "OPTIONS":
                                    InputDevice2.LockController(InputDevice2.PPG_Player.Player_1, whoPressedConfirm);
                                    screen = titleScreens.optionScreen;
                                    fade_state = FadeState.fadeOut;
                                    fade = 0.0f;
                                    break;
                                case "QUIT":
                                    break;
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
                            menu_list[i].update(currentTime);
                        }
                    }
                    else
                    {
                        if (InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.Confirm) != InputDevice2.PlayerPad.NoPad)
                        {
                            confirm_pressed = true;

                            whoPressedConfirm = InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.Confirm);
                        }
                        else if (confirm_pressed)
                        {
                            confirm_pressed = false;

                            SaveGameModule.selectStorageDevice((PlayerIndex)whoPressedConfirm);
                            SaveGameModule.loadGame();

                            storageDevicePrompted = true;
                        }
                    }

                    break;
                /*****************************************************************************************************/
                default:
                    if (fade > max_fade_menu_timer)
                    {
                        isComplete = true;
                        BackGroundAudio.stopAllSongs();
                        Game1.videoPlayer.Stop();
                    }
                    break;
            }
        }

        public override void render(SpriteBatch sb)
        {
            AnimationLib.GraphicsDevice.Clear(Color.Black);
            
            sb.Begin();

            Console.WriteLine("Rendering Screen");

            if (screen == titleScreens.menuScreen || screen == titleScreens.playScreen || screen == titleScreens.optionScreen)
            {
                if (Game1.videoPlayer.State != Microsoft.Xna.Framework.Media.MediaState.Stopped)
                {
                    videoTexture = Game1.videoPlayer.GetTexture();
                }

                if (videoTexture != null)
                {
                    sb.Draw(videoTexture, new Rectangle(0, 0, GlobalGameConstants.GameResolutionWidth, GlobalGameConstants.GameResolutionHeight), fadeColour);
                }
            }
            
            switch (screen)
            {
                case titleScreens.introScreen:
                    sb.Draw(Game1.frostTreeLogo, new Vector2((GlobalGameConstants.GameResolutionWidth / 2) - (Game1.frostTreeLogo.Width / 2), (GlobalGameConstants.GameResolutionHeight / 2) - (Game1.frostTreeLogo.Height / 2)), null, fadeColour, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                    break;
                case titleScreens.logoScreen:
                    sb.Draw(Game1.frostTreeLogo, new Vector2((GlobalGameConstants.GameResolutionWidth / 2) - (Game1.frostTreeLogo.Width / 2), (GlobalGameConstants.GameResolutionHeight / 2) - (Game1.frostTreeLogo.Height / 2)), null, fadeColour, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                    break;
                case titleScreens.menuScreen:
                    if (storageDevicePrompted)
                    {
                        for (int i = 0; i < menu_list.Count(); i++)
                        {
                            sb.DrawString(Game1.tenbyFive24, menu_list[i].text, text_position + new Vector2((25 * menu_list[i].z_distance), 32 * i), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                        }
                    }
                    else
                    {
                        sb.DrawString(Game1.tenbyFive24, "Press (A)", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 514) - (Game1.tenbyFive24.MeasureString("Press (A)") / 2), fadeTextColour);
                    }
                    break;
                default:
                    for (int i = 0; i < menu_list.Count(); i++)
                    {
                        sb.DrawString(Game1.font, menu_list[i].text, text_position + new Vector2((25 * menu_list[i].z_distance), 32 * i), fadeTextColour, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.5f);
                    }
                    break;
            }
            
            sb.End();
            if (screen == titleScreens.menuScreen)
            {
                
            }

        }

        public override ScreenStateType nextLevelState()
        {
            if (screen == titleScreens.playScreen)
            {
                return ScreenStateType.GameSetupMenu;
            }
            else
            {
                return ScreenStateType.OptionsMenu;
            }
        }
    }
}
