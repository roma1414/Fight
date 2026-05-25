using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New AI", menuName = "Assets/AIs/New AI")]
public class AI : ScriptableObject
{
    [SerializeField] protected float SpellFraction;
    [SerializeField] protected float MeleeFraction;
    [SerializeField] protected float PsychicFraction;
    [SerializeField] protected float CloneFraction;
    [SerializeField] protected float MedicalFraction;
    [SerializeField] protected float NinTaiFraction;
    [SerializeField] protected float ProjectileFraction;
    [SerializeField] protected float SubFraction;
    [SerializeField] protected float SummonFraction;
    [SerializeField] protected bool  CustomAI;
    [SerializeField] protected ulong ID;

    public const int    MIN_HEALTH_TO_STILL_PROTECT_TEAMMATE_NEAR_DEATH = 55;
    public const int    MIN_HEALTH_TO_STILL_PROTECT_TEAMMATE_UNABLE_TO_MOVE = 14;
    public const int    MIN_MANA_TO_STILL_PROTECT_TEAMMATE_NEAR_DEATH = 30;
    public const int    MIN_MANA_TO_STILL_PROTECT_TEAMMATE_UNABLE_TO_MOVE = 18;
    public const int    POWER_UP_MANA = 60;
    public const int    POWER_UP_HEALTH = 80;
    public const float  POWER_UP_OVR_DIFF = .5f; // Use PowerUp if difference from maxEnemyOvr is this or greater.
    public const int    PROTECT_CHANCE_WHEN_TEAMMATE_CANNOT_MOVE = 85; // 85%
    public const int    PROTECT_CHANCE_WHEN_TEAMMATE_IS_NEAR_DEATH = 50; // 50%

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public bool CheckCustomAI() { return CustomAI; }

    public bool CheckForUsableMoves(Fight fight, Fighter fighter, Enums.MoveType moveType)
    {
        bool result = false;
        
        switch (moveType)
        {
            case Enums.MoveType.Melee:
            case Enums.MoveType.Spell:
            case Enums.MoveType.Psychic:
            case Enums.MoveType.NinTai:
            case Enums.MoveType.Projectile:
                {
                    foreach (OffensiveMove offensiveMove in fighter.GetOffensiveMoves())
                    {
                        if (offensiveMove.GetMoveType() == moveType && CheckIfCanPerformMove(fight, fighter, offensiveMove) == true)
                        {
                            result = true;
                            break;
                        }
                    }
                    break;
                }
            case Enums.MoveType.PowerUp:
                {
                    foreach (PowerUpMove powerUpMove in fighter.GetPowerUpMoves())
                    {
                        if (CheckIfCanPerformMove(fight, fighter, powerUpMove) == true)
                        {
                            result = true;
                            break;
                        }
                    }
                    break;
                }
            case Enums.MoveType.Medical:
                {
                    foreach (MedicalMove medicalMove in fighter.GetMedicalMoves())
                    {
                        if (CheckIfCanPerformMove(fight, fighter, medicalMove) == true)
                        {
                            result = true;
                            break;
                        }
                    }
                    break;
                }
            case Enums.MoveType.Substitution:
                {
                    foreach (SubMove submove in fighter.GetSubMoves())
                    {
                        if (CheckIfCanPerformMove(fight, fighter, submove) == true)
                        {
                            result = true;
                            break;
                        }
                    }
                    break;
                }
            case Enums.MoveType.Clone:
                {
                    foreach (CloneMove cloneMove in fighter.GetCloneMoves())
                    {
                        if (CheckIfCanPerformMove(fight, fighter, cloneMove) == true)
                        {
                            result = true;
                            break;
                        }
                    }
                    break;
                }
            case Enums.MoveType.Summon:
                {
                    foreach (SummonMove summonMove in fighter.GetSummonMoves())
                    {
                        if (CheckIfCanPerformMove(fight, fighter, summonMove) == true)
                        {
                            result = true;
                            break;
                        }
                    }
                    break;
                }
            default:
                {
                    Debug.LogError("Error! Unexpected MoveType [" + moveType + "] in AI.CheckForUsableMoves!");
                    result = false;
                    break;
                }
        }

        return result;
    }

    public bool CheckIfCanPerformMove(Fight fight, Fighter fighter, Move move)
    {
        if (fighter.CheckIfCapableOfMove(move) == false ||
            CheckIfMoveHasRemainingUses(fighter, move) == false ||
            CheckIfMoveHasViableTarget(fight, fighter, move) == false)
        {
            return false;
        }

        return true;
    }

    public bool CheckIfPsychicMoveHasViableTarget(Fight fight, Fighter fighter, Move move)
    {
        List<Enums.StatusType> requiredStatusList = move.GetRequiredTargetStatusesList();
        if (requiredStatusList.Count == 0)      // Move does not require its targets to have any statuses.
        {
            switch (move.GetTargetType())
            {
                case Enums.TargetType.OneEnemy:
                case Enums.TargetType.EnemyTeam:
                case Enums.TargetType.AllEnemies:
                    {
                        List<Enums.StatusType> psychicParalysisStatusList = new List<Enums.StatusType>();
                        psychicParalysisStatusList.Add(Enums.StatusType.PsychicParalysis);
                        List<Enums.StatusType> psychicControlStatusList = new List<Enums.StatusType>();
                        psychicControlStatusList.Add(Enums.StatusType.PsychicControl);

                        List<Fighter> enemiesUnderPsychicParalysis = GetEnemiesWithStatuses(fight, fighter, psychicParalysisStatusList);
                        List<Fighter> enemiesUnderPsychicControl = GetEnemiesWithStatuses(fight, fighter, psychicControlStatusList);
                        List<Fighter> enemies = GetEnemies(fight, fighter);

                        return enemies.Count > (enemiesUnderPsychicParalysis.Count + enemiesUnderPsychicControl.Count);
                    }
                default:
                    {
                        Debug.LogError("Error! Unexpected target type [" + move.GetTargetType() + "] for psychic move [" + move.GetName() + "] with ID [" + move.GetID() + "]!");
                        return false;
                    }
            }
        }

        // Move does require its targets to have statuses
        List<Fighter> enemiesWithRequiredStatuses = GetEnemiesWithStatuses(fight, fighter, requiredStatusList);
        List<Fighter> enemiesWithRequiredStatusesNotUnderPsychic = new List<Fighter>();

        foreach (Fighter enemy in enemiesWithRequiredStatuses)
        {
            if (enemy.CheckStatus(Enums.StatusType.PsychicParalysis) == false && enemy.CheckStatus(Enums.StatusType.PsychicControl) == false)
            {
                enemiesWithRequiredStatusesNotUnderPsychic.Add(enemy);
            }
        }

        switch (move.GetTargetType())
        {
            case Enums.TargetType.OneEnemy:
            case Enums.TargetType.EnemiesWithStatuses:
                {
                    if (enemiesWithRequiredStatusesNotUnderPsychic.Count > 0)
                    {
                        return true;
                    }

                    return false;
                }
            default:
                {
                    Debug.LogError("Error! Unexpected target type [" + move.GetTargetType() + "] for a move requiring its target to have statuses. Move [" + move.GetName() + "] with ID [" + move.GetID() + "]!");
                    return false;
                }
        }
    }

