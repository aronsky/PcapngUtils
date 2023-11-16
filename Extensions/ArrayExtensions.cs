using System;
using System.Linq;

namespace PcapngUtils.Extensions
{
    public static class ArrayExtensions
    {
        public static string AsString(this Array arr)
        {
            return $"{{{string.Join(",", arr.Cast<object>().ToArray())}}}";
        }   
    }
}
