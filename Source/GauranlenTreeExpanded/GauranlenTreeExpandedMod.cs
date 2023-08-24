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
        var listing_Standard = new Listing_Standard
        {
            verticalSpacing = -4f,
            maxOneColumn = false
        };
        listing_Standard.Begin(inRect);
        listing_Standard.Label("MaxBonusDryadLabel".Translate());
        listing_Standard.Label(
            "CurrentValueGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.MaxBonusDryad * 100f, 2)));
        GauranlenTreeSettings.MaxBonusDryad = listing_Standard.Slider(GauranlenTreeSettings.MaxBonusDryad, 0f, 3f);
        listing_Standard.Label("DaysForDryadsToGrow".Translate());
        listing_Standard.Label(
            "CurrentValueDaysGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.SpawnDays, 2)));
        GauranlenTreeSettings.SpawnDays = listing_Standard.Slider(GauranlenTreeSettings.SpawnDays, 0.5f, 12f);
        listing_Standard.Label("MaxMossRadiusExpandedLabel".Translate());
        listing_Standard.Label(
            "CurrentValueRadiusGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.MaxMossRadius, 1)));
        GauranlenTreeSettings.MaxMossRadius = listing_Standard.Slider(GauranlenTreeSettings.MaxMossRadius, 1f, 12f);
        listing_Standard.Label("MaxBuildingRadiusExpandedLabel".Translate());
        listing_Standard.Label(
            "CurrentValueRadiusGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.BuildingRadius, 1)));
        GauranlenTreeSettings.BuildingRadius = listing_Standard.Slider(GauranlenTreeSettings.BuildingRadius, 0f, 12f);
        listing_Standard.Label("ConnectionTornTicksExpanded".Translate());
        listing_Standard.Label(
            "CurrentValueDaysGauranlenTreeExpanded".Translate(
                (GauranlenTreeSettings.ConnectionTornTicks / GenDate.TicksPerDay).ToString()));
        GauranlenTreeSettings.ConnectionTornTicks = (int)(GenDate.TicksPerDay *
                                                          listing_Standard.Slider(
                                                              (float)GauranlenTreeSettings.ConnectionTornTicks /
                                                              GenDate.TicksPerDay, 1f, 30f));
        listing_Standard.Label("PruningTicksExpanded".Translate());
        listing_Standard.Label(
            "CurrentValueHoursGauranlenTreeExpanded".Translate(GauranlenTreeSettings.PruningDuration
                .ToStringTicksToPeriod()));
        GauranlenTreeSettings.PruningDuration =
            (int)listing_Standard.Slider(GauranlenTreeSettings.PruningDuration, 625f, 5000f);
        listing_Standard.Label("DebuffDurationDaysGauranlenTreeExpanded".Translate());
        listing_Standard.Label(
            "CurrentValueDaysGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.DurationDays, 1)));
        GauranlenTreeSettings.DurationDays = listing_Standard.Slider(GauranlenTreeSettings.DurationDays, 1f, 15f);
        listing_Standard.Label("DebuffBaseMoodValueGauranlenTreeExpanded".Translate());
        listing_Standard.Label(
            "CurrentValueHoursGauranlenTreeExpanded".Translate(Math.Round(GauranlenTreeSettings.BaseMoodDebuff, 0)));
        GauranlenTreeSettings.BaseMoodDebuff =
            (float)Math.Round(listing_Standard.Slider(GauranlenTreeSettings.BaseMoodDebuff, 5f, 20f), 0);
        listing_Standard.verticalSpacing = 2f;
        listing_Standard.CheckboxLabeled("DisableTreeExtractingExpanded".Translate(),
            ref GauranlenTreeSettings.TreeExtraction);
        listing_Standard.CheckboxLabeled("DisableDisconnectionRitualExpanded".Translate(),
            ref GauranlenTreeSettings.EnableDisconnectionRitual);
        if (listing_Standard.ButtonText("RestoreDefaultsGauranlenTreeExpanded".Translate()))
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
        }

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("CurrentModVersionGauranlenTreeExpanded".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }

    public override string SettingsCategory()
    {
        return "Gauranlen Tree Expanded";
    }
}