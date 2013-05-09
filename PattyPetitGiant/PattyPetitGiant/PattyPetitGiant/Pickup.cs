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
    class Pickup : Entity
    {
        GlobalGameConstants.itemType item_type = GlobalGameConstants.itemType.Bomb;
        public Pickup()
        {
            
        }
        public void update(Player parent, GameTime currentTime, LevelState parentWorld)
        {

        }
        public void daemonupdate(GameTime currentTime, LevelState parentWorld)
        {
        }
        public void draw(SpriteBatch sb)
        {
        }
    }
}
