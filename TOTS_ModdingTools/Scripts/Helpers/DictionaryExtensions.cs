using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static bool TryAdd<T, Y>(this Dictionary<T, Y> dictionary, T key, Y value)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
            return true;
        }
        return false;
    }
}