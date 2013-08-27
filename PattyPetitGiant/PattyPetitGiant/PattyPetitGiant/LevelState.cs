using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private enum PushMessageQueueState
        {
            Wait,
            FadeIn,
            ShowMessage,
            FadeOut,
        }

#if DEBUG
        private bool showStats = false;
#endif

        private LoadingState state = LoadingState.UninitializedAndWaiting;

        private bool pauseButtonDown = false;
        private float pauseDialogMinimumTime;
        private int pauseMenuItem = 0;
        private bool returnToTitle = false;

        private bool player1DownPressed = false;
        private bool player1UpPressed = false;
        private bool player1ConfirmPressed = false;

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

        private bool player1Dead;
        public bool Player1Dead { get { return player1Dead; } set { player1Dead = value; } }

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

        private float fadeOutTime;
        private const float fadeOutDuration = 5000f;

        private static float elapsedLevelTime;
        public static float ElapsedLevelTime { get { return elapsedLevelTime; } }

        private static int elapsedCoinAmount;
        public static int ElapsedCoinAmount { get { return elapsedCoinAmount; } set { elapsedCoinAmount = value; } }

        private Queue<string> pushMessageQueue = null;
        private PushMessageQueueState messageQueueState;
        private float queueTimer;

        private List<MutantAcidSpitter> acidSpitters = null;

        public LevelState()
        {
            currentSeed = Game1.rand.Next();

            state = LoadingState.LevelLoading;

            pushMessageQueue = new Queue<string>(5);
            messageQueueState = PushMessageQueueState.Wait;

            PresentationParameters pp = AnimationLib.GraphicsDevice.PresentationParameters;
            textureScreen = new RenderTarget2D(AnimationLib.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, AnimationLib.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
            halfTextureScreen = new RenderTarget2D(AnimationLib.GraphicsDevice, pp.BackBufferWidth / 2, pp.BackBufferHeight / 2, false, AnimationLib.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
            quarterTextureScreen = new RenderTarget2D(AnimationLib.GraphicsDevice, pp.BackBufferWidth / 4, pp.BackBufferHeight / 4, false, AnimationLib.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);

            new Thread(loadLevel).Start();
        }

        private void loadLevel()
        {
            for (int i = 0; i < GameCampaign.CampaignIntroductionValues[GameCampaign.PlayerLevelProgress].Length; i++)
            {
                GameCampaign.AvailableEntityTypes.Add(GameCampaign.CampaignIntroductionValues[GameCampaign.PlayerLevelProgress][i]);
            }

            nodeMap = DungeonGenerator.generateRoomData(GlobalGameConstants.StandardMapSize.x, GlobalGameConstants.StandardMapSize.y, currentSeed);
            //nodeMap = DungeonGenerator.generateEntityZoo();
            map = new TileMap(this, nodeMap, GlobalGameConstants.TileSize);

            string tileSetName = "deathStar";

            if (GameCampaign.PlayerFloorHeight == 0) { tileSetName = "hightech"; }
            if (GameCampaign.PlayerFloorHeight == 1) { tileSetName = "factory"; }
            if (GameCampaign.PlayerFloorHeight == 2) { tileSetName = "prisoncell"; }

            map.TileSkin[0] = TextureLib.getLoadedTexture(tileSetName + "/0.png");
            map.TileSkin[1] = TextureLib.getLoadedTexture(tileSetName + "/1.png");
            map.TileSkin[2] = TextureLib.getLoadedTexture(tileSetName + "/2.png");
            map.TileSkin[3] = TextureLib.getLoadedTexture(tileSetName + "/3.png");

            map.ElevatorRoomSkin = TextureLib.getLoadedTexture("elevator/0.png");

            Thread.Sleep(250);

            endFlagReached = false;

            gui = new InGameGUI(this);
            keyModule = new LevelKeyModule();

            particleSet = new ParticleSet();

            Thread.Sleep(250);

            entityList = new List<Entity>();
            acidSpitters = new List<MutantAcidSpitter>();

            coinPool = new Coin[coinPoolSize];
            freeCoinIndex = 0;
            for (int i = 0; i < coinPoolSize; i++)
            {
                coinPool[i] = new Coin(this, new Vector2(-100, -100));
                entityList.Add(coinPool[i]);
            }

            Thread.Sleep(250);

            populateRooms(nodeMap, currentSeed);

            Thread.Sleep(250);

            for (int i = 0; i < entityList.Count; i++)
            {
                if (entityList[i] is Player && ((Player)entityList[i]).Index == InputDevice2.PPG_Player.Player_1) 
                {
                    cameraFocus = entityList[i];
                }
            }

            fadeOutTime = 0.0f;

            elapsedLevelTime = 0.0f;
            elapsedCoinAmount = 0;

            player1Dead = false;
            end_flag_placed = false;

            BackGroundAudio.playSong("RPG Game", true);
            BackGroundAudio.changeVolume(1.0f);

            state = LoadingState.LevelRunning;
        }

        private void placeMonstersInRoom(int roomTileX, int roomTileY, Entity.EnemyType faction, float intensity, Random rand)
        {
            int iteration = 0;
            int placedMonsterCount = 0;

            while (placedMonsterCount < 5)
            {
            // deeply-nested loops; justified as a special continue statement
            MonsterCheck:

                if (iteration > 14)
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

                if (faction == Entity.EnemyType.Prisoner)
                {
                    if (randomSpawnValue < 0.25)
                    {
                        entityList.Add(new MolotovEnemy(this, spawnPos));
                    }

                    else if (randomSpawnValue < 0.35)
                    {
                        entityList.Add(new HookPrisonerEnemy(this, spawnPos.X, spawnPos.Y));
                    }
                    else if (randomSpawnValue < 0.5)
                    {
                        entityList.Add(new ChargerMutantEnemy(this, spawnPos));
                    }
                    else
                    {
                        entityList.Add(new ChaseEnemy(this, spawnPos.X, spawnPos.Y));
                    }
                }
                else if (faction == Entity.EnemyType.Guard)
                {
                    if (randomSpawnValue < 0.1)
                    {
                        entityList.Add(new GuardSquadLeader(this, spawnPos.X, spawnPos.Y));
                    }
                    else if (randomSpawnValue < 0.25)
                    {
                        entityList.Add(new GuardMech(this, spawnPos.X, spawnPos.Y));
                    }
                    else if (randomSpawnValue < 0.15)
                    {
                        entityList.Add(new AntiFairy(this, spawnPos + new Vector2(1, 0)));
                        entityList.Add(new AntiFairy(this, spawnPos + new Vector2(1, 1)));
                        entityList.Add(new AntiFairy(this, spawnPos + new Vector2(0, 1)));
                        entityList.Add(new AntiFairy(this, spawnPos + new Vector2(1, -1)));
                    }
                    else
                    {
                        entityList.Add(new PatrolGuard(this, spawnPos));
                    }
                }
                else if (faction == Entity.EnemyType.Alien)
                {
                    if (randomSpawnValue < 0.15)
                    {
                        entityList.Add(new BroodLord(this, spawnPos));
                    }
                    else if (randomSpawnValue < 0.25)
                    {
                        entityList.Add(new BallMutant(this, spawnPos.X, spawnPos.Y));
                    }
                    else if (randomSpawnValue < 0.4)
                    {
                        entityList.Add(new AlienChaser(this, spawnPos));
                    }
                    else
                    {
                        MutantAcidSpitter spitter = new MutantAcidSpitter(this, spawnPos.X, spawnPos.Y);
                        entityList.Add(spitter);
                        acidSpitters.Add(spitter);
                    }
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
                        entityList.Add(new Player(this, (currentRoomX + 8) * GlobalGameConstants.TileSize.X, (currentRoomY + 8) * GlobalGameConstants.TileSize.Y, InputDevice2.PPG_Player.Player_1));

                        entityList.Add(new GuardSquadLeader(this, (currentRoomX + 7) * GlobalGameConstants.TileSize.X, (currentRoomY + 8) * GlobalGameConstants.TileSize.Y));

                        //entityList.Add(new PatrolGuard(this, new Vector2((currentRoomX + 7) * GlobalGameConstants.TileSize.X, (currentRoomY + 8) * GlobalGameConstants.TileSize.Y)));

                        if (GameCampaign.IsATwoPlayerGame)
                        {
                            entityList.Add(new Player(this, (currentRoomX + 14) * GlobalGameConstants.TileSize.X, (currentRoomY + 14) * GlobalGameConstants.TileSize.Y, InputDevice2.PPG_Player.Player_2));
                        }
                    }
                    else if (rooms[i, j].attributes.Contains("pickup"))
                    {
                        entityList.Add(new Pickup(this, new Vector2((currentRoomX + (GlobalGameConstants.TilesPerRoomWide / 2)) * GlobalGameConstants.TileSize.X, (currentRoomY + (GlobalGameConstants.TilesPerRoomHigh / 2)) * GlobalGameConstants.TileSize.Y), rand));
                    }
                    else if (rooms[i, j].attributes.Contains("end"))
                    {
                        if (end_flag_placed == false)
                        {
                            entityList.Add(new BetaEndLevelFag(this, new Vector2((currentRoomX + 12) * GlobalGameConstants.TileSize.X, (currentRoomY + 12) * GlobalGameConstants.TileSize.Y)));
                            end_flag_placed = true;

                            for (int ii = 0; ii < GlobalGameConstants.TilesPerRoomWide; ii++)
                            {
                                for (int jj = 0; jj < GlobalGameConstants.TilesPerRoomHigh; jj++)
                                {
                                    map.mapMod[(i * GlobalGameConstants.TilesPerRoomWide) + ii, (j * GlobalGameConstants.TilesPerRoomHigh) + jj] = TileMap.WallMod.Elevator;
                                    map.floorMap[(i * GlobalGameConstants.TilesPerRoomWide) + ii, (j * GlobalGameConstants.TilesPerRoomHigh) + jj] = TileMap.FloorType.Elevator;
                                }
                            }
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

        private void loadingUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            //throw new NotImplementedException("asset loading not necessary yet");
        }

        private void gameLogicUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            if (InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.PauseButton) && !player1Dead)
            {
                state = LoadingState.LevelPaused;
                pauseDialogMinimumTime = 0;
                pauseMenuItem = 0;
                AudioLib.playSoundEffect("monitorOpening");
            }

            if (GameCampaign.Player_Ammunition < 0)
            {
                GameCampaign.Player_Ammunition = 0;
            }
            if (GameCampaign.Player2_Ammunition < 0)
            {
                GameCampaign.Player2_Ammunition = 0;
            }

            if (GameCampaign.Player_Ammunition > 100)
            {
                GameCampaign.Player_Ammunition = 100;
            }
            if (GameCampaign.Player2_Ammunition > 100)
            {
                GameCampaign.Player2_Ammunition = 100;
            }

            if (messageQueueState == PushMessageQueueState.Wait)
            {
                if (pushMessageQueue.Count > 0)
                {
                    messageQueueState = PushMessageQueueState.FadeIn;
                    queueTimer = 0;
                }
            }
            else if (messageQueueState == PushMessageQueueState.ShowMessage)
            {
                queueTimer += currentTime.ElapsedGameTime.Milliseconds;

                if (queueTimer > 1000f + (25 * pushMessageQueue.Peek().Length))
                {
                    messageQueueState = PushMessageQueueState.FadeOut;
                    queueTimer = 0;
                }
            }
            else if (messageQueueState == PushMessageQueueState.FadeIn)
            {
                queueTimer += currentTime.ElapsedGameTime.Milliseconds;

                if (queueTimer > 200f)
                {
                    messageQueueState = PushMessageQueueState.ShowMessage;
                    queueTimer = 0;
                }
            }
            else if (messageQueueState == PushMessageQueueState.FadeOut)
            {
                queueTimer += currentTime.ElapsedGameTime.Milliseconds;

                if (queueTimer > 200f)
                {
                    messageQueueState = PushMessageQueueState.Wait;
                    queueTimer = 0;
                    pushMessageQueue.Dequeue();
                }
            }

            for (int i = 0; i < entityList.Count; i++)
            {
                if (Vector2.Distance(cameraFocus.CenterPoint, entityList[i].CenterPoint) < 800)
                {
                    entityList[i].update(currentTime);
                }
            }

            elapsedLevelTime += currentTime.ElapsedGameTime.Milliseconds;

            particleSet.update(currentTime);

#if WINDOWS
            entityList.RemoveAll(en => en.Remove_From_List == true);
#elif XBOX
            XboxTools.RemoveAll(entityList, XboxTools.IsEntityToBeRemoved);
            XboxTools.RemoveAll(acidSpitters, XboxTools.IsEntityToBeRemoved);
#endif

            if (cameraFocus != null)
            {
                int pointX = (int)cameraFocus.CenterPoint.X;
                int pointY = (int)cameraFocus.CenterPoint.Y;

                camera = Matrix.CreateTranslation(new Vector3((pointX * -1) + (GlobalGameConstants.GameResolutionWidth / 2), (pointY * -1) + (GlobalGameConstants.GameResolutionHeight / 2), 0.0f));
                //camera = Matrix.CreateScale(0.1f);
            }

            gui.update(currentTime);

            if (player1Dead)
            {
                fadeOutTime += currentTime.ElapsedGameTime.Milliseconds;

                gui.BlackFadeOverlay = fadeOutTime / fadeOutDuration;

                if (fadeOutTime >= fadeOutDuration)
                {
                    BackGroundAudio.stopAllSongs();
                    isComplete = true;
                }
            }

            if (endFlagReached)
            {
                BackGroundAudio.stopAllSongs();
                isComplete = true;
            }

#if DEBUG
            if (InputDevice2.IsAnyControllerButtonDown(InputDevice2.PlayerButton.SwitchItem2) != InputDevice2.PlayerPad.NoPad)
            {
                showStats = true;
            }
            else
            {
                showStats = false;
            }
#endif
        }

        private void pausedUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            pauseDialogMinimumTime += currentTime.ElapsedGameTime.Milliseconds;

            if (!player1DownPressed && InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.DownDirection))
            {
                player1DownPressed = true;
            }
            else if (player1DownPressed && !InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.DownDirection))
            {
                player1DownPressed = false;

                pauseMenuItem = (pauseMenuItem + 1) % 2;
                AudioLib.playSoundEffect("menuSelect");
            }

            if (!player1UpPressed && InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.UpDirection))
            {
                player1UpPressed = true;
            }
            else if (player1UpPressed && !InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.UpDirection))
            {
                player1UpPressed = false;

                pauseMenuItem--;
                if (pauseMenuItem < 0) { pauseMenuItem = 1; }
                AudioLib.playSoundEffect("menuSelect");
            }

            if (!player1ConfirmPressed && InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.Confirm))
            {
                player1ConfirmPressed = true;
            }
            else if (player1ConfirmPressed && !InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.Confirm))
            {
                player1ConfirmPressed = false;

                if (pauseMenuItem == 0)
                {
                    state = LoadingState.LevelRunning;
                }
                else if (pauseMenuItem == 1)
                {
                    returnToTitle = true;

                    isComplete = true;
                }
            }

            if (!pauseButtonDown && InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.PauseButton))
            {
                pauseButtonDown = true;
            }
            else if (pauseButtonDown && !InputDevice2.IsPlayerButtonDown(InputDevice2.PPG_Player.Player_1, InputDevice2.PlayerButton.PauseButton))
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
                case LoadingState.LevelLoading:
                    loadingUpdate(currentTime);
                    break;
                case LoadingState.InvalidState:
                default:
                    throw new InvalidLevelStateExcepton();
            }
        }

        private void renderGameStuff(SpriteBatch sb)
        {
            AnimationLib.GraphicsDevice.SetRenderTarget(textureScreen);
            AnimationLib.renderSpineEntities(camera, entityList, cameraFocus, map, particleSet, acidSpitters);
            AnimationLib.GraphicsDevice.SetRenderTarget(null);
            screenResult = (Texture2D)textureScreen;

            AnimationLib.GraphicsDevice.SetRenderTarget(halfTextureScreen);
            AnimationLib.renderSpineEntities(camera * Matrix.CreateScale(0.5f), entityList, cameraFocus, map, particleSet, acidSpitters);
            AnimationLib.GraphicsDevice.SetRenderTarget(null);
            halfSizeTexture = (Texture2D)halfTextureScreen;

            AnimationLib.GraphicsDevice.SetRenderTarget(quarterTextureScreen);
            AnimationLib.renderSpineEntities(camera * Matrix.CreateScale(0.25f), entityList, cameraFocus, map, particleSet, acidSpitters);
            AnimationLib.GraphicsDevice.SetRenderTarget(null);
            quarterSizeTexture = (Texture2D)quarterTextureScreen;

            AnimationLib.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1.0f, 0);

            Game1.BloomFilter.Parameters["halfResMap"].SetValue(halfSizeTexture);
            Game1.BloomFilter.Parameters["quarterResMap"].SetValue(quarterSizeTexture);
            Game1.BloomFilter.Parameters["Threshold"].SetValue(0.7f);
            Game1.BloomFilter.Parameters["BlurDistanceX"].SetValue(0.0005f);
            Game1.BloomFilter.Parameters["BlurDistanceY"].SetValue(0.0005f);

            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, Game1.BloomFilter, Matrix.Identity);
            sb.Draw(screenResult, new Vector2(0), Color.White);
            sb.End();

            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.Identity);
            gui.render(sb);

            if (messageQueueState == PushMessageQueueState.ShowMessage)
            {
                sb.DrawString(Game1.tenbyFive24, pushMessageQueue.Peek(), new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 576) - Game1.tenbyFive24.MeasureString(pushMessageQueue.Peek()) / 2, Color.White);
            }
            else if (messageQueueState == PushMessageQueueState.FadeIn)
            {
                sb.DrawString(Game1.tenbyFive24, pushMessageQueue.Peek(), new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 576) - Game1.tenbyFive24.MeasureString(pushMessageQueue.Peek()) / 2 - new Vector2(0, 50 * (1 - (queueTimer / 200))), Color.Lerp(Color.Transparent, Color.White, queueTimer / 200));
            }
            else if (messageQueueState == PushMessageQueueState.FadeOut)
            {
                sb.DrawString(Game1.tenbyFive24, pushMessageQueue.Peek(), new Vector2(GlobalGameConstants.GameResolutionWidth / 2, 576) - Game1.tenbyFive24.MeasureString(pushMessageQueue.Peek()) / 2 + new Vector2(0, 50 * (queueTimer / 200)), Color.Lerp(Color.White, Color.Transparent, queueTimer / 200));
            }

