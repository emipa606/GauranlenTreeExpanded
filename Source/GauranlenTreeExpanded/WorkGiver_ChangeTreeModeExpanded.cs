using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class WorkGiver_ChangeTreeModeExpanded : WorkGiver_Scanner
{
    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return pawn.Map.listerThings.ThingsMatching(ThingRequest.ForDef(ThingDefOf.Plant_TreeGauranlen));
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!ModsConfig.IdeologyActive)
        {
            return false;
        }

        var compTreeConnectionExpanded = t.TryGetComp<CompTreeConnectionExpanded>();
        return compTreeConnectionExpanded != null && compTreeConnectionExpanded.ConnectedPawns.Contains(pawn) &&
               compTreeConnectionExpanded.Mode != compTreeConnectionExpanded.desiredMode;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var job = JobMaker.MakeJob(JobDefOf.ChangeTreeMode, t);
        job.playerForced = forced;
        return job;
    }
}