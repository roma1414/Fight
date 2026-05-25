using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectMove : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    protected VisualElement root;
    protected List<Move> Moves;
    protected Fighter SelectedFighter;
    protected bool SortDescending = true;
    protected Enums.SelectMoveSortType SortType;

    public void OnLevelClickEvent(ClickEvent evt)
    {
        if (SortType == Enums.SelectMoveSortType.Level)
        {
            SortDescending = SortDescending ? false : true;
        }
        else
        {
            SortType = Enums.SelectMoveSortType.Level;
        }

        SortMoves();
    }

    public void OnMedicalClickEvent(ClickEvent evt)
    {
        List<MedicalMove> medicalMoves = SelectedFighter.GetMedicalMoves();
        List<Move> medicalMovesResult = new List<Move>(medicalMoves.Count);
        medicalMovesResult.AddRange(medicalMoves);
        Moves = medicalMovesResult;
        SortMoves();
    }
    
    public void OnOffensiveClickEvent(ClickEvent evt)
    {
        List<OffensiveMove> offensiveMoves = SelectedFighter.GetOffensiveMoves();
        List<Move> offensiveMovesResult = new List<Move>(offensiveMoves.Count);
        offensiveMovesResult.AddRange(offensiveMoves);
        Moves = offensiveMovesResult;
        SortMoves();
    }

    public void OnPowerUpClickEvent(ClickEvent evt)
    {
        List<OffensiveMove> offensiveMoves = SelectedFighter.GetOffensiveMoves();
        List<Move> offensiveMovesResult = new List<Move>(offensiveMoves.Count);
        offensiveMovesResult.AddRange(offensiveMoves);
        Moves = offensiveMovesResult;
        SortMoves();
    }

    public void SortMoves()
    {
        switch (SortType)
        {
            case Enums.SelectMoveSortType.Level:
                {
                    if (SortDescending)
                    {
                        Moves.Sort((left, right) => left.GetLevel().CompareTo(right.GetLevel())); // Sort in ascending order. I think...
                    }
                    else
                    {
                        Moves.Sort((left, right) => right.GetLevel().CompareTo(left.GetLevel())); // Sort in descending order. I think...
                    }
                    break;
                }
            case Enums.SelectMoveSortType.Mana:
                {
                    if (SortDescending)
                    {
                        Moves.Sort((left, right) => left.GetMana().CompareTo(right.GetMana())); // Sort in ascending order. I think...
                    }
                    else
                    {
                        Moves.Sort((left, right) => right.GetMana().CompareTo(left.GetMana())); // Sort in descending order. I think...
                    }
                    break;
                }
            case Enums.SelectMoveSortType.TargetType:
                {
                    if (SortDescending)
                    {
                        Moves.Sort((left, right) => left.GetTargetType().CompareTo(right.GetTargetType())); // Sort in ascending order. I think...
                    }
                    else
                    {
                        Moves.Sort((left, right) => right.GetTargetType().CompareTo(left.GetTargetType())); // Sort in descending order. I think...
                    }
                    break;
                }
            case Enums.SelectMoveSortType.MoveType:
                {
                    if (SortDescending)
                    {
                        Moves.Sort((left, right) => left.GetMoveType().CompareTo(right.GetMoveType())); // Sort in ascending order. I think...
                    }
                    else
                    {
                        Moves.Sort((left, right) => right.GetMoveType().CompareTo(left.GetMoveType())); // Sort in descending order. I think...
                    }
                    break;
                }
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = uiDocument.rootVisualElement;

        //var offensive = root.Q<VisualElement>("OffensiveTab");
        //offensive.RegisterCallback<ClickEvent>(OnOffensiveClickEvent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
