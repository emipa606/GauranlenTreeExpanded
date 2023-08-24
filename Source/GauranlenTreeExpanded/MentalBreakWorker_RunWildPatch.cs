using HarmonyLib;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

[HarmonyPatch(typeof(MentalBreakWorker_RunWild), "TryStart")]
public static class MentalBreakWorker_RunWildPatch
{
    public static void Postfix(bool __result, Pawn pawn)
    {
        if (!__result)
        {
            return;
        }

        if (pawn == null)
        {
            return;
        }

        for (var num = pawn.connections.ConnectedThings.Count - 1; num >= 0; num--)
        {
            var compTreeConnectionExpanded =
                pawn.connections?.ConnectedThings[num].TryGetComp<CompTreeConnectionExpanded>();
            if (compTreeConnectionExpanded == null)
            {
                continue;
            }

            compTreeConnectionExpanded.TearConnection(pawn);
            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(DefOfClass.TearedConnectionMemoryExpanded);
            pawn.connections?.Notify_ConnectedThingDestroyed(pawn.connections.ConnectedThings[num]);
        }
    }
}