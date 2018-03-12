using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    /// <summary>
    /// Utility class to hold utility functions.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Shifts the list by 1 in a cirular fashion.
        /// </summary>
        /// <param name="oldList">List to shift</param>
        /// <returns>Shifted List</returns>
        public static Google.Protobuf.Collections.RepeatedField<string> Shift(this Google.Protobuf.Collections.RepeatedField<string> oldList)
        {
            Google.Protobuf.Collections.RepeatedField<string> newList = new Google.Protobuf.Collections.RepeatedField<string>();
            for (int i = 0; i < oldList.Count; i++)
            {
                if (i < oldList.Count - 1)
                    newList.Add(oldList[i + 1]);
                else
                    newList.Add(oldList[0]);
            }
            Console.WriteLine(newList.ToString());
            return newList;
        }
    }
}
