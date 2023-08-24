using HarmonyLib;
using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

[StaticConstructorOnStartup]
public class StartUpClass
{
    static StartUpClass()
    {
        new Harmony("GauranlenTreeExpandedMod.patch").PatchAll();
        foreach (var allDef in DefDatabase<ThinkTreeDef>.AllDefs)
        {
            if (allDef.defName == "Dryad")
            {
                allDef.thinkRoot = DefOfClass.DryadExpanded.thinkRoot;
            }
        }

        foreach (var allDef2 in DefDatabase<ThoughtDef>.AllDefs)
        {
            if (allDef2.defName != "TearedConnectionMemoryExpanded")
            {
                continue;
            }

            allDef2.durationDays = GauranlenTreeSettings.DurationDays;
            foreach (var stage in allDef2.stages)
            {
                stage.baseMoodEffect = -1f * GauranlenTreeSettings.BaseMoodDebuff;
            }
        }
    }
}