#if DEBUG
            if (showStats)
            {
                sb.DrawString(Game1.tenbyFive14, "Allegiance: " + GameCampaign.PlayerAllegiance + "\nName: " + GameCampaign.PlayerName + "\nContract: " + GameCampaign.currentContract.type + "\nSeed: " + currentSeed + "\nContract Kills: " + GameCampaign.currentContract.killCount, Vector2.Zero, Color.LimeGreen);
            }
#endif
            sb.End();
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

        private void renderPauseOverlay(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            sb.Draw(Game1.whitePixel, Vector2.Zero, null, new Color(0, 0, 0, pauseDialogMinimumTime < 250 ? (pauseDialogMinimumTime / 250) * 0.35f : 0.35f), 0.0f, Vector2.Zero, new Vector2(GlobalGameConstants.GameResolutionWidth, GlobalGameConstants.GameResolutionHeight), SpriteEffects.None, 0.0f);

            sb.DrawString(Game1.tenbyFive72, "PAUSED", new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - Game1.tenbyFive72.MeasureString("PAUSED").X / 2, 150), Color.White, 0.0f, new Vector2(0, 0), new Vector2(1.0f, pauseDialogMinimumTime > 250 ? 1.0f : (pauseDialogMinimumTime / 250)), SpriteEffects.None, 0.0f);
            sb.DrawString(Game1.tenbyFive24, "Resume", new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - Game1.tenbyFive24.MeasureString("Resume").X / 2, 250), pauseMenuItem == 0 ? Color.White : new Color(1, 1, 1, 0.19f));
            sb.DrawString(Game1.tenbyFive24, "Quit to Title", new Vector2(GlobalGameConstants.GameResolutionWidth / 2 - Game1.tenbyFive24.MeasureString("Quit to Title").X / 2, 280), pauseMenuItem == 1 ? Color.White : new Color(1, 1, 1, 0.19f));

            sb.Draw(Game1.whitePixel, new Rectangle((GlobalGameConstants.GameResolutionWidth / 2 - 275), 350, 550, 200), new Color(0, 0, 0, 0.4f));
            drawBox(sb, new Rectangle((GlobalGameConstants.GameResolutionWidth / 2 - 275), 350, 550, 200), Color.White, 2);
            sb.DrawString(Game1.tenbyFive24, "Contract", new Vector2((GlobalGameConstants.GameResolutionWidth / 2 - Game1.tenbyFive24.MeasureString("Contract").X / 2), 350), Color.White);
            sb.DrawString(Game1.tenbyFive14, GameCampaign.levelMap[GameCampaign.PlayerLevelProgress, GameCampaign.PlayerFloorHeight].contract.contractMessage, new Vector2((GlobalGameConstants.GameResolutionWidth / 4) + 55, 385), Color.White);

            //sb.DrawString(Game1.font, "PAUSED HOMIE\n\nCHUNK NAME: " + nodeMap[((int)(cameraFocus.CenterPoint.X / GlobalGameConstants.TileSize.X) / GlobalGameConstants.TilesPerRoomWide), ((int)(cameraFocus.CenterPoint.Y / GlobalGameConstants.TileSize.Y) / GlobalGameConstants.TilesPerRoomHigh)].chunkName + "\n\nAllegiance: " + GameCampaign.PlayerAllegiance + "\nName: " + GameCampaign.PlayerName + "\nContract: " + GameCampaign.currentContract.type + "\nSeed: " + currentSeed, new Vector2((GlobalGameConstants.GameResolutionWidth / 2) - (Game1.font.MeasureString("PAUSED HOMIE\nSeed: " + currentSeed).X / 2), GlobalGameConstants.GameResolutionHeight / 2), Color.Lerp(Color.Pink, Color.Turquoise, (float)(Math.Sin(pauseDialogMinimumTime / 1000f))));

            sb.End();
        }

        private void renderLoadScreen(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            sb.Draw(Game1.whitePixel, new Rectangle(0, 0, GlobalGameConstants.GameResolutionWidth, GlobalGameConstants.GameResolutionHeight), Color.White);

            sb.DrawString(Game1.font, "LOADING...", new Vector2(GlobalGameConstants.GameResolutionWidth / 2, GlobalGameConstants.GameResolutionHeight / 2) - Game1.font.MeasureString("LOADING..."), Color.Black);

            sb.End();
        }

        public override void render(SpriteBatch sb)
        {
            switch (state)
            {
                case LoadingState.LevelRunning:
                    BackGroundAudio.changeVolume(1.0f);
                    renderGameStuff(sb);
                    break;
                case LoadingState.LevelPaused:
                    BackGroundAudio.changeVolume(0.4f);
                    renderGameStuff(sb);
                    renderPauseOverlay(sb);
                    break;
                case LoadingState.LevelLoading:
                    renderLoadScreen(sb);
                    break;
                default:
                    break;
            }
        }

        public override ScreenState.ScreenStateType nextLevelState()
        {
            if (returnToTitle)
            {
                return ScreenStateType.TitleScreen;
            }
            else if (endFlagReached)
            {
                if (GameCampaign.PlayerLevelProgress == 5)
                {
                    return ScreenStateType.EndingCutScene;
                }
                else
                    return ScreenStateType.FMV_ELEVATOR_EXIT;
            }
            else if (player1Dead)
            {
                return ScreenStateType.HighScoresState;
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

        public void pushCoin(Entity parent)
        {
            int total_coin_count = 0;
            int value = 0;
            Coin.DropItemType drop_type = Coin.DropItemType.CoinDrop;

            if (parent.Enemy_Type == GameCampaign.currentContract.killTarget)
            {
                total_coin_count = GameCampaign.currentContract.goldPerKill;
                drop_type = Coin.DropItemType.CoinDrop;

                while (total_coin_count != 0)
                {
                    double coin_value = Game1.rand.NextDouble();

                    if (coin_value < 0.20)
                    {
                        if ((int)Coin.CoinValue.Borden <= total_coin_count)
                        {
                            total_coin_count -= (int)Coin.CoinValue.Borden;
                            value = (int)Coin.CoinValue.Borden;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (coin_value < 0.35)
                    {
                        if ((int)Coin.CoinValue.Mackenzie <= total_coin_count)
                        {
                            value = (int)Coin.CoinValue.Mackenzie;
                            total_coin_count -= value;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (coin_value < 0.45)
                    {
                        if ((int)Coin.CoinValue.Elizabeth <= total_coin_count)
                        {
                            value = (int)Coin.CoinValue.Elizabeth;
                            total_coin_count -= value;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (coin_value < 0.55)
                    {
                        if ((int)Coin.CoinValue.MacDonald <= total_coin_count)
                        {
                            value = (int)Coin.CoinValue.MacDonald;
                            total_coin_count -= value;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (coin_value < 0.70)
                    {
                        if ((int)Coin.CoinValue.Laurier <= total_coin_count)
                        {
                            value = (int)Coin.CoinValue.Laurier;
                            total_coin_count -= value;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (coin_value < 0.85)
                    {
                        if ((int)Coin.CoinValue.Twoonie <= total_coin_count)
                        {
                            value = (int)Coin.CoinValue.Twoonie;
                            total_coin_count -= value;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (coin_value <= 1.0)
                    {
                        if ((int)Coin.CoinValue.Loonie <= total_coin_count)
                        {
                            value = (int)Coin.CoinValue.Loonie;
                            total_coin_count -= value;
                        }
                        else
                        {
                            continue;
                        }

                    }

                    int lastAt = freeCoinIndex;

                    do
                    {
                        freeCoinIndex = (freeCoinIndex + 1) % coinPoolSize;

                        if (coinPool[freeCoinIndex].State == Coin.DropState.Inactive)
                        {
                            coinPool[freeCoinIndex].activate(parent.CenterPoint, drop_type, value);
                            break;
                        }
                    }
                    while (freeCoinIndex != lastAt);

                }
            }
            else
            {
                int number_of_items_drop = parent.numberDropItems;
                while (number_of_items_drop > 0)
                {
                    double drop_type_value = Game1.rand.NextDouble();
                    
                    if (drop_type_value < parent.probItemDrop)
                    {
                        double health_or_ammo = Game1.rand.NextDouble();
                        drop_type_value = Game1.rand.NextDouble();
                        if (health_or_ammo < 0.5)
                        {
                            drop_type = Coin.DropItemType.MedDrop;
                            if (drop_type_value < 0.10)
                            {
                                value = (int)Coin.MedValue.fullPack;
                            }
                            else if (drop_type_value < 0.25)
                            {
                                value = (int)Coin.MedValue.largePack;
                            }
                            else if (drop_type_value < 0.50)
                            {
                                value = (int)Coin.MedValue.mediumPack;
                            }
                            else if (drop_type_value <= 1.0)
                            {
                                value = (int)Coin.MedValue.smallPack;
                            }
                        }
                        else
                        {
                            drop_type = Coin.DropItemType.AmmoDrop;
                            if (drop_type_value < 0.10)
                            {
                                value = (int)Coin.AmmoValue.fullAmmo;
                            }
                            else if (drop_type_value < 0.25)
                            {
                                value = (int)Coin.AmmoValue.largeAmmo;
                            }
                            else if (drop_type_value < 0.50)
                            {
                                value = (int)Coin.AmmoValue.mediumAmmo;
                            }
                            else if (drop_type_value <= 1.0)
                            {
                                value = (int)Coin.AmmoValue.smallAmmo;
                            }
                        }
                    }
                    else
                    {
                        drop_type = Coin.DropItemType.CoinDrop;
                        drop_type_value = Game1.rand.NextDouble();
                        if (drop_type_value < 0.1)
                        {
                            value = (int)Coin.CoinValue.Borden;
                        }
                        else if (drop_type_value < 0.2)
                        {
                            value = (int)Coin.CoinValue.Mackenzie;
                        }
                        else if (drop_type_value < 0.35)
                        {
                            value = (int)Coin.CoinValue.Elizabeth;
                        }
                        else if (drop_type_value < 0.50)
                        {
                            value = (int)Coin.CoinValue.MacDonald;
                        }
                        else if (drop_type_value < 0.65)
                        {
                            value = (int)Coin.CoinValue.Laurier;
                        }
                        else if (drop_type_value < 0.80)
                        {
                            value = (int)Coin.CoinValue.Twoonie;
                        }
                        else if (drop_type_value <= 1.0)
                        {
                            value = (int)Coin.CoinValue.Loonie;
                        }
                    }

                    int lastAt = freeCoinIndex;

                    do
                    {
                        freeCoinIndex = (freeCoinIndex + 1) % coinPoolSize;

                        if (coinPool[freeCoinIndex].State == Coin.DropState.Inactive)
                        {
                            coinPool[freeCoinIndex].activate(parent.CenterPoint, drop_type, value);
                            break;
                        }
                    }
                    while (freeCoinIndex != lastAt);

                    number_of_items_drop--;
                }
            }
            
        }

        public void pushMessage(string message)
        {
            //don't bother with spam
            if (pushMessageQueue.Count > 4 || message == null || message.Length > 140 || message.Length < 1)
            {
                return;
            }

            pushMessageQueue.Enqueue(message);
        }
    }
}