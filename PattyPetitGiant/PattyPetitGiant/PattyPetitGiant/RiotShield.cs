using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class RiotShield : Item
    {
        public enum RiotShieldState
        {
            InvalidState = -1,
            ShieldDown = 0,
            ShieldUp = 1,
        }

        public RiotShield()
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

        public void draw(Spine.SkeletonRenderer sb)
        {
            //
        }

        public GlobalGameConstants.itemType ItemType()
        {
            return GlobalGameConstants.itemType.RocketLauncher;
        }

        public string getEnumType()
        {
            return "RiotShield";
        }
    }
}
