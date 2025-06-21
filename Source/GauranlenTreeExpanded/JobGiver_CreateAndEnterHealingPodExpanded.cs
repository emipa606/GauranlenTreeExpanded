using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

public class JobGiver_CreateAndEnterHealingPodExpanded : JobGiver_CreateAndEnterDryadHolderExpanded
{
    protected override JobDef JobDef => DefOfClass.CreateAndEnterHealingPodExpanded;

    protected override bool ExtraValidator(Pawn pawn, CompTreeConnectionExpanded connectionComp)
    {
        if (pawn.mindState is not { returnToHealingPod: true })
        {
            return base.ExtraValidator(pawn, connectionComp);
        }

        if (HealthAIUtility.ShouldSeekMedicalRest(pawn))
        {
            return true;
        }

        return pawn.health.hediffSet.GetMissingPartsCommonAncestors().Any() ||
               base.ExtraValidator(pawn, connectionComp);
    }
}