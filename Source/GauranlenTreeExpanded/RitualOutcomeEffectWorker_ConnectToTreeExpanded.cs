using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GauranlenTreeExpanded;

internal class RitualOutcomeEffectWorker_ConnectToTreeExpanded : RitualOutcomeEffectWorker_FromQuality
{
    public RitualOutcomeEffectWorker_ConnectToTreeExpanded()
    {
    }

    public RitualOutcomeEffectWorker_ConnectToTreeExpanded(RitualOutcomeEffectDef def)
        : base(def)
    {
    }

    public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
    {
        var thing = jobRitual.selectedTarget.Thing;
        var quality = GetQuality(jobRitual, progress);
        var num = Mathf.Max(1, Mathf.RoundToInt(quality * 50f));
        var compSpawnSubplantDuration = thing.TryGetComp<CompSpawnSubplantDuration>();
        if (compSpawnSubplantDuration != null)
        {
            _ = compSpawnSubplantDuration.Props.subplant;
            foreach (var key in totalPresence.Keys)
            {
                _ = key;
                for (var i = 0; i < num; i++)
                {
                    compSpawnSubplantDuration.DoGrowSubplant(true);
                }
            }

            compSpawnSubplantDuration.SetupNextSubplantTick();
        }

        var pawn = jobRitual.PawnWithRole("connector");
        var compTreeConnectionExpanded = thing.TryGetComp<CompTreeConnectionExpanded>();
        if (pawn == null || compTreeConnectionExpanded == null)
        {
            return;
        }

        compTreeConnectionExpanded.ConnectToPawn(pawn, quality);
        Find.LetterStack.ReceiveLetter("LetterLabelPawnConnected".Translate(thing.Named("TREE")),
            "LetterTextPawnConnected".Translate(thing.Named("TREE"), pawn.Named("CONNECTOR")),
            LetterDefOf.RitualOutcomePositive, pawn, null, null, [thing.def]);
    }
}