//#define PROFILE
//#define CONTROLLER_DATA

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PattyPetitGiant
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static SpriteFont font;
        public static SpriteFont testComputerFont;
        public static SpriteFont tenbyFive8;
        public static SpriteFont tenbyFive10;
        public static SpriteFont tenbyFive14;
        public static SpriteFont tenbyFive24;
        public static SpriteFont tenbyFive72;

        private static Effect bloomFilter = null;
        public static Effect BloomFilter { get { return bloomFilter; } }
        public static Texture2D whitePixel = null;
        public static Texture2D frostTreeLogo = null;
        public static Texture2D testArrow = null;
        public static Texture2D backGroundPic = null;
        public static Texture2D heartPic = null;
        public static Texture2D laserPic = null;
        public static Texture2D healthBar = null;
        public static Texture2D healthColor = null;
        public static Texture2D energyColor = null;
        public static Texture2D energyOverlay = null;
        public static Texture2D popUpBackground = null;
        public static Texture2D greyBar = null;
        public static Texture2D creditImage = null;
        public static Texture2D guardIcon = null;
        public static Texture2D prisonerIcon = null;
        public static Texture2D p2Icon = null;
        public static Random rand = new Random();

        private Texture2D pleaseWaitDialog = null;
        private bool preloadedAssets = false;
        private bool preloadedJSon = false;

        private static bool gameIsRunningSlowly;
        public static bool GameIsRunningSlowly { get { return gameIsRunningSlowly; } }

        public static float aspectRatio;
        private ScreenState currentGameScreen = null;

        public static VideoPlayer videoPlayer = null;
        public static Video levelExitVideo = null;
        public static Video levelEnterVideo = null;
        public static Video levelExitVideoCoop = null;
        public static Video levelEnterVideoCoop = null;
        public static Video titleScreenVideo = null;
        public static Video introCutScene = null;
        public static Video introCutSceneCoop = null;
        public static Video guardEndCutScene = null;
        public static Video alienEndCutScene = null;
        public static Video prisonerEndCutScene = null;
        public static Video guardEndCutSceneCoop = null;
        public static Video alienEndCutSceneCoop = null;
        public static Video prisonerEndCutSceneCoop = null;

        private static Texture2D saveIcon = null;

        private SpriteFont debugFont = null;
        private Texture2D asteroidsSpriteSheet = null;
        private Vector2 shipPosition = Vector2.Zero;
        private Vector2 shipVelocity = Vector2.Zero;
        private const float shipThrust = 0.007f;
        private float shipRotation = 0.0f;
        private const int starCount = 200;
        private Vector2[] stars = new Vector2[starCount];
        private float[] starRotation = new float[starCount];

        public static bool exitGame;

#if PROFILE
        private int frameCounter = 0;
        private int frameRate = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
#endif

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = GlobalGameConstants.GameResolutionWidth;
            graphics.PreferredBackBufferHeight = GlobalGameConstants.GameResolutionHeight;

#if PROFILE
            //this.IsFixedTimeStep = false;
            //graphics.SynchronizeWithVerticalRetrace = false;
            //graphics.ApplyChanges();
#endif 
            // for onscreen keyboard and profiles
            GamerServicesComponent GSC = new GamerServicesComponent(this);
            Components.Add(GSC);

            exitGame = false;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.

            InputDevice2.Initalize();

            HighScoresState.InitalizeHighScores();
            
            //replace this with a join screen later
#if XBOX
            InputDevice2.LockController(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerPad.GamePad1);
#elif WINDOWS
            InputDevice2.LockController(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerPad.Keyboard);
