using System;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;

namespace GauranlenTreeExpanded;

public class GauranlenTreeExpandedMod : Mod
{
    private static string currentVersion;

    public GauranlenTreeExpandedMod(ModContentPack content)
        : base(content)
    {
        GetSettings<GauranlenTreeSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard
        {
            verticalSpacing = -4f,
            maxOneColumn = false
        };
        listingStandard.Begin(inRect);
        listingStandard.Label("MaxBonusDryadLabel".Translate());
        listingStandard.Label(
            "CurrentValueGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.MaxBonusDryad * 100f, 2)));
        GauranlenTreeSettings.MaxBonusDryad = listingStandard.Slider(GauranlenTreeSettings.MaxBonusDryad, 0f, 3f);
        listingStandard.Label("MaxConnectedPawnsLabel".Translate());
        listingStandard.Label(
            "CurrentValueHoursGauranlenTreeExpanded".Translate(GauranlenTreeSettings.MaxConnectedPawns));
        GauranlenTreeSettings.MaxConnectedPawns =
            (int)Math.Round(listingStandard.Slider(GauranlenTreeSettings.MaxConnectedPawns, 1f, 20f));
        listingStandard.Label("DaysForDryadsToGrow".Translate());
        listingStandard.Label(
            "CurrentValueDaysGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.SpawnDays, 2)));
        GauranlenTreeSettings.SpawnDays = listingStandard.Slider(GauranlenTreeSettings.SpawnDays, 0.5f, 12f);
        listingStandard.Label("MaxMossRadiusExpandedLabel".Translate());
        listingStandard.Label(
            "CurrentValueRadiusGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.MaxMossRadius, 1)));
        GauranlenTreeSettings.MaxMossRadius = listingStandard.Slider(GauranlenTreeSettings.MaxMossRadius, 1f, 12f);
        listingStandard.Label("MaxBuildingRadiusExpandedLabel".Translate());
        listingStandard.Label(
            "CurrentValueRadiusGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.BuildingRadius, 1)));
        GauranlenTreeSettings.BuildingRadius = listingStandard.Slider(GauranlenTreeSettings.BuildingRadius, 0f, 12f);
        listingStandard.Label("ConnectionTornTicksExpanded".Translate());
        listingStandard.Label(
            "CurrentValueDaysGauranlenTreeExpanded".Translate(
                (GauranlenTreeSettings.ConnectionTornTicks / GenDate.TicksPerDay).ToString()));
        GauranlenTreeSettings.ConnectionTornTicks = (int)(GenDate.TicksPerDay *
                                                          listingStandard.Slider(
                                                              (float)GauranlenTreeSettings.ConnectionTornTicks /
                                                              GenDate.TicksPerDay, 1f, 30f));
        listingStandard.Label("PruningTicksExpanded".Translate());
        listingStandard.Label(
            "CurrentValueHoursGauranlenTreeExpanded".Translate(GauranlenTreeSettings.PruningDuration
                .ToStringTicksToPeriod()));
        GauranlenTreeSettings.PruningDuration =
            (int)listingStandard.Slider(GauranlenTreeSettings.PruningDuration, 625f, 5000f);
        listingStandard.Label("DebuffDurationDaysGauranlenTreeExpanded".Translate());
        listingStandard.Label(
            "CurrentValueDaysGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.DurationDays, 1)));
        GauranlenTreeSettings.DurationDays = listingStandard.Slider(GauranlenTreeSettings.DurationDays, 1f, 15f);
        listingStandard.Label("DebuffBaseMoodValueGauranlenTreeExpanded".Translate());
        listingStandard.Label(
            "CurrentValueHoursGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.BaseMoodDebuff, 0)));
        GauranlenTreeSettings.BaseMoodDebuff =
            (float)Math.Round(listingStandard.Slider(GauranlenTreeSettings.BaseMoodDebuff, 5f, 20f), 0);
        listingStandard.verticalSpacing = 2f;
        listingStandard.CheckboxLabeled("DisableTreeExtractingExpanded".Translate(),
            ref GauranlenTreeSettings.TreeExtraction);
        listingStandard.CheckboxLabeled("DisableDisconnectionRitualExpanded".Translate(),
            ref GauranlenTreeSettings.EnableDisconnectionRitual);
        if (listingStandard.ButtonText("RestoreDefaultsGauranlenTreeExpanded".Translate()))
        {
            GauranlenTreeSettings.MaxBonusDryad = 1.5f;
            GauranlenTreeSettings.SpawnDays = 8f;
            GauranlenTreeSettings.TreeExtraction = true;
            GauranlenTreeSettings.MaxMossRadius = 7.9f;
            GauranlenTreeSettings.BuildingRadius = 7.9f;
            GauranlenTreeSettings.EnableDisconnectionRitual = true;
            GauranlenTreeSettings.ConnectionTornTicks = 450000;
            GauranlenTreeSettings.PruningDuration = 2500;
            GauranlenTreeSettings.BaseMoodDebuff = 10f;
            GauranlenTreeSettings.DurationDays = 5f;
            GauranlenTreeSettings.MaxConnectedPawns = 4;
        }

        if (currentVersion != null)
        {
            GUI.contentColor = Color.gray;
            listingStandard.Label("CurrentModVersionGauranlenTreeExpanded".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }

    public override string SettingsCategory()
    {
        return "Gauranlen Tree Expanded";
    }
}