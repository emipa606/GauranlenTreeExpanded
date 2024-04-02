using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

[HarmonyPatch(typeof(Pawn_ConnectionsTracker), nameof(Pawn_ConnectionsTracker.Notify_PawnKilled))]
public static class Pawn_ConnectionsTrackerHarmony
{
    public static void Prefix(Pawn ___pawn, List<Thing> ___connectedThings)
    {
        for (var num = ___connectedThings.Count - 1; num >= 0; num--)
        {
            var compTreeConnectionExpanded = ___connectedThings[num].TryGetComp<CompTreeConnectionExpanded>();
            if (compTreeConnectionExpanded == null)
            {
                continue;
            }

            compTreeConnectionExpanded.Notify_PawnDied(___pawn);
            ___connectedThings.RemoveAt(num);
        }
    }
}