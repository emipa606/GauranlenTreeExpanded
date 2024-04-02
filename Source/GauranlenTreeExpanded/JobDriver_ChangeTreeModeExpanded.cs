using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class JobDriver_ChangeTreeModeExpanded : JobDriver
{
    private const int WaitTicks = 120;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.WaitWith(TargetIndex.A, WaitTicks, true);
        yield return Toils_General.Do(delegate
        {
            job.targetA.Thing.TryGetComp<CompTreeConnectionExpanded>().FinalizeMode(pawn);
        }).PlaySustainerOrSound(SoundDefOf.DryadCasteSet);
        yield return Toils_General.Wait(60, TargetIndex.A);
    }
}