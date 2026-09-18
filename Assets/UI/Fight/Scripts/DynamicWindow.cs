using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System;
//using GLTFast.Schema;
//using System.Numerics;

public class DynamicWindow : MonoBehaviour
{
    [SerializeField] private Fight          Fight;
    [SerializeField] private GameObject     FighterPanel, FightersPanel, DynamicTextPanel, StatementPanel, StatementTextPanel,
                                            StatementBackgroundPanel;
    [SerializeField] private Image          Fighter, FighterBackground, FightersBackground, StatementBackground, 
                                            StatementFighter, FightersPrefab, AttackersPrefab;
    [SerializeField] private TMP_Text       DynamicText, StatementText, StatementFighterName;
    [SerializeField] private RectTransform  FightersContainer, AttackersContainer;
    [SerializeField] private Animator       StatementAnimator, FighterAnimator;
    [SerializeField] private List<string>   StatementAnimations, FighterAnimations;
    private int                             PreviousStatementAnimation = -1, PreviousFighterAnimation = -1;
    private List<Image>                     SpawnedFighterImages = new List<Image>();
    private List<Image>                     SpawnedAttackerImages = new List<Image>();
    private Coroutine                       TalkingCoroutine;
    public const float FIGHTER_BACKGROUND_MAX_OFFSET = 1330f;
    public const float STATEMENT_BACKGROUND_MAX_OFFSET = 3698;
    public const float STATEMENT_BACKGROUND_PANEL_MAX_ANIMATION_OFFSET = 8f;
    public const float STATEMENT_FIGHTER_MAX_ANIMATION_OFFSET = 16f;

    private IEnumerator AnimateTalking(Sprite[] frames)
    {
        int index = 0;

        while (true)
        {
            StatementFighter.sprite = frames[index];
            index = (index + 1) % frames.Length;

            yield return new WaitForSeconds(0.15f);
        }
    }

    public void ConfigureFightWindowForAttackAgainstTargets(MoveEvent moveEvent)
    {
        List<Fighter> targets = moveEvent.GetTargets();
        int targetTeam = targets[0].GetTeam();
        FightersBackground.sprite = Fight.GetMapArt().GetBackground(targetTeam);

        for (int i = 0; i < targets.Count; i++)
        {
            Fighter target = targets[i];
            
            Image spawnedTargetImage = Instantiate(
                FightersPrefab,
                FightersContainer,
                false
            );
            SpawnedFighterImages.Add(spawnedTargetImage);

            spawnedTargetImage.sprite = null;
            Sprite targetSprite = target.GetArt().GetBody();
            if (targetSprite != null)
            {
                spawnedTargetImage.sprite = targetSprite;
            }
            else
            {
                Debug.LogError($"Error! Fighter {target.GetName()} has no body sprite in ConfigureFightWindowForAttack!");
            }
        }

        List<Fighter> attackers = moveEvent.GetFighters();
        for (int i = 0; i < attackers.Count; i++)
        {
            Fighter attacker = attackers[i];
            
            Image spawnedAttackerImage = Instantiate(
                AttackersPrefab,
                AttackersContainer,
                false
            );
            SpawnedAttackerImages.Add(spawnedAttackerImage);

            spawnedAttackerImage.sprite = null;
            Sprite attackerSprite = attacker.GetArt().GetBack();
            if (attackerSprite != null)
            {
                spawnedAttackerImage.sprite = attackerSprite;
            }
            else
            {
                Debug.LogError($"Error! Fighter {attacker.GetName()} has no body sprite in ConfigureFightWindowForAttack!");
            }
        }

        HorizontalLayoutGroup layoutGroup = AttackersContainer.GetComponent<HorizontalLayoutGroup>();
        float availableWidth = FightersContainer.rect.width - layoutGroup.padding.left - layoutGroup.padding.right;
        float totalImageWidths = 0f;
        foreach (Image image in SpawnedAttackerImages)
        {
            totalImageWidths += image.rectTransform.rect.width;
        }
        float attackerLayoutGroupSpacing = SpawnedAttackerImages.Count > 1
            ? Mathf.Min(0f, (availableWidth - totalImageWidths) / (SpawnedAttackerImages.Count - 1))
            : 0f;

        layoutGroup.spacing = attackerLayoutGroupSpacing;

        DynamicText.text = "";
    }
    
