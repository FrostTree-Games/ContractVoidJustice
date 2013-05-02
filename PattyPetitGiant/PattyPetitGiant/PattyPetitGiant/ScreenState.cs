using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    public abstract class ScreenState
    {
        protected bool pause = false;
        public bool Pause { get { return pause; } set { pause = value; } }

        public void update(GameTime currentTime)
        {
            if (pause)
            {
                return;
            }

            doUpdate(currentTime);
        }

        protected abstract void doUpdate(GameTime currentTime);

        protected abstract void draw(SpriteBatch sb);
    }
}
