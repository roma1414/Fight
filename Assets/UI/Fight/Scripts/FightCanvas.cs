using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.UIElements;
using TMPro;
using System.Linq;

public class FightCanvas : MonoBehaviour
{
    [SerializeField] private Fight          Fight;
    [SerializeField] private Fighter        SelectedFighter;
    [SerializeField] private TMP_Text       NameText, RoundText, HealthText, ManaText;
    [SerializeField] private ToggleGroup    BottomTabs, TopTabs;
    [SerializeField] private RectTransform  MovesContent;
    [SerializeField] private MoveRowUI      MoveRowPrefab;
    [SerializeField] private RectTransform  TargetsContent;
    [SerializeField] private TargetRowUI    TargetRowPrefab;
    [SerializeField] protected MapArt       MapArt;
    [SerializeField] private Image          Background;
    private bool                            SortDescending = true;
    private bool                            Advance = false;
    private List<Move>                      Moves = new List<Move>();
    private MoveRowUI                       SelectedMoveRow;
    public Move                             SelectedMove { get; private set; }
    public event Action<Move>               MoveSelectionChanged;
    private readonly List<MoveRowUI>        MoveRows = new List<MoveRowUI>();
    private TargetRowUI                     SelectedTargetRow;
    public Target                           SelectedTarget { get; private set; }
    private List<Target>                    Targets = new List<Target>();
    public event Action<Target>             TargetSelectionChanged;
    private readonly List<TargetRowUI>      TargetRows = new List<TargetRowUI>();

    public void ConfigureFightWindowForAttackers(MoveEvent moveEvent)
    {
        int team = moveEvent.GetFighters()[0].GetTeam();
        if (team == 1)
        {
            Background.sprite = MapArt.GetBackground_1();
        }
        else if (team == 2)
        {
            Background.sprite = MapArt.GetBackground_2();
        }
        else
        {
            Background.sprite = MapArt.GetBackground_3();
        }

        Vector2 position = Background.rectTransform.anchoredPosition;
        position.x = 0f;
        Background.rectTransform.anchoredPosition = position;
    }

    public void ConfigureForFighter(Fighter fighter)
    {
        SelectedFighter = fighter;

        NameText.text = SelectedFighter.GetName();
        RoundText.text = $"Round: {Fight.GetRoundNumber()}";
        HealthText.text = $"Health: {SelectedFighter.GetHealth()}";
        ManaText.text = $"Mana: {SelectedFighter.GetMana()}";

        Toggle selectedTopTab = TopTabs.ActiveToggles().FirstOrDefault();
        OnTopTabClickEvent(selectedTopTab.name);
        // OnSubTabClickEvent will invert sort direction, so we invert it first
        SortDescending = !SortDescending;
        Toggle selectedBottomTab = BottomTabs.ActiveToggles().FirstOrDefault();
        OnBottomTabClickEvent(selectedBottomTab.name);
    }

