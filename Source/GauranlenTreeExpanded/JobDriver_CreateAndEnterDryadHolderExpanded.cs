using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public abstract class JobDriver_CreateAndEnterDryadHolderExpanded : JobDriver
{
    private const int TicksToCreate = 200;

    private CompTreeConnectionExpanded TreeComp => job.targetA.Thing.TryGetComp<CompTreeConnectionExpanded>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => TreeComp.ShouldReturnToTree(pawn));
        yield return Toils_General.Do(delegate
        {
            if (!CellFinder.TryFindRandomCellNear(job.GetTarget(TargetIndex.A).Cell, pawn.Map, 4,
                    c => GauranlenUtility.CocoonAndPodCellValidator(c, pawn.Map), out var result))
            {
                Log.Error($"Could not find cell to place dryad holder. Dryad={pawn.GetUniqueLoadID()}");
            }

            job.targetB = result;
        });
        yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
        yield return Toils_General.Wait(200).WithProgressBarToilDelay(TargetIndex.B)
            .FailOnDespawnedOrNull(TargetIndex.B);
        yield return EnterToil();
    }

    public abstract Toil EnterToil();
}