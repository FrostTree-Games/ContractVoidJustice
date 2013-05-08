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

        private TileMap map = null;
        public TileMap Map { get { return map; } }

        private List<Entity> entityList = null;
        public List<Entity> EntityList { get { return entityList; } }

        private Entity cameraFocus = null;
        public Entity CameraFocus { get { return cameraFocus; } set { value = cameraFocus; } }
        Matrix camera;

        public LevelState()
        {
            map = new TileMap(DungeonGenerator.generateRoomData(GlobalGameConstants.StandardMapSize.x, GlobalGameConstants.StandardMapSize.y), GlobalGameConstants.TileSize);
            map.TileSkin = TextureLib.getLoadedTexture("tileTemplate.png");

            entityList = new List<Entity>();

#if TEST_ENTITIES

            entityList.Add(new Player(this, map.StartPosition.X, map.StartPosition.Y));
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
        /// Really crudle
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

            entityList.RemoveAll(en=>en.Remove_From_List==true);

            if (cameraFocus != null)
            {
                camera = Matrix.Identity * Matrix.CreateTranslation(new Vector3((cameraFocus.CenterPoint.X * -1) + (GlobalGameConstants.GameResolutionWidth / 2), (cameraFocus.CenterPoint.Y * -1) + (GlobalGameConstants.GameResolutionHeight / 2), 0.0f));
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

            sb.End();

            AnimationLib.renderSpineEntities(camera, entityList);
        }
    }
}
