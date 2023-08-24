using Verse;

namespace GauranlenTreeExpanded;

public class JobGiver_CreateAndEnterCocoonExpanded : JobGiver_CreateAndEnterDryadHolderExpanded
{
    public override JobDef JobDef => DefOfClass.CreateAndEnterCocoonExpanded;

    public override bool ExtraValidator(Pawn pawn, CompTreeConnectionExpanded connectionComp)
    {
        return connectionComp.DryadKind != pawn.kindDef || base.ExtraValidator(pawn, connectionComp);
    }
}