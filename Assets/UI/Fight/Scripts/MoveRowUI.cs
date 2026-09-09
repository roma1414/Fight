using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveRowUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;

    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text LevelText;
    [SerializeField] private TMP_Text ManaText;
    [SerializeField] private TMP_Text TargetText;
    [SerializeField] private TMP_Text TypeText;

    [SerializeField] private Color normalColor =
        new Color(0.15f, 0.15f, 0.15f, 1f);

    [SerializeField] private Color selectedColor =
        new Color(0.2f, 0.45f, 0.8f, 1f);

    public Move Move { get; private set; }

    private Action<MoveRowUI> onSelected;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void Bind(Move move, Action<MoveRowUI> selectionCallback)
    {
        Move = move;
        onSelected = selectionCallback;

        NameText.text = move.GetName();
        LevelText.text = move.GetLevel().ToString();
        ManaText.text = move.GetMana().ToString();
        TargetText.text = move.GetTargetType().ToString();
        TypeText.text = move.GetMoveType().ToString();

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
