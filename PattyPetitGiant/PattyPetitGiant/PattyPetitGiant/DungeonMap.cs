using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class DungeonMap : Item
    {
        public DungeonMap()
        {
            //
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            // this will need to be altered later for press/release
            parentWorld.RenderNodeMap = !parentWorld.RenderNodeMap;

            parent.State = Player.playerState.Moving;
            parent.Disable_Movement = true;
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            //
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.DungeonMap;
        }

        public string getEnumType()
        {
            return GlobalGameConstants.itemType.DungeonMap.ToString();
        }

        public void draw(SpriteBatch sb)
        {
            //
        }
    }
}
