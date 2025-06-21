using HarmonyLib;
using RimWorld;
using Verse;

namespace GauranlenTreeExpanded.HarmonyPatches;

[HarmonyPatch(typeof(Designator_ExtractTree), "CanDesignateThing")]
public class Designator_ExtractTree_CanDesignateThing
{
    public static void Postfix(ref AcceptanceReport __result, Thing t)
    {
        if (__result.Accepted && t.def == ThingDefOf.Plant_TreeGauranlen)
        {
            __result = GauranlenTreeSettings.TreeExtraction;
        }
    }
}