using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using System.Threading.Tasks;

public class SelectMove : MonoBehaviour
{
    [SerializeField] UIDocument         uiDocument;
    [SerializeField] VisualTreeAsset    MoveRowTemplate, TargetRowTemplate;
    protected VisualElement             root;
    protected List<Move>                Moves;
    protected List<Target>              Targets;
    [SerializeField] protected Fight    Fight;
    [SerializeField] protected Fighter  SelectedFighter;
    protected bool                      SortDescending = true;
    protected VisualElement             CurrentlySelectedTab;
    protected VisualElement             CurrentlySelectedSubTab;
    protected VisualElement             OffensiveTab, MedicalTab, PowerUpTab, SummonTab, SubTab, ProtectTab, NameTab, LevelTab, ManaTab, TargetTab, TypeTab;
    protected ListView                  MovesListView, TargetsListView;
    protected Label                     NameLabel, RoundLabel, HealthLabel, ManaLabel;
    protected Button                    AdvanceButton;
    protected Move                      SelectedMove;
    protected Target                    SelectedTarget;
    protected bool                      Advance = false;
    protected Image                     Portrait;

    public async void AnimatePortrait()
    {
        Portrait.AddToClassList("PortraitQuote");
        await Task.Delay(2000);
    }

    void BindMoveItem(VisualElement element, int index)
    {
        Move move = Moves[index];

        element.Q<Label>("name-label").text = move.GetName();
        element.Q<Label>("level-label").text = move.GetLevel().ToString();//$"HP: {move.GetLevel()}";
        element.Q<Label>("mana-label").text = move.GetMana().ToString();
        element.Q<Label>("target-label").text = move.GetTargetType().ToString();
        element.Q<Label>("type-label").text = move.GetMoveType().ToString();//$"HP: {move.GetLevel()}";
    }

    void BindTargetItem(VisualElement element, int index)
    {
        Target target = Targets[index];

        element.Q<Label>("name-label").text = target.GetName();
        element.Q<Label>("level-label").text = target.GetLevel().ToString();
        element.Q<Label>("health-label").text = target.GetHealth().ToString();
        element.Q<Label>("mana-label").text = target.GetMana().ToString();
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
        MovesListView.selectionChanged += MoveSelectionChanged;
        MovesListView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
        MovesListView.selectionType = SelectionType.Single;
        SortMoves();
    }