    public bool CheckIfMedicalOrPowerUpMoveHasViableTarget(Fight fight, Fighter fighter, Move move)
    {
        List<Enums.StatusType> requiredStatusList = move.GetRequiredTargetStatusesList();
        if (requiredStatusList.Count == 0)      // Move does not require its targets to have any statuses.
        {
            switch (move.GetTargetType())
            {
                case Enums.TargetType.OneTeamMember:
                case Enums.TargetType.Team:
                case Enums.TargetType.Self:
                    {
                        return true;
                    }
                default:
                    {
                        Debug.LogError("Error! Unexpected target type [" + move.GetTargetType() + "] for medical move [" + move.GetName() + "] with ID [" + move.GetID() + "]!");
                        return false;
                    }
            }
        }

        // Move does require its targets to have statuses.
        List<Fighter> teamMembersWithRequiredStatuses = GetTeammatesWithStatuses(fight, fighter, requiredStatusList);

        bool casterHasRequiredStatuses = fighter.CheckStatuses(requiredStatusList);

        if (casterHasRequiredStatuses)    // If caster has the required statuses they should be added to the list of potential targets.
        {
            teamMembersWithRequiredStatuses.Add(fighter);
        }

        switch (move.GetTargetType())
        {
            case Enums.TargetType.OneTeamMember:
            case Enums.TargetType.TeamMembersWithStatuses:
                {
                    if (teamMembersWithRequiredStatuses.Count > 0)
                    {
                        return true;
                    }

                    return false;
                }
            case Enums.TargetType.Self:
                {
                    if (casterHasRequiredStatuses == true)
                    {
                        return true;
                    }

                    return false;
                }
            default:
                {
                    Debug.LogError("Error! Unexpected target type [" + move.GetTargetType() + "] for a move requiring its target to have statuses. Move [" + move.GetName() + "] with ID [" + move.GetID() + "]!");
                    return false;
                }
        }
    }

    public bool CheckIfMoveHasRemainingUses(Fighter fighter, Move move)
    {
        if (move.GetUsesPerFight() == 0)
        {
            return true;
        }

        int uses = 0;
        int usesPerFight = move.GetUsesPerFight();

        List<UsedMove> usedMoves = fighter.GetUsedMoves();
        foreach (UsedMove usedMove in usedMoves)
        {
            if (usedMove.GetMove() == move)
            {
                ++uses;
            }
        }

        if (uses >= usesPerFight)
        {
            return false;
        }

        return true;
    }

    public bool CheckIfMoveHasViableTarget(Fight fight, Fighter fighter, Move move)
    {        
        switch (move.GetMoveType())
        {
            case Enums.MoveType.Melee:
            case Enums.MoveType.Spell:
            case Enums.MoveType.NinTai:
            case Enums.MoveType.Projectile:
                {
                    return CheckIfOffensiveMoveHasViableTarget(fight, fighter, move);
                }

            case Enums.MoveType.Psychic:
                {
                    return CheckIfPsychicMoveHasViableTarget(fight, fighter, move);
                }
            case Enums.MoveType.PowerUp:
            case Enums.MoveType.Medical:
                {
                    return CheckIfMedicalOrPowerUpMoveHasViableTarget(fight, fighter, move);
                }
            case Enums.MoveType.Substitution:
            case Enums.MoveType.Clone:
            case Enums.MoveType.Defensive:
            case Enums.MoveType.Avoid:
                {
                    return true;
                }
            default:
                {
                    Debug.LogError("Error! Unexpected move type [" + move.GetMoveType() + "] in AI.CheckIfMoveHasViableTarget!");
                    return false;
                }
        }
    }

    public bool CheckIfOffensiveMoveHasViableTarget(Fight fight, Fighter fighter, Move move)
    {
        List<Enums.StatusType> requiredStatusList = move.GetRequiredTargetStatusesList();
        if (requiredStatusList.Count == 0) // Generally just return true, unless the move requires the target to have statuses
        {
            return true;
        }

        List<Fighter> enemiesWithRequiredStatuses = GetEnemiesWithStatuses(fight, fighter, requiredStatusList);

        switch (move.GetTargetType())
        {
            case Enums.TargetType.OneEnemy:
            case Enums.TargetType.EnemiesWithStatuses:
                {
                    if (enemiesWithRequiredStatuses.Count > 0)
                    {
                        return true;
                    }

                    return false;
                }
            default:
                {
                    Debug.LogError("Error! Unexpected target type [" + move.GetTargetType() + "] for a move requiring its target to have statuses. Move [" + move.GetName() + "] with ID [" + move.GetID() + "]!");
                    return false;
                }
        }
    }

    public virtual bool CheckIfShouldPowerUp(Fight fight, Fighter fighter)
    {
        List<PowerUpMove> possiblePowerUpMoves = new List<PowerUpMove>();
        List<Fighter> enemies = GetEnemies(fight, fighter);
        float maxEnemyOverall = 0f;

        foreach (Fighter enemy in enemies)
        {
            float enemyOverall = enemy.GetOverallRating();
            if (enemyOverall > maxEnemyOverall)
            {
                maxEnemyOverall = enemyOverall;
            }
        }

        foreach (PowerUpMove powerUpMove in fighter.GetPowerUpMoves())
        {
            if (CheckIfCanPerformMove(fight, fighter, powerUpMove) == true)
            {
                possiblePowerUpMoves.Add(powerUpMove);
            }
        }

        // Fighter will power up if there is a strong enough enemy or if their health/mana levels are low enough.
        if (possiblePowerUpMoves.Count > 0 && (fighter.GetOverallRating() - maxEnemyOverall <= POWER_UP_OVR_DIFF || fighter.GetMana() <= POWER_UP_MANA || fighter.GetHealth() <= POWER_UP_HEALTH))
        {
            return true;
        }

        return false;
    }

    public virtual bool CheckIfShouldProtectTeammate(Fight fight, Fighter fighter)
    {
        List<Fighter> teammates = GetTeammates(fight, fighter);
        List<Fighter> teammatesUnableToMove = GetTeammatesUnableToMove(fight, fighter);
        List<Fighter> teammatesNearDefeat = GetTeammatesNearDefeat(fight, fighter);
        List<Fighter> enemies = GetEnemies(fight, fighter);
        List<Fighter> enemiesUnableToMove = GetEnemiesUnableToMove(fight, fighter);

        bool thereIsAnEnemyWhoCanMove = enemies.Count > enemiesUnableToMove.Count;
        bool thereIsATeammateUnableToMove = teammatesUnableToMove.Count > 0;
        bool thereIsATeammateNearDefeat = teammatesNearDefeat.Count > 0;

        int randomNumber = Random.Range(0, 100);

        if (thereIsATeammateUnableToMove &&
            thereIsAnEnemyWhoCanMove &&
            fighter.GetMana() >= MIN_MANA_TO_STILL_PROTECT_TEAMMATE_UNABLE_TO_MOVE &&
            fighter.GetHealth() >= MIN_HEALTH_TO_STILL_PROTECT_TEAMMATE_UNABLE_TO_MOVE)
        {
            int possibleProtectors = 1 + teammates.Count - teammatesUnableToMove.Count;
            int percentChance = 100 / possibleProtectors;

            if (randomNumber < percentChance && Random.Range(0, 100) < PROTECT_CHANCE_WHEN_TEAMMATE_CANNOT_MOVE) // 15 percent of the time a fighter will attack instead of protecting their teammate unable to move.
            {
                return true;
            }
        }
        else if (thereIsATeammateNearDefeat &&
            thereIsAnEnemyWhoCanMove &&
            randomNumber < PROTECT_CHANCE_WHEN_TEAMMATE_IS_NEAR_DEATH && // 50% of the time a fighter will attack instead of protecting their teammate near death.
            fighter.GetMana() >= MIN_MANA_TO_STILL_PROTECT_TEAMMATE_NEAR_DEATH &&
            fighter.GetHealth() >= MIN_HEALTH_TO_STILL_PROTECT_TEAMMATE_NEAR_DEATH)
        {
            return true;
        }

        return false;
    }

    public float GetCloneFraction() { return CloneFraction; }

