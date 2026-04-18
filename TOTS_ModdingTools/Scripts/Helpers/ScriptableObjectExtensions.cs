using UnityEngine;

namespace TOTS_ModdingTools.Helpers
{
    public static class ScriptableObjectExtensions
    {
        public static T Copy<T>(this T scriptableObject) where T : ScriptableObject
        {
            T copy = GameObject.Instantiate(scriptableObject);

            return copy;
        }
    }
}