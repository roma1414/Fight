using UnityEngine;
using UnityEngine.UI;
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
    private Move                            SelectedMove;
    private List<Move>                      Moves;
    private List<Target>                    Targets;
    private bool                            Advance = false;
    private bool                            SortDescending = true;

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

    public void OnBottomTabClickEvent(string tabName)
    {
        Toggle selected = BottomTabs.ActiveToggles().FirstOrDefault();
        if (tabName == selected.name)
        {
            SortDescending = !SortDescending;
        }

        SortMoves();
    }

    public void OnTopTabClickEvent(string tabName)
    {
        Toggle selected = TopTabs.ActiveToggles().FirstOrDefault();
        if (selected.name != tabName)
        {
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
                case "ProtectTab":
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
    }

    void Start()
    {
        
    }

    public IEnumerator WaitForSelection()
    {
        yield return new WaitUntil(() => Advance == true);
    }
}
