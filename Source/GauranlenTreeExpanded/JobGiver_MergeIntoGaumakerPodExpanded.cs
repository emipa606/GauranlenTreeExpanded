using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class JobGiver_MergeIntoGaumakerPodExpanded : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!ModsConfig.IdeologyActive)
        {
            return null;
        }

        if (pawn.connections == null || pawn.connections.ConnectedThings.NullOrEmpty())
        {
            return null;
        }

        foreach (var connectedThing in pawn.connections.ConnectedThings)
        {
            var compTreeConnectionExpanded = connectedThing.TryGetComp<CompTreeConnectionExpanded>();
            if (compTreeConnectionExpanded != null && compTreeConnectionExpanded.ShouldEnterGaumakerPod(pawn) &&
                pawn.CanReach(compTreeConnectionExpanded.gaumakerPod, PathEndMode.Touch, Danger.Deadly))
            {
                return JobMaker.MakeJob(DefOfClass.MergeIntoGaumakerPodExpanded, connectedThing,
                    compTreeConnectionExpanded.gaumakerPod);
            }
        }

        return null;
    }
}