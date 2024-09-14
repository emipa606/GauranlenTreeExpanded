using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace GauranlenTreeExpanded;

public class CompTreeConnectionExpanded : ThingComp
{
    private readonly List<Pawn> tmpDryads = [];

    private Lord cachedLordJob;

    private Material cachedPodMat;
    private List<Pawn> connectedPawns = [];

    private Dictionary<Pawn, float> connectionStrength = new Dictionary<Pawn, float>();

    private GauranlenTreeModeDef currentMode;

    public float DesiredConnectionStrength = 0.5f;

    public GauranlenTreeModeDef desiredMode;

    private List<Pawn> dryads = [];

    public Thing gaumakerPod;

    private Dictionary<Pawn, int> lastPrunedTicks = new Dictionary<Pawn, int>();

    private int lastSubPlantTick = -1;

    private Effecter leafEffecter;

    private List<float> list1;

    private List<int> list2;

    private List<Pawn> list3;

    private List<Pawn> list4;

    private int lordJobCoolDown;

    private int nextUntornTick = -1;

    public bool NoWildDryadSpawning;

    private Gizmo_PruningConfigExpanded pruningGizmo;

    private int spawnTick = -1;

    private float MaxBonusDryad => GauranlenTreeSettings.MaxBonusDryad;

    private float SpawnDays => GauranlenTreeSettings.SpawnDays;

    private int ConnectionTornTicks => GauranlenTreeSettings.ConnectionTornTicks;

    private float MaxBuildingRadius => GauranlenTreeSettings.BuildingRadius;

    public List<Pawn> ConnectedPawns => connectedPawns;

    public bool ConnectionTorn => nextUntornTick >= Find.TickManager.TicksGame;

    public bool HasProductionMode => desiredMode != null;

    public int UntornInDurationTicks => nextUntornTick - Find.TickManager.TicksGame;

    private float ConnectionStrength
    {
        get
        {
            if (!Connected)
            {
                return 0f;
            }

            var num = 0f;
            foreach (var connectedPawn in connectedPawns)
            {
                if (connectionStrength.TryGetValue(connectedPawn, out var value))
                {
                    num += value;
                }
            }

            return num / connectedPawns.Count;
        }
    }

    public bool CanBeConnected => connectedPawns.Count < GauranlenTreeSettings.MaxConnectedPawns;

    public int MaxDryads
    {
        get
        {
            if (!Connected)
            {
                if (NoWildDryadSpawning)
                {
                    return 0;
                }

                return Props?.maxDryadsWild ?? 0;
            }

            var num = 0;
            foreach (var connectedPawn in connectedPawns)
            {
                num += (int)Props.maxDryadsPerConnectionStrengthCurve.Evaluate(GetConnectionStrength(connectedPawn));
            }

            return (int)Math.Round((float)num / connectedPawns.Count *
                                   GenMath.LerpDouble(1f, GauranlenTreeSettings.MaxConnectedPawns, 1f, MaxBonusDryad,
                                       connectedPawns.Count));
        }
    }

    public bool Connected => connectedPawns.Count > 0;

    public CompProperties_TreeConnectionExpanded Props => (CompProperties_TreeConnectionExpanded)props;

    public GauranlenTreeModeDef Mode => currentMode;

    private List<Thing> BuildingsReducingConnectionStrength =>
        parent.Map.listerArtificialBuildingsForMeditation.GetForCell(parent.Position, MaxBuildingRadius);

    private int SpawningDurationTicks => (int)(GenDate.TicksPerDay * SpawnDays);

    private Material PodMat
    {
        get
        {
            if (cachedPodMat == null)
            {
                cachedPodMat = MaterialPool.MatFrom("Things/Building/Misc/DryadFormingPod/DryadFormingPod",
                    ShaderDatabase.Cutout);
            }

            return cachedPodMat;
        }
    }

    public PawnKindDef DryadKind => Mode?.pawnKindDef ?? PawnKindDefOf.Dryad_Basic;

    private float MinConnectionStrengthForSingleDryad
    {
        get
        {
            foreach (var point in Props.maxDryadsPerConnectionStrengthCurve.Points)
            {
                if (point.y > 0f)
                {
                    return point.x;
                }
            }

            return 0f;
        }
    }

