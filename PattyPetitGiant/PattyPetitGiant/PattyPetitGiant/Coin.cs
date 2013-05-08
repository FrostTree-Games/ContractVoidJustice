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
    class Coin : Entity
    {
        public Coin()
        {
        }
        public Coin(float initial_x, float initial_y, LevelState parentWorld)
        {
            position = new Vector2(initial_x, initial_y);
            this.parentWorld = parentWorld;
        }
    }
}