    public virtual MoveEvent GetCloneMoveEvent(Fight fight, Fighter fighter)
    {
        MoveEvent moveEvent = new MoveEvent();
        moveEvent.SetMoveType(Enums.MoveType.Clone);
        moveEvent.AddFighter(fighter);
        moveEvent.SetTargetType(Enums.TargetType.Self);
        moveEvent.AddTarget(fighter);
        moveEvent.AddRandomAdd(Fight.RandomAdd());

        List<CloneMove> possibleCloneMoves = new List<CloneMove>();

        foreach (CloneMove cloneMove in fighter.GetCloneMoves())
        {
            if (CheckIfCanPerformMove(fight, fighter, cloneMove) == true)
            {
                possibleCloneMoves.Add(cloneMove);
            }
        }

        List<CloneMove> weightedPossibleCloneMoves = GetWeightedListOfCloneMoves(fight, fighter, possibleCloneMoves);

        if (weightedPossibleCloneMoves.Count < 1)
        {
            Debug.LogError("Error! No possible clone moves in AI.GetCloneMoveEvent! Returning a Skip MoveEvent.");
            return GetSkipMoveEvent(fighter);
        }

        int choice = Random.Range(0, weightedPossibleCloneMoves.Count);
        moveEvent.AddMove(weightedPossibleCloneMoves[choice]);

        return moveEvent;
    }

    static public List<Fighter> GetEnemies(Fight fight, Fighter fighter)
    {
        List<Fighter> enemies = new List<Fighter>();
        int team = fighter.GetTeam();

        switch (team)
        {
            case 1:
                enemies.AddRange(fight.GetTeamList(2));
                enemies.AddRange(fight.GetTeamList(3));
                break;
            case 2:
                enemies.AddRange(fight.GetTeamList(1));
                enemies.AddRange(fight.GetTeamList(3));
                break;
            case 3:
                enemies.AddRange(fight.GetTeamList(1));
                enemies.AddRange(fight.GetTeamList(2));
                break;
        }

        return enemies;
    }

    public List<Fighter> GetEnemiesNearDefeat(Fight fight, Fighter fighter)
    {
        List<Fighter> enemiesCloseToDeath = new List<Fighter>();
        List<Fighter> enemies = GetEnemies(fight, fighter);

        foreach (Fighter enemy in enemies)
        {
            if (enemy.CheckIfNearDefeat() == true)
            {
                enemiesCloseToDeath.Add(enemy);
            }
        }

        return enemiesCloseToDeath;
    }

    public List<Fighter> GetEnemiesUnableToMove(Fight fight, Fighter fighter)
    {
        List<Fighter> enemiesUnableToMove = new List<Fighter>();
        List<Fighter> enemies = GetEnemies(fight, fighter);

        foreach (Fighter enemy in enemies)
        {
            if (enemy.CheckIfCanMove() == false)
            {
                enemiesUnableToMove.Add(enemy);
            }
        }

        return enemiesUnableToMove;
    }

    public List<Fighter> GetEnemiesWithStatuses(Fight fight, Fighter fighter, List<Enums.StatusType> statusTypes)
    {
        List<Fighter> enemies = GetEnemies(fight, fighter);
        List<Fighter> enemiesWithAllRequiredStatuses = new List<Fighter>();

        foreach (Fighter enemy in enemies)
        {
            bool enemyHasAllRequiredStatuses = true;

            foreach (Enums.StatusType statusType in statusTypes)
            {
                if (enemy.CheckStatus(statusType) == false)
                {
                    enemyHasAllRequiredStatuses = false;
                }
            }

            if (enemyHasAllRequiredStatuses)
            {
                enemiesWithAllRequiredStatuses.Add(enemy);
            }
        }

        return enemiesWithAllRequiredStatuses;
    }

    public List<Fighter> GetEnemiesWithStatuses(Fight fight, Fighter fighter, Enums.StatusType[] statusTypes)
    {
        List<Fighter> enemies = GetEnemies(fight, fighter);
        List<Fighter> enemiesWithAllRequiredStatuses = new List<Fighter>();

        foreach (Fighter enemy in enemies)
        {
            bool enemyHasAllRequiredStatuses = true;

            foreach (Enums.StatusType statusType in statusTypes)
            {
                if (enemy.CheckStatus(statusType) == false)
                {
                    enemyHasAllRequiredStatuses = false;
                }
            }

            if (enemyHasAllRequiredStatuses)
            {
                enemiesWithAllRequiredStatuses.Add(enemy);
            }
        }

        return enemiesWithAllRequiredStatuses;
    }

    public float GetPsychicFraction() { return PsychicFraction; }
    public ulong GetID() { return ID; }
    public float GetMedicalFraction() { return MedicalFraction; }
    public float GetNinjutsuFraction() { return SpellFraction; }
    public float GetNinTaiFraction() { return NinTaiFraction; }
    public float GetProjectileFraction() { return ProjectileFraction; }
    public float GetSubFraction() { return SubFraction; }
    public float GetSummonFraction() { return SummonFraction; }
    public float GetMeleeFraction() { return MeleeFraction; }

    public MoveEvent GetCPUMoveEvent(Fight fight, Fighter fighter)
    {
        if (CustomAI == true)
        {
            return GetCustomAIMoveEvent(fight, fighter);
        }

        return GetStandardAIMoveEvent(fight, fighter);
    }

    public virtual MoveEvent GetCustomAIMoveEvent(Fight fight, Fighter fighter)
    {
        return GetStandardAIMoveEvent(fight, fighter);
    }

    public virtual MoveEvent GetMedicalMoveEvent(Fight fight, Fighter fighter)
    {
        MoveEvent moveEvent = new MoveEvent();
        moveEvent.SetMoveType(Enums.MoveType.Medical);
        moveEvent.AddFighter(fighter);
        moveEvent.AddRandomAdd(Fight.RandomAdd());

        List<MedicalMove> teamHealthMoves = new List<MedicalMove>();
        List<MedicalMove> teamManaMoves = new List<MedicalMove>();
        List<MedicalMove> selfHealthMoves = new List<MedicalMove>();
        List<MedicalMove> oneTeamMemberHealthMoves = new List<MedicalMove>();
        List<MedicalMove> oneTeamMemberManaMoves = new List<MedicalMove>();

        GetPossibleMedicalMovesByType(fight, fighter, teamHealthMoves, teamManaMoves, selfHealthMoves, oneTeamMemberHealthMoves, oneTeamMemberManaMoves);

        int someHealth = 15;
        List<Fighter> teamMembersMissingSomeHealth = GetTeammatesMissingXHealth(fight, fighter, someHealth);

        if (fighter.CheckIfMissingXHealth(someHealth) == true) // If the caster is missing someHealth they should be added to the list.
        {
            teamMembersMissingSomeHealth.Add(fighter);
        }

        // Check to see if we should heal our team
        if (teamHealthMoves.Count > 0 && teamMembersMissingSomeHealth.Count >= 3)
        {
            List<MedicalMove> weightedTeamHealthMoves = GetWeightedListOfMedicalMoves(fight, fighter, teamHealthMoves);
            int choice = Random.Range(0, weightedTeamHealthMoves.Count);
            MedicalMove medicalMove = weightedTeamHealthMoves[choice];

            moveEvent.AddMove(medicalMove);
            moveEvent.SetTargetType(Enums.TargetType.Team);
            moveEvent.SetTargetTeam(fighter.GetTeam());

            return moveEvent;
        }

        List<Fighter> teammatesWithLowMana = GetTeammatesWithManaBelowX(fight, fighter, Fighter.LOW_MANA_VALUE);

        // Check to see if we should restore mana for our team (checking for low mana)
        if (teamManaMoves.Count > 0 && teammatesWithLowMana.Count >= 2)
        {
            List<MedicalMove> weightedTeamManaMoves = GetWeightedListOfMedicalMoves(fight, fighter, teamManaMoves);
            int choice = Random.Range(0, weightedTeamManaMoves.Count);
            MedicalMove medicalMove = weightedTeamManaMoves[choice];

            moveEvent.AddMove(medicalMove);
            moveEvent.SetTargetType(Enums.TargetType.Team);
            moveEvent.SetTargetTeam(fighter.GetTeam());

            return moveEvent;
        }

        // Check to see if we should heal self
        if (selfHealthMoves.Count > 0 && fighter.CheckIfMissingXHealth(50) == true)
        {
            List<MedicalMove> weightedSelfHealthMoves = GetWeightedListOfMedicalMoves(fight, fighter, selfHealthMoves);
            int choice = Random.Range(0, weightedSelfHealthMoves.Count);
            MedicalMove medicalMove = weightedSelfHealthMoves[choice];

            moveEvent.AddMove(medicalMove);
            moveEvent.AddTarget(fighter);
            moveEvent.SetTargetType(medicalMove.GetTargetType());

            return moveEvent;
        }

        List<Fighter> teamMembersMissingHealth = GetTeammatesMissingXHealth(fight, fighter, 1);

        if (fighter.CheckIfMissingXHealth(1) == true)
        {
            teamMembersMissingHealth.Add(fighter);
        }

        // Check to see if we should heal one team member
        if (oneTeamMemberHealthMoves.Count > 0 && teamMembersMissingHealth.Count > 0)
        {
            List<MedicalMove> weightedOneTeamMemberHealthMoves = GetWeightedListOfMedicalMoves(fight, fighter, oneTeamMemberHealthMoves);
            int choice = Random.Range(0, weightedOneTeamMemberHealthMoves.Count);
            MedicalMove medicalMove = weightedOneTeamMemberHealthMoves[choice];

            List<Fighter> weightedTargetList = GetWeightedListOfMedicalTargets(fight, fighter, medicalMove, teamMembersMissingHealth);
            choice = Random.Range(0, weightedTargetList.Count);
            Fighter target = weightedTargetList[choice];

            moveEvent.AddMove(medicalMove);
            moveEvent.AddTarget(target);
            moveEvent.SetTargetType(Enums.TargetType.OneTeamMember);

            return moveEvent;
        }

        List<Fighter> teammatesMissingMana = GetTeammatesMissingXMana(fight, fighter, 1);

        // Check to see if we should restore a teammate's mana
        if (oneTeamMemberManaMoves.Count > 0 && teammatesMissingMana.Count > 0)
        {
            List<MedicalMove> weightedOneTeamMemberManaMoves = GetWeightedListOfMedicalMoves(fight, fighter, oneTeamMemberHealthMoves);
            int choice = Random.Range(0, weightedOneTeamMemberManaMoves.Count);
            MedicalMove medicalMove = weightedOneTeamMemberManaMoves[choice];

            List<Fighter> weightedTargetList = GetWeightedListOfMedicalTargets(fight, fighter, medicalMove, teammatesMissingMana);
            choice = Random.Range(0, weightedTargetList.Count);
            Fighter target = weightedTargetList[choice];

            moveEvent.AddMove(medicalMove);
            moveEvent.AddTarget(target);
            moveEvent.SetTargetType(Enums.TargetType.OneTeamMember);

            return moveEvent;
        }

        Debug.LogError("Error! Unable to find create a medical MoveEvent in AI.GetMedicalMoveEvent! Returning a Skip MoveEvent.");
        return GetSkipMoveEvent(fighter);
    }

