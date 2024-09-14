using Verse;

namespace GauranlenTreeExpanded;

[StaticConstructorOnStartup]
public class GauranlenTreeSettings : ModSettings
{
    public static int MaxConnectedPawns = 4;

    public static float MaxBonusDryad = 1.5f;

    public static float SpawnDays = 8f;

    public static bool TreeExtraction = true;

    public static float MaxMossRadius = 7.9f;

    public static float BuildingRadius = 7.9f;

    public static bool EnableDisconnectionRitual = true;

    public static int ConnectionTornTicks = 450000;

    public static int PruningDuration = 2500;

    public static float DurationDays = 5f;

    public static float BaseMoodDebuff = 10f;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref MaxConnectedPawns, "MaxConnectedPawns", 4);
        Scribe_Values.Look(ref MaxBonusDryad, "MaxBonusDryad", 1.5f);
        Scribe_Values.Look(ref SpawnDays, "SpawnDays", 8f);
        Scribe_Values.Look(ref TreeExtraction, "TreeExtraction", true);
        Scribe_Values.Look(ref MaxMossRadius, "MaxMossRadius", 7.9f);
        Scribe_Values.Look(ref BuildingRadius, "BuildingRadius", 7.9f);
        Scribe_Values.Look(ref EnableDisconnectionRitual, "EnableDisconnectionRitual", true);
        Scribe_Values.Look(ref ConnectionTornTicks, "ConnectionTornTicks", 450000);
        Scribe_Values.Look(ref PruningDuration, "PruningDuration", 2500);
        Scribe_Values.Look(ref DurationDays, "DurationDays", 5f);
        Scribe_Values.Look(ref BaseMoodDebuff, "BaseMoodDebuff", 10f);
    }
}