using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class DynamicWindow : MonoBehaviour
{
    [SerializeField] private Fight          Fight;
    [SerializeField] private GameObject     AttackerPanel;
    [SerializeField] private Image          AttackerBackground;
    [SerializeField] private Image          Attacker;
    [SerializeField] private GameObject     AttackersPanel;
    [SerializeField] private Image          AttackersBackground;
    [SerializeField] private Image          Attackers;
    [SerializeField] private Image          StatementBackground;
    [SerializeField] private Image          StatementFighter;
    [SerializeField] private GameObject     StatementPanel;
    public const float ATTACKER_BACKGROUND_MAX_OFFSET = 1330f;
    public const float STATEMENT_BACKGROUND_MAX_OFFSET = 3706;

    public void ConfigureFightWindowForAttacker(MoveEvent moveEvent)
    {
        int team = moveEvent.GetFighters()[0].GetTeam();
        if (team == 1)
        {
            AttackerBackground.sprite = Fight.GetMapArt().GetBackground_1();
        }
        else if (team == 2)
        {
            AttackerBackground.sprite = Fight.GetMapArt().GetBackground_2();
        }
        else
        {
            AttackerBackground.sprite = Fight.GetMapArt().GetBackground_3();
        }

        List<Fighter> teamList = Fight.GetTeamList(team);
        int teamSize = teamList.Count;
        float offsetPerFighter = 0f;
        if (teamSize > 1)
        {
            offsetPerFighter = ATTACKER_BACKGROUND_MAX_OFFSET / (teamSize - 1);
        }
        int fighterIndex = teamList.IndexOf(moveEvent.GetFighters()[0]);
        float offset = offsetPerFighter * fighterIndex * -1f; // Multiply by -1 to move left for higher index fighters

        Vector2 position = AttackerBackground.rectTransform.anchoredPosition;
        position.x = offset;
        AttackerBackground.rectTransform.anchoredPosition = position;

        Attacker.sprite = null;
        Sprite attackerSprite = moveEvent.GetFighters()[0].GetArt().GetBody();
        if (attackerSprite != null)
        {
            Attacker.sprite = attackerSprite;
        }
        else
        {
            Debug.LogError($"Error! Fighter {moveEvent.GetFighters()[0].GetName()} has no body sprite in ConfigureFightWindowForAttacker!");
        }
    }

    public void ConfigureFightWindowForAttackers(MoveEvent moveEvent)
    {
        int team = moveEvent.GetFighters()[0].GetTeam();
        if (team == 1)
        {
            AttackersBackground.sprite = Fight.GetMapArt().GetBackground_1();
        }
        else if (team == 2)
        {
            AttackersBackground.sprite = Fight.GetMapArt().GetBackground_2();
        }
        else
        {
            AttackersBackground.sprite = Fight.GetMapArt().GetBackground_3();
        }

        Vector2 position = AttackersBackground.rectTransform.anchoredPosition;
        position.x = 0f;
        AttackersBackground.rectTransform.anchoredPosition = position;

        Attackers.sprite = null;
        Sprite attackerSprite = moveEvent.GetFighters()[0].GetArt().GetBody();
        if (attackerSprite != null)
        {
            Attackers.sprite = attackerSprite;
        }
        else
        {
            Debug.LogError($"Error! Fighter {moveEvent.GetFighters()[0].GetName()} has no body sprite!");
        }
    }

    public void ConfigureFightWindowForStatement(Text statement, Fighter fighter)
    {
        int team = fighter.GetTeam();
        if (team == 1)
        {
            StatementBackground.sprite = Fight.GetMapArt().GetBackground_1();
        }
        else if (team == 2)
        {
            StatementBackground.sprite = Fight.GetMapArt().GetBackground_2();
        }
        else
        {
            StatementBackground.sprite = Fight.GetMapArt().GetBackground_3();
        }

        List<Fighter> teamList = Fight.GetTeamList(team);
        int teamSize = teamList.Count;
        float offsetPerFighter = 0f;
        if (teamSize > 1)
        {
            offsetPerFighter = STATEMENT_BACKGROUND_MAX_OFFSET / (teamSize - 1);
        }
        int fighterIndex = teamList.IndexOf(fighter);
        float offset = offsetPerFighter * fighterIndex * -1f; // Multiply by -1 to move left for higher index fighters

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
    }

    public IEnumerator DisplayMovePreview(MoveEvent moveEvent)
    {
        HidePanels();
        int numberOfFighters = moveEvent.GetFighters().Count;
        if (numberOfFighters > 1)
        {
            ConfigureFightWindowForAttackers(moveEvent);
            AttackersPanel.SetActive(true);
        }
        else
        {
            ConfigureFightWindowForAttacker(moveEvent);
            AttackerPanel.SetActive(true);
        }

        yield return new WaitForSeconds(1f);
    }

    public IEnumerator DisplayStatement(Text statement, Fighter fighter)
    {
        HidePanels();
        ConfigureFightWindowForStatement(statement, fighter);
        StatementPanel.SetActive(true);

        yield return new WaitForSeconds(3f);
    }

    public void HidePanels()
    {
        AttackerPanel.SetActive(false);
        AttackersPanel.SetActive(false);
        StatementPanel.SetActive(false);
    }
}
