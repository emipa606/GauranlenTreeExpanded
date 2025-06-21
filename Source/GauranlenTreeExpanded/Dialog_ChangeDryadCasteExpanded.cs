using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GauranlenTreeExpanded;

internal class Dialog_ChangeDryadCasteExpanded : Window
{
    private const float HeaderHeight = 35f;

    private const float LeftRectWidth = 400f;

    private const float OptionSpacing = 52f;

    private const float ChangeFormButtonHeight = 55f;

    private static readonly Vector2 OptionSize = new(190f, 46f);

    private static readonly Vector2 ButSize = new(200f, 40f);

    private readonly List<GauranlenTreeModeDef> allDryadModes;

    private readonly Pawn connectedPawn;

    private readonly GauranlenTreeModeDef currentMode;
    private readonly CompTreeConnectionExpanded treeConnection;

    private float rightViewWidth;

    private Vector2 scrollPosition;

    private GauranlenTreeModeDef selectedMode;

    public Dialog_ChangeDryadCasteExpanded(Thing tree)
    {
        treeConnection = tree.TryGetComp<CompTreeConnectionExpanded>();
        currentMode = treeConnection.desiredMode;
        selectedMode = currentMode;
        connectedPawn = treeConnection.ConnectedPawns[0];
        forcePause = true;
        closeOnAccept = false;
        doCloseX = true;
        doCloseButton = true;
        allDryadModes = DefDatabase<GauranlenTreeModeDef>.AllDefs.ToList();
    }

    public override Vector2 InitialSize => new(Mathf.Min(900, UI.screenWidth), 650f);

    private PawnKindDef SelectedKind => selectedMode.pawnKindDef;

    public override void PreOpen()
    {
        if (!ModLister.CheckIdeology("Dryad upgrades"))
        {
            Close();
        }

        base.PreOpen();
        setupView();
    }

