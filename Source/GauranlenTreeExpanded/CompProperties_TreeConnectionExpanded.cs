using UnityEngine;
using Verse;

namespace GauranlenTreeExpanded;

public class CompProperties_TreeConnectionExpanded : CompProperties
{
    public SimpleCurve connectionLossDailyPerBuildingDistanceCurve;

    public SimpleCurve connectionLossPerLevelCurve;

    public float connectionStrengthGainPerHourPruningBase = 0.01f;

    public SimpleCurve connectionStrengthGainPerPlantSkill;

    public float connectionStrengthLossPerDryadDeath = 0.1f;

    public FloatRange initialConnectionStrengthRange;

    public SimpleCurve maxDryadsPerConnectionStrengthCurve;

    public int maxDryadsWild;
    public PawnKindDef pawnKind;

    public float radiusToBuildingForConnectionStrengthLoss = 7.9f;

    public Vector3 spawningPodOffset;

    public FloatRange spawningPodSizeRange = FloatRange.One;

    public CompProperties_TreeConnectionExpanded()
    {
        compClass = typeof(CompTreeConnectionExpanded);
    }
}