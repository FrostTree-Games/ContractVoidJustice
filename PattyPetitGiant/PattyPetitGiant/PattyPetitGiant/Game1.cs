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

        private static Vector2 tileSize = new Vector2(48, 48);
        public static TileMap map = new TileMap(new TileMap.TileDimensions(50, 50), tileSize);

        //creating new list
        private static List<Entity> level_entity_list = null;
        private static float position_x = 150.0f;
        private static float position_y = 150.0f;
        private static float enemy_pos_x = 300.0f;
        private static float enemy_pos_y = 300.0f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
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
            // TODO: Add your initialization logic here

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

            map.blobTestWalls();
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

            // TODO: Add your update logic here

            if(level_entity_list == null)
            {
                level_entity_list = new List<Entity>();
                level_entity_list.Add(new Player(position_x, position_y));
                level_entity_list.Add(new Enemy(enemy_pos_x, enemy_pos_y));
                new Entity(level_entity_list);
            }

            foreach (Entity en in level_entity_list)
            {
                en.update(gameTime);
            }

           /* foreach (Entity en in level_entity_list)
            {
                en.draw(spriteBatch);
            }*/

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            map.render(spriteBatch, 1.0f);
            spriteBatch.Draw(Game1.whitePixel, new Vector2(100, 100), null, Color.Red, 0.0f, Vector2.Zero, new Vector2(48, 48), SpriteEffects.None, 1.0f);

            foreach (Entity en in level_entity_list)
            {
                en.draw(spriteBatch);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
