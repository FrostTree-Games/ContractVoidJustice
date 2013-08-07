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
                    else if (randomSpawnValue < 0.2)
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
                    if (randomSpawnValue < 0.15)
                    {
                        entityList.Add(new BroodLord(this, spawnPos));
                    }
                    else if (randomSpawnValue < 0.4)
                    {
                        entityList.Add(new AlienChaser(this, spawnPos));
                    }
                    else
                    {
                        entityList.Add(new MutantAcidSpitter(this, spawnPos.X, spawnPos.Y));
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

                        if (GameCampaign.IsATwoPlayerGame)
                        {
                            entityList.Add(new Player(this, (currentRoomX + 8) * GlobalGameConstants.TileSize.X, (currentRoomY + 8) * GlobalGameConstants.TileSize.Y, InputDevice2.PPG_Player.Player_2));
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
            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.PauseButton) && !player1Dead)
            {
                state = LoadingState.LevelPaused;
                pauseDialogMinimumTime = 0;
            }

            if (GameCampaign.Player_Ammunition < 0)
            {
                GameCampaign.Player_Ammunition = 0;
            }
            if (GameCampaign.Player2_Ammunition < 0)
            {
                GameCampaign.Player2_Ammunition = 0;
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
                    isComplete = true;
                }
            }

            if (endFlagReached)
            {
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

        private void renderPauseOverlay(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            sb.DrawString(Game1.font, "PAUSED HOMIE\nAllegiance: " + GameCampaign.PlayerAllegiance + "\nName: " + GameCampaign.PlayerName + "\nContract: " + GameCampaign.currentContract.type + "\nSeed: " + currentSeed, new Vector2((GlobalGameConstants.GameResolutionWidth / 2) - (Game1.font.MeasureString("PAUSED HOMIE\nSeed: " + currentSeed).X / 2), GlobalGameConstants.GameResolutionHeight / 2), Color.Lerp(Color.Pink, Color.Turquoise, 0.4f));

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
                    renderGameStuff(sb);
                    break;
                case LoadingState.LevelPaused:
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
            if (endFlagReached)
            {
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

        public void pushCoin(Vector2 position, Coin.DropItemType drop_type, int value)
        {
            int lastAt = freeCoinIndex;

            do
            {
                freeCoinIndex = (freeCoinIndex + 1) % coinPoolSize;

                if (coinPool[freeCoinIndex].State == Coin.DropState.Inactive)
                {
                    coinPool[freeCoinIndex].activate(position, drop_type,value);
                    break;
                }
            }
            while (freeCoinIndex != lastAt);
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