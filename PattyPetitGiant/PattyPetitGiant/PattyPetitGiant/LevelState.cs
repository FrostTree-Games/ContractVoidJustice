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

        private ParticleSet particleSet = null;
        public ParticleSet Particles { get { return particleSet; } }

        private RenderTarget2D textureScreen = null;
        private RenderTarget2D halfTextureScreen = null;
        private RenderTarget2D quarterTextureScreen = null;
        private Texture2D screenResult = null;
        private Texture2D halfSizeTexture = null;
        private Texture2D quarterSizeTexture = null;

        public LevelState()
        {
            currentSeed = Game1.rand.Next();

            PresentationParameters pp = AnimationLib.GraphicsDevice.PresentationParameters;
            textureScreen = new RenderTarget2D(AnimationLib.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, AnimationLib.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
            halfTextureScreen = new RenderTarget2D(AnimationLib.GraphicsDevice, pp.BackBufferWidth / 2, pp.BackBufferHeight / 2, false, AnimationLib.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
            quarterTextureScreen = new RenderTarget2D(AnimationLib.GraphicsDevice, pp.BackBufferWidth / 4, pp.BackBufferHeight / 4, false, AnimationLib.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);

            nodeMap = DungeonGenerator.generateRoomData(GlobalGameConstants.StandardMapSize.x, GlobalGameConstants.StandardMapSize.y, currentSeed);
            //nodeMap = DungeonGenerator.generateEntityZoo();
            map = new TileMap(this, nodeMap, GlobalGameConstants.TileSize);
            map.TileSkin = TextureLib.getLoadedTexture("scifiTemplate.png");
            map.ShopTileSkin = TextureLib.getLoadedTexture("tileTemplate.png");

            endFlagReached = false;

            gui = new InGameGUI(this);
            keyModule = new LevelKeyModule();

            particleSet = new ParticleSet();

            entityList = new List<Entity>();

            coinPool = new Coin[coinPoolSize];
            freeCoinIndex = 0;
            for (int i = 0; i < coinPoolSize; i++)
            {
                coinPool[i] = new Coin(this, new Vector2(-100, -100));
                entityList.Add(coinPool[i]);
            }

            populateRooms(nodeMap, currentSeed);

            for (int i = 0; i < entityList.Count; i++)
            {
                if (entityList[i] is Player)
                {
                    cameraFocus = entityList[i];
                }
            }

            end_flag_placed = false;
            state = LoadingState.LevelRunning;
        }

        private void placeMonstersInRoom(int roomTileX, int roomTileY, Entity.EnemyType faction, float intensity, Random rand)
        {
            int iteration = 0;
            int placedMonsterCount = 0;

            while (placedMonsterCount < 3)
            {
            // deeply-nested loops; justified as a special continue statement
            MonsterCheck:

                if (iteration > 10)
                {
                    return;
                }
                
                iteration++;
                int randX = rand.Next() % GlobalGameConstants.TilesPerRoomWide;
                int randY = rand.Next() % GlobalGameConstants.TilesPerRoomHigh;

                // don't place an entity on an edge;
                if (randX == 0 || randX == GlobalGameConstants.TilesPerRoomWide - 1 || randY == 0 || randY == GlobalGameConstants.TilesPerRoomHigh - 1)
                {
                    goto MonsterCheck;
                }

                for (int i = randX - 1; i <= randX + 1; i++)
                {
                    for (int j = randY - 1; j <= randY + 1; j++)
                    {
                        if (map.Map[roomTileX + i, roomTileY + j] != TileMap.TileType.NoWall)
                        {
                            goto MonsterCheck; //bad vibes man; I feel like Dijkstra's breathing down my neck
                        }
                    }
                }

                placedMonsterCount++;

                Vector2 spawnPos = new Vector2((roomTileX + randX) * GlobalGameConstants.TileSize.X, (roomTileY + randY) * GlobalGameConstants.TileSize.Y) - new Vector2(16);
                double randomSpawnValue = rand.NextDouble();

                //entityList.Add(new GuardSquadLeader(this, spawnPos.X, spawnPos.Y));
                //placedMonsterCount++;
                //placedMonsterCount++;
                faction = Entity.EnemyType.Prisoner;

                if (faction == Entity.EnemyType.Prisoner)
                {
                    if (randomSpawnValue < 0.25)
                    {
                        entityList.Add(new MolotovEnemy(this, spawnPos));
                    }
                    else if (randomSpawnValue < 0.5)
                    {
                        entityList.Add(new ChargerMutantEnemy(this, spawnPos));
                    }
                    else
                    {
                        entityList.Add(new ChaseEnemy(this, spawnPos));
                    }
                }
                else if (faction == Entity.EnemyType.Guard)
                {
                    if (randomSpawnValue < 0.25)
                    {
                        entityList.Add(new GuardSquadLeader(this, spawnPos.X, spawnPos.Y));
                    }
                    else if (randomSpawnValue < 0.5)
                    {
                        entityList.Add(new GuardMech(this, spawnPos.X, spawnPos.Y));
                    }
                    else
                    {
                        entityList.Add(new PatrolGuard(this, spawnPos));
                    }
                }
                else if (faction == Entity.EnemyType.Alien)
                {
                    //entityList.Add(new BroodLord(this, spawnPos));
                }
            }
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
                    else if (rooms[i, j].attributes.Contains("pickup"))
                    {
                        entityList.Add(new Pickup(this, new Vector2((currentRoomX + (GlobalGameConstants.TilesPerRoomWide / 2)) * GlobalGameConstants.TileSize.X, (currentRoomY + (GlobalGameConstants.TilesPerRoomHigh / 2)) * GlobalGameConstants.TileSize.Y), rand));
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

                        double enemyValuesSum = GameCampaign.CurrentAlienRate + GameCampaign.CurrentGuardRate + GameCampaign.CurrentPrisonerRate;
                        double rollRoomValue = rand.NextDouble();

                        if (rollRoomValue < GameCampaign.CurrentGuardRate / enemyValuesSum)
                        {
                            placeMonstersInRoom(currentRoomX, currentRoomY, Entity.EnemyType.Guard, intensityLevel, rand);
                        }
                        else if (rollRoomValue < (GameCampaign.CurrentGuardRate + GameCampaign.CurrentAlienRate) / enemyValuesSum)
                        {
                            placeMonstersInRoom(currentRoomX, currentRoomY, Entity.EnemyType.Alien, intensityLevel, rand);
                        }
                        else
                        {
                            placeMonstersInRoom(currentRoomX, currentRoomY, Entity.EnemyType.Prisoner, intensityLevel, rand);
                        }
                    }
                }
            }

            // run a garbage collection to clean up the heap before running
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

            if (GameCampaign.Player_Ammunition < 0)
            {
                GameCampaign.Player_Ammunition = 0;
            }


            for (int i = 0; i < entityList.Count; i++)
            {
                entityList[i].update(currentTime);
            }

            particleSet.update(currentTime);

#if WINDOWS
            entityList.RemoveAll(en => en.Remove_From_List == true);
#elif XBOX
            XboxTools.RemoveAll(entityList, XboxTools.IsEntityToBeRemoved);
#endif

            if (cameraFocus != null)
            {
                int pointX = (int)cameraFocus.CenterPoint.X;
                int pointY = (int)cameraFocus.CenterPoint.Y;

                camera = Matrix.CreateTranslation(new Vector3((pointX * -1) + (GlobalGameConstants.GameResolutionWidth / 2), (pointY * -1) + (GlobalGameConstants.GameResolutionHeight / 2), 0.0f));
                //camera = Matrix.CreateScale(0.1f);
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
            AnimationLib.GraphicsDevice.SetRenderTarget(textureScreen);
            AnimationLib.renderSpineEntities(camera, entityList, cameraFocus, map, particleSet);
            AnimationLib.GraphicsDevice.SetRenderTarget(null);
            screenResult = (Texture2D)textureScreen;

            AnimationLib.GraphicsDevice.SetRenderTarget(halfTextureScreen);
            AnimationLib.renderSpineEntities(camera * Matrix.CreateScale(0.5f), entityList, cameraFocus, map, particleSet);
            AnimationLib.GraphicsDevice.SetRenderTarget(null);
            halfSizeTexture = (Texture2D)halfTextureScreen;

            AnimationLib.GraphicsDevice.SetRenderTarget(quarterTextureScreen);
            AnimationLib.renderSpineEntities(camera * Matrix.CreateScale(0.25f), entityList, cameraFocus, map, particleSet);
            AnimationLib.GraphicsDevice.SetRenderTarget(null);
            quarterSizeTexture = (Texture2D)quarterTextureScreen;

            AnimationLib.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);

            Game1.BloomFilter.Parameters["halfResMap"].SetValue(halfSizeTexture);
            Game1.BloomFilter.Parameters["quarterResMap"].SetValue(quarterSizeTexture);

            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, Game1.BloomFilter, Matrix.Identity);
            sb.Draw(screenResult, new Vector2(0), Color.White);
            sb.End();

            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.Identity);
            gui.render(sb);
            sb.End();
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
                return ScreenStateType.LevelSelectState;
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