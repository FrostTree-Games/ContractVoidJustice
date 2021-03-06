﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    interface EnemyComponents
    {
        void update(Enemy parent, GameTime currentTime, LevelState parentWorld);
        void update(Enemy parent, Entity player, GameTime currentTime, LevelState parentWorld);
    }
}
