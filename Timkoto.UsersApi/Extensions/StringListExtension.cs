using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Extensions
{
    public static class StringListExtension
    {
        /// <summary>
        /// Adds the with time stamp.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="s">The s.</param>
        public static void AddWithTimeStamp(this List<string> list, string s)
        {
            list.Add($"{DateTime.UtcNow:MM/dd/yyyy HH:mm:ss.fff} - {s}");
        }
    }
}
