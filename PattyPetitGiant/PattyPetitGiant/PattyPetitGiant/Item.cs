using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    interface Item
    {
        void update(Player parent, GameTime currentTime, LevelState parentWorld);
        void daemonupdate(GameTime currentTime, LevelState parentWorld);
        GlobalGameConstants.itemType ItemType();
        string getEnumType();
        void draw(SpriteBatch sb);
    }
}
