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

        private enum titleScreens
        {
            introScreen,
            logoScreen,
            menuScreen,
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
                        z_distance += z_text_speed * currentTime.ElapsedGameTime.Milliseconds;
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
                        z_distance -= z_text_speed * currentTime.ElapsedGameTime.Milliseconds;
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

        private Texture2D ship_texture;

        private Vector2 text_position = new Vector2(960, 420);
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
        private const float max_fade_timer = 2000.0f;
        private const float max_fade_menu_timer = 4000.0f;
        private float model_rotation_y = 0.0f;
        private float model_rotation_x = 0.0f;

        private int menu_item_selected = 0;

        private Model myModel;
        private float aspectRatio;

        private RasterizerState rasterizer_state = new RasterizerState();

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

        public TitleScreen(Model model, float aspectRatio, Texture2D texture)
        {
            menu_list = new List<TitleMenuOptions>(3);
            menu_list.Add(new TitleMenuOptions("START"));
            menu_list.Add(new TitleMenuOptions("OPTIONS"));
            menu_list.Add(new TitleMenuOptions("QUIT"));

            menu_item_selected = 0;

            screen = titleScreens.introScreen;

            fade_state = FadeState.stay;

            myModel = model;
            ship_texture = texture;
            this.aspectRatio = aspectRatio;
            rasterizer_state.CullMode = CullMode.None;
        }

        private const int width = 32;
        private const int height = 32;
        protected override void doUpdate(GameTime currentTime)
        {
            button_pressed_timer += currentTime.ElapsedGameTime.Milliseconds;
            fade += currentTime.ElapsedGameTime.Milliseconds;

            if (screen == titleScreens.menuScreen)
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
                    if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.AnyButton) && !confirm_pressed)
                    {
                        confirm_pressed = true;
                        fade = 0.0f;
                        fade_state = FadeState.fadeOut;
                    }
                    else if(confirm_pressed)
                    {
                        if(fade > max_fade_timer)
                        {
                            fade = 0.0f;
                            screen = titleScreens.logoScreen;
                            confirm_pressed = false;
                            fade_state = FadeState.fadeIn;
                        }
                    }
                    break;
                /**************************************************************************************************************************/
                case titleScreens.logoScreen:
                    if (fade > max_fade_timer)
                    {
                        fade = 0.0f;
                        fade_state = (FadeState)(((int)fade_state+1)%3);
                        Console.WriteLine(fade_state);
                        if (fade_state == FadeState.fadeIn)
                        {
                            fade = 0.0f;
                            screen = titleScreens.menuScreen;
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

                    if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection))
                    {
                        if(!down_pressed)
                            button_pressed_timer = 0.0f;
                
                        down_pressed = true;
                    }

                    if ((down_pressed && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection)) || (down_pressed && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection) && button_pressed_timer > max_button_pressed_timer))
                    {
                        down_pressed = false;
                        button_pressed_timer = 0.0f;

                        menu_item_selected++;
                        if (menu_item_selected >= menu_list.Count())
                        {
                            menu_item_selected = menu_item_selected % menu_list.Count();
                        }
                        else if (menu_item_selected < 0)
                        {
                            menu_item_selected += menu_list.Count();
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
                        up_pressed = false;
                        button_pressed_timer = 0.0f;

                        menu_item_selected--;
                        if (menu_item_selected >= menu_list.Count())
                        {
                            menu_item_selected = menu_item_selected % menu_list.Count();
                        }
                        else if (menu_item_selected < 0)
                        {
                            menu_item_selected += menu_list.Count();
                        }
                    }

                    if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.Confirm))
                    {
                        confirm_pressed = true;
                    }
                    else if (confirm_pressed)
                    {
                        confirm_pressed = false;

                        switch(menu_list[menu_item_selected].text)
                        {
                            case "START":
                                isComplete = true;
                                BackGroundAudio.stopAllSongs();
                                break;
                            case "OPTIONS":
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

                    model_rotation_x += (float)currentTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.01f);
                    model_rotation_y += (float)currentTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.002f);

                    break;
                /*****************************************************************************************************/
                default:
                    break;
            }
        }

        public override void render(SpriteBatch sb)
        {
            AnimationLib.GraphicsDevice.Clear(Color.Black);
            
            sb.Begin();

            //sb.Draw(Game1.whitePixel,new Vector2(3 * GlobalGameConstants.GameResolutionWidth / 4.0f, 3 * GlobalGameConstants.GameResolutionHeight / 4), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.5f
            switch (screen)
            {
                case titleScreens.introScreen:
                    sb.DrawString(Game1.font, "PattyPetitGiant", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, GlobalGameConstants.GameResolutionHeight / 2), fadeColour);
                    sb.DrawString(Game1.font, "Press Any Key to Continue", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, GlobalGameConstants.GameResolutionHeight / 2 + 32), fadeColour);
                    break;
                case titleScreens.logoScreen:
                    sb.Draw(Game1.frostTreeLogo, new Vector2(GlobalGameConstants.GameResolutionWidth / 2.0f, GlobalGameConstants.GameResolutionHeight / 2), null, fadeColour, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
                    break;
                case titleScreens.menuScreen:
                    sb.Draw(Game1.whitePixel, new Vector2(3 * GlobalGameConstants.GameResolutionWidth / 4.0f, 3 * GlobalGameConstants.GameResolutionHeight / 4), null, fadeColour, 0.0f, Vector2.Zero, 150.0f, SpriteEffects.None, 0.5f);

                    for (int i = 0; i < menu_list.Count(); i++)
                    {
                        sb.DrawString(Game1.font, menu_list[i].text, text_position + new Vector2((25 * menu_list[i].z_distance), 32 * i), fadeColour, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.5f);
                    }

                    /*Matrix[] transforms = new Matrix[myModel.Bones.Count];
                    myModel.CopyAbsoluteBoneTransformsTo(transforms);

                    AnimationLib.GraphicsDevice.BlendState = BlendState.Opaque;
                    AnimationLib.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    AnimationLib.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

                    foreach (ModelMesh mesh in myModel.Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            effect.EnableDefaultLighting();
                            effect.AmbientLightColor = new Vector3(0.2f,0.2f,0.2f);
                            effect.DirectionalLight0.Enabled = true;
                            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(100, 0, 0));
                            effect.DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0.0f, 0.0f);
                            effect.DirectionalLight1.Enabled = false;
                            effect.DirectionalLight2.Enabled = false;
                            effect.TextureEnabled = true;
                            effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateRotationY(model_rotation_y) * Matrix.CreateRotationX(model_rotation_x) * Matrix.CreateTranslation(model_position);
                            effect.View = Matrix.CreateLookAt(camera_Position, Vector3.Zero, Vector3.Up);
                            effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
                            //effect.Parameters["ColorMap"].SetValue(ship_texture);
                            //effect.Alpha = 1.0f;
                        
                            //effect.Texture = ship_texture;
                        }
                        mesh.Draw();
                    }*/
                    break;
                default:
                    break;
            }
            
            sb.End();
            if (screen == titleScreens.menuScreen)
            {
                
            }

        }

        public override ScreenStateType nextLevelState()
        {
            return ScreenStateType.LevelSelectState;
        }
    }
}
