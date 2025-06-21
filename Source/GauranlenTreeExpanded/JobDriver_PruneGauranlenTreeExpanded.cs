using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class JobDriver_PruneGauranlenTreeExpanded : JobDriver
{
    private const TargetIndex TreeIndex = TargetIndex.A;

    private const TargetIndex AdjacentCellIndex = TargetIndex.B;

    private const int MaxPositions = 8;

    private readonly int DurationTicks = 2500;
    private int numPositions = 1;

    private CompTreeConnectionExpanded TreeConnection =>
        job.GetTarget(TreeIndex).Thing.TryGetComp<CompTreeConnectionExpanded>();

    public override void Notify_Starting()
    {
        base.Notify_Starting();
        var num = TreeConnection.desiredConnectionStrength - TreeConnection.GetConnectionStrength(pawn);
        numPositions = Mathf.Min(MaxPositions,
            Mathf.CeilToInt(num / TreeConnection.ConnectionStrengthGainPerHourOfPruning(pawn)) + 1);
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TreeIndex);
        var num = Mathf.RoundToInt(DurationTicks / pawn.GetStatValue(StatDefOf.PruningSpeed));
        var findAdjacentCell = Toils_General.Do(delegate
        {
            job.targetB = getAdjacentCell(job.GetTarget(TreeIndex).Thing);
        });
        var goToAdjacentCell = Toils_Goto.GotoCell(AdjacentCellIndex, PathEndMode.OnCell).FailOn(() =>
            TreeConnection.GetConnectionStrength() >= TreeConnection.desiredConnectionStrength);
        var prune = Toils_General.WaitWith(TreeIndex, num).WithEffect(EffecterDefOf.Harvest_MetaOnly, TreeIndex)
            .WithEffect(EffecterDefOf.GauranlenDebris, TreeIndex)
            .PlaySustainerOrSound(SoundDefOf.Interact_Prune);
        prune.WithProgressBarToilDelay(AdjacentCellIndex);
        prune.AddPreTickAction(delegate
        {
            TreeConnection.Prune(pawn);
            pawn.skills?.Learn(SkillDefOf.Plants, 0.085f);
            if (TreeConnection.GetConnectionStrength() >= TreeConnection.desiredConnectionStrength)
            {
                ReadyForNextToil();
            }
        });
        prune.activeSkill = () => SkillDefOf.Plants;
        for (var i = 0; i < numPositions; i++)
        {
            yield return findAdjacentCell;
            yield return goToAdjacentCell;
            yield return prune;
        }
    }

    private IntVec3 getAdjacentCell(Thing treeThing)
    {
        return (from x in GenAdj.CellsAdjacent8Way(treeThing)
            where x.InBounds(pawn.Map) && !x.Fogged(pawn.Map) && !x.IsForbidden(pawn) &&
                  pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Some)
            select x).TryRandomElement(out var result)
            ? result
            : treeThing.Position;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref numPositions, "numPositions", 1);
    }
}