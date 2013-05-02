using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class LevelState : ScreenState
    {
        private TileMap map = null;
        public TileMap Map { get { return map; } }

        private List<Entity> entityList = null;
        public List<Entity> EntityList { get { return entityList; } }

        public LevelState()
        {
            map = new TileMap(GlobalGameConstants.StandardMapSize, GlobalGameConstants.TileSize);
            entityList = new List<Entity>();
        }

        protected override void doUpdate(Microsoft.Xna.Framework.GameTime currentTime)
        {
            foreach (Entity en in entityList)
            {
                en.update(currentTime);
            }
        }

        public override void draw(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }
    }
}
