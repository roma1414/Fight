using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class DynamicWindow : MonoBehaviour
{
    [SerializeField] private Fight          Fight;
    [SerializeField] private GameObject     FighterPanel, FightersPanel, DynamicTextPanel, StatementPanel, StatementTextPanel;
    [SerializeField] private Image          Fighter, FighterBackground, FightersBackground, StatementBackground, 
                                            StatementFighter, FightersPrefab;
    [SerializeField] private TMP_Text       DynamicText, StatementText, StatementFighterName;
    [SerializeField] private RectTransform  FightersContainer;
    private List<Image>                     SpawnedFighterImages = new List<Image>();
    public const float FIGHTER_BACKGROUND_MAX_OFFSET = 1330f;
    public const float STATEMENT_BACKGROUND_MAX_OFFSET = 3706;
    public const float FIGHTER_ZERO_FOR_PREFBABS = 240f; 
    public const float FIGHTERS_BACKGROUND_MAX_OFFSET = 1040f;

    public void ConfigureFightWindowForAttacker(int index, MoveEvent moveEvent, bool includeTargets = false)
    {
        Fighter fighter = moveEvent.GetFighters()[index];
        int team = fighter.GetTeam();
        if (team == 1)
        {
            FighterBackground.sprite = Fight.GetMapArt().GetBackground_1();
        }
        else if (team == 2)
        {
            FighterBackground.sprite = Fight.GetMapArt().GetBackground_2();
        }
        else
        {
            FighterBackground.sprite = Fight.GetMapArt().GetBackground_3();
        }

        List<Fighter> teamList = Fight.GetTeamList(team);
        int teamSize = teamList.Count;
        float offsetPerFighter = 0f;
        if (teamSize > 1)
        {
            offsetPerFighter = FIGHTER_BACKGROUND_MAX_OFFSET / (teamSize - 1);
        }
        int fighterTeamIndex = teamList.IndexOf(fighter);
        float offset = offsetPerFighter * fighterTeamIndex * -1f; // Multiply by -1 to move left for higher index fighters

        Vector2 position = FighterBackground.rectTransform.anchoredPosition;
        position.x = offset;
        FighterBackground.rectTransform.anchoredPosition = position;

        Fighter.sprite = null;
        Sprite fighterSprite = fighter.GetArt().GetBody();
        if (fighterSprite != null)
        {
            Fighter.sprite = fighterSprite;
        }
        else
        {
            Debug.LogError($"Error! Fighter {fighter.GetName()} has no body sprite in ConfigureFightWindowForAttacker!");
        }

        Move move = moveEvent.GetMoves()[index];
        string text = fighter.GetName() + " uses " + move.GetName();

        if (includeTargets)
        {
            List<Fighter> targets = moveEvent.GetTargets();

            Enums.TargetType targetType = moveEvent.GetTargetType();
            if (targetType == Enums.TargetType.Self)
                {
                    text += "!";
                }
            else
            {
                Enums.MoveType moveType = moveEvent.GetMoveType();

                if (moveType == Enums.MoveType.Medical)
                {
                    text += " on ";
                }
                else
                {
                    text += " against ";
                }

                switch (targetType)
                {
                    case Enums.TargetType.Enemy:
                    case Enums.TargetType.TeamMember:
                        {
                            Fighter target = targets[0];
                            text += target.GetName() + "!";
                            break;
                        }
                    case Enums.TargetType.EnemyTeam:
                        {
                            int targetTeam = moveEvent.GetTargetTeam();
                            text += "Team " + targetTeam + "!";
                            break;
                        }
                    case Enums.TargetType.AllEnemies:
                        {
                            text += "all enemies!";
                            break;
                        }
                    case Enums.TargetType.EnemiesWithStatuses:
                    case Enums.TargetType.TeamMembersWithStatuses:
                        {
                            text += Util.ListString(targets) + "!";
                            break;
                        }
                    case Enums.TargetType.Team:
                        {
                            int targetTeam = moveEvent.GetTargetTeam();
                            text += "all members of Team " + targetTeam + "!";
                            break;
                        }
                    default:
                        Debug.LogError("Error! Unexpected TargetType [" + targetType + "] in Fight.PrintAttackString!");
                        break;
                }
            }
        }
        else
        {
            text += "!";
        }
        
        DynamicText.text = text;
    }

    public void ConfigureFightWindowForAttackers(MoveEvent moveEvent)
    {
        int team = moveEvent.GetFighters()[0].GetTeam();
        if (team == 1)
        {
            FightersBackground.sprite = Fight.GetMapArt().GetBackground_1();
        }
        else if (team == 2)
        {
            FightersBackground.sprite = Fight.GetMapArt().GetBackground_2();
        }
        else
        {
            FightersBackground.sprite = Fight.GetMapArt().GetBackground_3();
        }

        List<Fighter> fighters = moveEvent.GetFighters();
        
        for (int i = 0; i < fighters.Count; i++)
        {
            Fighter fighter = fighters[i];
            
            Image spawnedFighterImage = Instantiate(
                FightersPrefab,
                FightersContainer,
                false
            );
            SpawnedFighterImages.Add(spawnedFighterImage);

            spawnedFighterImage.sprite = null;
            Sprite attackerSprite = fighter.GetArt().GetBody();
            if (attackerSprite != null)
            {
                spawnedFighterImage.sprite = attackerSprite;
            }
            else
            {
                Debug.LogError($"Error! Fighter {fighter.GetName()} has no body sprite!");
            }
        }

        List<Fighter> targets = moveEvent.GetTargets();
        DynamicText.text = Util.ListString(fighters) + " attack " + Util.ListString(targets) + "!";
    }

    public void ConfigureFightWindowForTarget(int index, MoveEvent moveEvent, bool includeTargets = false)
    {
        
    }

    public void ConfigureFightWindowForStatement(string statement, Fighter fighter)
    {
        int team = fighter.GetTeam();
        if (team == 1)
        {
            StatementBackground.sprite = Fight.GetMapArt().GetBackground_1();
            StatementFighterName.color = Fight.GetTeamColor(1);
        }
        else if (team == 2)
        {
            StatementBackground.sprite = Fight.GetMapArt().GetBackground_2();
            StatementFighterName.color = Fight.GetTeamColor(2);
        }
        else
        {
            StatementBackground.sprite = Fight.GetMapArt().GetBackground_3();
            StatementFighterName.color = Fight.GetTeamColor(3);
        }

        List<Fighter> teamList = Fight.GetTeamList(team);
        int teamSize = teamList.Count;
        float offsetPerFighter = 0f;
        if (teamSize > 1)
        {
            offsetPerFighter = STATEMENT_BACKGROUND_MAX_OFFSET / (teamSize - 1);
        }
        int fighterIndex = teamList.IndexOf(fighter);
        float offset = offsetPerFighter * fighterIndex * -1f; // Multiply by -1 to move the background left for higher index fighters

        Vector2 position = StatementBackground.rectTransform.anchoredPosition;
        position.x = offset;
        StatementBackground.rectTransform.anchoredPosition = position;

        StatementFighter.sprite = null;
        Sprite attackerSprite = fighter.GetArt().GetTalkingSprite();
        if (attackerSprite != null)
        {
            StatementFighter.sprite = attackerSprite;
        }
        else
        {
            Debug.LogError($"Error! Fighter {fighter.GetName()} has no talking sprite in ConfigureFightWindowForStatement!");
        }

        StatementFighterName.text = $"{fighter.GetName()}:";
        StatementText.text = statement;
    }

    public IEnumerator DisplayMovePreview(MoveEvent moveEvent)
    {
        if (moveEvent.GetMoveType() == Enums.MoveType.Protect)
        {
            yield break;
        }
        
        HidePanels();
        int numberOfFighters = moveEvent.GetFighters().Count;
        if (numberOfFighters > 1)
        {
            ConfigureFightWindowForAttackers(moveEvent);
            FightersPanel.SetActive(true);
        }
        else
        {
            ConfigureFightWindowForAttacker(0, moveEvent, true);
            FightersPanel.SetActive(true);
        }

        DynamicTextPanel.SetActive(true);

        yield return new WaitForSeconds(2.5f);
    }

    public IEnumerator DisplayStatement(string statement, Fighter fighter)
    {
        HidePanels();
        ConfigureFightWindowForStatement(statement, fighter);
        StatementPanel.SetActive(true);
        StatementTextPanel.SetActive(true);

        yield return new WaitForSeconds(3f);
    }

    public IEnumerator DisplayStatements(List<string> statements, List<Fighter> fighters)
    {
        for (int i = 0; i < statements.Count; i++)
        {
            yield return DisplayStatement(statements[i], fighters[i]);
        }

        yield break;
    }

    public void HidePanels()
    {
        FighterPanel.SetActive(false);
        FightersPanel.SetActive(false);
        StatementPanel.SetActive(false);
        DynamicTextPanel.SetActive(false);
        StatementTextPanel.SetActive(false);

        foreach (Image spawnedFighterImage in SpawnedFighterImages)
        {
            if (spawnedFighterImage != null)
            {
                Destroy(spawnedFighterImage.gameObject);
            }
        }
        SpawnedFighterImages.Clear();
    }
}
