using RimWorld;
using Verse;
using Verse.Sound;

namespace GauranlenTreeExpanded;

public class CompDryadCocoonExpanded : CompDryadHolderExpanded
{
    private PawnKindDef dryadKind;
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

    public override void TryAcceptPawn(Pawn p)
    {
        base.TryAcceptPawn(p);
        p.Rotation = Rot4.South;
        tickComplete = Find.TickManager.TicksGame + (int)(GenDate.TicksPerDay * Props.daysToComplete);
        tickExpire = -1;
        dryadKind = TreeComp.DryadKind;
    }

    protected override void Complete()
    {
        tickComplete = Find.TickManager.TicksGame;
        var treeComp = TreeComp;
        if (treeComp != null && innerContainer.Count > 0)
        {
            var pawn = (Pawn)innerContainer[0];
            var ageBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
            treeComp.RemoveDryad(pawn);
            var pawn2 = treeComp.GenerateNewDryad(dryadKind);
            pawn2.ageTracker.AgeBiologicalTicks = ageBiologicalTicks;
            if (!pawn.Name.Numerical)
            {
                pawn2.Name = pawn.Name;
            }

            pawn.Destroy();
            innerContainer.TryAddOrTransfer(pawn2, 1);
            EffecterDefOf.DryadEmergeFromCocoon.Spawn(parent.Position, parent.Map).Cleanup();
        }

        parent.Destroy();
    }

    public override void PostDeSpawn(Map map)
    {
        innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near, delegate(Thing t, int _)
        {
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

        if (dryadKind != null && dryadKind != TreeComp.DryadKind)
        {
            parent.Destroy();
        }
        else if (innerContainer.Count > 0 && tree != null && TreeComp.ShouldReturnToTree((Pawn)innerContainer[0]))
        {
            parent.Destroy();
        }
        else if (tickExpire >= 0 && Find.TickManager.TicksGame >= tickExpire)
        {
            tickExpire = -1;
            parent.Destroy();
        }
    }

    public override string CompInspectStringExtra()
    {
        var text = base.CompInspectStringExtra();
        if (innerContainer.NullOrEmpty() || dryadKind == null)
        {
            return text;
        }

        if (!text.NullOrEmpty())
        {
            text += "\n";
        }

        text += "ChangingDryadIntoType".Translate(innerContainer[0].Named("DRYAD"), dryadKind.Named("TYPE"))
            .Resolve();

        return text;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref tickExpire, "tickExpire", -1);
        Scribe_Defs.Look(ref dryadKind, "dryadKind");
    }
}