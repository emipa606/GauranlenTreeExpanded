using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GauranlenTreeExpanded;

[StaticConstructorOnStartup]
public class Gizmo_PruningConfigExpanded : Gizmo
{
    private const float Width = 212f;

    private static readonly Texture2D strengthTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Orange);

    private static readonly Texture2D strengthHighlightTex =
        SolidColorMaterials.NewSolidColorTexture(ColorLibrary.LightOrange);

    private static readonly Texture2D emptyBarTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

    private static readonly Texture2D strengthTargetTex =
        SolidColorMaterials.NewSolidColorTexture(ColorLibrary.DarkOrange);

    private readonly CompTreeConnectionExpanded connection;

    private readonly float extraHeight = Text.LineHeight * 1.5f;

    private bool draggingBar;

    private float selectedStrengthTarget = -1f;

    public Gizmo_PruningConfigExpanded(CompTreeConnectionExpanded connection)
    {
        this.connection = connection;
        base.Order = -100f;
    }

    private float DesiredConnectionStrength =>
        !draggingBar ? connection.desiredConnectionStrength : selectedStrengthTarget;

    private float OverrideHeight => 75f + extraHeight;

    public override float GetWidth(float maxWidth)
    {
        return Width;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        if (!ModsConfig.IdeologyActive)
        {
            return new GizmoResult(GizmoState.Clear);
        }

        var rect = new Rect(topLeft.x, topLeft.y - extraHeight, GetWidth(maxWidth), OverrideHeight);
        var rect2 = rect.ContractedBy(6f);
        Widgets.DrawWindowBackground(rect);
        var rect3 = rect2;
        var curY = rect3.yMin;
        Text.Anchor = TextAnchor.UpperCenter;
        Text.Font = GameFont.Small;
        Widgets.Label(rect3.x, ref curY, rect3.width, "ConnectionStrength".Translate());
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.Label(rect3.x, ref curY, rect3.width,
            "DesiredConnectionStrength".Translate() + ": " + DesiredConnectionStrength.ToStringPercent());
        var num = 0f;
        foreach (var connectedPawn in connection.ConnectedPawns)
        {
            num += connection.PruningHoursToMaintain(DesiredConnectionStrength, connectedPawn);
        }

        Widgets.Label(rect3.x, ref curY, rect3.width,
            "PruningHoursToMaintain".Translate() + ": " + (num / connection.ConnectedPawns.Count).ToString("F1"));
        Text.Font = GameFont.Small;
        if (Mouse.IsOver(rect2) && !draggingBar)
        {
            Widgets.DrawHighlight(rect2);
            TooltipHandler.TipRegion(rect2, getTip, 9493937);
        }

        drawBar(rect2, curY);
        return new GizmoResult(GizmoState.Clear);
    }

    private string getTip()
    {
        var text = connection.ConnectedPawns.Count == 1
            ? "DesiredConnectionStrengthDesc".Translate(connection.parent.Named("TREE"),
                    connection.ConnectedPawns[0].Named("CONNECTEDPAWN"),
                    connection.ConnectionStrengthLossPerDay(connection.ConnectedPawns[0]).ToStringPercent()
                        .Named("FALL"))
                .Resolve()
            : "DesiredConnectionStrengthDescNew".Translate(connection.parent.Named("TREE"),
                connection.ConnectionStrengthLossPerDayAll().ToStringPercent().Named("FALL")).Resolve();
        var text2 = connection.AffectingBuildingsDescription("CurrentlyAffectedBy");
        if (!text2.NullOrEmpty())
        {
            text = $"{text}\n\n{text2}";
        }

        return text;
    }

    private static void drawThreshold(Rect rect, float percent, float strValue)
    {
        var rect2 = default(Rect);
        rect2.x = rect.x + 3f + ((rect.width - 8f) * percent);
        rect2.y = rect.y + rect.height - 9f;
        rect2.width = 2f;
        rect2.height = 6f;
        var position = rect2;
        GUI.DrawTexture(position, strValue < percent ? BaseContent.GreyTex : BaseContent.BlackTex);
    }

    private static void drawStrengthTarget(Rect rect, float percent)
    {
        var num = Mathf.Round((rect.width - 8f) * percent);
        GUI.DrawTexture(new Rect(rect.x + 3f + num, rect.y, 2f, rect.height), strengthTargetTex);
        var num2 = UIScaling.AdjustCoordToUIScalingFloor(rect.x + 2f + num);
        var xMax = UIScaling.AdjustCoordToUIScalingCeil(num2 + 4f);
        var rect2 = default(Rect);
        rect2.y = rect.y - 3f;
        rect2.height = 5f;
        rect2.xMin = num2;
        rect2.xMax = xMax;
        var rect3 = rect2;
        GUI.DrawTexture(rect3, strengthTargetTex);
        var position = rect3;
        position.y = rect.yMax - 2f;
        GUI.DrawTexture(position, strengthTargetTex);
    }

    private void drawBar(Rect inRect, float curY)
    {
        var rect = inRect;
        rect.xMin += 10f;
        rect.xMax -= 10f;
        rect.yMax = inRect.yMax - 4f;
        rect.yMin = curY + 10f;
        var mouseIsOver = Mouse.IsOver(rect);
        var connectionStrength = connection.GetConnectionStrength();
        Widgets.FillableBar(rect, connectionStrength, mouseIsOver ? strengthHighlightTex : strengthTex, emptyBarTex,
            true);
        foreach (var point in connection.Props.maxDryadsPerConnectionStrengthCurve.Points)
        {
            if (point.x > 0f)
            {
                drawThreshold(rect, point.x, connectionStrength);
            }
        }

        var num = Mathf.Clamp(
            Mathf.Round((Event.current.mousePosition.x - (rect.x + 3f)) / (rect.width - 8f) * 20f) / 20f, 0f, 1f);
        var current2 = Event.current;
        if (current2.type == EventType.MouseDown && current2.button == 0 && mouseIsOver)
        {
            selectedStrengthTarget = num;
            draggingBar = true;
            SoundDefOf.DragSlider.PlayOneShotOnCamera();
            current2.Use();
        }

        if (current2.type == EventType.MouseDrag && current2.button == 0 && draggingBar && mouseIsOver)
        {
            if (Mathf.Abs(num - selectedStrengthTarget) > float.Epsilon)
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
            }

            selectedStrengthTarget = num;
            current2.Use();
        }

        if (current2.type == EventType.MouseUp && current2.button == 0 && draggingBar)
        {
            if (selectedStrengthTarget >= 0f)
            {
                connection.desiredConnectionStrength = selectedStrengthTarget;
            }

            selectedStrengthTarget = -1f;
            draggingBar = false;
            current2.Use();
        }

        drawStrengthTarget(rect, DesiredConnectionStrength);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(rect, connection.GetConnectionStrength().ToStringPercent());
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;
    }
}