using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PattyPetitGiant
{
    /// <summary>
    /// A variety of methods, types, and classes for use with adapting to the .NET Compact Framework on the Xbox 360.
    /// </summary>
    public static class XboxTools
    {
        /// <summary> 
        /// Removes all elements from the List that match the conditions defined by the specified predicate. 
        /// </summary> 
        /// <typeparam name="T">The type of elements held by the List.</typeparam> 
        /// <param name="list">The List to remove the elements from.</param> 
        /// <param name="match">The Predicate delegate that defines the conditions of the elements to remove.</param> 
        public static int RemoveAll<T>(this System.Collections.Generic.List<T> list, Func<T, bool> match)
        {
            int numberRemoved = 0;

            // Loop through every element in the List, in reverse order since we are removing items. 
            for (int i = (list.Count - 1); i >= 0; i--)
            {
                // If the predicate function returns true for this item, remove it from the List. 
                if (match(list[i]))
                {
                    list.RemoveAt(i);
                    numberRemoved++;
                }
            }

            // Return how many items were removed from the List. 
            return numberRemoved;
        }

        /// <summary> 
        /// Returns true if the List contains elements that match the conditions defined by the specified predicate. 
        /// </summary> 
        /// <typeparam name="T">The type of elements held by the List.</typeparam> 
        /// <param name="list">The List to search for a match in.</param> 
        /// <param name="match">The Predicate delegate that defines the conditions of the elements to match against.</param> 
        public static bool Exists<T>(this System.Collections.Generic.List<T> list, Func<T, bool> match)
        {
            // Loop through every element in the List, until a match is found. 
            for (int i = 0; i < list.Count; i++)
            {
                // If the predicate function returns true for this item, return that at least one match was found. 
                if (match(list[i]))
                    return true;
            }

            // Return that no matching elements were found in the List. 
            return false;
        }

        /// <summary>
        /// A checker function to see if an entity needs to be removed from a list
        /// </summary>
        /// <param name="en">The Entity to be checked</param>
        /// <returns></returns>
        public static bool IsEntityToBeRemoved(Entity en)
        {
            return en.Remove_From_List;
        }


        /// <summary>
        /// A function for linear interpolation on values of degrees.
        /// Copypasted from http://stackoverflow.com/questions/2708476/rotation-interpolation
        /// </summary>
        /// <param name="start">First degree value to interpolate</param>
        /// <param name="end">Second degree value to interpolate</param>
        /// <param name="amount">Interpolation value</param>
        /// <returns></returns>
        public static float LerpDegrees(float start, float end, float amount)
        {
            float difference = Math.Abs(end - start);
            if (difference > 180)
            {
                // We need to add on to one of the values.
                if (end > start)
                {
                    // We'll add it on to start...
                    start += 360;
                }
                else
                {
                    // Add it on to end.
                    end += 360;
                }
            }

            // Interpolate it.
            float value = (start + ((end - start) * amount));

            // Wrap it..
            float rangeZero = 360;

            if (value >= 0 && value <= 360)
                return value;

            return (value % rangeZero);
        }

        /// <summary>
        /// A function for linear interpolation on values of radians.
        /// </summary>
        /// <param name="start">First radian value to interpolate</param>
        /// <param name="end">Second radian value to interpolate</param>
        /// <param name="amount">Interpolation value</param>
        /// <returns></returns>
        public static float LerpRadians(float start, float end, float amount)
        {
            return LerpDegrees(start * (float)(Math.PI / 180), end * (float)(Math.PI / 180), amount);
        }
    }
}
