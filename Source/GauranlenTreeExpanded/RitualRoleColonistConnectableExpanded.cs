using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

internal class RitualRoleColonistConnectableExpanded : RitualRoleColonistConnectable
{
    public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget,
        LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null,
        bool skipReason = false)
    {
        if (!p.connections.ConnectedThings.Any(x => x.def == ThingDefOf.Plant_TreeGauranlen))
        {
            return base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason);
        }

        reason = "PawnIsAlreadyConnectedToThatTree".Translate(p.Name.ToStringFull);
        return false;
    }
}