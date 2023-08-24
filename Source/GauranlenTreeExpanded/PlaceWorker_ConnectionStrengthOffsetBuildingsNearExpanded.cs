using RimWorld;
using UnityEngine;
using Verse;

namespace GauranlenTreeExpanded;

internal class
    PlaceWorker_ConnectionStrengthOffsetBuildingsNearExpanded : PlaceWorker_ConnectionStrengthOffsetBuildingsNear
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        if (!ModsConfig.IdeologyActive)
        {
            return;
        }

        var forCell =
            Find.CurrentMap.listerArtificialBuildingsForMeditation.GetForCell(center,
                GauranlenTreeSettings.BuildingRadius);
        GenDraw.DrawRadiusRing(center, GauranlenTreeSettings.BuildingRadius, Color.white);
        if (forCell.NullOrEmpty())
        {
            return;
        }

        var num = 0;
        foreach (var item in forCell)
        {
            if (num++ > 10)
            {
                break;
            }

            GenDraw.DrawLineBetween(GenThing.TrueCenter(center, Rot4.North, def.size, def.Altitude), item.TrueCenter(),
                SimpleColor.Red);
        }
    }
}