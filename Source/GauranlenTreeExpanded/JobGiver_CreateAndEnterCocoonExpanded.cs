using Verse;

namespace GauranlenTreeExpanded;

public class JobGiver_CreateAndEnterCocoonExpanded : JobGiver_CreateAndEnterDryadHolderExpanded
{
    protected override JobDef JobDef => DefOfClass.CreateAndEnterCocoonExpanded;

    protected override bool ExtraValidator(Pawn pawn, CompTreeConnectionExpanded connectionComp)
    {
        return connectionComp.DryadKind != pawn.kindDef || base.ExtraValidator(pawn, connectionComp);
    }
}