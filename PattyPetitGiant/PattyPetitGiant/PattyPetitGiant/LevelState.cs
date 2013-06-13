using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    public class LevelState : ScreenState
    {
        private enum LoadingState
        {
            InvalidState = -1,
            UninitializedAndWaiting,
            LevelLoading,
            LevelRunning,
            LevelPaused,
            LevelClosing,
            ClosedAndWaiting,
        }

        private LoadingState state = LoadingState.UninitializedAndWaiting;

        private bool pauseButtonDown = false;
        private float pauseDialogMinimumTime;

        private InGameGUI gui = null;
        public InGameGUI GUI { get { return gui; } }

        private LevelKeyModule keyModule = null;
        public LevelKeyModule KeyModule { get { return keyModule; } }

        private DungeonGenerator.DungeonRoom[,] nodeMap = null;
        public DungeonGenerator.DungeonRoom[,] NodeMap { get { return nodeMap; } }
        private bool renderNodeMap = false;
        public bool RenderNodeMap { get { return renderNodeMap; } set { renderNodeMap = value; } }

        private TileMap map = null;
        public TileMap Map { get { return map; } }

        private List<Entity> entityList = null;
        public List<Entity> EntityList { get { return entityList; } }

        private Coin[] coinPool = null;
        private const int coinPoolSize = 50;
        private int freeCoinIndex;

        private Entity cameraFocus = null;
        public Entity CameraFocus { get { return cameraFocus; } set { value = cameraFocus; } }
        Matrix camera;

        private bool endFlagReached;
        public bool EndFlagReached { get { return endFlagReached; } set { endFlagReached = value; } }

        private bool end_flag_placed;

        private int currentSeed;
        public int CurrentSeed { get { return currentSeed; } }

        public LevelState()
        {
            currentSeed = Game1.rand.Next();

            nodeMap = DungeonGenerator.generateRoomData(GlobalGameConstants.StandardMapSize.x, GlobalGameConstants.StandardMapSize.y, currentSeed);
            //nodeMap = DungeonGenerator.generateEntityZoo();
            map = new TileMap(this, nodeMap, GlobalGameConstants.TileSize);
            map.TileSkin = TextureLib.getLoadedTexture("scifiTemplate.png");

            endFlagReached = false;

            gui = new InGameGUI(this);
            keyModule = new LevelKeyModule();

            entityList = new List<Entity>();

            coinPool = new Coin[coinPoolSize];
            freeCoinIndex = 0;
            for (int i = 0; i < coinPoolSize; i++)
            {
                coinPool[i] = new Coin(this, new Vector2(-100, -100));
                entityList.Add(coinPool[i]);
            }

            populateRooms(nodeMap, currentSeed);

            foreach (Entity en in entityList)
            {
                if (en is Player)
                {
                    cameraFocus = en;
                }
            }

            end_flag_placed = false;
            state = LoadingState.LevelRunning;
        }

        /// <summary>
        /// Populates a dungeon with entities depending on its properties
        /// </summary>
        /// <param name="rooms"></param>
        private void populateRooms(DungeonGenerator.DungeonRoom[,] rooms, int seed)
        {
            Random rand = new Random(seed);

            for (int i = 0; i < rooms.GetLength(0); i++)
            {
                for (int j = 0; j < rooms.GetLength(1); j++)
                {
                    if (rooms[i, j].attributes == null)
                    {
                        continue;
                    }

                    int currentRoomX = i * GlobalGameConstants.TilesPerRoomWide;
                    int currentRoomY = j * GlobalGameConstants.TilesPerRoomHigh;

                    //add doors based on neighboring room color values
                    {
                        if (rooms[i, j].north)
                        {
                            if (rooms[i, j].colors != rooms[i, j - 1].colors)
                            {
                                if (!rooms[i, j].colors.Blue && rooms[i, j - 1].colors.Blue)
                                {
                                    entityList.Add(new KeyDoor(this, new Vector2((currentRoomX + (GlobalGameConstants.TilesPerRoomWide / 2)) * GlobalGameConstants.TileSize.X, (currentRoomY) * GlobalGameConstants.TileSize.Y), LevelKeyModule.KeyColor.Blue, KeyDoor.DoorDirection.EastWest));
                                }
                            }
                        }

                        if (rooms[i, j].south)
                        {
                            if (rooms[i, j].colors != rooms[i, j + 1].colors)
                            {
                                if (!rooms[i, j].colors.Blue && rooms[i, j + 1].colors.Blue)
                                {
                                    entityList.Add(new KeyDoor(this, new Vector2((currentRoomX + (GlobalGameConstants.TilesPerRoomWide / 2)) * GlobalGameConstants.TileSize.X, (currentRoomY + GlobalGameConstants.TilesPerRoomHigh) * GlobalGameConstants.TileSize.Y), LevelKeyModule.KeyColor.Blue, KeyDoor.DoorDirection.EastWest));
                                }
                            }
                        }

                        if (rooms[i, j].east)
                        {
                            if (rooms[i, j].colors != rooms[i + 1, j].colors)
                            {
                                if (!rooms[i, j].colors.Blue && rooms[i + 1, j].colors.Blue)
                                {
                                    entityList.Add(new KeyDoor(this, new Vector2((currentRoomX + (GlobalGameConstants.TilesPerRoomWide)) * GlobalGameConstants.TileSize.X, (currentRoomY + (GlobalGameConstants.TilesPerRoomHigh / 2)) * GlobalGameConstants.TileSize.Y), LevelKeyModule.KeyColor.Blue, KeyDoor.DoorDirection.NorthSouth));
                                }
                            }
                        }

                        if (rooms[i, j].west)
                        {
                            if (rooms[i, j].colors != rooms[i - 1, j].colors)
                            {
                                if (!rooms[i, j].colors.Blue && rooms[i - 1, j].colors.Blue)
                                {
                                    entityList.Add(new KeyDoor(this, new Vector2((currentRoomX) * GlobalGameConstants.TileSize.X, (currentRoomY + (GlobalGameConstants.TilesPerRoomHigh / 2)) * GlobalGameConstants.TileSize.Y), LevelKeyModule.KeyColor.Blue, KeyDoor.DoorDirection.NorthSouth));
                                }
                            }
                        }
                    }

                    if (rooms[i, j].attributes.Contains("key"))
                    {
                        entityList.Add(new Key(this, new Vector2((currentRoomX + (GlobalGameConstants.TilesPerRoomWide / 2)) * GlobalGameConstants.TileSize.X, (currentRoomY + (GlobalGameConstants.TilesPerRoomHigh / 2)) * GlobalGameConstants.TileSize.Y), LevelKeyModule.KeyColor.Blue));
                    }

                    if (rooms[i, j].attributes.Contains("shopkeeper"))
                    {
                        entityList.Add(new ShopKeeper(this, new Vector2(i * GlobalGameConstants.TilesPerRoomWide * GlobalGameConstants.TileSize.X + ((GlobalGameConstants.TilesPerRoomWide / 2) * GlobalGameConstants.TileSize.X) - GlobalGameConstants.TileSize.X / 2, j * GlobalGameConstants.TilesPerRoomHigh * GlobalGameConstants.TileSize.Y + (5 * GlobalGameConstants.TileSize.Y))));
                    }
                    else if (rooms[i, j].attributes.Contains("start"))
                    {
                        entityList.Add(new Player(this, (currentRoomX + 8) * GlobalGameConstants.TileSize.X, (currentRoomY + 8) * GlobalGameConstants.TileSize.Y));
                    }
                    else if (rooms[i, j].attributes.Contains("end"))
                    {
                        if (end_flag_placed == false)
                        {
                            entityList.Add(new BetaEndLevelFag(this, new Vector2((currentRoomX + 8) * GlobalGameConstants.TileSize.X, (currentRoomY + 8) * GlobalGameConstants.TileSize.Y)));
                            end_flag_placed = true;
                        }
                    }
                    else
                    {
                        int intensityLevel = (int)(rooms[i, j].intensity / 0.2f);

                        for (int ec = 0; ec < intensityLevel; ec++)
                        {
                            int randX = 0;
                            int randY = 0;

                            do
                            {
                                randX = rand.Next() % GlobalGameConstants.TilesPerRoomWide;
                                randY = rand.Next() % GlobalGameConstants.TilesPerRoomHigh;
                            }
                            while (map.Map[randX + (GlobalGameConstants.TilesPerRoomWide * i), randY + (GlobalGameConstants.TilesPerRoomHigh * j)] != TileMap.TileType.NoWall);

                            const int enemyTypeCount = 3;
                            int randomEnemyNumber = rand.Next(); ;

                            switch (randomEnemyNumber % enemyTypeCount)
                            {
                                case 0:
                                    entityList.Add(new MolotovEnemy(this, new Vector2((randX * GlobalGameConstants.TileSize.X) + (GlobalGameConstants.TileSize.X * GlobalGameConstants.TilesPerRoomWide * i), (randY * GlobalGameConstants.TileSize.Y) + (GlobalGameConstants.TileSize.Y * GlobalGameConstants.TilesPerRoomHigh * j))));
                                    break;
                                case 1:
                                    entityList.Add(new ChaseEnemy(this, new Vector2((randX * GlobalGameConstants.TileSize.X) + (GlobalGameConstants.TileSize.X * GlobalGameConstants.TilesPerRoomWide * i), (randY * GlobalGameConstants.TileSize.Y) + (GlobalGameConstants.TileSize.Y * GlobalGameConstants.TilesPerRoomHigh * j))));
                                    break;
                                case 2:
                                    entityList.Add(new ChargerMutantEnemy(this, new Vector2((randX * GlobalGameConstants.TileSize.X) + (GlobalGameConstants.TileSize.X * GlobalGameConstants.TilesPerRoomWide * i), (randY * GlobalGameConstants.TileSize.Y) + (GlobalGameConstants.TileSize.Y * GlobalGameConstants.TilesPerRoomHigh * j))));
                                    break;
                            }

                        }
                    }
                }
            }

            GC.Collect();
        }

        /// <summary>
        /// Really crude
        /// </summary>
        private void testPopulateEnemies()
        {
            if (map == null)
            {
                return;
            }

            Random rand = new Random();
            for (int i = 0; i < 50; i++ )
            {
                int placeX = rand.Next() % map.Map.GetLength(0);
                int placeY = rand.Next() % map.Map.GetLength(1);

                if (map.Map[placeX, placeY] == TileMap.TileType.NoWall)
                {
                    switch (rand.Next() % 2)
                    {
                        case 1:
                            //entityList.Add(new ChaseEnemy(this, placeX * GlobalGameConstants.TileSize.X, placeY * GlobalGameConstants.TileSize.Y + 60));
                            break;
                        case 0:
                        default:
                            entityList.Add(new TestEnemy(this, new Vector2(placeX * GlobalGameConstants.TileSize.X, placeY * GlobalGameConstants.TileSize.Y + 60)));
                            break;
                    }
                }
            }
        }

        private void loadingUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            throw new NotImplementedException("asset loading not necessary yet");
        }

        private void gameLogicUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.PauseButton))
            {
                state = LoadingState.LevelPaused;
                pauseDialogMinimumTime = 0;
            }

            if (GlobalGameConstants.Player_Ammunition < 0)
            {
                GlobalGameConstants.Player_Ammunition = 0;
            }

            foreach (Entity en in entityList)
            {
                en.update(currentTime);
            }