    public void ConfigureTargetsListView()
    {
        TargetsListView.itemsSource = new List<Target>();
        TargetsListView.makeItem = MakeTargetItem;
        TargetsListView.bindItem = BindTargetItem;
        TargetsListView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
        TargetsListView.selectionType = SelectionType.Single;
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

    public IEnumerator GetUserMoveEvent(Fighter fighter, System.Action<MoveEvent> onMoveEventSelected)
    {
        SelectedFighter = fighter;
        ConfigureForFighter(fighter);

        Advance = false;
        SelectedMove = null;
        SelectedTarget = null;
        yield return WaitForSelection();

        MoveEvent moveEvent = new MoveEvent();
        moveEvent.AddFighter(SelectedFighter);
        moveEvent.AddMove(SelectedMove);
        moveEvent.AddRandomAdd(Fight.RandomAdd());
        moveEvent.SetMoveType(SelectedMove.GetMoveType());

        moveEvent.SetTargetType(SelectedMove.GetTargetType());
        switch (SelectedTarget.GetTargetType())
        {
            case Enums.TargetType.OneEnemy:
            case Enums.TargetType.EnemiesWithStatuses:
            case Enums.TargetType.OneTeamMember:
            case Enums.TargetType.TeamMembersWithStatuses:
                moveEvent.AddTarget(SelectedTarget.GetFighterTarget());
                break;
            case Enums.TargetType.EnemyTeam:
                {
                    List<Fighter> enemyTeam = Fight.GetTeamList(SelectedTarget.GetTargetTeam());
                    moveEvent.AddTargets(enemyTeam);
                    moveEvent.SetTargetTeam(SelectedTarget.GetTargetTeam());
                    break;
                }
            case Enums.TargetType.AllEnemies:
                {
                    List<Fighter> enemies = AI.GetEnemies(Fight, SelectedFighter);
                    moveEvent.AddTargets(enemies);
                    break;
                }
            case Enums.TargetType.Team:
                {
                    moveEvent.SetTargetTeam(SelectedTarget.GetTargetTeam());
                    List<Fighter> team = Fight.GetTeamList(SelectedFighter.GetTeam());
                    moveEvent.AddTargets(team);
                    moveEvent.SetTargetTeam(SelectedFighter.GetTeam());
                    break;
                }
            case Enums.TargetType.Self:
                moveEvent.AddTarget(SelectedFighter);
                break;
            default:
                Debug.LogError("Error! Unexpected SelectedTarget.GetTargetType() in GetUserMoveEvent!");
                break;
        }

        onMoveEventSelected(moveEvent);
    }

    VisualElement MakeMoveItem() { return MoveRowTemplate.Instantiate(); }
    VisualElement MakeTargetItem() { return TargetRowTemplate.Instantiate(); }

    void MoveSelectionChanged(IEnumerable<object> selectedItems)
    {
        if (MovesListView.selectedItem != null)
        {
            SelectedMove = (Move)MovesListView.selectedItem;
            Targets = new List<Target>();
            List<Fighter> FighterTargets = new List<Fighter>();

            switch (SelectedMove.GetTargetType())
            {
                case Enums.TargetType.OneEnemy:
                    {
                        if (SelectedMove.GetRequiredTargetStatusesList().Count > 0)
                        {
                            FighterTargets = SelectedFighter.GetAI().GetEnemiesWithStatuses(Fight, SelectedFighter, SelectedMove.GetRequiredTargetStatusesList());
                        }
                        else
                        {
                            FighterTargets = AI.GetEnemies(Fight, SelectedFighter);
                        }
                        break;
                    }
                case Enums.TargetType.EnemyTeam:
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            if (SelectedFighter.GetTeam() != i && i <= Fight.GetTeams() && Fight.GetTeamList(i).Count > 0)
                            {
                                Target target = new Target();
                                target.SetName($"Team {i}");
                                target.SetLevel("");
                                target.SetHealth("");
                                target.SetMana("");
                                target.SetTargetType(Enums.TargetType.EnemyTeam);
                                target.SetTargetTeam(i);
                                Targets.Add(target);
                            }
                        }
                        break;
                    }
                case Enums.TargetType.AllEnemies:
                    {
                        Target target = new Target();
                        target.SetName("All Enemies");
                        target.SetLevel("");
                        target.SetHealth("");
                        target.SetMana("");
                        target.SetTargetType(Enums.TargetType.AllEnemies);
                        Targets.Add(target);
                        break;
                    }
                case Enums.TargetType.EnemiesWithStatuses:
                    {
                        FighterTargets = SelectedFighter.GetAI().GetEnemiesWithStatuses(Fight, SelectedFighter, SelectedMove.GetRequiredTargetStatusesList());
                        break;
                    }
                case Enums.TargetType.OneTeamMember:
                    {
                        if (SelectedMove.GetRequiredTargetStatusesList().Count > 0)
                        {
                            if (SelectedFighter.CheckStatuses(SelectedMove.GetRequiredTargetStatusesList()))
                            {
                                FighterTargets.Add(SelectedFighter);
                            }
                            FighterTargets.AddRange(SelectedFighter.GetAI().GetTeammatesWithStatuses(Fight, SelectedFighter, SelectedMove.GetRequiredTargetStatusesList()));
                        }
                        else
                        {
                            FighterTargets.Add(SelectedFighter);
                            FighterTargets.AddRange(SelectedFighter.GetAI().GetTeammates(Fight, SelectedFighter));
                        }
                        break;
                    }
                case Enums.TargetType.Team:
                    {
                        Target target = new Target();
                        target.SetName($"Team {SelectedFighter.GetTeam()}");
                        target.SetLevel("");
                        target.SetHealth("");
                        target.SetMana("");
                        target.SetTargetType(Enums.TargetType.Team);
                        target.SetTargetTeam(SelectedFighter.GetTeam());
                        Targets.Add(target);
                        break;
                    }
                case Enums.TargetType.TeamMembersWithStatuses:
                    {
                        if (SelectedFighter.CheckStatuses(SelectedMove.GetRequiredTargetStatusesList()))
                        {
                            FighterTargets.Add(SelectedFighter);
                        }
                        FighterTargets = SelectedFighter.GetAI().GetTeammatesWithStatuses(Fight, SelectedFighter, SelectedMove.GetRequiredTargetStatusesList());
                        break;
                    }
                case Enums.TargetType.Self:
                    {
                        Target target = new Target();
                        target.SetName("Self");
                        target.SetLevel(SelectedFighter.GetLevel().ToString());
                        target.SetHealth(SelectedFighter.GetHealth().ToString());
                        target.SetMana(SelectedFighter.GetMana().ToString());
                        target.SetTargetType(Enums.TargetType.Self);
                        Targets.Add(target);
                        break;
                    }
                default:
                    Debug.LogError("Error! Unexpected SelectedMove.GetTargetType() in MoveSelectionChanged!");
                    break;
            }

            foreach (Fighter target in FighterTargets)
            {
                Target targetInfo = new Target();
                targetInfo.SetName(target.GetName());
                targetInfo.SetLevel(target.GetLevel().ToString());
                targetInfo.SetHealth(target.GetHealth().ToString());
                targetInfo.SetMana(target.GetMana().ToString());
                targetInfo.SetFighterTarget(target);
                if (target.GetTeam() == SelectedFighter.GetTeam())
                {
                    targetInfo.SetTargetType(Enums.TargetType.OneTeamMember);
                }
                else
                {
                    targetInfo.SetTargetType(Enums.TargetType.OneEnemy);
                }
                Targets.Add(targetInfo);
            }

            TargetsListView.itemsSource = Targets;
            TargetsListView.Rebuild();
        }
        else
        {
            Targets = new List<Target>();
            TargetsListView.itemsSource = Targets;
            TargetsListView.Rebuild();
        }
    }

    public void OnAdvanceClickEvent()
    {
        if (MovesListView.selectedItem != null && TargetsListView.selectedItem != null)
        {
            SelectedMove = (Move)MovesListView.selectedItem;
            SelectedTarget = (Target)TargetsListView.selectedItem;
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
        MoveSelectionChanged(MovesListView.selectedItems);
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
        Moves = new List<Move>();//SelectedFighter.GetMoves(Enums.MoveType.Offensive);

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

        TargetsListView = root.Q<ListView>("TargetsListView");  
        MovesListView = root.Q<ListView>("MovesListView");        
        /*MovesListView.selectionChanged += selectedItems =>
        {
            foreach (Move move in selectedItems)
            {
                Debug.Log($"Selected: {move.GetName()}");
            }
        };*/
        ConfigureTargetsListView();
        ConfigureMovesListView();

        NameLabel = root.Q<Label>(name:"NameLabel");
        NameLabel.text = SelectedFighter.GetName();
        RoundLabel = root.Q<Label>(name:"RoundLabel");
        RoundLabel.text = $"Round: {Fight.GetRoundNumber()}";
        HealthLabel = root.Q<Label>(name:"HealthLabel");
        HealthLabel.text = "Health: 100";
        ManaLabel = root.Q<Label>(name:"ManaLabel");
        ManaLabel.text = "Mana: 100";
        //ConfigureForFighter(SelectedFighter);

        AdvanceButton = root.Q<Button>("AdvanceButton");
        AdvanceButton.RegisterCallback<ClickEvent>(evt => OnAdvanceClickEvent());

        Portrait = root.Q<Image>("Portrait");
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
