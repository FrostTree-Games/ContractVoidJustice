//comment this out if you don't want to generate any entities
#define TEST_ENTITIES

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

        private DungeonGenerator.DungeonRoom[,] nodeMap = null;

        private TileMap map = null;
        public TileMap Map { get { return map; } }

        private List<Entity> entityList = null;
        public List<Entity> EntityList { get { return entityList; } }

        private Entity cameraFocus = null;
        public Entity CameraFocus { get { return cameraFocus; } set { value = cameraFocus; } }
        Matrix camera;

        private bool endFlagReached;
        public bool EndFlagReached { get { return endFlagReached; } set { endFlagReached = value; } }

        public LevelState()
        {
            nodeMap = DungeonGenerator.generateRoomData(GlobalGameConstants.StandardMapSize.x, GlobalGameConstants.StandardMapSize.y);
            map = new TileMap(nodeMap, GlobalGameConstants.TileSize);
            map.TileSkin = TextureLib.getLoadedTexture("tileTemplate.png");

            endFlagReached = false;

            entityList = new List<Entity>();

            populateRooms(nodeMap);

#if TEST_ENTITIES

            entityList.Add(new Player(this, map.StartPosition.X, map.StartPosition.Y));
            entityList.Add(new BetaEndLevelFag(this, map.EndFlagPosition));
            testPopulateEnemies();

            foreach (Entity en in entityList)
            {
                if (en is Player)
                {
                    cameraFocus = en;
                }
            }
#endif

            state = LoadingState.Running;
        }

        /// <summary>
        /// Populates a dungeon with entities depending on its properties
        /// </summary>
        /// <param name="rooms"></param>
        private void populateRooms(DungeonGenerator.DungeonRoom[,] rooms)
        {
            for (int i = 0; i < rooms.GetLength(0); i++)
            {
                for (int j = 0; j < rooms.GetLength(1); j++)
                {
                    if (rooms[i, j].attributes == null)
                    {
                        continue;
                    }

                    if (rooms[i, j].attributes.Contains("shopkeeper"))
                    {
                        entityList.Add(new ShopKeeper(this, new Vector2(i * GlobalGameConstants.TilesPerRoomWide * GlobalGameConstants.TileSize.X + ((GlobalGameConstants.TilesPerRoomWide / 2) * GlobalGameConstants.TileSize.X ), j * GlobalGameConstants.TilesPerRoomHigh * GlobalGameConstants.TileSize.Y + (5 * GlobalGameConstants.TileSize.Y))));
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

            //entityList.RemoveAll(en=>en.Remove_From_List==true);

            if (cameraFocus != null)
            {
                camera = Matrix.Identity * Matrix.CreateTranslation(new Vector3((cameraFocus.CenterPoint.X * -1) + (GlobalGameConstants.GameResolutionWidth / 2), (cameraFocus.CenterPoint.Y * -1) + (GlobalGameConstants.GameResolutionHeight / 2), 0.0f));
            }

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

            sb.Begin();

            string player_health_display = "Health: " + GlobalGameConstants.Player_Health;
            sb.DrawString(Game1.font, player_health_display, new Vector2(10, 10), Color.Black);

            string ammunition_amount_display = "Ammunition: " + GlobalGameConstants.Player_Ammunition;
            sb.DrawString(Game1.font, ammunition_amount_display, new Vector2(10, 42), Color.Black);

            string coin_amount_display = "Coin: " + GlobalGameConstants.Player_Coin_Amount;
            sb.DrawString(Game1.font, coin_amount_display, new Vector2(10, 74), Color.Black);

            string player_item_1 = "Item 1: " + GlobalGameConstants.Player_Item_1;
            sb.DrawString(Game1.font, player_item_1, new Vector2(320,10), Color.Black);

            string player_item_2 = "Item 1: " + GlobalGameConstants.Player_Item_2;
            sb.DrawString(Game1.font, player_item_2, new Vector2(320, 42), Color.Black);

            sb.End();

            AnimationLib.renderSpineEntities(camera, entityList);
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
