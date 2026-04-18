using System.Collections.Generic;

namespace TOTS_ModdingTools.Helpers
{
    public static class ListExtensions
    {
        public static bool NullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }

        public static List<T> Copy<T>(this List<T> list)
        {
            return new List<T>(list);
        }
    }
}