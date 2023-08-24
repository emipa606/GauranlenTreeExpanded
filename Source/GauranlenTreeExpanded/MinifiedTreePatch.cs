using HarmonyLib;
using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

[HarmonyPatch(typeof(MinifiedTree), "RecordTreeDeath")]
public static class MinifiedTreePatch
{
    public static void Postfix(MinifiedTree __instance)
    {
        if (__instance?.InnerTree.AllComps == null)
        {
            return;
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < __instance.InnerTree.AllComps.Count; i++)
        {
            __instance.InnerTree.AllComps[i].PostDestroy(DestroyMode.Vanish, __instance.MapHeld);
        }
    }
}