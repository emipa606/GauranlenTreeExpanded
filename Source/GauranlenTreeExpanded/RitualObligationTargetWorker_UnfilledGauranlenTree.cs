using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

internal class RitualObligationTargetWorker_UnfilledGauranlenTree : RitualObligationTargetFilter
{
    public RitualObligationTargetWorker_UnfilledGauranlenTree()
    {
    }

    public RitualObligationTargetWorker_UnfilledGauranlenTree(RitualObligationTargetFilterDef def)
        : base(def)
    {
    }

    public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
    {
        var trees = map.listerThings.ThingsOfDef(ThingDefOf.Plant_TreeGauranlen);
        foreach (var targets in trees)
        {
            var compTreeConnectionExpanded = targets.TryGetComp<CompTreeConnectionExpanded>();
            if (compTreeConnectionExpanded is { CanBeConnected: true, ConnectionTorn: false })
            {
                yield return targets;
            }
        }
    }

    protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
    {
        var thing = target.Thing;
        var compTreeConnectionExpanded = thing.TryGetComp<CompTreeConnectionExpanded>();
        if (compTreeConnectionExpanded == null)
        {
            return false;
        }

        if (!compTreeConnectionExpanded.CanBeConnected)
        {
            return "RitualCannotConnectMorePawnsGauranlenTreeExpanded".Translate();
        }

        if (compTreeConnectionExpanded.ConnectionTorn)
        {
            return "RitualTargetConnectionTornGauranlenTree".Translate(thing.Named("TREE"),
                compTreeConnectionExpanded.UntornInDurationTicks.ToStringTicksToPeriod()).CapitalizeFirst();
        }

        return true;
    }

    public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
    {
        yield return "RitualTargetUnfilledGaruanlenTreeExpandedInfo".Translate();
    }
}