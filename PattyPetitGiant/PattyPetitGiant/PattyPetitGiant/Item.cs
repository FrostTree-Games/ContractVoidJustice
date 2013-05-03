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
        GlobalGameConstants.itemType itemCheck { get; }
        void update(Player parent, GameTime currentTime, LevelState parentWorld);
        void draw(SpriteBatch sb);
    }
}