    private void setupView()
    {
        foreach (var allDryadMode in allDryadModes)
        {
            rightViewWidth = Mathf.Max(rightViewWidth, getPosition(allDryadMode, InitialSize.y).x + OptionSize.x);
        }

        rightViewWidth += 20f;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        string label = selectedMode?.LabelCap ?? "ChangeMode".Translate();
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, HeaderHeight), label);
        Text.Font = GameFont.Small;
        var num = inRect.y + HeaderHeight + 10f;
        var curY = num;
        var num2 = inRect.height - num;
        num2 -= ButSize.y + 10f;
        drawLeftRect(new Rect(inRect.xMin, num, LeftRectWidth, num2), ref curY);
        drawRightRect(new Rect(inRect.x + LeftRectWidth + 17f, num, inRect.width - LeftRectWidth - 17f, num2));
    }

    private void drawLeftRect(Rect rect, ref float curY)
    {
        var rect2 = new Rect(rect.x, curY, rect.width, rect.height)
        {
            yMax = rect.yMax
        };
        var rect3 = rect2.ContractedBy(4f);
        if (selectedMode == null)
        {
            Widgets.Label(rect3,
                "ChooseProductionModeInitialDesc".Translate(connectedPawn.Named("PAWN"),
                    treeConnection.parent.Named("TREE"),
                    ThingDefOf.DryadCocoon.GetCompProperties<CompProperties_DryadCocoon>().daysToComplete
                        .Named("UPGRADEDURATION")));
            return;
        }

        Widgets.Label(rect3.x, ref curY, rect3.width, selectedMode.Description);
        curY += 10f;
        if (!selectedMode.requiredMemes.NullOrEmpty())
        {
            Widgets.Label(rect3.x, ref curY, rect3.width, "RequiredMemes".Translate() + ":");
            var text = "";
            foreach (var memeDef in selectedMode.requiredMemes)
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n";
                }

                text =
                    $"{text}  - {memeDef.LabelCap.ToString().Colorize(connectedPawn.Ideo.HasMeme(memeDef) ? Color.white : ColorLibrary.RedReadable)}";
            }

            Widgets.Label(rect3.x, ref curY, rect3.width, text);
            curY += 10f;
        }

        if (selectedMode.previousStage != null)
        {
            Widgets.Label(rect3.x, ref curY, rect3.width,
                $"{"RequiredStage".Translate()}: {selectedMode.previousStage.pawnKindDef.LabelCap.ToString().Colorize(Color.white)}");
            curY += 10f;
        }

        if (selectedMode.displayedStats != null)
        {
            foreach (var statDef in selectedMode.displayedStats)
            {
                Widgets.Label(rect3.x, ref curY, rect3.width,
                    statDef.LabelCap + ": " + statDef.ValueToString(SelectedKind.race.GetStatValueAbstract(statDef),
                        statDef.toStringNumberSense));
            }

            curY += 10f;
        }

        if (selectedMode.hyperlinks != null)
        {
            foreach (var item in Dialog_InfoCard.DefsToHyperlinks(selectedMode.hyperlinks))
            {
                Widgets.HyperlinkWithIcon(new Rect(rect3.x, curY, rect3.width, Text.LineHeight), item);
                curY += Text.LineHeight;
            }

            curY += 10f;
        }

        var rect4 = new Rect(rect3.x, rect3.yMax - ChangeFormButtonHeight, rect3.width, ChangeFormButtonHeight);
        if (meetsRequirements(selectedMode) && selectedMode != currentMode)
        {
            if (!Widgets.ButtonText(rect4, "Accept".Translate()))
            {
                return;
            }

            var window = Dialog_MessageBox.CreateConfirmation(
                "GauranlenModeChangeDescFull".Translate(treeConnection.parent.Named("TREE"),
                    connectedPawn.Named("CONNECTEDPAWN"),
                    ThingDefOf.DryadCocoon.GetCompProperties<CompProperties_DryadCocoon>().daysToComplete
                        .Named("DURATION")), startChange);
            Find.WindowStack.Add(window);
        }
        else
        {
            string label = selectedMode == currentMode ? "AlreadySelected".Translate() :
                !meetsMemeRequirements(selectedMode) ? (string)"MissingRequiredMemes".Translate() :
                selectedMode.previousStage == null || currentMode == selectedMode.previousStage ? "Locked".Translate() :
                (string)("Locked".Translate() + ": " + "MissingRequiredCaste".Translate());
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawHighlight(rect4);
            Widgets.Label(rect4.ContractedBy(5f), label);
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }

    private void startChange()
    {
        treeConnection.desiredMode = selectedMode;
        SoundDefOf.GauranlenProductionModeSet.PlayOneShotOnCamera();
        Close(false);
    }

    private void drawRightRect(Rect rect)
    {
        Widgets.DrawMenuSection(rect);
        var rect2 = new Rect(0f, 0f, rightViewWidth, rect.height - 16f);
        var rect3 = rect2.ContractedBy(10f);
        Widgets.ScrollHorizontal(rect, ref scrollPosition, rect2);
        Widgets.BeginScrollView(rect, ref scrollPosition, rect2);
        GUI.BeginGroup(rect3);
        drawDependencyLines(rect3);
        foreach (var allDryadMode in allDryadModes)
        {
            drawDryadStage(rect3, allDryadMode);
        }

        GUI.EndGroup();
        Widgets.EndScrollView();
    }

    private bool meetsMemeRequirements(GauranlenTreeModeDef stage)
    {
        if (stage.requiredMemes.NullOrEmpty())
        {
            return true;
        }

        foreach (var requiredMeme in stage.requiredMemes)
        {
            if (!treeConnection.ConnectedPawns.Any(x => x.Ideo.HasMeme(requiredMeme)))
            {
                return false;
            }
        }

        return true;
    }

    private bool meetsRequirements(GauranlenTreeModeDef mode)
    {
        if (mode.previousStage != null && currentMode != mode.previousStage)
        {
            return false;
        }

        return meetsMemeRequirements(mode);
    }

    private Color getBoxColor(GauranlenTreeModeDef mode)
    {
        var result = TexUI.AvailResearchColor;
        if (mode == currentMode)
        {
            result = TexUI.ActiveResearchColor;
        }
        else if (!meetsRequirements(mode))
        {
            result = TexUI.LockedResearchColor;
        }

        if (selectedMode == mode)
        {
            result += TexUI.HighlightBgResearchColor;
        }

        return result;
    }

    private Color getBoxOutlineColor(GauranlenTreeModeDef mode)
    {
        if (selectedMode != null && selectedMode == mode)
        {
            return TexUI.HighlightBorderResearchColor;
        }

        return TexUI.DefaultBorderResearchColor;
    }

    private Color getTextColor(GauranlenTreeModeDef mode)
    {
        return !meetsRequirements(mode) ? ColorLibrary.RedReadable : Color.white;
    }

    private void drawDependencyLines(Rect fullRect)
    {
        foreach (var allDryadMode in allDryadModes)
        {
            if (allDryadMode.previousStage != null)
            {
                drawLineBetween(allDryadMode, allDryadMode.previousStage, fullRect.height,
                    TexUI.DefaultLineResearchColor);
            }
        }

        foreach (var allDryadMode2 in allDryadModes)
        {
            if (allDryadMode2.previousStage != null &&
                (allDryadMode2.previousStage == selectedMode || selectedMode == allDryadMode2))
            {
                drawLineBetween(allDryadMode2, allDryadMode2.previousStage, fullRect.height,
                    TexUI.HighlightLineResearchColor, 3f);
            }
        }
    }

    private void drawDryadStage(Rect rect, GauranlenTreeModeDef stage)
    {
        var position = getPosition(stage, rect.height);
        var rect2 = new Rect(position.x, position.y, OptionSize.x, OptionSize.y);
        Widgets.DrawBoxSolidWithOutline(rect2, getBoxColor(stage), getBoxOutlineColor(stage));
        var rect3 = new Rect(rect2.x, rect2.y, rect2.height, rect2.height);
        Widgets.DefIcon(rect3.ContractedBy(4f), stage.pawnKindDef);
        GUI.color = getTextColor(stage);
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(new Rect(rect3.xMax, rect2.y, rect2.width - rect3.width, rect2.height).ContractedBy(4f),
            stage.LabelCap);
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;
        if (!Widgets.ButtonInvisible(rect2))
        {
            return;
        }

        selectedMode = stage;
        SoundDefOf.Click.PlayOneShotOnCamera();
    }

    private void drawLineBetween(GauranlenTreeModeDef left, GauranlenTreeModeDef right, float height, Color color,
        float width = 2f)
    {
        var start = getPosition(left, height) + new Vector2(5f, OptionSize.y / 2f);
        var end = getPosition(right, height) + (OptionSize / 2f);
        Widgets.DrawLine(start, end, color, width);
    }

    private static Vector2 getPosition(GauranlenTreeModeDef stage, float height)
    {
        return new Vector2((stage.drawPosition.x * OptionSize.x) + (stage.drawPosition.x * OptionSpacing),
            (height - OptionSize.y) * stage.drawPosition.y);
    }
}