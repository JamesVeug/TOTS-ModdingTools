using System.Collections.Generic;
using HarmonyLib;

namespace TOTS_ModdingTools;

[HarmonyPatch]
public partial class WIKI
{
    private static string exportCSScriptsPath = "";

    class NameComparer : IEqualityComparer<(string name, string enu, string locale)>
    {
        public bool Equals((string name, string enu, string locale) x, (string name, string enu, string locale) y)
        {
            return x.name == y.name;
        }

        public int GetHashCode((string name, string enu, string locale) obj)
        {
            return obj.name.GetHashCode();
        }
    }
}