    public MoveEvent GetMoveEvent(Fight fight, Fighter fighter)
    {
        if (fighter.GetControlType() == Enums.ControlType.CPU)
        {
            return GetCPUMoveEvent(fight, fighter);
        }

        // Get move selection from human-controlled user. CPU for now, until that function exists. TODO
        return GetCPUMoveEvent(fight, fighter); // GetUserMoveEvent(fight, fighter);
    }

    public MoveEvent GetMoveEventOfType(Fight fight, Fighter fighter, Enums.MoveType moveType)
    {        
        switch (moveType)
        {
            case Enums.MoveType.Spell:
            case Enums.MoveType.Melee:
            case Enums.MoveType.Psychic:
            case Enums.MoveType.NinTai:
            case Enums.MoveType.Projectile:
                {
                    return GetOffensiveMoveEvent(fight, fighter, moveType);
                }
            case Enums.MoveType.PowerUp:
                {
                    return GetPowerUpMoveEvent(fight, fighter);
                }
            case Enums.MoveType.Protect:
                {
                    return GetProtectionMoveEvent(fight, fighter);
                }
            case Enums.MoveType.Medical:
                {
                    return GetMedicalMoveEvent(fight, fighter);
                }
            case Enums.MoveType.Substitution:
                {
                    return GetSubMoveEvent(fight, fighter);
                }
            case Enums.MoveType.Clone:
                {
                    return GetCloneMoveEvent(fight, fighter);
                }
            case Enums.MoveType.Summon:
                {
                    return GetSummonMoveEvent(fight, fighter);
                }
            case Enums.MoveType.Skip:
                {
                    return GetSkipMoveEvent(fighter);
                }
        }

        Debug.Log("Error! Unrecognized MoveType [" + moveType + "] in AI.GetMoveEventOfType!");
        return GetSkipMoveEvent(fighter);
    }

    public Enums.MoveType GetMoveTypeFromTendencies(Fight fight, Fighter fighter)
    {
        // Upper and lower bounds for an upcoming selection result based on probability
        float spellLower = 1, spellUpper = 1, ninTaiLower = 1, ninTaiUpper = 1, projectileLower = 1, projectileUpper = 1, subLower = 1;
        float subUpper = 1, summonLower = 1, summonUpper = 1, meleeLower = 1, meleeUpper = 1, cloneLower = 1, cloneUpper = 1;
        float psychicLower = 1, psychicUpper = 1, medicalLower = 1, medicalUpper = 1;
        float currentUpper = 0; // This keeps track of the full probability range to use in the final move type selection calculation.

        if (SpellFraction > 0 && CheckForUsableMoves(fight, fighter, Enums.MoveType.Spell) == true)
        {
            spellLower = currentUpper;
            currentUpper += SpellFraction;
            spellUpper = currentUpper;
        }
        if (NinTaiFraction > 0 && CheckForUsableMoves(fight, fighter, Enums.MoveType.NinTai) == true)
        {
            ninTaiLower = currentUpper;
            currentUpper += NinTaiFraction;
            ninTaiUpper = currentUpper;
        }
        if (ProjectileFraction > 0 && CheckForUsableMoves(fight, fighter, Enums.MoveType.Projectile) == true)
        {
            projectileLower = currentUpper;
            currentUpper += ProjectileFraction;
            projectileUpper = currentUpper;
        }
        if (SubFraction > 0 && CheckForUsableMoves(fight, fighter, Enums.MoveType.Substitution) == true)
        {
            subLower = currentUpper;
            currentUpper += SubFraction;
            subUpper = currentUpper;
        }
        if (SummonFraction > 0 && CheckForUsableMoves(fight, fighter, Enums.MoveType.Summon) == true)
        {
            summonLower = currentUpper;
            currentUpper += SummonFraction;
            summonUpper = currentUpper;
        }
        if (MeleeFraction > 0 && CheckForUsableMoves(fight, fighter, Enums.MoveType.Melee) == true)
        {
            meleeLower = currentUpper;
            currentUpper += MeleeFraction;
            meleeUpper = currentUpper;
        }
        if (CloneFraction > 0 && CheckForUsableMoves(fight, fighter, Enums.MoveType.Clone) == true)
        {
            cloneLower = currentUpper;
            currentUpper += CloneFraction;
            cloneUpper = currentUpper;
        }
        if (PsychicFraction > 0 && CheckForUsableMoves(fight, fighter, Enums.MoveType.Psychic) == true)
        {
            psychicLower = currentUpper;
            currentUpper += PsychicFraction;
            psychicUpper = currentUpper;
        }
        if (MedicalFraction > 0 && CheckForUsableMoves(fight, fighter, Enums.MoveType.Medical) == true)
        {
            medicalLower = currentUpper;
            currentUpper += MedicalFraction;
            medicalUpper = currentUpper;
        }

        float randomNumber = Random.Range(0f, currentUpper);

        if (spellLower <= randomNumber && randomNumber <= spellUpper)
        {
            return Enums.MoveType.Spell;
        }
        else if (ninTaiLower <= randomNumber && randomNumber <= ninTaiUpper)
        {
            return Enums.MoveType.NinTai;
        }
        else if (projectileLower <= randomNumber && randomNumber <= projectileUpper)
        {
            return Enums.MoveType.Projectile;
        }
        else if (subLower <= randomNumber && randomNumber <= subUpper)
        {
            return Enums.MoveType.Substitution;
        }
        else if (summonLower <= randomNumber && randomNumber <= summonUpper)
        {
            return Enums.MoveType.Summon;
        }
        else if (meleeLower <= randomNumber && randomNumber <= meleeUpper)
        {
            return Enums.MoveType.Melee;
        }
        else if (cloneLower <= randomNumber && randomNumber <= cloneUpper)
        {
            return Enums.MoveType.Clone;
        }
        else if (psychicLower <= randomNumber && randomNumber <= psychicUpper)
        {
            return Enums.MoveType.Psychic;
        }
        else if (medicalLower <= randomNumber && randomNumber <= medicalUpper)
        {
            return Enums.MoveType.Medical;
        }

        return Enums.MoveType.Skip;
    }