#endif

            spriteBatch = new SpriteBatch(GraphicsDevice);

            AnimationLib al = new AnimationLib(GraphicsDevice, spriteBatch);
            AnimationLib.cacheAtlasFiles();

            //some of these assets are critical to render anything to the screen, and thus are not stored in a seperate thread
            saveIcon = Content.Load<Texture2D>("gfx/saveIcon");
            debugFont = Content.Load<SpriteFont>("testFont");
            whitePixel = Content.Load<Texture2D>("whitePixel");
            pleaseWaitDialog = Content.Load<Texture2D>("pleaseWait");
            asteroidsSpriteSheet = Content.Load<Texture2D>("ppg_asteroids");

            for (int i = 0; i < starCount; i++)
            {
                stars[i] = new Vector2(rand.Next() % 2000 - 1000, rand.Next() % 2000 - 1000);
                starRotation[i] = (float)(rand.NextDouble() % (Math.PI * 2));
            }

            new Thread(loadSpine2).Start();
            new Thread(loadContent2).Start();
        }

        private void loadSpine2()
        {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(5);
#endif

            AnimationLib.cacheSpineJSON();

            preloadedJSon = true;
        }

        private void loadContent2()
        {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(3);
#endif
            backGroundPic = Content.Load<Texture2D>("titleScreenPic");
            frostTreeLogo = Content.Load<Texture2D>("FrostTreeLogo");
            testArrow = Content.Load<Texture2D>("gfx/testArrow");
            laserPic = Content.Load<Texture2D>("gfx/laser");
            healthBar = Content.Load<Texture2D>("bg");
            healthColor = Content.Load<Texture2D>("healthTexture");
            energyColor = Content.Load<Texture2D>("ammoTexture");
            energyOverlay = Content.Load<Texture2D>("overlay");
            popUpBackground = Content.Load<Texture2D>("popUpBackground");
            greyBar = Content.Load<Texture2D>("grayTexture");
            creditImage = Content.Load<Texture2D>("EndCredits");
            heartPic = Content.Load<Texture2D>("heartSheet");
            guardIcon = Content.Load<Texture2D>("guard");
            prisonerIcon = Content.Load<Texture2D>("prisoner");
            p2Icon = Content.Load<Texture2D>("P2Icon");

            TextureLib ts = new TextureLib(GraphicsDevice);
            TextureLib.loadFromManifest();

            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            bloomFilter = Content.Load<Effect>("BloomShader");

            BackGroundAudio music = new BackGroundAudio(Content);

            music.addSong("Menu");
            music.addSong("RPG Game");
            AudioLib lb = new AudioLib();

            tenbyFive8 = Content.Load<SpriteFont>("tenbyFive/tenbyFive8");
            tenbyFive10 = Content.Load<SpriteFont>("tenbyFive/tenbyFive10");
            tenbyFive14 = Content.Load<SpriteFont>("tenbyFive/tenbyFive14");
            tenbyFive24 = Content.Load<SpriteFont>("tenbyFive/tenbyFive24");
            tenbyFive72 = Content.Load<SpriteFont>("tenbyFive/tenbyFive72");
            font = tenbyFive14;
            testComputerFont = tenbyFive24;

            levelExitVideo = Content.Load<Video>("fmv/elevatorExit");
            levelEnterVideo = Content.Load<Video>("fmv/levelStart");
            titleScreenVideo = Content.Load<Video>("fmv/menu");
            introCutScene = Content.Load<Video>("fmv/intro");
            introCutSceneCoop = Content.Load<Video>("fmv/intro COOP");
            levelEnterVideoCoop = Content.Load<Video>("fmv/levelStartCOOP");
            levelExitVideoCoop = Content.Load<Video>("fmv/elevatorExitCOOP");
            guardEndCutScene = Content.Load<Video>("fmv/endGuardSingle");
            alienEndCutScene = Content.Load<Video>("fmv/endAlienSingle");
            prisonerEndCutScene = Content.Load<Video>("fmv/endPrisonerSingle");
            guardEndCutSceneCoop = Content.Load<Video>("fmv/endGuardCoop");
            alienEndCutSceneCoop = Content.Load<Video>("fmv/endAlienCoop");
            prisonerEndCutSceneCoop = Content.Load<Video>("fmv/endPrisonerCoop");
            videoPlayer = new VideoPlayer();

            ChunkLib cs = new ChunkLib();

            AnimationLib.loadFrameFromManifest();

            GlobalGameConstants.WeaponDictionary.InitalizePriceData();

            GameCampaign.InitalizeCampaignData();
            GameCampaign.ResetPlayerValues("INIT", 0);

            // lol so many game screens
            currentGameScreen = new TitleScreen(TitleScreen.titleScreens.logoScreen);
            //currentGameScreen = new CutsceneVideoState(testVideo, ScreenState.ScreenStateType.LevelReviewState);
            //currentGameScreen = new CampaignLobbyState();
            //currentGameScreen = new HighScoresState(true);

            // DANIEL UNCOMMMENT THIS LINE BEFORE YOU MERGE WITH MASTER
            preloadedAssets = true;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
#if PROFILE
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromMilliseconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            if (GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed)
            {
                Thread.Sleep(5);
            }

            if (GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed)
            {
                return;
            }
#endif

#if WINDOWS
#if DEBUG
            // Allows the game to exit; win32 only
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
#endif
#endif

            if(exitGame)
                this.Exit();

            if (!(preloadedAssets && preloadedJSon))
            {
                InputDevice2.Update(gameTime);

                if (InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.RightDirection) != InputDevice2.PlayerPad.NoPad)
                {
                    shipRotation += 0.02f;
                }
                else if (InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.LeftDirection) != InputDevice2.PlayerPad.NoPad)
                {
                    shipRotation -= 0.02f;
                }

                if (InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.Confirm) != InputDevice2.PlayerPad.NoPad)
                {
                    shipVelocity += new Vector2((float)(Math.Cos(shipRotation)), (float)(Math.Sin(shipRotation))) * shipThrust;
                    shipVelocity = Vector2.Clamp(shipVelocity, new Vector2(-0.91f), new Vector2(0.91f));
                }

                shipPosition += shipVelocity * gameTime.ElapsedGameTime.Milliseconds;
                shipVelocity *= 0.99f;

                if (shipPosition.X < -1850) { shipPosition.X = 1850; }
                if (shipPosition.X > 1850) { shipPosition.X = -1850; }
                if (shipPosition.Y < -1300) { shipPosition.Y = 1300; }
                if (shipPosition.Y > 1300) { shipPosition.Y = -1300; }

                return;
            }

            gameIsRunningSlowly = gameTime.IsRunningSlowly;

            if (currentGameScreen.IsComplete)
            {
                currentGameScreen = ScreenState.SwitchToNewScreen(currentGameScreen.nextLevelState());
            }

            InputDevice2.Update(gameTime);

            currentGameScreen.update(gameTime);

            base.Update(gameTime);
        }

        private void drawLine(SpriteBatch sb, Vector2 origin, float length, float rotation, Color color, float width)
        {
            sb.Draw(Game1.whitePixel, origin, null, color, rotation, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0.5f);
        }

        private void drawBox(SpriteBatch sb, Rectangle rect, Color clr, float lineWidth)
        {
            drawLine(sb, new Vector2(rect.X, rect.Y), rect.Width, 0.0f, clr, lineWidth);
            drawLine(sb, new Vector2(rect.X, rect.Y), rect.Height, (float)(Math.PI / 2), clr, lineWidth);
            drawLine(sb, new Vector2(rect.X - lineWidth, rect.Y + rect.Height), rect.Width + lineWidth, 0.0f, clr, lineWidth);
            drawLine(sb, new Vector2(rect.X + rect.Width, rect.Y), rect.Height, (float)(Math.PI / 2), clr, lineWidth);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!(preloadedAssets && preloadedJSon))
            {
                GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                for (int i = 0; i < starCount; i++)
                {
                    if (stars[i].X - shipPosition.X < -425 || stars[i].X - shipPosition.X > 425 || stars[i].Y - shipPosition.Y < -150 || stars[i].Y - shipPosition.Y > 150)
                    {
                        continue;
                    }

                    spriteBatch.Draw(asteroidsSpriteSheet, new Rectangle((GlobalGameConstants.GameResolutionWidth / 2 - 425) + 425 + (int)(stars[i].X - shipPosition.X), (GlobalGameConstants.GameResolutionHeight / 3 - 100) + 150 + (int)(stars[i].Y - shipPosition.Y), 20, 16), new Rectangle(555, 172, 30, 28), Color.White, starRotation[i], new Vector2(15, 14), SpriteEffects.None, 0.0f);
                }

                spriteBatch.Draw(asteroidsSpriteSheet, new Rectangle((GlobalGameConstants.GameResolutionWidth / 2 - 425) + 425, (GlobalGameConstants.GameResolutionHeight / 3 - 100) + 150, 30, 30), new Rectangle(0, 172, 120, 120), Color.White, shipRotation + (float)(Math.PI / 2), new Vector2(60), SpriteEffects.None, 0.0f);
                drawBox(spriteBatch, new Rectangle(GlobalGameConstants.GameResolutionWidth / 2 - 425, GlobalGameConstants.GameResolutionHeight / 3 - 100, 850, 300), Color.White, 2.0f);

                spriteBatch.Draw(pleaseWaitDialog, (new Vector2(GlobalGameConstants.GameResolutionWidth / 2, GlobalGameConstants.GameResolutionHeight * 0.75f) - new Vector2(pleaseWaitDialog.Width / 2, pleaseWaitDialog.Height / 2)) , Color.White);

                spriteBatch.End();
                
                return;
            }

            currentGameScreen.render(spriteBatch);

            if (SaveGameModule.TouchingStorageDevice)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(saveIcon, new Rectangle(1100, 588, 54, 59), new Rectangle(3, 28, 54, 59), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.5f);
                spriteBatch.End();
            }