    public float GetConnectionStrength(Pawn pawn = null)
    {
        if (pawn != null && connectionStrength.TryGetValue(pawn, out var value))
        {
            return value;
        }

        return ConnectionStrength;
    }

    public void SetConnectionStrength(Pawn pawn, float value)
    {
        lock (connectionStrength)
        {
            connectionStrength.SetOrAdd(pawn, value);
        }
    }

    public int GetLastPrunedTick(Pawn pawn)
    {
        return pawn != null ? lastPrunedTicks.TryGetValue(pawn) : 0;
    }

    public void SetLastPrunedTick(Pawn pawn, int value)
    {
        lock (lastPrunedTicks)
        {
            lastPrunedTicks.SetOrAdd(pawn, value);
        }
    }

    private float ClosestDistanceToBlockingBuilding(List<Thing> buildings)
    {
        var num = float.PositiveInfinity;
        foreach (var thing in buildings)
        {
            var num2 = thing.Position.DistanceTo(parent.Position);
            if (num2 < num)
            {
                num = num2;
            }
        }

        return num;
    }

    private void SpawnDryad()
    {
        spawnTick = Find.TickManager.TicksGame + (int)(GenDate.TicksPerDay * SpawnDays);
        var pawn = GenerateNewDryad(Props.pawnKind);
        GenSpawn.Spawn(pawn, parent.Position, parent.Map).Rotation = Rot4.South;
        EffecterDefOf.DryadSpawn.Spawn(parent.Position, parent.Map).Cleanup();
        SoundDefOf.Pawn_Dryad_Spawn.PlayOneShot(SoundInfo.InMap(pawn));
    }

    public Pawn GenerateNewDryad(PawnKindDef dryadCaste)
    {
        var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(dryadCaste, null,
            PawnGenerationContext.NonPlayer, -1, false, false, false, true, false, 1f, false, true, false, true, true,
            false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, Gender.Male, null,
            null, null, null, false, false, false, false, null, null, null, null, null, 0f,
            DevelopmentalStage.Newborn));
        ResetDryad(pawn);
        var connections = pawn.connections;
        connections?.ConnectTo(parent);