    public IEnumerator DisplayMovePreview(MoveEvent moveEvent)
    {
        ConfigureFightWindowForAttackers(moveEvent);
        yield return new WaitForSeconds(1f);
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
        //SelectedMove = null;
        //SelectedTarget = null;
        yield return WaitForSelection();

        MoveEvent moveEvent = new MoveEvent();
        moveEvent.AddFighter(SelectedFighter);
        moveEvent.AddMove(SelectedMove);
        moveEvent.AddRandomAdd(Fight.RandomAdd());
        moveEvent.SetMoveType(SelectedMove.GetMoveType());

        moveEvent.SetTargetType(SelectedMove.GetTargetType());
        switch (SelectedTarget.GetTargetType())
        {
            case Enums.TargetType.Enemy:
            case Enums.TargetType.EnemiesWithStatuses:
            case Enums.TargetType.TeamMember:
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

    public void OnAdvanceClickEvent()
    {
        if (SelectedMove != null && SelectedTarget != null)
        {
            Advance = true;
        }
    }

    public void OnBottomTabClickEvent(string tabName)
    {
        Toggle selected = BottomTabs.ActiveToggles().FirstOrDefault();
        if (tabName == selected.name)
        {
            SortDescending = !SortDescending;
        }

        SortMoves();
    }

    private void OnDisable()
    {
        MoveSelectionChanged -= OnMoveSelected;
    }

    private void OnEnable()
    {
        MoveSelectionChanged += OnMoveSelected;
    }

    void OnMoveSelected(Move move)
    {
        if (move != null)
        {
            SelectedMove = move;
            Targets = new List<Target>();
            List<Fighter> FighterTargets = new List<Fighter>();

            switch (SelectedMove.GetTargetType())
            {
                case Enums.TargetType.Enemy:
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
                case Enums.TargetType.TeamMember:
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
                    targetInfo.SetTargetType(Enums.TargetType.TeamMember);
                }
                else
                {
                    targetInfo.SetTargetType(Enums.TargetType.Enemy);
                }
                Targets.Add(targetInfo);
            }

            //TargetsListView.itemsSource = Targets;
            //TargetsListView.Rebuild();
            PopulateTargets(Targets);
        }
        else
        {
            Targets = new List<Target>();
            //TargetsListView.itemsSource = Targets;
            //TargetsListView.Rebuild();
            PopulateTargets(Targets);
        }
    }

    public void OnTopTabClickEvent(string tabName)
    {
        Toggle selected = TopTabs.ActiveToggles().FirstOrDefault();
        Enums.MoveType moveType = Enums.MoveType.Offensive;
        switch (tabName)
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
            case "OtherTab":
                moveType = Enums.MoveType.Protect;
                break;
            default:
                Debug.LogError("Error! Unexpected CurrentlySelectedTab.name in UpdateMovesForTab!");
                break;
        }

        Moves = GetPossibleMoves(moveType);
        //MovesListView.itemsSource = Moves;
        SortMoves();
        //MoveSelectionChanged(MovesListView.selectedItems);
    }

    public void PopulateMoves(IEnumerable<Move> moves)
    {
        SelectedMoveRow = null;
        SelectedMove = null;

        foreach (MoveRowUI row in MoveRows)
        {
            // Hide immediately; Destroy runs at the end of the frame.
            row.gameObject.SetActive(false);
            Destroy(row.gameObject);
        }

        MoveRows.Clear();

        foreach (Move move in moves)
        {
            if (move == null)
                continue;

            MoveRowUI row = Instantiate(MoveRowPrefab, MovesContent);
            row.Bind(move, SelectMoveRow);
            MoveRows.Add(row);
        }

        // Select the first row automatically when the list isn't empty.
        if (MoveRows.Count > 0)
            SelectMoveRow(MoveRows[0]);
        else
            MoveSelectionChanged?.Invoke(null);
    }

    public void PopulateTargets(IEnumerable<Target> targets)
    {
        SelectedTargetRow = null;
        SelectedTarget = null;

        foreach (TargetRowUI row in TargetRows)
        {
            // Hide immediately; Destroy runs at the end of the frame.
            row.gameObject.SetActive(false);
            Destroy(row.gameObject);
        }

        TargetRows.Clear();

        foreach (Target target in targets)
        {
            if (target == null)
                continue;

            TargetRowUI row = Instantiate(TargetRowPrefab, TargetsContent);
            row.Bind(target, SelectTargetRow);
            TargetRows.Add(row);
        }

        // Select the first row automatically when the list isn't empty.
        if (TargetRows.Count > 0)
            SelectTargetRow(TargetRows[0]);
        else
            TargetSelectionChanged?.Invoke(null);
    }

    private void SelectMoveRow(MoveRowUI row)
    {
        if (SelectedMoveRow == row)
            return;

        if (SelectedMoveRow != null)
            SelectedMoveRow.SetSelected(false);

        SelectedMoveRow = row;
        SelectedMove = row.Move;
        SelectedMoveRow.SetSelected(true);

        MoveSelectionChanged?.Invoke(SelectedMove);
    }

    private void SelectTargetRow(TargetRowUI row)
    {
        if (SelectedTargetRow == row)
            return;

        if (SelectedTargetRow != null)
            SelectedTargetRow.SetSelected(false);

        SelectedTargetRow = row;
        SelectedTarget = row.Target;
        SelectedTargetRow.SetSelected(true);

        TargetSelectionChanged?.Invoke(SelectedTarget);
    }

    public void SortMoves()
    {
        Toggle selected = BottomTabs.ActiveToggles().FirstOrDefault();

        switch (selected.name)
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

        //MovesListView.Rebuild();
        PopulateMoves(Moves);
    }

    void Start()
    {
        
    }

    public IEnumerator WaitForSelection()
    {
        yield return new WaitUntil(() => Advance == true);
    }
}
