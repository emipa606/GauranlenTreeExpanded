using RimWorld;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public abstract class JobGiver_CreateAndEnterDryadHolderExpanded : ThinkNode_JobGiver
{
    public const int SquareRadius = 4;

    public abstract JobDef JobDef { get; }

    public virtual bool ExtraValidator(Pawn pawn, CompTreeConnectionExpanded connectionComp)
    {
        return false;
    }

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
            if (compTreeConnectionExpanded != null && ExtraValidator(pawn, compTreeConnectionExpanded) &&
                !connectedThing.IsForbidden(pawn) && pawn.CanReach(connectedThing, PathEndMode.Touch, Danger.Deadly) &&
                CellFinder.TryFindRandomCellNear(connectedThing.Position, pawn.Map, 4,
                    c => GauranlenUtility.CocoonAndPodCellValidator(c, pawn.Map), out var _))
            {
                return JobMaker.MakeJob(JobDef, connectedThing);
            }
        }

        return null;
    }
}