    public void ConfigureFightWindowForAttacker(int index, MoveEvent moveEvent, bool includeTargets = false)
    {
        Fighter fighter = moveEvent.GetFighters()[index];
        int team = fighter.GetTeam();
        FighterBackground.sprite = Fight.GetMapArt().GetBackground(team);

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
        FightersBackground.sprite = Fight.GetMapArt().GetBackground(team);

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
                Debug.LogError($"Error! Fighter {fighter.GetName()} has no body sprite in ConfigureFightWindowForAttackers!");
            }
        }

        List<Fighter> targets = moveEvent.GetTargets();
        DynamicText.text = Util.ListString(fighters) + " attack " + Util.ListString(targets) + "!";
    }

    public void ConfigureFightWindowForTarget(int index, MoveEvent moveEvent, bool includeTargets = false)
    {
        
    }

    TalkingFrames ConfigureFightWindowForStatement(string statement, Fighter fighter, string animation)
    {
        int team = fighter.GetTeam();
        StatementBackground.sprite = Fight.GetMapArt().GetBackground(team);
        StatementFighterName.color = Fight.GetTeamColor(team);

        List<Fighter> teamList = Fight.GetTeamList(team);
        int teamSize = teamList.Count;
        float offsetPerFighter = 0f;
        if (teamSize > 1)
        {
            offsetPerFighter = (STATEMENT_BACKGROUND_MAX_OFFSET - STATEMENT_BACKGROUND_PANEL_MAX_ANIMATION_OFFSET) / (teamSize - 1);
        }
        int fighterIndex = teamList.IndexOf(fighter);
        float backgroundOffset = -STATEMENT_BACKGROUND_PANEL_MAX_ANIMATION_OFFSET - offsetPerFighter * fighterIndex;

        Vector2 backgroundPosition = StatementBackground.rectTransform.anchoredPosition;
        backgroundPosition.x = backgroundOffset;
        StatementBackground.rectTransform.anchoredPosition = backgroundPosition;

        /*Vector2 fighterPosition = StatementFighter.rectTransform.anchoredPosition;
        Vector2 backgroundPanelPosition = StatementBackgroundPanel.GetComponent<RectTransform>().anchoredPosition;
        switch (animation)
        {
            case "Statement_Left":
                fighterPosition.x = STATEMENT_FIGHTER_MAX_ANIMATION_OFFSET;
                backgroundPanelPosition.x = STATEMENT_BACKGROUND_PANEL_MAX_ANIMATION_OFFSET;
                break;
            case "Statement_Right":
                fighterPosition.x = -STATEMENT_FIGHTER_MAX_ANIMATION_OFFSET;
                backgroundPanelPosition.x = -STATEMENT_BACKGROUND_PANEL_MAX_ANIMATION_OFFSET;
                break;
            default:
                Debug.LogError($"Error! Unknown animation {animation} in ConfigureFightWindowForStatement!");
                break;
        }
        StatementFighter.rectTransform.anchoredPosition = fighterPosition;
        StatementBackgroundPanel.GetComponent<RectTransform>().anchoredPosition = backgroundPanelPosition;*/

        StatementFighter.sprite = null;
        TalkingFrames talkingFrames = fighter.GetArt().GetTalkingFrames();
        if (talkingFrames != null)
        {
            StatementFighter.sprite = talkingFrames.GetFrames()[0];
        }
        else
        {
            Debug.LogError($"Error! Fighter {fighter.GetName()} has no talking frames in ConfigureFightWindowForStatement!");
        }

        StatementFighterName.text = $"{fighter.GetName()}:";
        StatementText.text = statement;

        return talkingFrames;
    }

    public IEnumerator DisplayAttackAgainstTarget(MoveEvent moveEvent, Fighter target)
    {
        HidePanels();
        ConfigureFightWindowForAttackAgainstTargets(moveEvent);
        FightersPanel.SetActive(true);
        DynamicTextPanel.SetActive(true);

        yield return new WaitForSeconds(3f);
    }

    public IEnumerator DisplayAttackerMovePreview(MoveEvent moveEvent)
    {
        HidePanels();
        ConfigureFightWindowForAttacker(0, moveEvent, true);
        FighterPanel.SetActive(true);
        DynamicTextPanel.SetActive(true);
        string animation = GetFighterAnimation();
        FighterAnimator.Play(animation, 0, 0f);

        yield return new WaitForSeconds(2f);
    }

    public IEnumerator DisplayAttackersMovePreview(MoveEvent moveEvent)
    {
        HidePanels();
        ConfigureFightWindowForAttackers(moveEvent);
        FightersPanel.SetActive(true);
        DynamicTextPanel.SetActive(true);        
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < moveEvent.GetFighters().Count; i++)
        {
            HidePanels();
            ConfigureFightWindowForAttacker(i, moveEvent, false);
            FighterPanel.SetActive(true);
            DynamicTextPanel.SetActive(true);
            string animation = GetFighterAnimation();
            FighterAnimator.Play(animation, 0, 0f);
            yield return new WaitForSeconds(2f);

        }
    }

    public IEnumerator DisplayMovePreview(MoveEvent moveEvent)
    {
        if (moveEvent.GetMoveType() == Enums.MoveType.Protect)
        {
            yield break;
        }
        
        int numberOfFighters = moveEvent.GetFighters().Count;
        if (numberOfFighters > 1)
        {
            yield return DisplayAttackersMovePreview(moveEvent);
        }
        else
        {
            yield return DisplayAttackerMovePreview(moveEvent);
        }
    }

    public IEnumerator DisplayStatement(string statement, Fighter fighter)
    {
        HidePanels();
        string animation = GetStatementAnimation();
        TalkingFrames talkingFrames = ConfigureFightWindowForStatement(statement, fighter, animation);
        StatementPanel.SetActive(true);
        StatementTextPanel.SetActive(true);
        StatementAnimator.Play(animation, 0, 0f);
        StartTalking(talkingFrames.GetFrames());
        yield return new WaitForSeconds(1.5f);
        StopTalking(talkingFrames.GetFrames());

        yield return new WaitForSeconds(1.5f);
    }

    public IEnumerator DisplayStatements(List<string> statements, List<Fighter> fighters)
    {
        for (int i = 0; i < statements.Count; i++)
        {
            yield return DisplayStatement(statements[i], fighters[i]);
        }

        yield break;
    }

    public string GetFighterAnimation()
    {
        string animation = "";
        if (FighterAnimations.Count == 0)
        {
            return animation;
        }

        List<string> possibleAnimations = new List<string>();
        for (int i = 0; i < FighterAnimations.Count; i++)
        {
            if (i != PreviousFighterAnimation)
            {
                possibleAnimations.Add(FighterAnimations[i]);
            }
        }

        if (possibleAnimations.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, possibleAnimations.Count);
            animation = possibleAnimations[index];
            PreviousFighterAnimation = FighterAnimations.IndexOf(animation);
            
            return animation;
        }

        return FighterAnimations[0];
    }

    public string GetStatementAnimation()
    {
        string animation = "";
        if (StatementAnimations.Count == 0)
        {
            return animation;
        }

        List<string> possibleAnimations = new List<string>();
        for (int i = 0; i < StatementAnimations.Count; i++)
        {
            if (i != PreviousStatementAnimation)
            {
                possibleAnimations.Add(StatementAnimations[i]);
            }
        }

        if (possibleAnimations.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, possibleAnimations.Count);
            animation = possibleAnimations[index];
            PreviousStatementAnimation = StatementAnimations.IndexOf(animation);
            
            return animation;
        }

        return StatementAnimations[0];
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

        foreach (Image spawnedAttackerImage in SpawnedAttackerImages)
        {
            if (spawnedAttackerImage != null)
            {
                Destroy(spawnedAttackerImage.gameObject);
            }
        }
        SpawnedAttackerImages.Clear();
    }

    private void OnDisable()
    {
        Sprite[] emptySpriteArray = new Sprite[0];
        StopTalking(emptySpriteArray);
    }

    private void StartTalking(Sprite[] frames)
    {
        StopTalking(frames);

        if (frames == null || frames.Length == 0)
            return;

        TalkingCoroutine = StartCoroutine(
            AnimateTalking(frames)
        );
    }

    private void StopTalking(Sprite[] frames)
    {
        if (TalkingCoroutine != null)
        {
            StopCoroutine(TalkingCoroutine);
            TalkingCoroutine = null;
            // First frame should be the resting frame.
            if (frames.Length > 0)
            {
                StatementFighter.sprite = frames[0];
            }
        }
    }
}
