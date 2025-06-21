using RimWorld;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class JobDriver_CreateAndEnterHealingPodExpanded : JobDriver_CreateAndEnterDryadHolderExpanded
{
    protected override Toil EnterToil()
    {
        return Toils_General.Do(delegate
        {
            GenSpawn.Spawn(ThingDefOf.DryadHealingPod, job.targetB.Cell, pawn.Map)
                .TryGetComp<CompDryadHealingPodExpanded>().TryAcceptPawn(pawn);
        });
    }
}