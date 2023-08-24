using RimWorld;
using RimWorld.Planet;
using Verse;

namespace GauranlenTreeExpanded;

public class CompGaumakerPodExpanded : CompDryadHolderExpanded
{
    public bool Full => innerContainer.Count >= 3;

    public override void PostDeSpawn(Map map)
    {
        if (Find.TickManager.TicksGame < tickComplete)
        {
            innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
        }
    }

    public override void TryAcceptPawn(Pawn p)
    {
        base.TryAcceptPawn(p);
        if (!Full)
        {
            return;
        }

        _ = TreeComp.ConnectedPawns;
        tickComplete = Find.TickManager.TicksGame + (int)(GenDate.TicksPerDay * Props.daysToComplete);
    }

    protected override void Complete()
    {
        tickComplete = Find.TickManager.TicksGame;
        if (TreeComp != null)
        {
            for (var num = innerContainer.Count - 1; num >= 0; num--)
            {
                if (innerContainer[num] is not Pawn pawn)
                {
                    continue;
                }

                TreeComp.RemoveDryad(pawn);
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            }

            TreeComp.gaumakerPod = null;
            ((Plant)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Plant_PodGauranlen), parent.Position, parent.Map))
                .Growth = 1f;
        }

        parent.Destroy();
    }

    public override string CompInspectStringExtra()
    {
        var text = base.CompInspectStringExtra();
        if (innerContainer.Count >= 3)
        {
            return text;
        }

        if (!text.NullOrEmpty())
        {
            text += "\n";
        }

        text =
            $"{text}{GenLabel.BestKindLabel(PawnKindDefOf.Dryad_Gaumaker, Gender.Male, true).CapitalizeFirst()}: {innerContainer.Count}/{3}";

        return text;
    }
}