        dryads.Add(pawn);
        return pawn;
    }

    private void ResetDryad(Pawn dryad)
    {
        if (Connected && dryad.Faction != connectedPawns[0]?.Faction)
        {
            dryad.SetFaction(connectedPawns[0]?.Faction);
        }

        if (dryad.training == null)
        {
            return;
        }

        foreach (var allDef in DefDatabase<TrainableDef>.AllDefs)
        {
            if (!dryad.training.CanAssignToTrain(allDef).Accepted)
            {
                continue;
            }

            dryad.training.SetWantedRecursive(allDef, true);
            foreach (var connectedPawn in connectedPawns)
            {
                var num = 0;
                foreach (var dryad2 in dryads)
                {
                    if (dryad2?.playerSettings.Master == connectedPawn)
                    {
                        num++;
                    }
                }

                if ((int)Props.maxDryadsPerConnectionStrengthCurve.Evaluate(GetConnectionStrength(connectedPawn)) <=
                    num)
                {
                    continue;
                }

                dryad.training.Train(allDef, connectedPawn, true);
                if (allDef == TrainableDefOf.Release)
                {
                    dryad.playerSettings.followDrafted = true;
                }

                break;
            }
        }
    }

    public void Prune(Pawn pawn)
    {
        SetLastPrunedTick(pawn, Find.TickManager.TicksGame);
        var value = GetConnectionStrength(pawn) + (connectedPawns.Count * ConnectionStrengthGainPerHourOfPruning(pawn) /
                                                   GauranlenTreeSettings.PruningDuration);
        SetConnectionStrength(pawn, value);
        if (lastSubPlantTick >= Find.TickManager.TicksGame)
        {
            return;
        }

        parent.TryGetComp<CompSpawnSubplantDurationExpanded>()?.DoGrowSubplant(true);
        lastSubPlantTick = Find.TickManager.TicksGame + 15000;
    }

    public bool ShouldBePrunedNow(bool forced, Pawn pawn)
    {
        if (ConnectionStrength >= DesiredConnectionStrength || GetConnectionStrength(pawn) >= DesiredConnectionStrength)
        {
            return false;
        }

        if (forced)
        {
            return true;
        }

        if (ConnectionStrength >= DesiredConnectionStrength - 0.03f)
        {
            return false;
        }

        return Find.TickManager.TicksGame >= GetLastPrunedTick(pawn) + 10000;
    }

    public float ConnectionStrengthGainPerHourOfPruning(Pawn pawn)
    {
        var num = Props.connectionStrengthGainPerHourPruningBase * pawn.GetStatValue(StatDefOf.PruningSpeed);
        if (Props.connectionStrengthGainPerPlantSkill != null)
        {
            num *= Props.connectionStrengthGainPerPlantSkill.Evaluate(pawn.skills.GetSkill(SkillDefOf.Plants).Level);
        }

        return num;
    }

    public float PruningHoursToMaintain(float desired, Pawn pawn)
    {
        var num = Props.connectionLossPerLevelCurve.Evaluate(desired);
        var buildingsReducingConnectionStrength = BuildingsReducingConnectionStrength;
        if (buildingsReducingConnectionStrength.Any())
        {
            num += Props.connectionLossDailyPerBuildingDistanceCurve.Evaluate(
                ClosestDistanceToBlockingBuilding(buildingsReducingConnectionStrength));
        }

        return num / ConnectionStrengthGainPerHourOfPruning(pawn);
    }

    public float ConnectionStrengthLossPerDay(Pawn pawn)
    {
        var num = Props.connectionLossPerLevelCurve.Evaluate(connectionStrength.TryGetValue(pawn));
        var buildingsReducingConnectionStrength = BuildingsReducingConnectionStrength;
        if (parent.Spawned && buildingsReducingConnectionStrength.Any())
        {
            num += Props.connectionLossDailyPerBuildingDistanceCurve.Evaluate(
                ClosestDistanceToBlockingBuilding(buildingsReducingConnectionStrength));
        }

        return num / connectedPawns.Count;
    }

    public float ConnectionStrengthLossPerDayAll()
    {
        var num = 0f;
        foreach (var connectedPawn in connectedPawns)
        {
            num += ConnectionStrengthLossPerDay(connectedPawn);
        }

        return num;
    }

    private bool TryGetGaumakerCell(out IntVec3 cell)
    {
        cell = IntVec3.Invalid;
        return CellFinder.TryFindRandomCellNear(parent.Position, parent.Map, 3,
            c => GauranlenUtility.CocoonAndPodCellValidator(c, parent.Map, ThingDefOf.Plant_PodGauranlen),
            out cell) || CellFinder.TryFindRandomCellNear(parent.Position, parent.Map, 3,
            c => GauranlenUtility.CocoonAndPodCellValidator(c, parent.Map, ThingDefOf.Plant_TreeGauranlen),
            out cell);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (Connected)
        {
            var command_Action = new Command_Action
            {
                defaultLabel = "ChangeMode".Translate(),
                defaultDesc = "ChangeModeDesc".Translate(parent.Named("TREE")),
                icon = Mode == null
                    ? ContentFinder<Texture2D>.Get("UI/Gizmos/UpgradeDryads")
                    : Widgets.GetIconFor(Mode.pawnKindDef.race),
                action = delegate { Find.WindowStack.Add(new Dialog_ChangeDryadCasteExpanded(parent)); }
            };
            var pawnIsHere = false;
            foreach (var connectedPawn in connectedPawns)
            {
                if (connectedPawn.Spawned || connectedPawn.Map == parent.Map)
                {
                    pawnIsHere = true;
                }
            }

            if (!pawnIsHere)
            {
                command_Action.Disable(connectedPawns.Count > 1
                    ? "AllConnectedPawnsAreAway".Translate()
                    : "ConnectedPawnAway".Translate(connectedPawns[0].Named("PAWN")));
            }

            yield return command_Action;
            if (pruningGizmo == null)
            {
                pruningGizmo = new Gizmo_PruningConfigExpanded(this);
            }

            yield return pruningGizmo;
            if (dryads.Count > 0)
            {
                var command_Action2 = new Command_Action
                {
                    defaultLabel = "DefendTreeLabelExpanded".Translate(),
                    defaultDesc = "DefendTreeExpandedDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Draft"),
                    action = delegate
                    {
                        if (cachedLordJob != null)
                        {
                            return;
                        }

                        foreach (var dryad in dryads)
                        {
                            dryad.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        }

                        cachedLordJob = LordMaker.MakeNewLord(dryads[0].Faction,
                            new LordJob_DefendPoint(parent.Position, 5f), parent.Map, dryads);
                        lordJobCoolDown = Find.TickManager.TicksGame + 12000;
                    }
                };
                if (cachedLordJob != null)
                {
                    command_Action2.Disable("CooldownTime".Translate() + " " +
                                            (lordJobCoolDown - Find.TickManager.TicksGame).ToStringTicksToPeriod());
                }

                yield return command_Action2;
            }
        }
        else
        {
            yield return new Command_Action
            {
                defaultLabel =
                    (NoWildDryadSpawning
                        ? "NoWildDryadSpawningGauranlenTreeExpanded"
                        : "WildDryadSpawningGauranlenTreeExpanded").Translate(),
                defaultDesc = "NoWildDryadSpawningGauranlenTreeExpandedTT".Translate(),
                icon = ContentFinder<Texture2D>.Get(
                    NoWildDryadSpawning ? "UI/WildDryadDisabled" : "UI/WildDryadEnabled"),
                action = delegate { NoWildDryadSpawning = !NoWildDryadSpawning; }
            };
        }

        if (!Prefs.DevMode)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "DEV: Spawn dryad",
            action = SpawnDryad
        };
        yield return new Command_Action
        {
            defaultLabel = "DEV: Connection strength -10%",
            action = delegate
            {
                foreach (var connectedPawn2 in connectedPawns)
                {
                    SetConnectionStrength(connectedPawn2, GetConnectionStrength(connectedPawn2) - 0.1f);
                }
            }
        };
        yield return new Command_Action
        {
            defaultLabel = "DEV: Connection strength +10%",
            action = delegate
            {
                foreach (var connectedPawn3 in connectedPawns)
                {
                    SetConnectionStrength(connectedPawn3, GetConnectionStrength(connectedPawn3) + 0.1f);
                }
            }
        };
        yield return new Command_Action
        {
            defaultLabel = "DEV: spawn subplant",
            action = delegate { parent?.TryGetComp<CompSpawnSubplantDurationExpanded>()?.DoGrowSubplant(true); }
        };
    }

    public override void CompTick()
    {
        if (!ModsConfig.IdeologyActive || !parent.Spawned)
        {
            return;
        }

        var ticksGame = Find.TickManager.TicksGame;
        if (cachedLordJob != null && ticksGame > lordJobCoolDown)
        {
            for (var num = cachedLordJob.ownedPawns.Count - 1; num >= 0; num--)
            {
                cachedLordJob.Notify_PawnLost(cachedLordJob.ownedPawns[num], PawnLostCondition.Undefined);
            }

            cachedLordJob = null;
        }

        if (ticksGame >= spawnTick)
        {
            SpawnDryad();
        }

        if (Connected)
        {
            if (leafEffecter == null)
            {
                leafEffecter = EffecterDefOf.GauranlenLeavesBatch.Spawn();
                leafEffecter.Trigger((TargetInfo)parent, (TargetInfo)parent);
            }

            leafEffecter?.EffectTick(parent, parent);
            foreach (var connectedPawn in connectedPawns)
            {
                if (ticksGame - lastPrunedTicks.TryGetValue(connectedPawn) <= 1)
                {
                    continue;
                }

                var value = connectionStrength.TryGetValue(connectedPawn) -
                            (ConnectionStrengthLossPerDay(connectedPawn) / GenDate.TicksPerDay);
                SetConnectionStrength(connectedPawn, value);
            }
        }

        if (!parent.IsHashIntervalTick(300))
        {
            return;
        }

        if (Mode == GauranlenTreeModeDefOf.Gaumaker && dryads.Count >= 3)
        {
            if (gaumakerPod == null && TryGetGaumakerCell(out var cell))
            {
                gaumakerPod = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.GaumakerCocoon), cell, parent.Map);
            }
        }
        else if (gaumakerPod is { Destroyed: false })
        {
            gaumakerPod.Destroy();
            gaumakerPod = null;
        }
    }

    public string AffectingBuildingsDescription(string descKey)
    {
        var buildingsReducingConnectionStrength = BuildingsReducingConnectionStrength;
        if (buildingsReducingConnectionStrength.Count <= 0)
        {
            return null;
        }

        var source = buildingsReducingConnectionStrength.Select(c => GenLabel.ThingLabel(c, 1, false)).Distinct();
        var taggedString = descKey.Translate() + ": " + source.Take(3).ToCommaList().CapitalizeFirst();
        if (source.Count() > 3)
        {
            taggedString += " " + "Etc".Translate();
        }

        return taggedString;
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (!ModLister.CheckIdeology("Tree connection"))
        {
            parent.Destroy();
        }
        else
        {
            if (respawningAfterLoad)
            {
                return;
            }

            foreach (var connectedPawn in connectedPawns)
            {
                SetLastPrunedTick(connectedPawn, Find.TickManager.TicksGame);
            }

            spawnTick = Find.TickManager.TicksGame + SpawningDurationTicks;
        }
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        leafEffecter?.Cleanup();
        leafEffecter = null;
        for (var num = dryads.Count - 1; num >= 0; num--)
        {
            dryads[num].connections?.Notify_ConnectedThingDestroyed(parent);
            dryads[num].forceNoDeathNotification = true;
            dryads[num].Kill(null);
            dryads[num].forceNoDeathNotification = false;
        }

        if (!Connected || connectedPawns[0].Faction != Faction.OfPlayer)
        {
            return;
        }

        foreach (var connectedPawn in connectedPawns)
        {
            Find.LetterStack.ReceiveLetter("LetterLabelConnectedTreeDestroyed".Translate(parent.Named("TREE")),
                "LetterTextConnectedTreeDestroyed".Translate(parent.Named("TREE"),
                    connectedPawn.Named("CONNECTEDPAWN")), LetterDefOf.NegativeEvent, connectedPawn);
            connectedPawn?.connections?.Notify_ConnectedThingDestroyed(parent);
        }

        for (var num2 = connectedPawns.Count - 1; num2 >= 0; num2--)
        {
            TearConnection(connectedPawns[num2]);
        }
    }

    public void ConnectToPawn(Pawn pawn, float ritualQuality)
    {
        if (ConnectionTorn)
        {
            return;
        }

        connectedPawns.Add(pawn);
        pawn.connections?.ConnectTo(parent);
        SetConnectionStrength(pawn, Props.initialConnectionStrengthRange.LerpThroughRange(ritualQuality));
        SetLastPrunedTick(pawn, 0);
        foreach (var dryad in dryads)
        {
            ResetDryad(dryad);
            dryad.MentalState?.RecoverFromState();
        }
    }

    public void FinalizeMode(Pawn pawn)
    {
        currentMode = desiredMode;
        if (Connected)
        {
            MoteMaker.MakeStaticMote((pawn.Position.ToVector3Shifted() + parent.Position.ToVector3Shifted()) / 2f,
                parent.Map, ThingDefOf.Mote_GauranlenCasteChanged);
        }
    }

    public bool ShouldEnterGaumakerPod(Pawn dryad)
    {
        if (gaumakerPod == null || gaumakerPod.Destroyed)
        {
            return false;
        }

        if (dryads.NullOrEmpty() || dryads.Count < 3 || !dryads.Contains(dryad))
        {
            return false;
        }

        tmpDryads.Clear();
        foreach (var pawn in dryads)
        {
            if (pawn.kindDef == PawnKindDefOf.Dryad_Gaumaker)
            {
                tmpDryads.Add(pawn);
            }
        }

        if (tmpDryads.Count < 3)
        {
            tmpDryads.Clear();
            return false;
        }

        tmpDryads.SortBy(x => -x.ageTracker.AgeChronologicalTicks);
        for (var j = 0; j < 3; j++)
        {
            if (tmpDryads[j] != dryad)
            {
                continue;
            }

            tmpDryads.Clear();
            return true;
        }

        tmpDryads.Clear();
        return false;
    }

    public void RemoveDryad(Pawn oldDryad)
    {
        dryads.Remove(oldDryad);
    }

    public void TearConnection(Pawn pawn)
    {
        Messages.Message(
            "MessageConnectedPawnDied".Translate(parent.Named("TREE"), pawn.Named("PAWN"),
                ConnectionTornTicks.ToStringTicksToDays().Named("DURATION")), parent, MessageTypeDefOf.NegativeEvent);
        foreach (var dryad in dryads)
        {
            ResetDryad(dryad);
        }

        SoundDefOf.GauranlenConnectionTorn.PlayOneShot(SoundInfo.InMap(parent));
        nextUntornTick = Find.TickManager.TicksGame + ConnectionTornTicks;
        connectedPawns.Remove(pawn);
        lastPrunedTicks.Remove(pawn);
        connectionStrength.Remove(pawn);
        if (!Connected)
        {
            currentMode = null;
        }
    }

    public void Notify_PawnDied(Pawn p)
    {
        if (connectedPawns.Contains(p))
        {
            TearConnection(p);
        }
        else
        {
            if (!Connected)
            {
                return;
            }

            for (var i = 0; i < dryads.Count; i++)
            {
                if (p != dryads[i])
                {
                    continue;
                }

                foreach (var connectedPawn in connectedPawns)
                {
                    connectedPawn.needs?.mood?.thoughts?.memories.TryGainMemory(ThoughtDefOf.DryadDied);
                    var value = GetConnectionStrength(connectedPawn) - Props.connectionStrengthLossPerDryadDeath;
                    SetConnectionStrength(connectedPawn, value);
                }

                dryads.RemoveAt(i);
                break;
            }
        }
    }

    public override void PostDraw()
    {
        if (dryads.Count >= MaxDryads)
        {
            return;
        }

        var matrix = default(Matrix4x4);
        var pos = parent.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingBelowTop.AltitudeFor()) +
                  Props.spawningPodOffset;
        var num = Props.spawningPodSizeRange.LerpThroughRange(1f - spawnTick -
                                                              (Find.TickManager.TicksGame /
                                                               (float)SpawningDurationTicks));
        matrix.SetTRS(pos, Quaternion.identity, new Vector3(num, 1f, num));
        Graphics.DrawMesh(MeshPool.plane10, matrix, PodMat, 0);
    }

    public bool ShouldReturnToTree(Pawn dryad)
    {
        if (dryads.NullOrEmpty() || !dryads.Contains(dryad))
        {
            return false;
        }

        foreach (var pawn in connectedPawns)
        {
            if (dryad.connections != null && dryad.playerSettings.Master == pawn &&
                (int)Props.maxDryadsPerConnectionStrengthCurve.Evaluate(GetConnectionStrength(pawn)) <
                dryads.Count(x => x.playerSettings.Master == pawn))
            {
                return true;
            }
        }

        var num = dryads.Count - MaxDryads;
        if (num <= 0)
        {
            return false;
        }

        tmpDryads.Clear();
        tmpDryads.AddRange(dryads);
        tmpDryads.SortBy(x => x.kindDef == DryadKind, x => x.ageTracker.AgeChronologicalTicks);
        for (var i = 0; i < num; i++)
        {
            if (tmpDryads[i] != dryad)
            {
                continue;
            }

            tmpDryads.Clear();
            return true;
        }

        tmpDryads.Clear();
        return false;
    }

    public override void PostDrawExtraSelectionOverlays()
    {
        foreach (var connectedPawn in connectedPawns)
        {
            connectedPawn?.connections?.DrawConnectionLine(parent);
        }
    }

    public override string CompInspectStringExtra()
    {
        var text = base.CompInspectStringExtra();
        if (!text.NullOrEmpty())
        {
            text += "\n";
        }

        if (ConnectionTorn)
        {
            text =
                $"{text}{"ConnectionTorn".Translate(UntornInDurationTicks.ToStringTicksToPeriod()).Resolve()}\n";
        }

        var text2 = string.Empty;
        if (dryads.Count < MaxDryads)
        {
            text2 = "SpawningDryadIn".Translate(Props.pawnKind.Named("DRYAD"),
                (spawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod().Named("TIME")).Resolve();
        }

        if (Connected)
        {
            text = $"{text}{"ConnectedPawn".Translate().Resolve()}: ";
            for (var i = 0; i < connectedPawns.Count; i++)
            {
                text += connectedPawns[i].NameFullColored;
                text = i == connectedPawns.Count - 1 ? $"{text}." : $"{text}, ";
            }

            foreach (var connectedPawn in connectedPawns)
            {
                if (lastPrunedTicks.TryGetValue(connectedPawn) >= 0 &&
                    Find.TickManager.TicksGame - lastPrunedTicks.TryGetValue(connectedPawn) <= 60)
                {
                    text =
                        $"{text}\n{connectedPawn.NameShortColored}: {"PruningConnectionStrength".Translate()}: {"PerHour".Translate(ConnectionStrengthGainPerHourOfPruning(connectedPawn).ToStringPercent()).Resolve()}";
                }
            }

            if (HasProductionMode && Mode != desiredMode)
            {
                text = connectedPawns.Count != 1
                    ? $"{text}\n{"WaitingForConnectorsToChangeCaste".Translate().Resolve()}"
                    : $"{text}\n{"WaitingForConnectorToChangeCaste"
                        .Translate(connectedPawns[0].Named("CONNECTEDPAWN")).Resolve()}";
            }

            if (Mode != null)
            {
                text += $"\n{"GauranlenTreeMode".Translate()}: " + Mode.LabelCap;
            }

            if (!text2.NullOrEmpty())
            {
                text = $"{text}\n{text2}";
            }

            if (MaxDryads > 0)
            {
                text = $"{text}\n{"DryadPlural".Translate()} ({dryads.Count}/{MaxDryads})";
                if (dryads.Count > 0)
                {
                    text =
                        $"{text}: {dryads.Select(x => x.NameShortColored.Resolve()).ToCommaList().CapitalizeFirst()}";
                }
            }
            else
            {
                text =
                    $"{text}\n{"NotEnoughConnectionStrengthForSingleDryad".Translate(MinConnectionStrengthForSingleDryad.ToStringPercent()).Colorize(ColorLibrary.RedReadable)}";
            }

            if (!HasProductionMode)
            {
                text =
                    $"{text}\n{"AlertGauranlenTreeWithoutDryadTypeLabel".Translate().Colorize(ColorLibrary.RedReadable)}";
            }

            if (Mode == GauranlenTreeModeDefOf.Gaumaker && MaxDryads < 3)
            {
                text =
                    $"{text}\n{"ConnectionStrengthTooWeakForGaumakerPod".Translate().Colorize(ColorLibrary.RedReadable)}";
            }

            var text3 = AffectingBuildingsDescription("ConnectionStrengthAffectedBy");
            if (!text3.NullOrEmpty())
            {
                text = $"{text}\n{text3}";
            }
        }
        else if (!text2.NullOrEmpty())
        {
            text += text2;
        }

        return text.NullOrEmpty() ? "" : text.Trim();
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref NoWildDryadSpawning, "NoWildDryadSpawning");
        Scribe_Defs.Look(ref currentMode, "currentMode");
        Scribe_Defs.Look(ref desiredMode, "desiredMode");
        Scribe_Values.Look(ref nextUntornTick, "nextUntornTick", -1);
        Scribe_Values.Look(ref spawnTick, "spawnTick", -1);
        Scribe_Values.Look(ref DesiredConnectionStrength, "DesiredConnectionStrength", 0.5f);
        Scribe_Values.Look(ref lastSubPlantTick, "lastSubPlantTick", -1);
        Scribe_Deep.Look(ref cachedLordJob, "cachedLordJob");
        Scribe_Values.Look(ref lordJobCoolDown, "lordJobCoolDown");
        Scribe_References.Look(ref gaumakerPod, "gaumakerPod");
        Scribe_Collections.Look(ref lastPrunedTicks, "lastPrunedTicks", LookMode.Reference, LookMode.Value, ref list3,
            ref list2);
        Scribe_Collections.Look(ref connectionStrength, "connectionStrength", LookMode.Reference, LookMode.Value,
            ref list4, ref list1);
        Scribe_Collections.Look(ref connectedPawns, "connectedPawns", LookMode.Reference);
        Scribe_Collections.Look(ref dryads, "dryads", LookMode.Reference);
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        dryads.RemoveAll(x => x?.Dead ?? true);
        if (connectionStrength == null)
        {
            connectionStrength = new Dictionary<Pawn, float>();
        }

        if (lastPrunedTicks == null)
        {
            lastPrunedTicks = new Dictionary<Pawn, int>();
        }

        if (connectedPawns == null)
        {
            connectedPawns = [];
        }
    }
}