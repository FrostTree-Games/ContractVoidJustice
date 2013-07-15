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
            Player.PlayerItems items = parent.CurrentItemTypes;

            if (items.item1 == GlobalGameConstants.itemType.DungeonMap && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem1))
            {
                parent.Velocity = Vector2.Zero;
                parentWorld.RenderNodeMap = true;
            }
            else if (items.item2 == GlobalGameConstants.itemType.DungeonMap && InputDeviceManager.isButtonDown(InputDeviceManager.PlayerButton.UseItem2))
            {
                parent.Velocity = Vector2.Zero;
                parentWorld.RenderNodeMap = true;
            }
            else
            {
                parentWorld.RenderNodeMap = false;
                parent.State = Player.playerState.Moving;
                parent.Disable_Movement = false;
            }
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

        public void draw(Spine.SkeletonRenderer sb)
        {
            //
        }
    }
}
