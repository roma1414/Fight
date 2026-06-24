using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectMove : MonoBehaviour
{
    [SerializeField] UIDocument         uiDocument;
    [SerializeField] VisualTreeAsset    MoveRowTemplate;
    protected VisualElement             root;
    protected List<Move>                Moves;
    [SerializeField] protected Fighter  SelectedFighter;
    protected bool                      SortDescending = true;
    protected VisualElement             CurrentlySelectedTab;
    protected VisualElement             CurrentlySelectedSubTab;
    protected VisualElement             OffensiveTab, MedicalTab, PowerUpTab, SummonTab, SubTab, NameTab, LevelTab, ManaTab, TargetTab, TypeTab;
    protected ListView                  MovesListView;

    void BindItem(VisualElement element, int index)
    {
        Move move = Moves[index];

        element.Q<Label>("name-label").text = move.GetName();
        element.Q<Label>("level-label").text = $"HP: {move.GetLevel()}";
    }
    
    public void ConfigureListView()
    {
        MovesListView.itemsSource = Moves;
        MovesListView.makeItem = MakeItem;
        MovesListView.bindItem = BindItem;
        MovesListView.fixedItemHeight = 22;
        MovesListView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
        MovesListView.selectionType = SelectionType.Single;
        MovesListView.Rebuild();
    }

    VisualElement MakeItem()
    {
        var row = new VisualElement();
        row.AddToClassList("move-row");

        var nameLabel = new Label();
        nameLabel.name = "name-label";

        var levelLabel = new Label();
        levelLabel.name = "level-label";

        row.Add(nameLabel);
        row.Add(levelLabel);

        return row;
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
            default:
                Debug.LogError("Error! Unexpected CurrentlySelectedTab.name in UpdateMovesForTab!");
                break;
        }

        Moves = SelectedFighter.GetMoves(moveType);
        SortMoves();
    }

    public void OnSubTabClickEvent(VisualElement clickedTab)
    {
        if (clickedTab == CurrentlySelectedSubTab)
        {
            SortDescending = SortDescending ? false : true;
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

    public void SortMoves()
    {
        switch (CurrentlySelectedSubTab.name)
        {
            case "NameTab":
                {
                    if (SortDescending)
                    {
                        Moves.Sort((left, right) => left.GetName().CompareTo(right.GetName())); // Sort in ascending order. I think...
                    }
                    else
                    {
                        Moves.Sort((left, right) => right.GetName().CompareTo(left.GetName())); // Sort in descending order. I think...
                    }
                    break;
                }
            case "LevelTab":
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
            case "ManaTab":
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
            case "TargetTab":
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
            case "TypeTab":
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
        CurrentlySelectedTab = OffensiveTab;
        CurrentlySelectedTab.AddToClassList("selected");
        Moves = SelectedFighter.GetMoves(Enums.MoveType.Offensive); //new List<Move>();

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
        ConfigureListView();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
