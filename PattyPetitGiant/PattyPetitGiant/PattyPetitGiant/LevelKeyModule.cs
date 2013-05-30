using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PattyPetitGiant
{
    public class LevelKeyModule
    {

#if XBOX
        private const int KeyColorCount = 5;
#endif

        public enum KeyColor
        {
            Red = 0,
            Green = 1,
            Blue = 2,
            Purple = 3,
            Gray = 4,
        }

        private bool[] keysFound;
        private Color[] keyColorSet = { Color.Red, Color.Green, Color.Blue, Color.Purple, Color.DarkGray };
        public Color[] KeyColorSet { get { return keyColorSet; } }

        public LevelKeyModule()
        {
#if XBOX
            keysFound = new bool[KeyColorCount];
#elif WINDOWS
            keysFound = new bool[Enum.GetNames(typeof(KeyColor)).Length];
#endif
        }

        public int NumberOfKeys
        {
            get
            {
#if XBOX
                return KeyColorCount;
#elif WINDOWS
                return Enum.GetNames(typeof(KeyColor)).Length;
#endif
            }
        }

        public bool isKeyFound(KeyColor color)
        {
            return keysFound[(int)color];
        }

        public void setKey(KeyColor color, bool found)
        {
            keysFound[(int)color] = found;
        }
    }
}