    public MoveEvent GetOffensiveMoveEvent(Fight fight, Fighter fighter, Enums.MoveType moveType)
    {
        MoveEvent moveEvent = new MoveEvent();
        moveEvent.SetMoveType(moveType);
        moveEvent.AddFighter(fighter);
        moveEvent.AddRandomAdd(Fight.RandomAdd());

        OffensiveMove offensiveMove = GetOffensiveMoveOfType(fight, fighter, moveType);
        moveEvent.AddMove(offensiveMove);

        Enums.TargetType targetType = offensiveMove.GetTargetType();
        moveEvent.SetTargetType(targetType);

        switch (targetType)
        {
            case Enums.TargetType.OneEnemy:
                {
                    Fighter target = GetTarget(fight, fighter, offensiveMove);
                    moveEvent.AddTarget(target);
                    break;
                }
            case Enums.TargetType.EnemyTeam:
                {
                    int targetTeam = GetTargetTeam(fight, fighter, offensiveMove);
                    moveEvent.SetTargetTeam(targetTeam);
                    break;
                }
            default:
                {
                    Debug.LogError("Error! Unexpected TargetType [" + targetType + "] in AI.GetMoveEventOfType for OffensiveMove " + offensiveMove.GetName() + " with ID: " + offensiveMove.GetID() + "!");
                    break;
                }
        }

        return moveEvent;
    }

    public OffensiveMove GetOffensiveMoveOfType(Fight fight, Fighter fighter, Enums.MoveType moveType)
    {
        List<OffensiveMove> possibleMoves = new List<OffensiveMove>();
        List<OffensiveMove> weightedPossibleMoves = new List<OffensiveMove>();

        foreach (OffensiveMove offensiveMove in fighter.GetOffensiveMoves())
        {
            if (offensiveMove.GetMoveType() == moveType && CheckIfCanPerformMove(fight, fighter, offensiveMove) == true)
            {
                possibleMoves.Add(offensiveMove);
            }
        }

        weightedPossibleMoves = GetWeightedListOfOffensiveMoves(fight, fighter, possibleMoves, moveType);
        int weightedPossibleMovesCount = weightedPossibleMoves.Count;

        if (weightedPossibleMovesCount < 1)
        {
            Debug.LogError("Error! Empty weighted possible move list in AI.GetOffensiveMoveOfType!");
            return null;
        }

        int choice = Random.Range(0, weightedPossibleMovesCount);

        return weightedPossibleMoves[choice];
    }

    public void GetPossibleMedicalMovesByType(Fight fight, Fighter fighter, List<MedicalMove> teamHealthMoves, List<MedicalMove> teamManaMoves, List<MedicalMove> selfHealthMoves, List<MedicalMove> oneTeamMemberHealthMoves, List<MedicalMove> oneTeamMemberManaMoves)
    {
        teamHealthMoves.Clear();
        teamManaMoves.Clear();
        selfHealthMoves.Clear();
        oneTeamMemberHealthMoves.Clear();
        oneTeamMemberManaMoves.Clear();

        foreach (MedicalMove medicalMove in fighter.GetMedicalMoves())
        {
            if (CheckIfCanPerformMove(fight, fighter, medicalMove) == true)
            {
                switch (medicalMove.GetTargetType())
                {
                    case Enums.TargetType.Team:
                        {
                            if (medicalMove.GetHealType() == Enums.HealType.Health || medicalMove.GetHealType() == Enums.HealType.HealthAndMana)
                            {
                                teamHealthMoves.Add(medicalMove);
                            }

                            if (medicalMove.GetHealType() == Enums.HealType.Mana || medicalMove.GetHealType() == Enums.HealType.HealthAndMana)
                            {
                                teamManaMoves.Add(medicalMove);
                            }
                            break;
                        }
                    case Enums.TargetType.Self:
                        {
                            if (medicalMove.GetHealType() == Enums.HealType.Health || medicalMove.GetHealType() == Enums.HealType.HealthAndMana)
                            {
                                selfHealthMoves.Add(medicalMove);
                            }
                            break;
                        }
                    case Enums.TargetType.OneTeamMember:
                        {
                            if (medicalMove.GetHealType() == Enums.HealType.Health || medicalMove.GetHealType() == Enums.HealType.HealthAndMana)
                            {
                                oneTeamMemberHealthMoves.Add(medicalMove);
                                selfHealthMoves.Add(medicalMove);
                            }

                            if (medicalMove.GetHealType() == Enums.HealType.Mana || medicalMove.GetHealType() == Enums.HealType.HealthAndMana)
                            {
                                oneTeamMemberManaMoves.Add(medicalMove);
                            }
                            break;
                        }
                }
            }
        }
    }

    public virtual MoveEvent GetPowerUpMoveEvent(Fight fight, Fighter fighter)
    {
        List<PowerUpMove> possiblePowerUpMoves = new List<PowerUpMove>();
        foreach (PowerUpMove possiblePowerUpMove in fighter.GetPowerUpMoves())
        {
            //if (fighter.CheckIfCapableOfMove(possiblePowerUpMove) == true && CheckIfMoveHasRemainingUses(fighter, possiblePowerUpMove) == true)
            if (fighter.GetAI().CheckIfCanPerformMove(fight, fighter, possiblePowerUpMove) == true)
            {
                possiblePowerUpMoves.Add(possiblePowerUpMove);
            }
        }

        if (possiblePowerUpMoves.Count < 1)
        {
            Debug.LogError("Error! Cannot get power up move in AI.GetPowerUpMoveEvent because there are no possible power up moves for fighter: " + fighter.GetName() + ". Returning a Skip MoveEvent.");
            return GetSkipMoveEvent(fighter);
        }

        possiblePowerUpMoves.Sort((left, right) => left.GetLevel().CompareTo(right.GetLevel()));    // Sort by level in ascending order.
        PowerUpMove powerUpMove = possiblePowerUpMoves[0];                                          // Fighter will use power ups in order of increasing level.

        MoveEvent moveEvent = new MoveEvent();
        moveEvent.SetMoveType(Enums.MoveType.PowerUp);
        moveEvent.AddMove(powerUpMove);
        moveEvent.AddFighter(fighter);
        moveEvent.AddTarget(fighter);
        moveEvent.SetTargetType(Enums.TargetType.Self);
        moveEvent.AddRandomAdd(Fight.RandomAdd());

        return moveEvent;
    }

