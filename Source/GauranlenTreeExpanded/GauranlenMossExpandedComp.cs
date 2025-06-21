using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

public class GauranlenMossExpandedComp : ThingComp
{
    private Plant tree;

    public void SetupParentTree(Plant tree)
    {
        this.tree = tree;
    }

    public override void CompTick()
    {
        if (!parent.IsHashIntervalTick(12000))
        {
            return;
        }

        if (tree is { Spawned: true } && IntVec3Utility.ManhattanDistanceFlat(parent.Position, tree.Position) > 35 ||
            tree == null)
        {
            parent.Kill();
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref tree, "tree");
    }
}