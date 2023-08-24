using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

public class RitualObligationTargetWorker_ConnectedGauranlenTree : RitualObligationTargetFilter
{
    public RitualObligationTargetWorker_ConnectedGauranlenTree()
    {
    }

    public RitualObligationTargetWorker_ConnectedGauranlenTree(RitualObligationTargetFilterDef def)
        : base(def)
    {
    }

    private bool Enabled => GauranlenTreeSettings.EnableDisconnectionRitual;

    public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
    {
        if (!Enabled)
        {
            yield break;
        }

        var trees = map.listerThings.ThingsOfDef(ThingDefOf.Plant_TreeGauranlen);
        foreach (var getTargets in trees)
        {
            var compTreeConnectionExpanded = getTargets.TryGetComp<CompTreeConnectionExpanded>();
            if (compTreeConnectionExpanded is { Connected: true })
            {
                yield return getTargets;
            }
        }
    }

    protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
    {
        if (!Enabled)
        {
            return false;
        }

        var compTreeConnectionExpanded = target.Thing.TryGetComp<CompTreeConnectionExpanded>();
        return compTreeConnectionExpanded is { Connected: true };
    }

    public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
    {
        yield return "RitualTargetConnectedGaruanlenTreeExpandedInfo".Translate();
    }
}