    public virtual MoveEvent GetProtectionMoveEvent(Fight fight, Fighter fighter)
    {
        List<Fighter> teammatesInNeed = new List<Fighter>();
        teammatesInNeed.AddRange(GetTeammatesUnableToMove(fight, fighter));
        teammatesInNeed.AddRange(GetTeammatesNearDefeat(fight, fighter));

        MoveEvent moveEvent = new MoveEvent();
        moveEvent.SetMoveType(Enums.MoveType.Protect);
        moveEvent.AddFighter(fighter);
        moveEvent.AddRandomAdd(Fight.RandomAdd());
        moveEvent.SetTargetType(Enums.TargetType.OneTeamMember);  // This could be changed to Self at the end if there are no teammates to protect.

        if (teammatesInNeed.Count > 0)  // Teammates near death or unable to move. There could be copies of teammates if someone is in both lists.
        {
            int index = Random.Range(0, teammatesInNeed.Count);
            moveEvent.AddTarget(teammatesInNeed[index]);

            return moveEvent;
        }

        List<Fighter> teammates = GetTeammates(fight, fighter);

        foreach (Fighter teammate in teammates)
        {
            float overallDiff = fighter.GetOverallRating() - teammate.GetOverallRating();

            if (overallDiff >= 2f)                      // Get teammates with a significantly lower overall rating.
            {
                teammatesInNeed.Add(teammate);
            }
        }

        if (teammatesInNeed.Count > 0)                  // Teammates with a significantly lower overall rating.
        {
            int index = Random.Range(0, teammatesInNeed.Count);
            moveEvent.AddTarget(teammatesInNeed[index]);

            return moveEvent;
        }

        List<Fighter> teammatesWeakened = GetTeammatesWeakened(fight, fighter);
        
        if (teammatesWeakened.Count > 0)                // Teammates weakened.
        {
            int index = Random.Range(0, teammatesWeakened.Count);
            moveEvent.AddTarget(teammatesWeakened[index]);

            return moveEvent;
        }

        if (teammates.Count > 0)                        // Teammates in general.
        {            
            int index = Random.Range(0, teammates.Count);
            moveEvent.AddTarget(teammates[index]);

            return moveEvent;
        }

        moveEvent.AddTarget(fighter);
        moveEvent.SetTargetType(Enums.TargetType.Self);

        return moveEvent;
    }

    public MoveEvent GetSkipMoveEvent(Fighter fighter)
    {
        MoveEvent moveEvent = new MoveEvent();
        moveEvent.SetMoveType(Enums.MoveType.Skip);
        moveEvent.SetTargetType(Enums.TargetType.Self);
        moveEvent.AddFighter(fighter);
        moveEvent.AddRandomAdd(Fight.RandomAdd());

        return moveEvent;
    }

    public MoveEvent GetStandardAIMoveEvent(Fight fight, Fighter fighter)
    {
        // Check to see if fighter should power up based on enemy's strength, fighter's health/mana, and power up move availability.
        if (CheckIfShouldPowerUp(fight, fighter) == true)
        {
            return GetPowerUpMoveEvent(fight, fighter);
        }

        // Check to see if a teammate needs protecting.
        if (CheckIfShouldProtectTeammate(fight, fighter) == true)
        {
            return GetProtectionMoveEvent(fight, fighter);
        }

        // Pick a move and target based on this AI's tendencies
        Enums.MoveType moveType = GetMoveTypeFromTendencies(fight, fighter);

        return GetMoveEventOfType(fight, fighter, moveType);
    }

    public virtual MoveEvent GetSubMoveEvent(Fight fight, Fighter fighter)
    {
        MoveEvent moveEvent = new MoveEvent();
        moveEvent.SetMoveType(Enums.MoveType.Substitution);
        moveEvent.AddFighter(fighter);
        moveEvent.SetTargetType(Enums.TargetType.Self);
        moveEvent.AddTarget(fighter);
        moveEvent.AddRandomAdd(Fight.RandomAdd());

        List<SubMove> possibleSubMoves = new List<SubMove>();

        foreach (SubMove subMove in fighter.GetSubMoves())
        {
            if (CheckIfCanPerformMove(fight, fighter, subMove) == true)
            {
                possibleSubMoves.Add(subMove);
            }
        }

        List<SubMove> weightedPossibleSubMoves = GetWeightedListOfSubMoves(fight, fighter, possibleSubMoves);

        if (weightedPossibleSubMoves.Count < 1)
        {
            Debug.LogError("Error! No possible substitution moves in AI.GetSubMoveEvent! Returning a Skip MoveEvent.");
            return GetSkipMoveEvent(fighter);
        }    

        int choice = Random.Range(0, weightedPossibleSubMoves.Count);
        moveEvent.AddMove(weightedPossibleSubMoves[choice]);

        return moveEvent;
    }

    public virtual MoveEvent GetSummonMoveEvent(Fight fight, Fighter fighter)
    {
        MoveEvent moveEvent = new MoveEvent();
        moveEvent.SetMoveType(Enums.MoveType.Summon);
        moveEvent.AddFighter(fighter);
        moveEvent.SetTargetType(Enums.TargetType.Self);
        moveEvent.AddTarget(fighter);
        moveEvent.AddRandomAdd(Fight.RandomAdd());

        List<SummonMove> possibleSummonMoves = new List<SummonMove>();

        foreach (SummonMove summonMove in fighter.GetSummonMoves())
        {
            if (CheckIfCanPerformMove(fight, fighter, summonMove) == true)
            {
                possibleSummonMoves.Add(summonMove);
            }
        }

        List<SummonMove> weightedPossibleSummonMoves = GetWeightedListOfSummonMoves(fight, fighter, possibleSummonMoves);

        if (weightedPossibleSummonMoves.Count < 1)
        {
            Debug.LogError("Error! No possible summon moves in AI.GetSummonMoveEvent! Returning a Skip MoveEvent.");
            return GetSkipMoveEvent(fighter);
        }

        int choice = Random.Range(0, weightedPossibleSummonMoves.Count);
        moveEvent.AddMove(weightedPossibleSummonMoves[choice]);

        return moveEvent;
    }

    public Fighter GetTarget(Fight fight, Fighter fighter, OffensiveMove offensiveMove)
    {
        Enums.TargetType targetType = offensiveMove.GetTargetType();

        if (targetType != Enums.TargetType.OneEnemy)
        {
            Debug.LogError("Error! Incorrect Enums.TargetType in AI.GetTarget for OffensiveMove " + offensiveMove.GetName() + " with ID: " + offensiveMove.GetID() + "!");
            return null;
        }
        
        Enums.Nature[] moveNatures = offensiveMove.GetNaturesArray();
        int choice = 0;

        List<Enums.StatusType> requiredStatuses = offensiveMove.GetRequiredTargetStatusesList();
        if (requiredStatuses.Count > 0)
        {
            List<Fighter> enemiesWithStatuses = GetEnemiesWithStatuses(fight, fighter, requiredStatuses);
            
            if (enemiesWithStatuses.Count < 1)
            {
                Debug.LogError("Error! There are no enemies with the the required statuses in AI.GetTarget(fight, fighter, offensiveMove) for " + fighter.GetName() + " with ID: " + fighter.GetID() 
                    + " while trying to use " + offensiveMove.GetName() + " with ID: " + offensiveMove.GetID() + "!");

                return null;
            }

            List<Fighter> weightedEnemiesWithStatuses = GetWeightedListOfOffensiveTargets(fight, fighter, offensiveMove, enemiesWithStatuses);
            choice = Random.Range(0, weightedEnemiesWithStatuses.Count);    // Returning random enemy that has all required statuses

            return weightedEnemiesWithStatuses[choice];
        }

        List<Fighter> enemies = GetEnemies(fight, fighter);

        if (offensiveMove.GetMoveType() == Enums.MoveType.Psychic)
        {
            List<Fighter> enemiesFreeOfPsychic = new List<Fighter>();

            foreach (Fighter enemy in enemies)
            {
                if (enemy.CheckStatus(Enums.StatusType.PsychicParalysis) == false && enemy.CheckStatus(Enums.StatusType.PsychicControl) == false)
                {
                    enemiesFreeOfPsychic.Add(enemy);
                }
            }

            if (enemiesFreeOfPsychic.Count > 1)
            {
                List<Fighter> weightedEnemiesFreeOfPsychic = GetWeightedListOfOffensiveTargets(fight, fighter, offensiveMove, enemiesFreeOfPsychic);
                choice = Random.Range(0, weightedEnemiesFreeOfPsychic.Count);  // Returning random enemy that is not under psychic control

                return weightedEnemiesFreeOfPsychic[choice];
            }
        }

        List<Fighter> enemiesUnableToMove = GetEnemiesUnableToMove(fight, fighter);
        int randomNumber = Random.Range(0, 100);

        if (enemiesUnableToMove.Count > 1 && randomNumber < 67)     // 67% of the time they'll go for an enemy unable to move
        {
            List<Fighter> weightedEnemiesUnableToMove = GetWeightedListOfOffensiveTargets(fight, fighter, offensiveMove, enemiesUnableToMove);
            choice = Random.Range(0, weightedEnemiesUnableToMove.Count);    // Returning random enemy that is unable to move

            return weightedEnemiesUnableToMove[choice];
        }

        List<Fighter> enemiesNearDeath = GetEnemiesNearDefeat(fight, fighter);
        randomNumber = Random.Range(0, 100);

        if (enemiesNearDeath.Count > 1 && randomNumber < 67)     // 67% of the time they'll go for an enemy close to death
        {
            List<Fighter> weightedEnemiesCloseToDeath = GetWeightedListOfOffensiveTargets(fight, fighter, offensiveMove, enemiesNearDeath);
            choice = Random.Range(0, weightedEnemiesCloseToDeath.Count);    // Returning random enemy that is close to death

            return weightedEnemiesCloseToDeath[choice];
        }

        List<Fighter> weightedEnemies = GetWeightedListOfOffensiveTargets(fight, fighter, offensiveMove, enemies);
        choice = Random.Range(0, weightedEnemies.Count);    // Returning random enemy

        return weightedEnemies[choice];
    }

