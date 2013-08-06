using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    class BetaEndLevelFag : Entity
    {
        private AnimationLib.FrameAnimationSet anims = null;

        public BetaEndLevelFag(LevelState parent, Vector2 position)
        {
            this.parentWorld = parent;
            this.position = position;
            this.dimensions = GlobalGameConstants.TileSize;

            anims = AnimationLib.getFrameAnimationSet("flagPic");
        }

        public override void update(GameTime currentTime)
        {
            for (int it = 0; it < parentWorld.EntityList.Count; it++)
            {
                if (parentWorld.EntityList[it] is Player)
                {
                    if (hitTest(parentWorld.EntityList[it]))
                    {
                        parentWorld.EndFlagReached = true;
                    }
                }
            }
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            anims.drawAnimationFrame(0.0f, sb, Position, new Vector2(1), 0.5f, 0.0f, Vector2.Zero, Color.White);
        }
        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            return;
        }
    }
}
