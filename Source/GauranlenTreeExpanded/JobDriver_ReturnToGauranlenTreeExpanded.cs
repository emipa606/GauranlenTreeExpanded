using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class JobDriver_ReturnToGauranlenTreeExpanded : JobDriver
{
    private const int WaitTicks = 180;

    private CompTreeConnectionExpanded TreeComp => job.targetA.Thing.TryGetComp<CompTreeConnectionExpanded>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => !TreeComp.ShouldReturnToTree(pawn));
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.WaitWith(TargetIndex.A, WaitTicks, true)
            .WithEffect(EffecterDefOf.GauranlenLeaves, TargetIndex.A).PlaySustainerOrSound(SoundDefOf.Interact_Sow);
        yield return Toils_General.Do(delegate
        {
            TreeComp.RemoveDryad(pawn);
            pawn.DeSpawn();
            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
        });
    }
}