    public int GetTargetTeam(Fight fight, Fighter fighter, OffensiveMove offensiveMove)
    {
        int team = fighter.GetTeam();
        List<int> possibleTeams = new List<int>();
        
        for (int targetTeam = 1; targetTeam < 4; ++targetTeam)
        {
            if (targetTeam != team && fight.GetTeamList(targetTeam).Count > 0)
            {
                possibleTeams.Add(targetTeam);
            }
        }

        int choice = Random.Range(0, possibleTeams.Count);

        return possibleTeams[choice];
    }

    public List<Fighter> GetTeammates(Fight fight, Fighter fighter)
    {
        int team = fighter.GetTeam();
        List<Fighter> teammates = new List<Fighter>();

        foreach (Fighter tempFighter in fight.GetFighters())
        {
            if (tempFighter.GetTeam() == team && tempFighter != fighter)
            {
                teammates.Add(tempFighter);
            }
        }

        return teammates;
    }

    public List<Fighter> GetTeammatesMissingXMana(Fight fight, Fighter fighter, int x)
    {
        List<Fighter> teammates = GetTeammates(fight, fighter);
        List<Fighter> teammatesMissingXMana = new List<Fighter>();

        foreach (Fighter teammate in teammates)
        {
            if (teammate.CheckIfMissingXMana(x) == true)
            {
                teammatesMissingXMana.Add(teammate);
            }
        }

        return teammatesMissingXMana;
    }

    public List<Fighter> GetTeammatesMissingXHealth(Fight fight, Fighter fighter, int x)
    {
        List<Fighter> teammates = GetTeammates(fight, fighter);
        List<Fighter> teammatesMissingXHealth = new List<Fighter>();

        foreach (Fighter teammate in teammates)
        {
            if (teammate.CheckIfMissingXHealth(x) == true)
            {
                teammatesMissingXHealth.Add(teammate);
            }
        }

        return teammatesMissingXHealth;
    }

    public List<Fighter> GetTeammatesNearDefeat(Fight fight, Fighter fighter)
    {
        List<Fighter> teammates = GetTeammates(fight, fighter);
        List<Fighter> teammatesNearDefeat = new List<Fighter>();

        foreach (Fighter teammate in teammates)
        {
            if (teammate.CheckIfNearDefeat() == true)
            {
                teammatesNearDefeat.Add(teammate);
            }
        }

        return teammatesNearDefeat;
    }

    public List<Fighter> GetTeammatesUnableToMove(Fight fight, Fighter fighter)
    {
        List<Fighter> teammates = GetTeammates(fight, fighter);
        List<Fighter> teammatesUnableToMove = new List<Fighter>();

        foreach (Fighter teammate in teammates)
        {
            if (teammate.CheckIfCanMove() == false)
            {
                teammatesUnableToMove.Add(teammate);
            }
        }

        return teammatesUnableToMove;
    }

    public List<Fighter> GetTeammatesWeakened(Fight fight, Fighter fighter)
    {
        List<Fighter> teammates = GetTeammates(fight, fighter);
        List<Fighter> teammatesWeakened = new List<Fighter>();

        foreach (Fighter teammate in teammates)
        {
            if (teammate.CheckIfWeakened() == true)
            {
                teammatesWeakened.Add(teammate);
            }
        }

        return teammatesWeakened;
    }

    public List<Fighter> GetTeammatesWithManaBelowX(Fight fight, Fighter fighter, int x)
    {
        List<Fighter> teammates = GetTeammates(fight, fighter);
        List<Fighter> teammatesWithManaBelowX = new List<Fighter>();

        foreach (Fighter teammate in teammates)
        {
            if (teammate.GetHealth() <= x)
            {
                teammatesWithManaBelowX.Add(teammate);
            }
        }

        return teammatesWithManaBelowX;
    }

    public List<Fighter> GetTeammatesWithHealthBelowX(Fight fight, Fighter fighter, int x)
    {
        List<Fighter> teammates = GetTeammates(fight, fighter);
        List<Fighter> teammatesWithHealthBelowX = new List<Fighter>();

        foreach (Fighter teammate in teammates)
        {
            if (teammate.GetHealth() <= x)
            {
                teammatesWithHealthBelowX.Add(teammate);
            }
        }

        return teammatesWithHealthBelowX;
    }

    public List<Fighter> GetTeammatesWithStatuses(Fight fight, Fighter fighter, List<Enums.StatusType> statuses)
    {
        List<Fighter> teammates = GetTeammates(fight, fighter);
        List<Fighter> teammatesWithAllRequiredStatuses = new List<Fighter>();

        foreach (Fighter teammate in teammates)
        {
            bool teammateHasAllRequiredStatuses = true;

            foreach (Enums.StatusType statusType in statuses)
            {
                if (teammate.CheckStatus(statusType) == false)
                {
                    teammateHasAllRequiredStatuses = false;
                }
            }

            if (teammateHasAllRequiredStatuses)
            {
                teammatesWithAllRequiredStatuses.Add(teammate);
            }
        }

        return teammatesWithAllRequiredStatuses;
    }

    public List<Fighter> GetTeammatesWithStatuses(Fight fight, Fighter fighter, Enums.StatusType[] statuses)
    {
        List<Fighter> teammates = GetTeammates(fight, fighter);
        List<Fighter> teammatesWithAllRequiredStatuses = new List<Fighter>();

        foreach (Fighter teammate in teammates)
        {
            bool teammateHasAllRequiredStatuses = true;

            foreach (Enums.StatusType statusType in statuses)
            {
                if (teammate.CheckStatus(statusType) == false)
                {
                    teammateHasAllRequiredStatuses = false;
                }
            }

            if (teammateHasAllRequiredStatuses)
            {
                teammatesWithAllRequiredStatuses.Add(teammate);
            }
        }

        return teammatesWithAllRequiredStatuses;
    }

