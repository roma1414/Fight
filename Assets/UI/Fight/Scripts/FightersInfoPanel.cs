using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class FightersInfoPanel : MonoBehaviour
{
    private const int EffectColumns = 4;
    private const float IconSize = 30f;
    private const float IconSpacing = 4f;
    private const string HoverHint = "Hover over an effect to see details.";
    private static readonly float[] ColumnEdges = { 0f, .30f, .42f, .55f, .68f, 1f };

    private readonly List<GameObject> Rows = new List<GameObject>();
    private RectTransform Content;
    private ScrollRect Scroll;
    private TMP_Text EffectDescription;
    private TMP_FontAsset Font;

    public void Configure(Fight fight, Fighter selectedFighter, TMP_FontAsset font)
    {
        EnsureLayout(font);

        foreach (GameObject row in Rows)
        {
            row.SetActive(false);
            Destroy(row);
        }
        Rows.Clear();

        foreach (Fighter fighter in fight.GetAllFighters())
        {
            CreateRow(fight, fighter, fighter == selectedFighter);
        }

        ShowEffectDescription(null);
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
        Scroll.StopMovement();
        Scroll.verticalNormalizedPosition = 1f;
    }

    private void EnsureLayout(TMP_FontAsset font)
    {
        if (Content != null)
        {
            return;
        }

        Font = font;
        RectTransform header = CreateRect("Header", transform);
        Stretch(header, new Vector2(8f, -36f), new Vector2(-24f, -6f));
        header.anchorMin = new Vector2(0f, 1f);

        string[] headings = { "Fighter", "Overall", "Health", "Energy", "Effects" };
        for (int i = 0; i < headings.Length; i++)
        {
            TMP_Text label = CreateLabel(headings[i], header, 17f, Color.white);
            SetColumn(label.rectTransform, i);
            label.fontStyle = FontStyles.Bold;
            label.enableAutoSizing = false;
            // Align by font metrics so descenders in words like "Energy" do not shift the baseline.
            label.alignment = TextAlignmentOptions.CaplineLeft;
        }

        RectTransform scrollArea = CreateRect("FightersScrollView", transform);
        Stretch(scrollArea, new Vector2(8f, 44f), new Vector2(-8f, -42f));
        Scroll = scrollArea.gameObject.AddComponent<ScrollRect>();
        Scroll.horizontal = false;
        Scroll.movementType = ScrollRect.MovementType.Clamped;
        Scroll.scrollSensitivity = 30f;

        RectTransform viewport = CreateRect("Viewport", scrollArea);
        Stretch(viewport, Vector2.zero, new Vector2(-16f, 0f));
        viewport.gameObject.AddComponent<Image>().color = Color.clear;
        viewport.gameObject.AddComponent<RectMask2D>();

        Content = CreateRect("Content", viewport);
        Content.anchorMin = new Vector2(0f, 1f);
        Content.anchorMax = Vector2.one;
        Content.pivot = new Vector2(.5f, 1f);
        Content.sizeDelta = Vector2.zero;
        VerticalLayoutGroup layout = Content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 4f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        Content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        Scroll.viewport = viewport;
        Scroll.content = Content;

        RectTransform scrollbarRect = CreateRect("Scrollbar", scrollArea);
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = Vector2.one;
        scrollbarRect.pivot = Vector2.one;
        scrollbarRect.sizeDelta = new Vector2(10f, 0f);
        scrollbarRect.gameObject.AddComponent<Image>().color = new Color(.07f, .07f, .07f);
        Scrollbar scrollbar = scrollbarRect.gameObject.AddComponent<Scrollbar>();
        RectTransform handle = CreateRect("Handle", scrollbarRect);
        Stretch(handle, Vector2.zero, Vector2.zero);
        Image handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = new Color(.55f, .55f, .55f);
        scrollbar.handleRect = handle;
        scrollbar.targetGraphic = handleImage;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        Scroll.verticalScrollbar = scrollbar;

        EffectDescription = CreateLabel(HoverHint, transform, 17f, new Color(.75f, .75f, .75f));
        EffectDescription.name = "EffectDescription";
        RectTransform footer = EffectDescription.rectTransform;
        Stretch(footer, new Vector2(8f, 4f), new Vector2(-8f, 38f));
        footer.anchorMax = new Vector2(1f, 0f);
        EffectDescription.textWrappingMode = TextWrappingModes.Normal;
    }

    private void CreateRow(Fight fight, Fighter fighter, bool selected)
    {
        RectTransform row = CreateRect("FighterInfoRow", Content);
        Rows.Add(row.gameObject);
        Image background = row.gameObject.AddComponent<Image>();
        background.color = selected ? new Color(.20f, .28f, .38f) : new Color(.15f, .15f, .15f);
        background.raycastTarget = false;

        Color teamColor = fight.GetTeamColor(fighter.GetTeam());
        TMP_Text name = CreateLabel(fighter.GetName(), row, 22f, teamColor);
        SetColumn(name.rectTransform, 0);
        name.rectTransform.offsetMin = new Vector2(6f, 22f);
        name.rectTransform.offsetMax = new Vector2(-4f, -4f);
        name.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;

        bool alive = fighter.CheckIfAlive();
        TMP_Text state = CreateLabel($"Team {fighter.GetTeam()}" + (alive ? "" : " / Defeated"), row, 14f,
            alive ? new Color(.7f, .7f, .7f) : new Color(1f, .6f, .6f));
        SetColumn(state.rectTransform, 0);
        state.rectTransform.anchorMax = new Vector2(ColumnEdges[1], 0f);
        state.rectTransform.offsetMin = new Vector2(6f, 4f);
        state.rectTransform.offsetMax = new Vector2(-4f, 22f);

        TMP_Text overall = CreateLabel($"{fighter.GetOverallRating():F1}", row, 21f, Color.white);
        SetColumn(overall.rectTransform, 1);
        TMP_Text health = CreateLabel(fighter.GetHealth().ToString(), row, 21f, Color.white);
        SetColumn(health.rectTransform, 2);
        int maxMana = fighter.GetMaxMana();
        float energy = maxMana > 0 ? 100f * fighter.GetMana() / maxMana : 0f;
        TMP_Text mana = CreateLabel($"{energy:F0}", row, 21f, Color.white);
        SetColumn(mana.rectTransform, 3);

        RectTransform effects = CreateRect("Effects", row);
        SetColumn(effects, 4);
        effects.offsetMin = new Vector2(0f, 6f);
        effects.offsetMax = new Vector2(-4f, -6f);
        GridLayoutGroup grid = effects.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(IconSize, IconSize);
        grid.spacing = new Vector2(IconSpacing, IconSpacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = EffectColumns;
        grid.childAlignment = TextAnchor.MiddleLeft;

        int round = fight.GetRoundNumber();
        List<PowerUp> powerUps = fighter.GetPowerUps();
        if (powerUps != null)
        {
            foreach (PowerUp powerUp in powerUps)
            {
                if (powerUp == null)
                {
                    continue;
                }

                Move move = powerUp.GetPowerUpMove();
                int endingRound = powerUp.GetEndingRoundNumber();
                if (!IsPersistentEffect(move, endingRound) && endingRound <= round)
                {
                    continue;
                }

                string effectName = move != null ? move.GetName() : "Power Up";
                AddEffect(effects, move, GetInitials(effectName),
                    $"Power Up: {effectName}{GetDuration(move, endingRound, round)}", new Color(.45f, .85f, 1f));
            }
        }

        List<Status> statuses = fighter.GetStatuses();
        if (statuses != null)
        {
            foreach (Status status in statuses)
            {
                if (status == null)
                {
                    continue;
                }

                Move move = status.GetMove();
                int endingRound = status.GetEndingRoundNumber();
                if (!IsPersistentEffect(move, endingRound) && endingRound <= round)
                {
                    continue;
                }

                string effectName = Util.EnumToText(status.GetStatusType());
                string source = move != null ? $" ({move.GetName()})" : "";
                AddEffect(effects, move, GetInitials(effectName),
                    $"Status: {effectName}{source}{GetDuration(move, endingRound, round)}", new Color(1f, .7f, .4f));
            }
        }

        int effectRows = (effects.childCount + EffectColumns - 1) / EffectColumns;
        float effectHeight = effectRows * (IconSize + IconSpacing) - IconSpacing + 12f;
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = Mathf.Max(62f, effectHeight);
    }

    private void AddEffect(RectTransform parent, Move move, string abbreviation, string description, Color color)
    {
        RectTransform icon = CreateRect("EffectIcon", parent);
        icon.gameObject.AddComponent<FighterEffectIconUI>().Bind(
            FighterEffectIconUI.GetMoveIcon(move), abbreviation, description, color, Font, ShowEffectDescription);
    }

    private void ShowEffectDescription(string description)
    {
        if (EffectDescription != null)
        {
            EffectDescription.text = string.IsNullOrEmpty(description) ? HoverHint : description;
        }
    }

    private static bool IsPersistentEffect(Move move, int endingRound)
    {
        // Zero-duration moves stay active until explicitly removed by combat logic.
        return endingRound == int.MaxValue || (move != null && move.GetDuration() == 0);
    }

    private static string GetDuration(Move move, int endingRound, int round)
    {
        if (IsPersistentEffect(move, endingRound))
        {
            return " / Persistent";
        }

        int remaining = endingRound - round;
        return $" / {remaining} round{(remaining == 1 ? "" : "s")} remaining";
    }

    private static string GetInitials(string value)
    {
        string initials = "";
        foreach (string word in value.Split(' '))
        {
            if (word.Length > 0)
            {
                initials += char.ToUpperInvariant(word[0]);
            }
            if (initials.Length == 2)
            {
                break;
            }
        }
        return initials;
    }

    private TMP_Text CreateLabel(string value, Transform parent, float size, Color color)
    {
        RectTransform rect = CreateRect(value, parent);
        TMP_Text label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = Font;
        label.text = value;
        label.fontSize = size;
        label.fontSizeMax = size;
        label.fontSizeMin = size * .7f;
        label.enableAutoSizing = true;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.layer = parent.gameObject.layer;
        child.transform.SetParent(parent, false);
        return (RectTransform)child.transform;
    }

    private static void SetColumn(RectTransform rect, int column)
    {
        rect.anchorMin = new Vector2(ColumnEdges[column], 0f);
        rect.anchorMax = new Vector2(ColumnEdges[column + 1], 1f);
        rect.offsetMin = new Vector2(2f, 4f);
        rect.offsetMax = new Vector2(-4f, -4f);
    }

    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = min;
        rect.offsetMax = max;
    }
}