#if WINDOWS
            entityList.RemoveAll(en => en.Remove_From_List == true);
#elif XBOX
            XboxTools.RemoveAll(entityList, XboxTools.IsEntityToBeRemoved);
#endif

            if (cameraFocus != null)
            {
                int pointX = (int)cameraFocus.CenterPoint.X;
                int pointY = (int)cameraFocus.CenterPoint.Y;

                camera = Matrix.Identity * Matrix.CreateTranslation(new Vector3((pointX * -1) + (GlobalGameConstants.GameResolutionWidth / 2), (pointY * -1) + (GlobalGameConstants.GameResolutionHeight / 2), 0.0f));
                //camera = Matrix.Identity * Matrix.CreateScale(0.2f);
            }

            gui.update(currentTime);

            if (endFlagReached)
            {
                //maybe have some nicer animation here later

                isComplete = true;
            }
        }

        private void pausedUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            pauseDialogMinimumTime += currentTime.ElapsedGameTime.Milliseconds;

            if (!pauseButtonDown && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.PauseButton))
            {
                pauseButtonDown = true;
            }
            else if (pauseButtonDown && !InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.PauseButton))
            {
                pauseButtonDown = false;

                if (pauseDialogMinimumTime > 300)
                {
                    state = LoadingState.LevelRunning;
                }
            }
        }

        protected override void doUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            switch (state)
            {
                case LoadingState.LevelRunning:
                    gameLogicUpdate(currentTime);
                    break;
                case LoadingState.LevelPaused:
                    pausedUpdate(currentTime);
                    break;
                case LoadingState.InvalidState:
                default:
                    throw new InvalidLevelStateExcepton();
            }
        }

        private void renderGameStuff(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, camera);

            map.render(sb, 0.0f);

            foreach (Entity en in entityList)
            {
                if (Vector2.Distance(en.Position, cameraFocus.Position) > 1000)
                {
                    continue;
                }

                en.draw(sb);
            }

            sb.End();

            AnimationLib.renderSpineEntities(camera, entityList, cameraFocus);

            gui.render(sb);
        }

        private void renderPauseOverlay(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            sb.DrawString(Game1.font, "PAUSED HOMIE\nSeed: " + currentSeed, new Vector2((GlobalGameConstants.GameResolutionWidth / 2) - (Game1.font.MeasureString("PAUSED HOMIE\nSeed: " + currentSeed).X / 2), GlobalGameConstants.GameResolutionHeight / 2), Color.Lerp(Color.Pink, Color.Turquoise, 0.4f));

            sb.End();
        }

        public override void render(SpriteBatch sb)
        {
            switch (state)
            {
                case LoadingState.LevelRunning:
                    renderGameStuff(sb);
                    break;
                case LoadingState.LevelPaused:
                    renderGameStuff(sb);
                    renderPauseOverlay(sb);
                    break;
                default:
                    break;
            }
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            if (endFlagReached)
            {
                return ScreenStateType.LevelState;
            }
            else
            {
                return ScreenStateType.InvalidScreenState;
            }
        }

        public class InvalidLevelStateExcepton : System.Exception
        {
            public InvalidLevelStateExcepton() { }
            public InvalidLevelStateExcepton(string message) { }
            public InvalidLevelStateExcepton(string message, System.Exception inner) { }
        }

        public void pushCoin(Vector2 position, Coin.CoinValue value)
        {
            int lastAt = freeCoinIndex;

            do
            {
                freeCoinIndex = (freeCoinIndex + 1) % coinPoolSize;

                if (coinPool[freeCoinIndex].State == Coin.CoinState.Inactive)
                {
                    coinPool[freeCoinIndex].activate(position, value);
                    break;
                }
            }
            while (freeCoinIndex != lastAt);
        }
    }
}