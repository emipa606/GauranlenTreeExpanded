using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class JobDriver_MergeIntoGaumakerPodExpanded : JobDriver
{
    private const int WaitTicks = 120;

    private CompGaumakerPod GaumakerPod => job.targetB.Thing.TryGetComp<CompGaumakerPod>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOnDespawnedOrNull(TargetIndex.B);
        this.FailOn(() => GaumakerPod.Full);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
        yield return Toils_General.WaitWith(TargetIndex.B, WaitTicks, true);
        yield return Toils_General.Do(delegate { GaumakerPod.TryAcceptPawn(pawn); });
    }
}