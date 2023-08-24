using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class JobGiver_ReturnToGauranlenTreeExpanded : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        if (pawn.connections == null || pawn.connections.ConnectedThings.NullOrEmpty())
        {
            return null;
        }

        foreach (var connectedThing in pawn.connections.ConnectedThings)
        {
            var compTreeConnectionExpanded = connectedThing.TryGetComp<CompTreeConnectionExpanded>();
            if (compTreeConnectionExpanded != null && compTreeConnectionExpanded.ShouldReturnToTree(pawn) &&
                pawn.CanReach(connectedThing, PathEndMode.Touch, Danger.Deadly))
            {
                return JobMaker.MakeJob(DefOfClass.ReturnToGauranlenTreeExpanded, connectedThing);
            }
        }

        return null;
    }
}