using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Texture2D whitePixel = null;

        private ScreenState currentGameScreen = null;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = GlobalGameConstants.GameResolutionWidth;
            graphics.PreferredBackBufferHeight = GlobalGameConstants.GameResolutionHeight;

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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            whitePixel = Content.Load<Texture2D>("whitePixel");

            TextureLib ts = new TextureLib(GraphicsDevice);
            TextureLib.loadTexture("derek.png");

            currentGameScreen = new LevelState();

            ChunkManager.Chunk c = new ChunkManager.Chunk();
            c.Name = "Test Chunk";
            c.tilemap = new int[GlobalGameConstants.TilesPerRoomWide * GlobalGameConstants.TilesPerRoomHigh];
            c.Attributes = new ChunkManager.Chunk.ChunkAttribute[3];
            c.Attributes[0] = new ChunkManager.Chunk.ChunkAttribute();
            c.Attributes[0].AttributeName = "north";
            c.Attributes[1] = new ChunkManager.Chunk.ChunkAttribute();
            c.Attributes[1].AttributeName = "south";
            c.Attributes[2] = new ChunkManager.Chunk.ChunkAttribute();
            c.Attributes[2].AttributeName = "shopkeeper";
            c.Attributes[2].Metadata = new string[2];
            c.Attributes[2].Metadata[0] = "sword";
            c.Attributes[2].Metadata[1] = "dance potion";
            ChunkManager.testWriteToDisk(c);
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

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
        }
    }
}