    public int GetWeightForMove(Fight fight, Fighter fighter, Move move, Enums.MoveType moveType)
    {
        if (fighter.CheckIfManaIsLow() == true)
        {
            if (move.GetMana() <= 10)
            {
                return 7;
            }

            return 1;
        }

        float fighterSkill = 0;

        switch (moveType)
        {
            case Enums.MoveType.Melee:
                fighterSkill = fighter.GetMelee();
                break;
            case Enums.MoveType.Psychic:
                fighterSkill = fighter.GetPsychic();
                break;
            case Enums.MoveType.Spell:
            case Enums.MoveType.Medical:
            case Enums.MoveType.Projectile:
            case Enums.MoveType.NinTai:
            case Enums.MoveType.Defensive:
            case Enums.MoveType.Avoid:
            case Enums.MoveType.Clone:
            case Enums.MoveType.PowerUp:
            case Enums.MoveType.Substitution:
            case Enums.MoveType.Summon:
                fighterSkill = fighter.GetSpellcraft();
                break;
            default:
                Debug.LogError("Error! Unexpected move type [" + moveType + "] in AI.GetWeightForMove!");
                return 1;
        }

        float skillDifference = fighterSkill - move.GetLevel();

        if (0f <= skillDifference && skillDifference <= 2f)
        {
            return 8;
        }
        if (2f < skillDifference && skillDifference <= 4f)
        {
            return 3;
        }

        return 1;
    }

    public int GetWeightForMedicalTarget(Fight fight, Fighter fighter, Fighter target, MedicalMove medicalMove)
    {
        switch (medicalMove.GetHealType())
        {
            case Enums.HealType.Health:
                {
                    if (target.GetHealth() <= 20)
                    {
                        return 18;
                    }
                    if (target.GetHealth() <= 50)
                    {
                        return 12;
                    }
                    if (target.GetHealth() <= 75)
                    {
                        return 6;
                    }
                    break;
                }
            case Enums.HealType.Mana:
                {
                    if (target.GetMana() <= 20)
                    {
                        return 18;
                    }
                    if (target.GetMana() <= 50)
                    {
                        return 12;
                    }
                    if (target.GetMana() <= 75)
                    {
                        return 6;
                    }
                    break;
                }
            case Enums.HealType.HealthAndMana:
                {
                    if (target.GetHealthCo() <= .40f)
                    {
                        return 18;
                    }
                    if (target.GetHealthCo() <= .70f)
                    {
                        return 12;
                    }
                    if (target.GetHealthCo() <= .92f)
                    {
                        return 6;
                    }
                    break;
                }
        }

        return 1;
    }

    public int GetWeightForOffensiveTarget(Fight fight, Fighter fighter, Fighter target, OffensiveMove offensiveMove)
    {
        if (offensiveMove.GetMoveType() == Enums.MoveType.Psychic)
        {
            if (target.CheckStatus(Enums.StatusType.PsychicControl) == true || target.CheckStatus(Enums.StatusType.PsychicParalysis) == true) // Target is already under psychic control
            {
                return 1;
            }

            float skillDifference = offensiveMove.GetLevel() - target.GetPsychic();

            if (skillDifference >= 2f)     // Target is significantly weaker in terms of psychic abilities.
            {
                return 5;
            }

            if (skillDifference >= 1f)     // Target is weaker in terms of psychic abilities.
            {
                return 2;
            }

            return 1;
        }
        
        if (target.CheckIfCanMove() == false)
        {
            return 10;
        }

        if (target.CheckIfNearDefeat() == true)
        {
            return 4;
        }

        if (target.CheckIfWeakened() == true)
        {
            return 2;
        }

        return 1;
    }

    public List<CloneMove> GetWeightedListOfCloneMoves(Fight fight, Fighter fighter, List<CloneMove> cloneMoves)
    {
        List<CloneMove> weightedCloneMoves = new List<CloneMove>();

        foreach (CloneMove cloneMove in cloneMoves)
        {
            int weight = GetWeightForMove(fight, fighter, cloneMove, Enums.MoveType.Clone);
            weightedCloneMoves.AddRange(Enumerable.Repeat(cloneMove, weight));
        }

        if (weightedCloneMoves.Count < 1)
        {
            Debug.LogError("Error! Empty weighted clone move list in AI.GetWeightedListOfCloneMoves!");
        }

        return weightedCloneMoves;
    }

    public List<MedicalMove> GetWeightedListOfMedicalMoves(Fight fight, Fighter fighter, List<MedicalMove> medicalMoves)
    {
        List<MedicalMove> weightedMedicalMoves = new List<MedicalMove>();

        foreach (MedicalMove medicalMove in medicalMoves)
        {
            int weight = GetWeightForMove(fight, fighter, medicalMove, Enums.MoveType.Medical);
            weightedMedicalMoves.AddRange(Enumerable.Repeat(medicalMove, weight));
        }

        if (weightedMedicalMoves.Count < 1)
        {
            Debug.LogError("Error! Empty weighted medical move list in AI.GetWeightedListOfMedicalMoves!");
        }

        return weightedMedicalMoves;
    }

    public List<OffensiveMove> GetWeightedListOfOffensiveMoves(Fight fight, Fighter fighter, List<OffensiveMove> offensiveMoves, Enums.MoveType moveType)
    {
        List<OffensiveMove> weightedOffensiveMoves = new List<OffensiveMove>();

        foreach (OffensiveMove offensiveMove in offensiveMoves)
        {
            int weight = GetWeightForMove(fight, fighter, offensiveMove, moveType);
            weightedOffensiveMoves.AddRange(Enumerable.Repeat(offensiveMove, weight));
        }

        if (weightedOffensiveMoves.Count < 1)
        {
            Debug.LogError("Error! Empty weighted offensive move list in AI.GetWeightedListOfOffensiveMoves!");
        }

        return weightedOffensiveMoves;
    }

    public List<Fighter> GetWeightedListOfMedicalTargets(Fight fight, Fighter fighter, MedicalMove medicalMove, List<Fighter> targets)
    {
        List<Fighter> weightedTargets = new List<Fighter>();

        foreach (Fighter target in targets)
        {
            int weight = GetWeightForMedicalTarget(fight, fighter, target, medicalMove);
            weightedTargets.AddRange(Enumerable.Repeat(target, weight));
        }

        if (weightedTargets.Count < 1)
        {
            Debug.LogError("Error! Empty weighted targets list in AI.GetWeightedListOfMedicalTargets!");
        }

        return weightedTargets;
    }

    public List<Fighter> GetWeightedListOfOffensiveTargets(Fight fight, Fighter fighter, OffensiveMove offensiveMove, List<Fighter> targets)
    {
        List<Fighter> weightedTargets = new List<Fighter>();

        foreach (Fighter target in targets)
        {
            int weight = GetWeightForOffensiveTarget(fight, fighter, target, offensiveMove);
            weightedTargets.AddRange(Enumerable.Repeat(target, weight));
        }

        if (weightedTargets.Count < 1)
        {
            Debug.LogError("Error! Empty weighted targets list in AI.GetWeightedListOfOffensiveTargets!");
        }

        return weightedTargets;
    }

    public List<SubMove> GetWeightedListOfSubMoves(Fight fight, Fighter fighter, List<SubMove> subMoves)
    {
        List<SubMove> weightedSubMoves = new List<SubMove>();

        foreach (SubMove subMove in subMoves)
        {
            int weight = GetWeightForMove(fight, fighter, subMove, Enums.MoveType.Substitution);
            weightedSubMoves.AddRange(Enumerable.Repeat(subMove, weight));
        }

        if (weightedSubMoves.Count < 1)
        {
            Debug.LogError("Error! Empty weighted medical move list in AI.GetWeightedListOfMedicalMoves!");
        }

        return weightedSubMoves;
    }

    public List<SummonMove> GetWeightedListOfSummonMoves(Fight fight, Fighter fighter, List<SummonMove> summonMoves)
    {
        List<SummonMove> weightedSummonMoves = new List<SummonMove>();

        foreach (SummonMove summonMove in summonMoves)
        {
            int weight = GetWeightForMove(fight, fighter, summonMove, Enums.MoveType.Summon);
            weightedSummonMoves.AddRange(Enumerable.Repeat(summonMove, weight));
        }

        if (weightedSummonMoves.Count < 1)
        {
            Debug.LogError("Error! Empty weighted summon move list in AI.GetWeightedListOfSummonMoves!");
        }

        return weightedSummonMoves;
    }
}
