using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

[HarmonyPatch(typeof(Pawn_ConnectionsTracker), "Notify_ConnectedThingDestroyed")]
public class Notify_ConnectedThingDestroyedPatch
{
    public static bool Prefix(Thing thing, List<Thing> ___connectedThings)
    {
        if (thing.Destroyed)
        {
            return true;
        }

        ___connectedThings.Remove(thing);
        return false;
    }
}