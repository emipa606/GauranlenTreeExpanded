using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GauranlenTreeExpanded;

public class RitualOutcomeEffectWorker_TearConnection : RitualOutcomeEffectWorker_FromQuality
{
    public RitualOutcomeEffectWorker_TearConnection()
    {
    }

    public RitualOutcomeEffectWorker_TearConnection(RitualOutcomeEffectDef def)
        : base(def)
    {
    }

    public override bool SupportsAttachableOutcomeEffect => false;

    public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
    {
        var thing = jobRitual.selectedTarget.Thing;
        var compTreeConnectionExpanded = thing.TryGetComp<CompTreeConnectionExpanded>();
        var pawn = jobRitual.PawnWithRole("disconnector");
        if (compTreeConnectionExpanded == null || pawn == null)
        {
            return;
        }

        compTreeConnectionExpanded.TearConnection(pawn);
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(DefOfClass.TearedConnectionMemoryExpanded);
        pawn.connections?.Notify_ConnectedThingDestroyed(thing);
    }
}