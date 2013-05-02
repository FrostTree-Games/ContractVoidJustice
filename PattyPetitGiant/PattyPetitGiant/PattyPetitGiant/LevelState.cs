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

        public LevelState()
        {
            map = new TileMap(GlobalGameConstants.StandardMapSize, GlobalGameConstants.TileSize);

            entityList = new List<Entity>();

#if TEST_ENTITIES
            map.blobTestWalls();

            entityList.Add(new Player(this, 150, 150));
            entityList.Add(new Enemy(this, 300, 272));
#endif

            state = LoadingState.Running;
        }

        protected override void doUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            foreach (Entity en in entityList)
            {
                en.update(currentTime);
            }
        }

        public override void render(SpriteBatch sb)
        {
            sb.Begin();

            map.render(sb, 0.7f);

            foreach (Entity en in entityList)
            {
                en.draw(sb);
            }

            sb.End();
        }
    }
}