#if CONTROLLER_DATA
            spriteBatch.Begin();

            try
            {
                spriteBatch.DrawString(Game1.tenbyFive14, "Player1: " + InputDevice2.GetPlayerGamePadIndex(InputDevice2.PPG_Player.Player_1).ToString(), new Vector2(32), Color.White);
            }
            catch (Exception)
            {
                spriteBatch.DrawString(Game1.tenbyFive14, "Player1: none", new Vector2(32), Color.White);
            }

            try
            {
                spriteBatch.DrawString(Game1.tenbyFive14, "Player2: " + InputDevice2.GetPlayerGamePadIndex(InputDevice2.PPG_Player.Player_2).ToString(), new Vector2(32, 64), Color.White);
            }
            catch (Exception)
            {
                spriteBatch.DrawString(Game1.tenbyFive14, "Player2: none", new Vector2(32, 64), Color.White);
            }

            spriteBatch.End();
#endif

#if PROFILE
            frameCounter++;

            string fps = string.Format("fps: {0}", frameRate);

            spriteBatch.Begin();

            spriteBatch.DrawString(font, fps, new Vector2(33, 33), Color.Black);
            spriteBatch.DrawString(font, fps, new Vector2(32, 32), Color.White);

            spriteBatch.End();
#endif
        }
    }
}
