using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetRowUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text LevelText;
    [SerializeField] private TMP_Text HealthText;
    [SerializeField] private TMP_Text ManaText;

    [SerializeField] private Color normalColor =
        new Color(0.15f, 0.15f, 0.15f, 1f);

    [SerializeField] private Color selectedColor =
        new Color(0.2f, 0.45f, 0.8f, 1f);

    public Target Target { get; private set; }

    private Action<TargetRowUI> onSelected;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void Bind(Target target, Action<TargetRowUI> selectionCallback)
    {
        Target = target;
        onSelected = selectionCallback;
        Enums.TargetType targetType = target.GetTargetType();

        if (targetType == Enums.TargetType.Self ||
            targetType == Enums.TargetType.OneEnemy ||
            targetType == Enums.TargetType.OneTeamMember ||
            targetType == Enums.TargetType.EnemiesWithStatuses ||
            targetType == Enums.TargetType.TeamMembersWithStatuses)
        {
            NameText.text = target.GetName();
            LevelText.text = $"Level: {target.GetLevel().ToString()}";
            HealthText.text = $"Health: {target.GetHealth().ToString()}";
            ManaText.text = $"Mana: {target.GetMana().ToString()}";
        }
        else
        {
            NameText.text = target.GetName();
            LevelText.text = "";
            HealthText.text = "";
            ManaText.text = "";
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        background.color = selected ? selectedColor : normalColor;
    }

    private void HandleClick()
    {
        onSelected?.Invoke(this);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(HandleClick);
    }
}
