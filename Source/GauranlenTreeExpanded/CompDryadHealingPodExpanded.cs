using RimWorld;
using Verse;
using Verse.Sound;

namespace GauranlenTreeExpanded;

public class CompDryadHealingPodExpanded : CompDryadHolderExpanded
{
    private int tickExpire = -1;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (respawningAfterLoad)
        {
            return;
        }

        innerContainer = new ThingOwner<Thing>(this, false);
        tickExpire = Find.TickManager.TicksGame + 600;
    }

    public override void PostDeSpawn(Map map)
    {
        innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near, delegate(Thing t, int _)
        {
            if (t is Pawn { mindState: not null } pawn)
            {
                pawn.mindState.returnToHealingPod = false;
            }

            t.Rotation = Rot4.South;
            SoundDefOf.Pawn_Dryad_Spawn.PlayOneShot(parent);
        }, null, false);
    }

    public override void CompTick()
    {
        base.CompTick();
        if (parent.Destroyed)
        {
            return;
        }

        if (innerContainer.Count > 0 && TreeComp.ShouldReturnToTree((Pawn)innerContainer[0]))
        {
            parent.Destroy();
        }
        else if (tickExpire >= 0 && Find.TickManager.TicksGame >= tickExpire)
        {
            tickExpire = -1;
            parent.Destroy();
        }
    }

    public override void TryAcceptPawn(Pawn p)
    {
        base.TryAcceptPawn(p);
        p.Rotation = Rot4.South;
        tickComplete = Find.TickManager.TicksGame + (int)(GenDate.TicksPerDay * Props.daysToComplete);
        tickExpire = -1;
    }

    protected override void Complete()
    {
        tickComplete = Find.TickManager.TicksGame;
        EffecterDefOf.DryadEmergeFromCocoon.Spawn(parent.Position, parent.Map).Cleanup();
        foreach (var item in innerContainer)
        {
            if (item is not Pawn pawn)
            {
                continue;
            }

            pawn.mindState.returnToHealingPod = false;
            var hediffs = pawn.health.hediffSet.hediffs;
            for (var num = hediffs.Count - 1; num >= 0; num--)
            {
                if (hediffs[num] is Hediff_MissingPart &&
                    !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(hediffs[num].Part))
                {
                    pawn.health.RemoveHediff(hediffs[num]);
                }
                else if (hediffs[num].def.isBad)
                {
                    pawn.health.RemoveHediff(hediffs[num]);
                }
            }
        }

        parent.Destroy();
    }
}