using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class WorkGiver_PruneGauranlenTreeExpanded : WorkGiver_Scanner
{
    public override bool Prioritized => true;

    public override float GetPriority(Pawn pawn, TargetInfo t)
    {
        if (!t.HasThing)
        {
            return 0f;
        }

        var compTreeConnectionExpanded = t.Thing.TryGetComp<CompTreeConnectionExpanded>();
        if (compTreeConnectionExpanded == null)
        {
            return 0f;
        }

        return compTreeConnectionExpanded.desiredConnectionStrength -
               compTreeConnectionExpanded.GetConnectionStrength(pawn);
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return pawn.Map.listerThings.ThingsMatching(ThingRequest.ForDef(ThingDefOf.Plant_TreeGauranlen));
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var compTreeConnectionExpanded = t.TryGetComp<CompTreeConnectionExpanded>();
        if (compTreeConnectionExpanded == null)
        {
            return false;
        }

        if (!compTreeConnectionExpanded.ConnectedPawns.Contains(pawn))
        {
            return false;
        }

        if (!compTreeConnectionExpanded.ShouldBePrunedNow(forced, pawn))
        {
            return false;
        }

        return !t.IsForbidden(pawn) && pawn.CanReserve(t, 1, -1, null, forced);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var job = JobMaker.MakeJob(DefOfClass.PruneGauranlenTreeExpanded, t, pawn);
        job.playerForced = forced;
        return job;
    }
}