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
            UninitializedAndWaiting,
            Loading,
            Running,
            Closing,
            ClosedAndWaiting,
        }

        private LoadingState state = LoadingState.UninitializedAndWaiting;

        private InGameGUI gui = null;
        public InGameGUI GUI { get { return gui; } }

        private DungeonGenerator.DungeonRoom[,] nodeMap = null;
        public DungeonGenerator.DungeonRoom[,] NodeMap { get { return nodeMap; } }
        private bool renderNodeMap = false;
        public bool RenderNodeMap { get { return renderNodeMap; } set { renderNodeMap = value; } }

        private TileMap map = null;
        public TileMap Map { get { return map; } }

        private List<Entity> entityList = null;
        public List<Entity> EntityList { get { return entityList; } }

        private Entity cameraFocus = null;
        public Entity CameraFocus { get { return cameraFocus; } set { value = cameraFocus; } }
        Matrix camera;

        private bool endFlagReached;
        public bool EndFlagReached { get { return endFlagReached; } set { endFlagReached = value; } }

        private bool end_flag_placed;

        public LevelState()
        {
            nodeMap = DungeonGenerator.generateRoomData(GlobalGameConstants.StandardMapSize.x, GlobalGameConstants.StandardMapSize.y);
            map = new TileMap(this, nodeMap, GlobalGameConstants.TileSize);
            map.TileSkin = TextureLib.getLoadedTexture("tileTemplate.png");

            endFlagReached = false;

            gui = new InGameGUI(this);

            entityList = new List<Entity>();

            populateRooms(nodeMap);

            foreach (Entity en in entityList)
            {
                if (en is Player)
                {
                    cameraFocus = en;
                }
            }

            end_flag_placed = false;
            state = LoadingState.Running;
        }

        /// <summary>
        /// Populates a dungeon with entities depending on its properties
        /// </summary>
        /// <param name="rooms"></param>
        private void populateRooms(DungeonGenerator.DungeonRoom[,] rooms)
        {
            Random rand = new Random();
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

                    if (rooms[i, j].attributes.Contains("shopkeeper"))
                    {
                        entityList.Add(new ShopKeeper(this, new Vector2(i * GlobalGameConstants.TilesPerRoomWide * GlobalGameConstants.TileSize.X + ((GlobalGameConstants.TilesPerRoomWide / 2) * GlobalGameConstants.TileSize.X), j * GlobalGameConstants.TilesPerRoomHigh * GlobalGameConstants.TileSize.Y + (5 * GlobalGameConstants.TileSize.Y))));
                    }
                    else if (rooms[i, j].attributes.Contains("start"))
                    {
                        entityList.Add(new Player(this, (currentRoomX + 8) * GlobalGameConstants.TileSize.X, (currentRoomY + 8) * GlobalGameConstants.TileSize.Y));
                        entityList.Add(new IdleChaseEnemy(this, (currentRoomX + 10) * GlobalGameConstants.TileSize.X, (currentRoomY) * GlobalGameConstants.TileSize.Y));
                    }
                    else if (rooms[i, j].attributes.Contains("end"))
                    {
                        if(end_flag_placed == false)
                        {
                            entityList.Add(new BetaEndLevelFag(this,new Vector2( (currentRoomX + 8) * GlobalGameConstants.TileSize.X, (currentRoomY + 8) * GlobalGameConstants.TileSize.Y)));
                            end_flag_placed = true;
                        }
                    }
                    else
                    {
                        for (int h = 0; h < (rand.Next() % 2) + 1; h++)
                        {
                            int place_x = currentRoomX + (rand.Next() % 16);
                            int place_y = currentRoomY + (rand.Next() % 16);
                            int entity_choice = rand.Next() % 4;

                            for (int k = 0; k < 50; k++)
                            {
                                entity_choice = rand.Next() % 3;
                                if (map.Map[place_x, place_y] == TileMap.TileType.NoWall)
                                {
                                    switch (entity_choice)
                                    {
                                        case 0:
                                            int item_choice = rand.Next() % 5;
                                            //entityList.Add(new Pickup(this, place_x * GlobalGameConstants.TileSize.X, place_y * GlobalGameConstants.TileSize.Y, (GlobalGameConstants.itemType)item_choice));
                                            break;
                                        case 1:
                                            //entityList.Add(new IdleChaseEnemy(this, place_x * GlobalGameConstants.TileSize.X, place_y * GlobalGameConstants.TileSize.Y));
                                            break;
                                        case 2:
                                            //entityList.Add(new TestEnemy(this, place_x * GlobalGameConstants.TileSize.X, place_y * GlobalGameConstants.TileSize.Y));
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                }
                                else
                                {
                                    place_x = currentRoomX + (rand.Next() % 16);
                                    place_y = currentRoomY + (rand.Next() % 16);
                                }
                            }
                        }
                    }
                }
            }
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
                            entityList.Add(new ChaseEnemy(this, placeX * GlobalGameConstants.TileSize.X, placeY * GlobalGameConstants.TileSize.Y + 60));
                            break;
                        case 0:
                        default:
                            entityList.Add(new TestEnemy(this, placeX * GlobalGameConstants.TileSize.X, placeY * GlobalGameConstants.TileSize.Y + 60));
                            break;
                    }
                }
            }
        }

        protected override void doUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            foreach (Entity en in entityList)
            {
                en.update(currentTime);
            }

#if WINDOWS
            entityList.RemoveAll(en=>en.Remove_From_List==true);
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

            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.DownDirection))
            {
                InGameGUI.BoxWindow box = new InGameGUI.BoxWindow("test", 100, 100, 300, "The quick brown fox hides a drawer and freaks out your mom after you move to Russia.");
                gui.pushBox(box);
            }

            if (InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UpDirection))
            {
                gui.popBox("test");
            }

            gui.update(currentTime);

            if (endFlagReached)
            {
                //maybe have some nicer animation here later

                isComplete = true;
            }
        }

        public override void render(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camera);

            map.render(sb, 0.0f);

            foreach (Entity en in entityList)
            {
                en.draw(sb);
            }

            sb.End();

            AnimationLib.renderSpineEntities(camera, entityList);

            gui.render(sb);
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
    }
}