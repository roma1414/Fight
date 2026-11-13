using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetRowUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text OverallText;
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

        switch (targetType)
        {
            case Enums.TargetType.Self:
            case Enums.TargetType.Enemy:
            case Enums.TargetType.TeamMember:
            case Enums.TargetType.EnemiesWithStatuses:
            case Enums.TargetType.TeamMembersWithStatuses:
                {
                    NameText.text = target.GetName();
                    OverallText.text = $"Ovr: {target.GetOverall().ToString()}";
                    HealthText.text = $"Health: {target.GetHealth().ToString()}";
                    ManaText.text = $"Energy: {target.GetMana().ToString()}";
                    break;
                }
            case Enums.TargetType.DeadFighter:
                {
                    NameText.text = target.GetName();
                    OverallText.text = $"Ovr: {target.GetOverall().ToString()}";
                    HealthText.text = $"Health: --";
                    ManaText.text = $"Energy: --";
                    break;
                }
            default:
                {
                    NameText.text = target.GetName();
                    OverallText.text = "";
                    HealthText.text = "";
                    ManaText.text = "";
                    break;
                }
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
