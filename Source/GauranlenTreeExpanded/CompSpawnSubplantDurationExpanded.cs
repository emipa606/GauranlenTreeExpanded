using RimWorld;
using Verse;
using Verse.AI;

namespace GauranlenTreeExpanded;

public class CompSpawnSubplantDurationExpanded : ThingComp
{
    private int nextSubplantTick;

    private static float MaxRadius => GauranlenTreeSettings.MaxMossRadius;

    private CompProperties_SpawnSubplant Props => (CompProperties_SpawnSubplant)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (!respawningAfterLoad)
        {
            setupNextSubplantTick();
        }
    }

    public override void CompTick()
    {
        if (Find.TickManager.TicksGame < nextSubplantTick)
        {
            return;
        }

        DoGrowSubplant();
        setupNextSubplantTick();
    }

    private void setupNextSubplantTick()
    {
        nextSubplantTick = Find.TickManager.TicksGame + (int)(GenDate.TicksPerDay * Props.subplantSpawnDays);
    }

    public void DoGrowSubplant(bool force = false)
    {
        if (!force && ((Plant)parent).Growth < Props.minGrowthForSpawn)
        {
            return;
        }

        var position = parent.Position;
        var num = GenRadial.NumCellsInRadius(MaxRadius);
        for (var i = 0; i < num; i++)
        {
            var intVec = position + GenRadial.RadialPattern[i];
            if (!intVec.InBounds(parent.Map) || !WanderUtility.InSameRoom(position, intVec, parent.Map))
            {
                continue;
            }

            var isSubPlant = false;
            var thingList = intVec.GetThingList(parent.Map);
            foreach (var item in thingList)
            {
                if (item.def == Props.subplant)
                {
                    isSubPlant = true;
                    break;
                }

                if (Props.plantsToNotOverwrite.NullOrEmpty())
                {
                    continue;
                }

                foreach (var thingDef in Props.plantsToNotOverwrite)
                {
                    if (item.def != thingDef)
                    {
                        continue;
                    }

                    isSubPlant = true;
                    break;
                }
            }

            if (isSubPlant)
            {
                continue;
            }

            if (!Props.canSpawnOverPlayerSownPlants)
            {
                var plant = intVec.GetPlant(parent.Map);
                var zone = parent.Map.zoneManager.ZoneAt(intVec);
                if (plant is { sown: true } && zone is Zone_Growing)
                {
                    continue;
                }
            }

            if (!Props.subplant.CanEverPlantAt(intVec, parent.Map, true))
            {
                continue;
            }

            for (var num2 = thingList.Count - 1; num2 >= 0; num2--)
            {
                if (thingList[num2].def.category == ThingCategory.Plant)
                {
                    thingList[num2].Destroy();
                }
            }

            var plant2 = (Plant)GenSpawn.Spawn(Props.subplant, intVec, parent.Map);
            if (Props.initialGrowthRange.HasValue)
            {
                plant2.Growth = Props.initialGrowthRange.Value.RandomInRange;
            }

            plant2?.TryGetComp<GauranlenMossExpandedComp>()?.SetupParentTree((Plant)parent);
            break;
        }
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref nextSubplantTick, "nextSubplantTick");
    }
}