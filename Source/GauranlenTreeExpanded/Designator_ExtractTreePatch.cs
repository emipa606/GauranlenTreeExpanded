using HarmonyLib;
using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

[HarmonyPatch(typeof(Designator_ExtractTree), "CanDesignateThing")]
public class Designator_ExtractTreePatch
{
    public static void Postfix(ref AcceptanceReport __result, Thing t)
    {
        if (__result.Accepted && t.def == ThingDefOf.Plant_TreeGauranlen)
        {
            __result = GauranlenTreeSettings.TreeExtraction;
        }
    }
}