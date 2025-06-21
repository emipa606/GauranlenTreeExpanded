using RimWorld;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class JobDriver_CreateAndEnterCocoonExpanded : JobDriver_CreateAndEnterDryadHolderExpanded
{
    protected override Toil EnterToil()
    {
        return Toils_General.Do(delegate
        {
            GenSpawn.Spawn(ThingDefOf.DryadCocoon, job.targetB.Cell, pawn.Map).TryGetComp<CompDryadCocoonExpanded>()
                .TryAcceptPawn(pawn);
        });
    }
}