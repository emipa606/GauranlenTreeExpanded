using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GauranlenTreeExpanded;

public abstract class CompDryadHolderExpanded : ThingComp, ISuspendableThingHolder
{
    protected const int ExpiryDurationTicks = 600;

    private Material cachedFrontMat;

    private CompTreeConnectionExpanded cachedTreeComp;

    protected ThingOwner innerContainer;
    protected int tickComplete = -1;

    protected Thing tree;

    protected CompProperties_DryadCocoon Props => (CompProperties_DryadCocoon)props;

    protected CompTreeConnectionExpanded TreeComp
    {
        get
        {
            cachedTreeComp ??= tree?.TryGetComp<CompTreeConnectionExpanded>();

            return cachedTreeComp;
        }
    }

    private Material FrontMat
    {
        get
        {
            if (cachedFrontMat == null)
            {
                cachedFrontMat = MaterialPool.MatFrom("Things/Building/Misc/DryadSphere/DryadSphereFront",
                    ShaderDatabase.Cutout);
            }

            return cachedFrontMat;
        }
    }

    public bool IsContentsSuspended => true;

    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (!respawningAfterLoad)
        {
            innerContainer = new ThingOwner<Thing>(this, false);
        }
    }

    public override void CompTick()
    {
        base.CompTick();
        if (tickComplete < 0)
        {
            return;
        }

        if (tree == null || tree.Destroyed)
        {
            parent.Destroy();
        }
        else if (Find.TickManager.TicksGame >= tickComplete)
        {
            Complete();
        }
    }

    public override void PostDraw()
    {
        if (!Props.drawContents)
        {
            return;
        }

        foreach (var thing in innerContainer)
        {
            thing.DrawNowAt(parent.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingBelowTop));
        }

        var matrix = default(Matrix4x4);
        var pos = parent.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingBelowTop.AltitudeFor() +
                                                               0.01f);
        var q = Quaternion.Euler(0f, parent.Rotation.AsAngle, 0f);
        var s = new Vector3(parent.Graphic.drawSize.x, 1f, parent.Graphic.drawSize.y);
        matrix.SetTRS(pos, q, s);
        Graphics.DrawMesh(MeshPool.plane10, matrix, FrontMat, 0);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (tickComplete < 0 || !Prefs.DevMode)
        {
            yield break;
        }

        var commandAction = new Command_Action
        {
            defaultLabel = "DEV: Complete",
            action = Complete
        };
        yield return commandAction;
    }

    public virtual void TryAcceptPawn(Pawn p)
    {
        p.DeSpawn();
        innerContainer.TryAddOrTransfer(p, 1);
        SoundDefOf.Pawn_EnterDryadPod.PlayOneShot(SoundInfo.InMap(parent));
        if (p.connections == null)
        {
            return;
        }

        foreach (var connectedThing in p.connections.ConnectedThings)
        {
            if (connectedThing.TryGetComp<CompTreeConnectionExpanded>() == null)
            {
                continue;
            }

            tree = connectedThing;
            break;
        }
    }

    public override string CompInspectStringExtra()
    {
        var text = base.CompInspectStringExtra();
        if (!text.NullOrEmpty())
        {
            text += "\n";
        }

        text += "CasketContains".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst();
        if (tickComplete >= 0)
        {
            text =
                $"{text}\n{"TimeLeft".Translate().CapitalizeFirst()}: {(tickComplete - Find.TickManager.TicksGame).ToStringTicksToPeriod()
                    .Colorize(ColoredText.DateTimeColor)}";
        }

        return text;
    }

    protected abstract void Complete();

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref tickComplete, "tickComplete", -1);
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        Scribe_References.Look(ref tree, "tree");
    }
}