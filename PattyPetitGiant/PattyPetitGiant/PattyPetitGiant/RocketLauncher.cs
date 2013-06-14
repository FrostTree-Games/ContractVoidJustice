using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class RocketLauncher : Item
    {
        public RocketLauncher()
        {
            //
        }

        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            //
        }

        public void daemonupdate(Player parent, GameTime currentTime, LevelState parentWorld)
        {
            //
        }

        public void draw(SpriteBatch sb)
        {
            //
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.RocketLauncher;
        }

        public string getEnumType()
        {
            return "RocketLauncher";
        }

    }
}
