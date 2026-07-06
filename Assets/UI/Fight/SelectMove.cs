using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectMove : MonoBehaviour
{
    [SerializeField] UIDocument         uiDocument;
    [SerializeField] VisualTreeAsset    MoveRowTemplate;
    protected VisualElement             root;
    protected List<Move>                Moves;
    [SerializeField] protected Fight    Fight;
    [SerializeField] protected Fighter  SelectedFighter;
    protected bool                      SortDescending = true;
    protected VisualElement             CurrentlySelectedTab;
    protected VisualElement             CurrentlySelectedSubTab;
    protected VisualElement             OffensiveTab, MedicalTab, PowerUpTab, SummonTab, SubTab, ProtectTab, NameTab, LevelTab, ManaTab, TargetTab, TypeTab;
    protected ListView                  MovesListView;
    protected Label                     NameLabel, RoundLabel, HealthLabel, ManaLabel;
    protected Button                    AdvanceButton;
    protected Move                      SelectedMove;
    protected bool                      Advance = false;

    void BindMoveItem(VisualElement element, int index)
    {
        Move move = Moves[index];

        element.Q<Label>("name-label").text = move.GetName();
        element.Q<Label>("level-label").text = move.GetLevel().ToString();//$"HP: {move.GetLevel()}";
        element.Q<Label>("mana-label").text = move.GetMana().ToString();
        element.Q<Label>("target-label").text = move.GetTargetType().ToString();
        element.Q<Label>("type-label").text = move.GetMoveType().ToString();//$"HP: {move.GetLevel()}";
    }

    public void ConfigureForFighter(Fighter fighter)
    {
        SelectedFighter = fighter;

        NameLabel.text = SelectedFighter.GetName();
        RoundLabel.text = $"Round: {Fight.GetRoundNumber()}";
        HealthLabel.text = $"Health: {SelectedFighter.GetHealth()}";
        ManaLabel.text = $"Mana: {SelectedFighter.GetMana()}";

        OnTabClickEvent(CurrentlySelectedTab);
        // OnSubTabClickEvent will invert sort direction, so we invert it first
        SortDescending = !SortDescending;
        OnSubTabClickEvent(CurrentlySelectedSubTab);
    }
    
    public void ConfigureMovesListView()
    {
        MovesListView.itemsSource = Moves;
        MovesListView.makeItem = MakeMoveItem;
        MovesListView.bindItem = BindMoveItem;
        MovesListView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
        MovesListView.selectionType = SelectionType.Single;
        SortMoves();
    }

    public List<Move> GetPossibleMoves(Enums.MoveType moveType)
    {
        List<Move> moves = SelectedFighter.GetMoves(moveType);
        List<Move> possibleMoves = new List<Move>();
        foreach(Move move in moves)
        {
            if (SelectedFighter.GetAI().CheckIfCanPerformMove(Fight, SelectedFighter, move))
            {
                possibleMoves.Add(move);
            }
        }
        
        return possibleMoves;
    }

    public MoveEvent GetUserMoveEvent(Fighter fighter)
    {
        SelectedFighter = fighter;
        ConfigureForFighter(fighter);

        MoveEvent moveEvent = new MoveEvent();
        moveEvent.AddFighter(SelectedFighter);
        moveEvent.AddRandomAdd(Fight.RandomAdd());

        moveEvent.SetMoveType(Enums.MoveType.Offensive);

        Advance = false;
        StartCoroutine(WaitForSelection());

        moveEvent.AddMove(SelectedMove);

        moveEvent.SetTargetType(Enums.TargetType.OneEnemy);
        List<Fighter> enemies = AI.GetEnemies(Fight, SelectedFighter);
        moveEvent.AddTarget(enemies[Random.Range(0, enemies.Count)]);

        return moveEvent;
    }

    VisualElement MakeMoveItem() { return MoveRowTemplate.Instantiate(); }

    public void OnAdvanceClickEvent()
    {
        if (MovesListView.selectedItem != null)
        {
            SelectedMove = (Move)MovesListView.selectedItem;
            Advance = true;
        }
    }

    public void OnSubTabClickEvent(VisualElement clickedTab)
    {
        if (clickedTab == CurrentlySelectedSubTab)
        {
            SortDescending = !SortDescending;
        }
        else
        {
            if (CurrentlySelectedSubTab != null)
            {
                CurrentlySelectedSubTab.RemoveFromClassList("selected");
            }

            CurrentlySelectedSubTab = clickedTab;
            CurrentlySelectedSubTab.AddToClassList("selected");
        }

        SortMoves();
    }

    public void OnTabClickEvent(VisualElement clickedTab)
    {
        if (CurrentlySelectedTab != null)
        {
            CurrentlySelectedTab.RemoveFromClassList("selected");
        }

        CurrentlySelectedTab = clickedTab;
        CurrentlySelectedTab.AddToClassList("selected");

        Enums.MoveType moveType = Enums.MoveType.Offensive;
        switch (CurrentlySelectedTab.name)
        {
            case "OffensiveTab":
                moveType = Enums.MoveType.Offensive;
                break;
            case "MedicalTab":
                moveType = Enums.MoveType.Medical;
                break;
            case "PowerUpTab":
                moveType = Enums.MoveType.PowerUp;
                break;
            case "SummonTab":
                moveType = Enums.MoveType.Summon;
                break;
            case "SubTab":
                moveType = Enums.MoveType.Substitution;
                break;
            case "ProtectTab":
                moveType = Enums.MoveType.Protect;
                break;
            default:
                Debug.LogError("Error! Unexpected CurrentlySelectedTab.name in UpdateMovesForTab!");
                break;
        }

        Moves = GetPossibleMoves(moveType);
        MovesListView.itemsSource = Moves;
        SortMoves();
    }

    public void SortMoves()
    {
        switch (CurrentlySelectedSubTab.name)
        {
            case "NameTab":
                {
                    if (SortDescending)
                    {
                        Moves.Sort((left, right) => right.GetName().CompareTo(left.GetName())); // Sort in descending order. I think...
                    }
                    else
                    {
                        Moves.Sort((left, right) => left.GetName().CompareTo(right.GetName())); // Sort in ascending order. I think...
                    }
                    break;
                }
            case "LevelTab":
                {
                    if (SortDescending)
                    {
                        Moves.Sort((left, right) => right.GetLevel().CompareTo(left.GetLevel())); // Sort in descending order. I think...
                    }
                    else
                    {
                        Moves.Sort((left, right) => left.GetLevel().CompareTo(right.GetLevel())); // Sort in ascending order. I think...
                    }
                    break;
                }
            case "ManaTab":
                {
                    if (SortDescending)
                    {
                        Moves.Sort((left, right) => right.GetMana().CompareTo(left.GetMana())); // Sort in descending order. I think...
                    }
                    else
                    {
                        Moves.Sort((left, right) => left.GetMana().CompareTo(right.GetMana())); // Sort in ascending order. I think...
                    }
                    break;
                }
            case "TargetTab":
                {
                    if (SortDescending)
                    {
                        Moves.Sort((left, right) => right.GetTargetType().CompareTo(left.GetTargetType())); // Sort in descending order. I think...
                    }
                    else
                    {
                        Moves.Sort((left, right) => left.GetTargetType().CompareTo(right.GetTargetType())); // Sort in ascending order. I think...
                    }
                    break;
                }
            case "TypeTab":
                {
                    if (SortDescending)
                    {
                        Moves.Sort((left, right) => right.GetMoveType().CompareTo(left.GetMoveType())); // Sort in descending order. I think...
                    }
                    else
                    {
                        Moves.Sort((left, right) => left.GetMoveType().CompareTo(right.GetMoveType())); // Sort in ascending order. I think...
                    }
                    break;
                }
            default:
                Debug.LogError("Error! Unexpected CurrentlySelectedSubTab.name in SortMoves!");
                break;
        }

        MovesListView.Rebuild();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = uiDocument.rootVisualElement;

        OffensiveTab = root.Q<VisualElement>("OffensiveTab");
        OffensiveTab.RegisterCallback<ClickEvent>(evt => OnTabClickEvent(OffensiveTab));
        MedicalTab = root.Q<VisualElement>("MedicalTab");
        MedicalTab.RegisterCallback<ClickEvent>(evt => OnTabClickEvent(MedicalTab));
        PowerUpTab = root.Q<VisualElement>("PowerUpTab");
        PowerUpTab.RegisterCallback<ClickEvent>(evt => OnTabClickEvent(PowerUpTab));
        SummonTab = root.Q<VisualElement>("SummonTab");
        SummonTab.RegisterCallback<ClickEvent>(evt => OnTabClickEvent(SummonTab));
        SubTab = root.Q<VisualElement>("SubTab");
        SubTab.RegisterCallback<ClickEvent>(evt => OnTabClickEvent(SubTab));
        ProtectTab = root.Q<VisualElement>("ProtectTab");
        ProtectTab.RegisterCallback<ClickEvent>(evt => OnTabClickEvent(ProtectTab));
        CurrentlySelectedTab = OffensiveTab;
        CurrentlySelectedTab.AddToClassList("selected");
        //Moves = GetPossibleMoves(Enums.MoveType.Offensive);
        Moves = SelectedFighter.GetMoves(Enums.MoveType.Offensive);

        NameTab = root.Q<VisualElement>(name:"NameTab");
        NameTab.RegisterCallback<ClickEvent>(evt => OnSubTabClickEvent(NameTab));
        LevelTab = root.Q<VisualElement>("LevelTab");
        LevelTab.RegisterCallback<ClickEvent>(evt => OnSubTabClickEvent(LevelTab));
        ManaTab = root.Q<VisualElement>("ManaTab");
        ManaTab.RegisterCallback<ClickEvent>(evt => OnSubTabClickEvent(ManaTab));
        TargetTab = root.Q<VisualElement>("TargetTab");
        TargetTab.RegisterCallback<ClickEvent>(evt => OnSubTabClickEvent(TargetTab));
        TypeTab = root.Q<VisualElement>("TypeTab");
        TypeTab.RegisterCallback<ClickEvent>(evt => OnSubTabClickEvent(TypeTab));
        CurrentlySelectedSubTab = LevelTab;
        CurrentlySelectedSubTab.AddToClassList("selected");

        MovesListView = root.Q<ListView>("MovesListView");        
        /*MovesListView.selectionChanged += selectedItems =>
        {
            foreach (Move move in selectedItems)
            {
                Debug.Log($"Selected: {move.GetName()}");
            }
        };*/
        ConfigureMovesListView();

        NameLabel = root.Q<Label>(name:"NameLabel");
        NameLabel.text = SelectedFighter.GetName();
        RoundLabel = root.Q<Label>(name:"RoundLabel");
        RoundLabel.text = $"Round: {Fight.GetRoundNumber()}";
        HealthLabel = root.Q<Label>(name:"HealthLabel");
        HealthLabel.text = "Health: 100";
        ManaLabel = root.Q<Label>(name:"ManaLabel");
        ManaLabel.text = "Mana: 100";

        AdvanceButton = root.Q<Button>("AdvanceButton");
        AdvanceButton.RegisterCallback<ClickEvent>(evt => OnAdvanceClickEvent());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator WaitForSelection()
    {
        yield return new WaitUntil(() => Advance == true);
    }
}
