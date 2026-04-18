using System;

namespace ModdingTools
{
    public static class Helpers
    {
        public static bool Contains(this string str, string other, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return str.IndexOf(other, comparisonType) >= 0;
        }
        
        public static bool Contains(this string str, char other, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return str.IndexOf(other.ToString(), comparisonType) >= 0;
        }
        
        public static int IndexOf(this string str, char other, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return str.IndexOf(other.ToString(), comparisonType);
        }
    }
}