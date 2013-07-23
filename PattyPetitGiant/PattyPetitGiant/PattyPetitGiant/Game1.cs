//#define PROFILE

using System;
using System.Collections.Generic;
using System.Linq;
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
        
        
        private static Model myModel;
        public static Model MyModel
        {
            get
            {
                return myModel;
            }
        }

        public static SpriteFont font;
        public static SpriteFont testComputerFont;
        private static Effect bloomFilter = null;
        public static Effect BloomFilter { get { return bloomFilter; } }
        public static Texture2D whitePixel = null;
        public static Texture2D frostTreeLogo = null;
        public static Texture2D testArrow = null;
        public static Texture2D shipTexture = null;
        public static Random rand = new Random();

        private static bool gameIsRunningSlowly;
        public static bool GameIsRunningSlowly { get { return gameIsRunningSlowly; } }

        public static float aspectRatio;
        private ScreenState currentGameScreen = null;

        public static VideoPlayer videoPlayer = null;
        public static Video testVideo = null;

        private InputDeviceManager input_device = null;

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

            input_device = new InputDeviceManager(graphics.GraphicsDevice);
            InputDevice2.Initalize();
            
            //replace this with a join screen later
#if XBOX
            InputDevice2.LockController(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerPad.GamePad1);
#elif WINDOWS
            InputDevice2.LockController(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerPad.Keyboard);
#endif

            spriteBatch = new SpriteBatch(GraphicsDevice);

            whitePixel = Content.Load<Texture2D>("whitePixel");
            frostTreeLogo = Content.Load<Texture2D>("FrostTreeLogo");
            testArrow = Content.Load<Texture2D>("gfx/testArrow");
            shipTexture = Content.Load<Texture2D>("Textures/PPG_Sheet");

            myModel = Content.Load<Model>("model3D/PPG");
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            bloomFilter = Content.Load<Effect>("BloomShader");

            BackGroundAudio music = new BackGroundAudio(Content);

            music.addSong("Menu");

            TextureLib ts = new TextureLib(GraphicsDevice);
            TextureLib.loadFromManifest();

            font = Content.Load<SpriteFont>("testFont");
            testComputerFont = Content.Load<SpriteFont>("TestComputerFont");

            testVideo = Content.Load<Video>("fmv/WilsonTestVideo");
            videoPlayer = new VideoPlayer();

            ChunkLib cs = new ChunkLib();

            AnimationLib al = new AnimationLib(GraphicsDevice, spriteBatch);
            AnimationLib.loadSpineFromManifest();
            AnimationLib.loadFrameFromManifest();

            AudioLib lb = new AudioLib();

            GlobalGameConstants.WeaponDictionary.InitalizePriceData();

            GameCampaign.ResetPlayerValues();

            //currentGameScreen = new TitleScreen(myModel, aspectRatio, shipTexture);
            currentGameScreen = new LevelState();
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

            if (Keyboard.GetState().IsKeyDown(Keys.PageUp))
            {
                Thread.Sleep(1);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.PageDown))
            {
                return;
            }
#endif
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            gameIsRunningSlowly = gameTime.IsRunningSlowly;

            if (currentGameScreen.IsComplete)
            {
                currentGameScreen = ScreenState.SwitchToNewScreen(currentGameScreen.nextLevelState());
            }

            input_device.update();
            InputDevice2.Update(gameTime);

            currentGameScreen.update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            currentGameScreen.render(spriteBatch);

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
