using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.UIElements;

public class Fight : MonoBehaviour
{
    [SerializeField] SelectMove                 SelectMoveUI;
    [SerializeField] protected Enums.DebugLevel DebugLevel;
    [SerializeField] bool                       NarutoMode;
    [SerializeField] protected int              Teams;
    [SerializeField] protected List<Fighter>    Team1, Team2, Team3;

    protected List<Clone>                       Clones;
    protected List<Fighter>                     Fighters, OriginalFighters, OriginalTeam1, OriginalTeam2, OriginalTeam3;
    protected List<Protection>                  Protections;
    protected int                               RoundNumber;
    protected List<Summon>                      Summons;

    StreamWriter mWriter;

    public const float  ATTACK_SEAL_SPEED_PENALTY = 3.0f;
    public const float  EASY_BLOCK_POWER_DIFF = 1.5f;
    public const float  EASY_DEFLECT_SKILL_DIFF = 1.5f;
    public const float  EASY_DODGE_SPEED_DIFF = 1.5f;
    public const float  EASY_GENJUTSU_POWER_DIFF = 1.5f;
    public const float  DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE = 2.0f;
    public const float  MAX_ATTACK_POWER_DIFF_TO_USE_DEFENSIVE_MOVE = 0.9f;
    public const float  MAX_MOVE_POWER_BONUS_FROM_NATURES = 0.75f;
    public const int    MAX_NUMBER_OF_ROUNDS = 100;
    public const float  MIN_DAMAGE_CO_FROM_PARTIAL_HIT = .05f;
    public const float  MOVE_POWER_BONUS_FROM_NATURE_COMBO = 0.25f;
    public const float  NATURE_COMBO_DAMAGE_CO_CHANGE = 1.0f / 3.0f;
    public const float  NATURE_WEAKNESS_DAMAGE_CO_CHANGE = 1.0f / 3.0f;
    public const int    RANDOM_ADD_NUM_POSSIBLE_RESULTS = 100;
    public const float  RANDOM_ADD_RANGE = 1.0f;
    public const float  SUBSTITUTION_BONUS = 3.0f;
    public const float  SUMLIST_MAX_INCREASE = 2.0f;
    public const float  TAIJUTSU_MOVE_POWER_PENALTY = 1.0f;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void ApplyBonusHealth()
    {
        foreach (Fighter fighter in Fighters)
        {
            int healthBonus = 0;

            foreach(AttributeBonus attributeBonus in fighter.GetAttributeBonuses())
            {
                if (attributeBonus.GetAttribute() == Enums.Attribute.Health)
                {
                    healthBonus += (int)(attributeBonus.GetAmount() + .5f); // .5 is for rounding
                }
            }

            if (healthBonus > 0)
            {
                fighter.AddHealth(healthBonus);
                mWriter.WriteLine(fighter.GetName() + " heals " + healthBonus + " health!");
                //mWriter.WriteLine(fighter.GetName() + " heals " + attributeBonus.GetAmount() + " health with " + powerUp.GetPowerUpMove().GetName() + "!");
            }
            else if (healthBonus < 0)
            {
                fighter.RemoveHealth(healthBonus);
                mWriter.WriteLine(fighter.GetName() + " is damaged " + healthBonus + " health!");
                //mWriter.WriteLine(fighter.GetName() + " is damaged " + Mathf.Abs(attributeBonus.GetAmount()) + " health by " + powerUp.GetPowerUpMove().GetName() + "!");
                RemoveDefeatedFighters();
            }
        }
    }

    public float AdjustNatureDamageCo(Enums.Nature attackNature, Enums.Nature relativeStrongNature, Enums.Nature comboNature, List<Enums.Nature> targetNatures, bool attackHasOnlyOneNature)
    {
        float adjustment = 0.0f;

        if (relativeStrongNature != Enums.Nature.None && targetNatures.Contains(relativeStrongNature))
        {
            adjustment -= NATURE_WEAKNESS_DAMAGE_CO_CHANGE;
        }
        if (targetNatures.Contains(attackNature) && attackHasOnlyOneNature)
        {
            adjustment -= NATURE_WEAKNESS_DAMAGE_CO_CHANGE;
        }
        if (comboNature != Enums.Nature.None && targetNatures.Contains(comboNature))
        {
            adjustment += NATURE_COMBO_DAMAGE_CO_CHANGE;
        }

        return adjustment;
    }

    public void ApplyStandardDamage(int damage, Enums.DamageType damageType, Fighter target, List<Fighter> attackers)
    {
        switch (damageType)
        {
            case Enums.DamageType.Health:
                target.RemoveHealth(damage);
                break;
            case Enums.DamageType.AbsorbMana:
                target.RemoveMana(damage);
                attackers[0].AddMana(damage);
                break;
            case Enums.DamageType.Mana:
                target.RemoveMana(damage);
                break;
            case Enums.DamageType.AbsorbHealth:
                target.RemoveHealth(damage);
                attackers[0].AddHealth(damage);
                break;
        }
    }

    public int AttackDamage(Fighter target, List<Fighter> attackers, List<Move> offensiveMoves, List<float> randomAdds)
    {
        float totalDamage = 0.0f;
        int attackersCount = attackers.Count;

        for (int index = 0; index < attackersCount; ++index) // Get the damage from each individual move.
        {
            Fighter attacker = attackers[index];
            Move offensiveMove = offensiveMoves[index];
            float randomAdd = randomAdds[index];
            float damage = 0.0f;

            Enums.MoveType moveType = offensiveMove.GetMoveType();

            switch (moveType)
            {
                case Enums.MoveType.Melee:
                    {
                        float luckCo = (offensiveMove.GetLevel() + randomAdd) / (float)offensiveMove.GetLevel();
                        float attackerStrengthAndSkillRatio = (attacker.GetStrength() + attacker.GetMelee()) / 10.0f; // = 1 for an average fighter. 2 for 10/10.
                        float attackerStrengthAndSkillCo = 1.0f;

                        if (attackerStrengthAndSkillRatio >= 1.0f)
                        {
                            attackerStrengthAndSkillCo = Mathf.Pow(attackerStrengthAndSkillRatio, 1.58496250072f) * attacker.GetHealthCo();
                        }
                        else
                        {
                            attackerStrengthAndSkillCo = attackerStrengthAndSkillRatio;
                        }

                        damage = offensiveMove.GetDamage() * luckCo * attackerStrengthAndSkillCo * target.GetDamageCo() * NaturesDamageCo(attacker, target, offensiveMove);
                        break;
                    }
                case Enums.MoveType.Spell:
                case Enums.MoveType.NinTai:
                case Enums.MoveType.Projectile:
                    {
                        float luckCo = MovePower(attacker, offensiveMove, randomAdd) / (float)offensiveMove.GetLevel();
                        damage = offensiveMove.GetDamage() * luckCo * target.GetDamageCo() * NaturesDamageCo(attacker, target, offensiveMove);
                        break;
                    }
                case Enums.MoveType.Psychic:
                    {
                        float luckCo = MovePower(attacker, offensiveMove, randomAdd) / (float)offensiveMove.GetLevel();
                        damage = offensiveMove.GetDamage() * luckCo * NaturesDamageCo(attacker, target, offensiveMove);
                        break;
                    }
                default:
                    {
                        Debug.LogError("Errro! Unexpected MoveType [" + moveType + "] in Fight.AttackDamage!");
                        break;
                    }
            }

            totalDamage += damage;
        }

        return (int)(totalDamage + 0.5f); // .5 is for rounding.
    }

    // AttackerSpeed applies to Melee/NinTai attacks. This differs from MoveEvent.GetMeleeMoveSpeed in that it includes subBonus and accuracy.
    public float AttackerSpeed(Fighter attacker, Move offensiveMove, float randomAdd, float subBonus)
    {
        return attacker.GetSpeed() * attacker.GetHealthCo() + subBonus + offensiveMove.GetAccuracy() + randomAdd;
    }

    // AttackerSpeed applies to combined Melee/NinTai attacks
    public float AttackersSpeed(MoveEvent MoveEvent, float subBonus)
    {
        List<float> speeds = new List<float>();
        int NumberOfAttackers = MoveEvent.GetFighters().Count;
        if (NumberOfAttackers > 1)
        {
            subBonus = 0;
        }

        for (int index = 0; index < NumberOfAttackers; index++)
        {
            Fighter fighter = MoveEvent.GetFighters()[index];
            Move move = MoveEvent.GetMoves()[index];
            float randomAdd = MoveEvent.GetRandomAdds()[index];
            float speed = AttackerSpeed(fighter, move, randomAdd, subBonus);
            speeds.Add(speed);
        }

        return SumList(speeds);
    }

    public float AttackersStrength(MoveEvent MoveEvent)
    {
        List<float> strengths = new List<float>();
        int NumberOfAttackers = MoveEvent.GetFighters().Count;

        for (int index = 0; index < NumberOfAttackers; index++)
        {
            Fighter fighter = MoveEvent.GetFighters()[index];
            float randomAdd = MoveEvent.GetRandomAdds()[index];
            float strength = fighter.GetStrength() * fighter.GetHealthCo();// + randomAdd; ????????????????????
            strengths.Add(strength);
        }

        return SumList(strengths);
    }

    public float AttackerMeleeSkill(Fighter attacker, Move offensiveMove, float randomAdd, float subBonus)
    {
        float attackerSkill = ((1.0f / 3.0f) * attacker.GetMelee() + (1.0f / 3.0f) * attacker.GetStrength()) * attacker.GetHealthCo() + (1.0f / 3.0f) * offensiveMove.GetLevel() + 0.5f * subBonus + randomAdd;

        if (offensiveMove.GetNaturesList().Contains(Enums.Nature.Teleportation))
        {
            attackerSkill += 0.5f * offensiveMove.GetAccuracy();
        }

        return attackerSkill;
    }

    public float AttackersMeleeSkill(List<Fighter> attackers, List<Move> offensiveMoves, List<float> randomAdds, float subBonus)
    {
        List<float> skills = new List<float>();
        int NumberOfAttackers = attackers.Count;
        if (NumberOfAttackers > 1)
        {
            subBonus = 0;
        }

        for (int index = 0; index < NumberOfAttackers; index++)
        {
            Fighter fighter = attackers[index];
            Move jutsu = offensiveMoves[index];
            float randomAdd = randomAdds[index];
            float attackerSkill = AttackerMeleeSkill(fighter, jutsu, randomAdd, subBonus);
            skills.Add(attackerSkill);
        }

        return SumList(skills);
    }

    public bool CheckIfShouldExecuteMoveEvent(MoveEvent originalMoveEvent)
    {
        bool result = false;

        MoveEvent moveEvent = GetMoveEventWithActualAttackersAndTargets(originalMoveEvent);

        if (moveEvent.GetFighters().Count > 0) // At least one fighter can move.
        {
            switch (moveEvent.GetTargetType())
            {
                case Enums.TargetType.OneEnemy:
                case Enums.TargetType.OneTeamMember:
                case Enums.TargetType.EnemiesWithStatuses:
                case Enums.TargetType.TeamMembersWithStatuses:
                    {
                        List<Fighter> targets = moveEvent.GetTargets();

                        int index = 0;
                        int targetsCount = targets.Count;
                        while (index < targetsCount && result == false)
                        {
                            if (Fighters.Contains(targets[index]) == true)   // Target is alive
                            {
                                result = true;
                            }

                            ++index;
                        }
                        break;
                    }
                case Enums.TargetType.EnemyTeam:
                case Enums.TargetType.Team:
                    {
                        List<Fighter> targetTeamList = GetTeamList(moveEvent.GetTargetTeam());
                        result = targetTeamList.Count > 0;
                        break;
                    }
                case Enums.TargetType.Self:
                case Enums.TargetType.AllEnemies:
                    {
                        result = true;
                        break;
                    }
                default:
                    {
                        Debug.LogError("Error! Unexpected TargetType [" + moveEvent.GetTargetType() + "] in AI.CheckIfShouldExecute! Returning false.");
                        result = false;
                        break;
                    }
            }
        }

        return result;
    }

    public bool CheckForEndOfFight()
    {
        if ((Team1.Count == 0 && Team2.Count == 0) || (Team1.Count == 0 && Team3.Count == 0) || (Team2.Count == 0 && Team3.Count == 0))
        {
            return true;
        }

        return false;
        // No longer considering mana.
        /*bool someoneOnTeam1HasMana = false;
        bool someoneOnTeam2HasMana = false;
        bool someoneOnTeam3HasMana = false;
        bool sufficientFightersStillHaveMana = false;

        int index = 0;
        while (index < Fighters.Count && sufficientFightersStillHaveMana == false)
        {
            Fighter fighter = Fighters[index];

            switch (fighter.GetTeam())
            {
                case 1:
                    if (fighter.GetMana() > 0)
                    {
                        someoneOnTeam1HasMana = true;
                    }
                    break;
                case 2:
                    if (fighter.GetMana() > 0)
                    {
                        someoneOnTeam2HasMana = true;
                    }
                    break;
                case 3:
                    if (fighter.GetMana() > 0)
                    {
                        someoneOnTeam3HasMana = true;
                    }
                    break;
            }

            sufficientFightersStillHaveMana = ((someoneOnTeam1HasMana && someoneOnTeam2HasMana) || (someoneOnTeam2HasMana && someoneOnTeam3HasMana));
            ++index;
        }

        return sufficientFightersStillHaveMana == false;    // If sufficient fighters still have mana then the fight has not ended. */
    }

    public bool CheckIfProtecting(Fighter fighter)
    {
        bool result = false;

        foreach (Protection protection in Protections)
        {
            if (protection.GetProtector() == fighter)
            {
                result = true;
                break;
            }
        }

        return result;
    }

    public List<MoveEvent> CombineMoveEvents(List<MoveEvent> moveEvents)
    {
        foreach (MoveEvent moveEvent in moveEvents) // Checking moveEvents list passed into function
        {
            Enums.MoveType moveType = moveEvent.GetMoves()[0].GetMoveType();
            if (moveType == Enums.MoveType.Protect || moveType == Enums.MoveType.Substitution)
            {
                Debug.LogError("Error! Unexpected MoveType [" + moveType + "] for MoveEvent passed into Fight.CombinMoveEvents!");
            }
            if (moveEvent.GetFighters().Count > 1)
            {
                Debug.LogError("Error! Unexpected nunber of fighters [" + moveEvent.GetFighters().Count + "] for MoveEvent passed into Fight.CombinMoveEvents!");
            }
        }

        List<MoveEvent> finalMoveEvents = new List<MoveEvent>();

        List<Fighter> targets = new List<Fighter>();
        Dictionary<Fighter, List<MoveEvent>> targetToMoveEventListMap = new Dictionary<Fighter, List<MoveEvent>>();

        foreach (MoveEvent moveEvent in moveEvents) // Put some moves into final move list. Put most moves (coordinated) into separate lists based on target
        {
            Fighter fighter = moveEvent.GetFighters()[0];
            Move move = moveEvent.GetMoves()[0];

            if (Fighters.Contains(fighter) == true && fighter.GetAI().CheckIfCanPerformMove(this, fighter, move)) // Fighter is alive and move has targets.
            {
                if (moveEvent.GetTargetType() == Enums.TargetType.OneEnemy && fighter.CheckCombineAttacks() && move.CheckOffensive())
                {
                    Enums.DamageType damageType = move.GetDamageType();

                    if (damageType == Enums.DamageType.Health)      // Currently other damage types cannot be combined into one attack.
                    {
                        Fighter target = moveEvent.GetTargets()[0];

                        if (targetToMoveEventListMap.ContainsKey(target) == true)
                        {
                            targetToMoveEventListMap[target].Add(moveEvent);
                        }
                        else
                        {
                            List<MoveEvent> moveEventList = new List<MoveEvent>();
                            moveEventList.Add(moveEvent);
                            targetToMoveEventListMap.Add(target, moveEventList);
                        }

                        if (targets.Contains(target) == false)
                        {
                            targets.Add(target);
                        }
                    }
                    else
                    {
                        finalMoveEvents.Add(moveEvent);
                    }
                }
                else
                {
                    finalMoveEvents.Add(moveEvent);
                }
            }
        }

        finalMoveEvents.Sort((left, right) => right.GetMoveEventCastingSpeed().CompareTo(left.GetMoveEventCastingSpeed()));   // Sort in descending order. I think...

        foreach (Fighter target in targets)
        {
            List<MoveEvent> targetMoveEvents = targetToMoveEventListMap[target];
            List<MoveEvent> combinedTargetMoveEvents = CombineMoveEventsForTarget(targetMoveEvents);

            InsertCombinedTargetMoveEvents(combinedTargetMoveEvents, finalMoveEvents);  // Inserts into correct spot based on move event speed.
        }

        return finalMoveEvents; // return PlacePsychicMoveEvents(finalMoveEvents);
    }

    public List<MoveEvent> CombineMoveEventsForTarget(List<MoveEvent> moveEvents)
    {
        List<MoveEvent> finalMoveEvents = new List<MoveEvent>();

        if (moveEvents.Count == 1)
        {
            finalMoveEvents.Add(moveEvents[0]);
            return finalMoveEvents;
        }

        float minMoveEventCastingSpeed = float.MaxValue;

        List<MoveEvent> psychicMoveEvents = new List<MoveEvent>();     // Should happen first.
        List<MoveEvent> projectileMoveEvents = new List<MoveEvent>();   // Should happen second.
        List<MoveEvent> meleeMoveEvents = new List<MoveEvent>();        // Should happen third.

        foreach (MoveEvent moveEvent in moveEvents)
        {
            if (moveEvent.GetTargetType() == Enums.TargetType.OneEnemy && moveEvent.CheckCombineAttacks() == true)
            {
                float moveEventCastingSpeed = moveEvent.GetMoveEventCastingSpeed();

                if (moveEventCastingSpeed < minMoveEventCastingSpeed)
                {
                    minMoveEventCastingSpeed = moveEventCastingSpeed;
                }

                switch (moveEvent.GetMoveType())
                {
                    case Enums.MoveType.Spell:
                    case Enums.MoveType.Projectile:
                        projectileMoveEvents.Add(moveEvent);
                        break;
                    case Enums.MoveType.Melee:
                    case Enums.MoveType.NinTai:
                        meleeMoveEvents.Add(moveEvent);
                        break;
                    case Enums.MoveType.Psychic:
                        psychicMoveEvents.Add(moveEvent);
                        break;
                    default:
                        Debug.LogError("Error! Unexpected MoveType [" + moveEvent.GetMoveType() + "] for a move with TargetType [" + moveEvent.GetTargetType() + "] in Fight.CombineMoveEventsForTarget!");
                        finalMoveEvents.Add(moveEvent);
                        break;
                }
            }
            else
            {
                finalMoveEvents.Add(moveEvent);
            }
        }

        if (meleeMoveEvents.Count > 0)
        {
            MoveEvent meleeMoveEvent = new MoveEvent();
            meleeMoveEvent.AddTarget(meleeMoveEvents[0].GetTargets()[0]);
            meleeMoveEvent.SetTargetType(meleeMoveEvents[0].GetTargetType());
            meleeMoveEvent.SetEffectiveMoveEventCastingSpeed(minMoveEventCastingSpeed);
            meleeMoveEvent.SetMoveType(meleeMoveEvents[0].GetMoveType());

            foreach (MoveEvent moveEvent in meleeMoveEvents)
            {
                meleeMoveEvent.AddMove(moveEvent.GetMoves()[0]);
                meleeMoveEvent.AddFighter(moveEvent.GetFighters()[0]);
                meleeMoveEvent.AddRandomAdd(moveEvent.GetRandomAdds()[0]);
            }
            finalMoveEvents.Add(meleeMoveEvent);
        }
        if (projectileMoveEvents.Count > 0)
        {
            MoveEvent projectileMoveEvent = new MoveEvent();
            projectileMoveEvent.AddTarget(projectileMoveEvents[0].GetTargets()[0]);
            projectileMoveEvent.SetTargetType(projectileMoveEvents[0].GetTargetType());
            projectileMoveEvent.SetEffectiveMoveEventCastingSpeed(minMoveEventCastingSpeed);
            projectileMoveEvent.SetMoveType(projectileMoveEvents[0].GetMoveType());

            foreach (MoveEvent moveEvent in projectileMoveEvents)
            {
                projectileMoveEvent.AddMove(moveEvent.GetMoves()[0]);
                projectileMoveEvent.AddFighter(moveEvent.GetFighters()[0]);
                projectileMoveEvent.AddRandomAdd(moveEvent.GetRandomAdds()[0]);
            }
            finalMoveEvents.Add(projectileMoveEvent);
        }
        if (psychicMoveEvents.Count > 0)
        {
            // Sort in ascending order. Psychic moves will be attempted in ascending order of power.
            psychicMoveEvents.Sort((left, right) => MovePower(left.GetFighters()[0], left.GetMoves()[0], left.GetRandomAdds()[0]).CompareTo(MovePower(right.GetFighters()[0], right.GetMoves()[0], right.GetRandomAdds()[0])));

            foreach (MoveEvent moveEvent in psychicMoveEvents) // Psychic moves are added separately and not combined into one MoveEvent.
            {
                MoveEvent psychicMoveEvent = new MoveEvent();
                //psychicMoveEvent.SetMoveType(psychicMoveEvents[0].GetMoveType());
                psychicMoveEvent.AddTarget(psychicMoveEvents[0].GetTargets()[0]);
                psychicMoveEvent.SetTargetType(psychicMoveEvents[0].GetTargetType());
                psychicMoveEvent.SetEffectiveMoveEventCastingSpeed(minMoveEventCastingSpeed);
                psychicMoveEvent.SetMoveType(Enums.MoveType.Psychic);

                psychicMoveEvent.AddMove(moveEvent.GetMoves()[0]);
                psychicMoveEvent.AddFighter(moveEvent.GetFighters()[0]);
                psychicMoveEvent.AddRandomAdd(moveEvent.GetRandomAdds()[0]);

                finalMoveEvents.Add(psychicMoveEvent);
            }
        }

        return finalMoveEvents;
    }

    public void DisplayFightResultsText()
    {
        if (DebugLevel > Enums.DebugLevel.None)
        {
            mWriter.WriteLine("");
            mWriter.WriteLine("###################################################################");
            mWriter.WriteLine("###################################################################\n");
            mWriter.WriteLine("The fight is over!");

            if (Team1.Count > 0)
            {
                mWriter.WriteLine("Team 1 wins!");
            }
            else if (Team2.Count > 0)
            {
                mWriter.WriteLine("Team 2 wins!");
            }
            else
            {
                mWriter.WriteLine("Team 3 wins!");
            }

            mWriter.WriteLine("Number of rounds: " + RoundNumber);
            mWriter.WriteLine("Surviving fighters:\n");

            foreach (Fighter fighter in Fighters)
            {
                mWriter.WriteLine(fighter.GetName() + "  Health: " + fighter.GetHealth() + "  Mana: " + fighter.GetMana());
            }
            mWriter.WriteLine("");
            // system("pause"); ??
        }

        mWriter.Close();
    }

    public void DisplayTeamsText()
    {
        string vsPrintString = "";
        if (Team1.Count > 0)
        {
            vsPrintString += Team1[0].GetName();
            if (Team1.Count > 1)
            {
                for (int index = 1; index < Team1.Count; index++)
                {
                    vsPrintString += "  +  " + Team1[index].GetName();
                }
            }
        }

        if (Team2.Count > 0)
        {
            vsPrintString += "  vs  " + Team2[0].GetName();
            if (Team2.Count > 1)
            {
                for (int index = 1; index < Team2.Count; index++)
                {
                    vsPrintString += "  +  " + Team2[index].GetName();
                }
            }
            if (Team3.Count == 0)
            {
                vsPrintString += "\n";
            }
        }

        if (Team3.Count > 0)
        {
            vsPrintString += "  vs  " + Team3[0].GetName();
            if (Team3.Count > 1)
            {
                for (int index = 1; index < Team3.Count; index++)
                {
                    vsPrintString += "  +  " + Team3[index].GetName();
                }
            }
            vsPrintString += "\n\n";
        }
        mWriter.WriteLine(vsPrintString);

        for (int i = 0; i < Team1.Count; i++)
        {
            string fighterPrintString = "";

            List<Move> theirPowerUpMoves = new List<Move>();
            foreach (PowerUp powerUp in Team1[i].GetPowerUps())
            {
                theirPowerUpMoves.Add(powerUp.GetPowerUpMove());
            }
            fighterPrintString += Team1[i].GetName() + "   Overall: " + Team1[i].GetOverallRating() + "  Health: " + Team1[i].GetHealth() + "  Mana: " + Team1[i].GetMana() + "  Health Co = " + Team1[i].GetHealthCo();
            if (Team1[i].GetStatuses().Count > 0)
            {
                fighterPrintString += "  Statuses:";
                for (int j = 0; j < Team1[i].GetStatuses().Count; j++)
                {
                    fighterPrintString += " " + Util.EnumToText(Team1[i].GetStatuses()[j].GetStatusType());
                }
            }
            if (theirPowerUpMoves.Count > 0)
            {
                fighterPrintString += "  PowerUps:";
                for (int j = 0; j < theirPowerUpMoves.Count; j++)
                {
                    fighterPrintString += " " + theirPowerUpMoves[i].GetName();
                }
            }
            mWriter.WriteLine(fighterPrintString);
        }
        mWriter.WriteLine("");

        for (int i = 0; i < Team2.Count; i++)
        {
            string fighterPrintString = "";

            List<Move> theirPowerUpMoves = new List<Move>();
            foreach (PowerUp powerUp in Team2[i].GetPowerUps())
            {
                theirPowerUpMoves.Add(powerUp.GetPowerUpMove());
            }
            fighterPrintString += Team2[i].GetName() + "   Overall: " + Team2[i].GetOverallRating() + "  Health: " + Team2[i].GetHealth() + "  Mana: " + Team2[i].GetMana() + "  Health Co = " + Team2[i].GetHealthCo();
            if (Team2[i].GetStatuses().Count > 0)
            {
                fighterPrintString += "  Statuses:";
                for (int j = 0; j < Team2[i].GetStatuses().Count; j++)
                {
                    fighterPrintString += " " + Util.EnumToText(Team2[i].GetStatuses()[j].GetStatusType());
                }
            }
            if (theirPowerUpMoves.Count > 0)
            {
                fighterPrintString += "  PowerUps:";
                for (int k = 0; k < theirPowerUpMoves.Count; k++)
                {
                    fighterPrintString += " " + theirPowerUpMoves[k].GetName();
                }
            }
            mWriter.WriteLine(fighterPrintString);
        }

        if (Team2.Count > 0)
        {
            mWriter.WriteLine("");
        }

        if (Teams == 3)
        {
            for (int i = 0; i < Team3.Count; i++)
            {
                string fighterPrintString = "";

                List<Move> theirPowerUpMoves = new List<Move>();
                foreach (PowerUp powerUp in Team3[i].GetPowerUps())
                {
                    theirPowerUpMoves.Add(powerUp.GetPowerUpMove());
                }
                fighterPrintString += Team3[i].GetName() + "   Overall: " + Team3[i].GetOverallRating() + "  Health: " + Team3[i].GetHealth() + "  Mana: " + Team3[i].GetMana() + "  Health Co = " + Team3[i].GetHealthCo();
                if (Team3[i].GetStatuses().Count > 0)
                {
                    fighterPrintString += "  Statuses:";
                    for (int j = 0; j < Team3[i].GetStatuses().Count; j++)
                    {
                        fighterPrintString += " " + Util.EnumToText(Team3[i].GetStatuses()[i].GetStatusType());
                    }
                }
                if (theirPowerUpMoves.Count > 0)
                {
                    fighterPrintString += "  PowerUps:";
                    for (int k = 0; k < theirPowerUpMoves.Count; k++)
                    {
                        fighterPrintString += " " + theirPowerUpMoves[k].GetName();
                    }
                }
                mWriter.WriteLine(fighterPrintString);
            }
            mWriter.WriteLine("");
        }
    }

    public void ExecuteMedicalMoveEvent(MoveEvent moveEvent)
    {
        List<Fighter> targets = new List<Fighter>();
        Enums.TargetType targetType = moveEvent.GetTargetType();
        Fighter fighter = moveEvent.GetFighters()[0]; // Only 1 for now. Medical moves cannot be combined.

        switch (targetType)
        {
            case Enums.TargetType.Self:
            case Enums.TargetType.OneTeamMember:
                {
                    Fighter target = moveEvent.GetTargets()[0];
                    targets.Add(target);
                    break;
                }
            case Enums.TargetType.Team:
                {
                    int team = moveEvent.GetFighters()[0].GetTeam();
                    targets = GetTeamList(team);
                    break;
                }
            case Enums.TargetType.TeamMembersWithStatuses:
                {
                    List<Enums.StatusType> requiredStatuses = moveEvent.GetMoves()[0].GetRequiredTargetStatusesList();
                    List<Fighter> teamMembersWithStatuses = fighter.GetAI().GetTeammatesWithStatuses(this, fighter, requiredStatuses);

                    if (fighter.CheckStatuses(requiredStatuses) == true)
                    {
                        teamMembersWithStatuses.Add(fighter);
                    }

                    targets = teamMembersWithStatuses;
                    break;
                }
            default:
                Debug.LogError("Error! Unexpected TargetType [" + targetType + "] in Fight.ExecuteMedicalMoveEvent!");
                break;
        }

        Move medicalMove = moveEvent.GetMoves()[0];
        float movePower = MoveEventPower(moveEvent);
        float movePowerCo = movePower / medicalMove.GetLevel();
        Enums.HealType healType = medicalMove.GetHealType();

        int manaRestoreAmount = (int)(medicalMove.GetManaRestoreAmount() * movePowerCo + .5f);   // .5 is for rounding.
        int healthAmount = (int)(medicalMove.GetHealthAmount() * movePowerCo + .5f);    // .5 is for rounding.
        Enums.StatusType[] statusTypes = medicalMove.GetStatusTypes();

        foreach (Fighter target in targets)
        {
            switch (healType)
            {
                case Enums.HealType.Health:
                    {
                        int initialHealth = target.GetHealth();
                        target.AddHealth(healthAmount);
                        int actualAmount = target.GetHealth() - initialHealth;

                        mWriter.WriteLine(target.GetName() + " recovers " + actualAmount + " health!");
                        break;
                    }
                case Enums.HealType.Mana:
                    {
                        if (target != fighter)    // A fighter cannot restore their own mana with a medical move.
                        {
                            target.AddMana(manaRestoreAmount);
                            mWriter.WriteLine(target.GetName() + " recovers " + manaRestoreAmount + " mana!");
                        }
                        break;
                    }
                case Enums.HealType.HealthAndMana:
                    {
                        if (target == fighter)    // A fighter cannot restore their own mana with a medical move.
                        {
                            int initialHealth = target.GetHealth();
                            target.AddHealth(healthAmount);
                            int actualAmount = target.GetHealth() - initialHealth;

                            mWriter.WriteLine(target.GetName() + " recovers " + actualAmount + " health!");
                        }
                        else
                        {
                            target.AddMana(manaRestoreAmount);

                            int initialHealth = target.GetHealth();
                            target.AddHealth(healthAmount);
                            int actualHealthAmount = target.GetHealth() - initialHealth;

                            mWriter.WriteLine(target.GetName() + " recovers " + actualHealthAmount + " health and " + manaRestoreAmount + " mana!");
                        }
                        break;
                    }
                default:
                    Debug.LogError("Error! Unexpected HealType in Fight.ExecuteMedicalMoveEvent!");
                    break;
            }

            foreach (Enums.StatusType statusType in statusTypes)
            {
                target.RemoveStatus(statusType);

                Status newStatus = new Status();
                newStatus.SetMove(medicalMove);
                newStatus.SetPower(movePower);
                newStatus.SetStatusType(statusType);
                newStatus.SetFighter(target);
                newStatus.SetEndingRoundNumber(RoundNumber + medicalMove.GetDuration());

                target.AddStatus(newStatus);
            }

            if (target.CheckStatus(Enums.StatusType.PsychicParalysis) == true || target.CheckStatus(Enums.StatusType.PsychicControl) == true)
            {
                target.RemoveStatus(Enums.StatusType.PsychicParalysis);
                target.RemoveStatus(Enums.StatusType.PsychicControl);
                mWriter.WriteLine(target.GetName() + " is released from psychic control!");
            }
        }

        fighter.RemoveMana(medicalMove.GetMana());
        fighter.AddUsedMove(medicalMove, RoundNumber);
    }

    public IEnumerator ExecuteMoveEvent(MoveEvent moveEvent)
    {
        PrintAttackString(moveEvent);

        Enums.MoveType moveType = moveEvent.GetMoveType();
        switch (moveType)
        {
            case Enums.MoveType.Melee:
            case Enums.MoveType.Spell:
            case Enums.MoveType.Psychic:
            case Enums.MoveType.NinTai:
            case Enums.MoveType.Projectile:
            case Enums.MoveType.Offensive:
                yield return ExecuteOffensiveMoveEvent(moveEvent);
                break;
            case Enums.MoveType.PowerUp:
                ExecutePowerUpMoveEvent(moveEvent);
                break;
            case Enums.MoveType.Medical:
                ExecuteMedicalMoveEvent(moveEvent);
                break;
            case Enums.MoveType.Protect:
                ExecuteProtectMoveEvent(moveEvent);
                break;
            case Enums.MoveType.Substitution:
                ExecuteSubMoveEvent(moveEvent);
                break;
            case Enums.MoveType.Clone:
                //ExecuteCloneMoveEvent(moveEvent);
                break;
            case Enums.MoveType.Summon:
                //ExecuteSummonMoveEvent(moveEvent);
                break;
            default:
                Debug.LogError("Error! Unexpected MoveType [" + moveType + "] in Fight.ExecuteMoveEvent!");
                break;
        }
    }

    public void ExecuteOffensiveMoveAgainstTarget(Enums.MoveType moveType, float movePower, List<Enums.Nature> moveNatures, List<Move> offensiveMoves, List<float> attackerRandomAdds, List<Fighter> attackers, Fighter target)
    {
        Hit hit = new Hit();
        float targetRandomAdd = RandomAdd();    // Each target gets a new RandomAdd each time they are attacked.

        List<Fighter> protectors = GetProtectors(target);
        Dictionary<Fighter, float> protectorRandomAddMap = new Dictionary<Fighter, float>(); // Each protector also gets a new RandomAdd for a given attack.
        foreach (Fighter protector in protectors)
        {
            protectorRandomAddMap.Add(protector, RandomAdd());
        }

        switch (moveType)
        {
            case Enums.MoveType.Melee:
            case Enums.MoveType.NinTai:
                hit = GetHitMelee(moveType, movePower, moveNatures, offensiveMoves, attackerRandomAdds, attackers, target, targetRandomAdd, protectors, protectorRandomAddMap);
                break;
            case Enums.MoveType.Spell:
            case Enums.MoveType.Projectile:
                hit = GetHitRanged(moveType, movePower, moveNatures, offensiveMoves, attackerRandomAdds, attackers, target, targetRandomAdd, protectors, protectorRandomAddMap);
                break;
            case Enums.MoveType.Psychic:
                hit = GetHitPsychic(moveType, movePower, moveNatures, offensiveMoves, attackerRandomAdds, attackers, target, targetRandomAdd, protectors);
                break;
            default:
                Debug.LogError("Error! Unexpected MoveType [" + moveType + "]in Fight.ExecuteOffensiveMoveAgainstTarget!");
                break;
        }

        Enums.DamageType damageType = offensiveMoves[0].GetDamageType();
        Enums.HitResult result = hit.GetResult();
        int damage = hit.GetDamage();

        string resultString = "";

        switch (result)
        {
            case Enums.HitResult.Miss:
                {
                    resultString += "It misses " + target.GetName() + "!";
                    break;
                }
            case Enums.HitResult.Hit:
                {
                    resultString += "It hits " + target.GetName() + Util.DamageString(damage, damageType, target, attackers, false);
                    ApplyStandardDamage(damage, damageType, target, attackers);
                    break;
                }
            case Enums.HitResult.PartialHit:
                {
                    resultString += "It partially hits " + target.GetName() + Util.DamageString(damage, damageType, target, attackers, false);
                    ApplyStandardDamage(damage, damageType, target, attackers);
                    break;
                }
            case Enums.HitResult.Blocked:
                {
                    List<Fighter> defenders = hit.GetDefenders(); // Can be more than one defender for blocked results.
                    List<Move> moves = hit.GetMoves();
                    int defendersCount = defenders.Count;
                    resultString += Util.ListString(defenders) + " block";
                    if (defendersCount == 1)
                    {
                        resultString += "s";
                    }
                    resultString += " it with " + Util.ListString(moves) + "!";
                    for (int index = 0; index < defendersCount; ++index)
                    {
                        Fighter defender = defenders[index];
                        Move move = moves[index];
                        defender.RemoveMana(move.GetMana());
                        defender.AddUsedMove(move, RoundNumber);
                    }
                    break;
                }
            case Enums.HitResult.PartiallyBlocked:
                {
                    List<Fighter> defenders = hit.GetDefenders();                   // Can be more than one defender for blocked results.
                    int defendersCount = defenders.Count;
                    List<Move> moves = hit.GetMoves();
                    resultString += Util.ListString(defenders) + " partially block";
                    if (defendersCount == 1)
                    {
                        resultString += "s";
                    }
                    resultString += " it with " + Util.ListString(moves) + Util.DamageString(damage, damageType, target, attackers, true);
                    ApplyStandardDamage(damage, damageType, target, attackers);
                    for (int index = 0; index < defendersCount; ++index)
                    {
                        Fighter defender = defenders[index];
                        Move move = moves[index];
                        defender.RemoveMana(move.GetMana());
                        defender.AddUsedMove(move, RoundNumber);
                    }
                    break;
                }
            case Enums.HitResult.Avoided:
                {
                    Fighter defender = hit.GetDefenders()[0];                   // Can only be one defender for an Avoid move.
                    Move defensiveMove = hit.GetDefensiveMoves()[0];
                    if (hit.CheckWasProtected() == true)
                    {
                        resultString += "It's avoided with " + defender.GetName() + "'s " + defensiveMove.GetName() + "!";
                    }
                    else
                    {
                        resultString += target.GetName() + " avoids it with " + defensiveMove.GetName() + "!";
                    }
                    defender.RemoveMana(defensiveMove.GetMana());           // Defender should be target if WasProtected == false.
                    defender.AddUsedMove(defensiveMove, RoundNumber);
                    break;
                }
            case Enums.HitResult.PartiallyAvoided:
                {
                    Fighter defender = hit.GetDefenders()[0];                   // Can only be one defender for an Avoid move.
                    Move defensiveMove = hit.GetDefensiveMoves()[0];
                    if (hit.CheckWasProtected() == true)
                    {
                        resultString += "It's partially avoided with " + defender.GetName() + "'s " + defensiveMove.GetName() + Util.DamageString(damage, damageType, target, attackers, false);
                    }
                    else
                    {
                        resultString += target.GetName() + " partially avoids it with " + defensiveMove.GetName() + Util.DamageString(damage, damageType, target, attackers, true);
                    }
                    ApplyStandardDamage(damage, damageType, target, attackers);
                    defender.RemoveMana(defensiveMove.GetMana());           // Defender should be target if WasProtected == false.
                    defender.AddUsedMove(defensiveMove, RoundNumber);
                    break;
                }
            case Enums.HitResult.Deflected:
                {
                    List<Fighter> defenders = hit.GetDefenders();                   // Can be more than one defender for Deflected attacks.
                    int defendersCount = defenders.Count;
                    resultString += Util.ListString(defenders) + " deflect";
                    if (defendersCount == 1)
                    {
                        resultString += "s";
                    }
                    resultString += " it!";
                    int offensiveMovesCount = offensiveMoves.Count;
                    int totalAttackMana = 0;
                    for (int index = 0; index < offensiveMoves.Count; ++index)
                    {
                        totalAttackMana += offensiveMoves[index].GetMana();
                    }
                    bool wasEasy = hit.CheckWasEasy();
                    // TODO: Decide how much mana it should cost to deflect. Currently it takes none, used to take 25% of incoming attack mana.
                    //int manaCostPerDefender = (int)(totalAttackMana * (0.25f) * (1f / defendersCount) + 0.5f);   // + .05 is for rounding.
                    foreach (Fighter defender in defenders)
                    {
                        //defender.RemoveMana(manaCostPerDefender);
                        if (wasEasy == true && defender.CheckTrait(Enums.Trait.TeleportationMarkDeflect) == true)
                        {
                            foreach (Fighter attacker in attackers)
                            {
                                Status status = new Status();   // TODO: Currently not setting the Move since there was none. Not sure about this.
                                status.SetFighter(attacker);
                                status.SetStatusType(Enums.StatusType.TeleportationMarked);
                                status.SetEndingRoundNumber(int.MaxValue);

                                attacker.AddStatus(status);
                            }
                        }
                    }
                    break;
                }
            case Enums.HitResult.PartiallyDeflected:
                {
                    List<Fighter> defenders = hit.GetDefenders();                   // Can be more than one defender for Deflected attacks.
                    int defendersCount = defenders.Count;
                    resultString += Util.ListString(defenders) + " partially deflect";
                    if (defendersCount == 1)
                    {
                        resultString += "s";
                    }
                    resultString += " it!";
                    int offensiveMovesCount = offensiveMoves.Count;
                    int totalAttackMana = 0;
                    for (int index = 0; index < offensiveMoves.Count; ++index)
                    {
                        totalAttackMana += offensiveMoves[index].GetMana();
                    }
                    // TODO: Decide how much mana it should cost to deflect. Currently it take 25% of incoming attack mana.
                    int manaCostPerDefender = (int)(totalAttackMana * (0.25f) * (1f / defendersCount) + 0.5f);   // .05 is for rounding.
                    foreach (Fighter defender in defenders)
                    {
                        defender.RemoveMana(manaCostPerDefender);
                    }
                    break;
                }
            case Enums.HitResult.Substitution:
                {
                    Substitution substitution = target.GetSubstition();
                    Move subMove = substitution.GetSubMove();
                    Fighter subFighter = substitution.GetFighter();
                    float subMovePower = substitution.GetPower();
                    bool subMoveDoesDamage = subMove.GetDamage() > 0;

                    resultString += "It hits " + target.GetName() + Util.DamageString(damage, damageType, target, attackers, false);
                    resultString += "\nBut it's a " + subMove.GetName() + "!";

                    foreach (Fighter attacker in attackers)
                    {
                        if (subMoveDoesDamage)
                        {
                            float powerCo = subMovePower / subMove.GetLevel();
                            float subMoveDamageFloat = subMove.GetDamage() * powerCo * attacker.GetDamageCo() * NaturesDamageCo(subFighter, attacker, subMove);
                            int subMoveDamage = (int)(subMoveDamageFloat + .5f); // .5 is for rounding.

                            resultString += "\nIt does " + subMoveDamage + " to " + attacker.GetName() + "!";
                            List<Fighter> listContainingTarget = new List<Fighter>() { target };
                            ApplyStandardDamage(subMoveDamage, subMove.GetDamageType(), attacker, listContainingTarget); // attacker and target are reversed in this call because it's the target's SubMove doing the damage.
                        }

                        Enums.StatusType[] statusesToApply = subMove.GetStatusTypes();
                        int duration = subMove.GetDuration();

                        foreach (Enums.StatusType statusType in statusesToApply)
                        {
                            Status status = new Status();
                            status.SetFighter(attacker);
                            status.SetStatusType(statusType);
                            status.SetMove(subMove);
                            status.SetPower(subMovePower);
                            status.SetEndingRoundNumber(RoundNumber + duration);

                            attacker.AddStatus(status);
                        }
                    }
                    target.RemoveSubstitution();
                    break;
                }
            case Enums.HitResult.PsychicSuccess:
                {
                    resultString += "The psychic attack is successful against " + target.GetName();
                    if (offensiveMoves[0].GetDamage() > 0)  // If psychic attack does damage.
                    {
                        resultString += Util.DamageString(damage, damageType, target, attackers, false);
                        ApplyStandardDamage(damage, damageType, target, attackers);
                    }
                    else
                    {
                        resultString += "!";
                    }
                    break;
                }
            case Enums.HitResult.PsychicFailureResist:
                {
                    resultString += target.GetName() + " resists the psychic attack and it fails!";
                    int manaCost = offensiveMoves[0].GetMana() / 4; // Currently takes 1/4 of the attack mana to dispel a psychic attack.
                    target.RemoveMana(manaCost);
                    break;
                }
            case Enums.HitResult.PsychicFailureAlreadyUnder:
                {
                    resultString += target.GetName() + " is already under psychic control!";
                    break;
                }
            case Enums.HitResult.PsychicFailureSubstitution:
                {
                    resultString += "The psychic attack fails against" + target.GetName() + " because it's a substitution!";
                    target.RemoveSubstitution();
                    break;
                }
            case Enums.HitResult.PsychicFailureSharinganAvoided:
                {
                    resultString += target.GetName() + " is careful to avoid looking at " + attackers[0].GetName() + "'s sharingan!";
                    break;
                }
        }

        mWriter.WriteLine(resultString);

        switch (result)     // Removes UnderPsychic and Trapped statuses before applying attack statuses. The attack landed or partially landed.
        {
            case Enums.HitResult.Hit:
            case Enums.HitResult.PartialHit:
            case Enums.HitResult.PartiallyBlocked:
            case Enums.HitResult.PsychicSuccess:
            case Enums.HitResult.PartiallyAvoided:
                {
                    target.RemoveStatus(Enums.StatusType.PsychicControl);
                    target.RemoveStatus(Enums.StatusType.PsychicParalysis);
                    target.RemoveStatus(Enums.StatusType.Trapped);

                    bool alreadyPrintedTrappedResultString = false;

                    int offensiveMovesCount = offensiveMoves.Count;
                    for (int index = 0; index < offensiveMovesCount; ++index)
                    {
                        Move offensiveMove = offensiveMoves[index];
                        float individualOffensiveMovePower = MovePower(attackers[index], offensiveMove, attackerRandomAdds[index]);
                        int duration = offensiveMove.GetDuration();
                        Enums.StatusType[] statusesToApply = offensiveMove.GetStatusTypes();

                        foreach (Enums.StatusType statusType in statusesToApply)
                        {
                            bool shouldApply = true;

                            if (statusType == Enums.StatusType.Trapped)
                            {
                                float targetStrength = target.GetStrength() * target.GetHealthCo();

                                if (targetStrength - TAIJUTSU_MOVE_POWER_PENALTY >= individualOffensiveMovePower)    // Target is too strong to be trapped.
                                {
                                    shouldApply = false;

                                    if (alreadyPrintedTrappedResultString == false)
                                    {
                                        mWriter.WriteLine(target.GetName() + " is too strong and cannot be trapped!");
                                        alreadyPrintedTrappedResultString = true;
                                    }
                                }
                                else
                                {
                                    if (alreadyPrintedTrappedResultString == false)
                                    {
                                        mWriter.WriteLine(target.GetName() + " is trapped and unable to move!");
                                        alreadyPrintedTrappedResultString = true;
                                    }
                                }
                            }

                            if (shouldApply == true)
                            {
                                Status status = new Status();
                                status.SetFighter(target);
                                status.SetStatusType(statusType);
                                status.SetMove(offensiveMove);
                                status.SetPower(individualOffensiveMovePower);
                                status.SetEndingRoundNumber(RoundNumber + duration);

                                target.AddStatus(status);
                            }
                        }
                    }
                    break;
                }
        }
    }

    public IEnumerator ExecuteOffensiveMoveEvent(MoveEvent originalMoveEvent)
    {
        MoveEvent moveEvent = GetMoveEventWithActualAttackersAndTargets(originalMoveEvent);

        float movePower = MoveEventPower(moveEvent);
        List<Enums.Nature> moveNatures = GetFinalNaturesInMoveEvent(moveEvent);
        List<Fighter> fighters = moveEvent.GetFighters();
        List<Move> offensiveMoves = moveEvent.GetMoves();
        List<float> attackerRandomAdds = moveEvent.GetRandomAdds();
        List<Fighter> targets = moveEvent.GetTargets();

        /*if (fighters[0].GetName() == "Fire Mage")
        {
            yield return SelectMoveUI.AnimatePortrait(fighters[0]);
        }*/

        foreach (Fighter target in targets)
        {
            ExecuteOffensiveMoveAgainstTarget(moveEvent.GetMoveType(), movePower, moveNatures, offensiveMoves, attackerRandomAdds, fighters, target);
            RemoveDefeatedFighters();
        }

        int fightersCount = fighters.Count;
        for (int index = 0; index < fightersCount; ++index)
        {
            Fighter fighter = fighters[index];
            Move offensiveMove = offensiveMoves[index];

            fighter.RemoveMana(offensiveMove.GetMana());
            fighter.RemoveSubstitution();
            fighter.AddUsedMove(offensiveMove, RoundNumber);
        }

        yield break;
    }

    public void ExecutePowerUpMoveEvent(MoveEvent moveEvent)
    {
        Move powerUpMove = moveEvent.GetMoves()[0];   // All fighters in MoveEvent should be performing the same PowerUpMove.
        int powerUpMoveManaCost = powerUpMove.GetMana();
        int endingRoundNumber = RoundNumber + powerUpMove.GetDuration();
        BonusData bonusData = powerUpMove.GetBonusData();
        List<Fighter> fighters = moveEvent.GetFighters();
        List<float> randomAdds = moveEvent.GetRandomAdds();

        int fightersCount = fighters.Count;
        for (int index = 0; index < fightersCount; ++index)
        {
            Fighter fighter = fighters[index];

            if (fighter.GetAI().CheckIfCanPerformMove(this, fighter, powerUpMove) == true)
            {
                float movePower = MovePower(fighter, powerUpMove, randomAdds[index]);

                fighter.AddBonusData(bonusData, movePower, Enums.BonusSource.PowerUpMove, null, null, powerUpMove, null, endingRoundNumber);

                // Checking to see if they're attempting to break free from psychic control with a PowerUp move.
                if (fighter.CheckStatus(Enums.StatusType.PsychicControl) == true || fighter.CheckStatus(Enums.StatusType.PsychicParalysis) == true)
                {
                    float psychicMovePower = 0f;
                    List<Status> fighterStatuses = fighter.GetStatuses();

                    foreach (Status status in fighterStatuses)
                    {
                        if (status.GetStatusType() == Enums.StatusType.PsychicControl || status.GetStatusType() == Enums.StatusType.PsychicParalysis)
                        {
                            psychicMovePower = status.GetPower(); //status.GetMove().GetLevel();
                            break;
                        }
                    }

                    if (movePower >= psychicMovePower)   // Fighter can break free if the move is strong enough.
                    {
                        fighter.RemoveStatus(Enums.StatusType.PsychicControl);
                        fighter.RemoveStatus(Enums.StatusType.PsychicParalysis);
                        mWriter.WriteLine(fighter.GetName() + " breaks free from psychic control!");
                    }
                }

                // Checking to see if they're attempting to break free from a trapped or paralyzed state.
                if (fighter.CheckStatus(Enums.StatusType.Trapped) == true)
                {
                    float trappingMovePower = 0f;
                    List<Status> fighterStatuses = fighter.GetStatuses();

                    foreach (Status status in fighterStatuses)
                    {
                        if (status.GetStatusType() == Enums.StatusType.Trapped)
                        {
                            trappingMovePower = status.GetPower(); //status.GetMove().GetLevel();
                            break;
                        }
                    }

                    float fighterStrength = fighter.GetStrength() * fighter.GetHealthCo();

                    if (fighterStrength >= trappingMovePower)   // Fighter can break free if they are strong enough.
                    {
                        fighter.RemoveStatus(Enums.StatusType.Trapped);
                        mWriter.WriteLine(fighter.GetName() + " is too strong and breaks free!");
                    }
                }

                PowerUp powerUp = new PowerUp();
                powerUp.SetEndingRoundNumber(endingRoundNumber);
                powerUp.SetFighter(fighter);
                powerUp.SetPowerUpMove(powerUpMove);

                fighter.AddPowerUp(powerUp);
                fighter.RemoveMana(powerUpMoveManaCost);
                fighter.AddUsedMove(powerUpMove, RoundNumber);
            }
        }
    }

    public void ExecuteProtectMoveEvent(MoveEvent moveEvent)
    {
        Protection protection = new Protection();
        protection.SetProtector(moveEvent.GetFighters()[0]);
        protection.SetProtected(moveEvent.GetTargets()[0]);

        Protections.Add(protection);
    }

    public void ExecuteSubMoveEvent(MoveEvent moveEvent)
    {
        Fighter fighter = moveEvent.GetFighters()[0];
        float randomAdd = moveEvent.GetRandomAdds()[0];

        Move subMove = moveEvent.GetMoves()[0];
        List<Fighter> enemies = AI.GetEnemies(this, fighter);
        List<Fighter> enemiesTricked = GetEnemiesTrickedBySubstitution(fighter, subMove, enemies);
        float movePower = MovePower(fighter, subMove, randomAdd);

        Substitution substitution = new Substitution();
        substitution.SetFighter(fighter);
        substitution.SetSubMove(subMove);
        substitution.SetPower(movePower);
        substitution.SetEnemiesTricked(enemiesTricked);

        fighter.AddSub(substitution); // Will also clear any existing Substitution.
    }

    /*public void ExecuteSummonMoveEvent(MoveEvent moveEvent)
    {
        List<Fighter> summoners = moveEvent.GetFighters();
        List<Fighter> summoned = moveEvent.GetFighters();
        SummonMove summonMove = moveEvent.GetSummonMoves()[0]; // There should only be one summon move, even if multiple people are performing it simulteneously.

        // Need to make instances of the summoned fighters.
    }*/

    public AttackBools GetAttackBoolsForOffensiveMoves(List<Move> offensiveMoves)
    {
        bool touchSuccess = false;
        bool absorbed = true;
        bool occular = false;

        foreach (Move offensiveMove in offensiveMoves)
        {
            if (offensiveMove.CheckTouchSuccess() == true)
            {
                touchSuccess = true;
            }

            if (offensiveMove.CheckAbsorbed() == false)
            {
                absorbed = false;
            }

            if (offensiveMove.CheckOccular() == true)
            {
                occular = true;
            }
        }

        AttackBools attackBools = new AttackBools();
        attackBools.SetTouchSuccess(touchSuccess);
        attackBools.SetAbsorbed(absorbed);
        attackBools.SetOccular(occular);

        return attackBools;
    }

    public List<Fighter> GetEnemiesTrickedBySubstitution(Fighter fighter, Move subMove, List<Fighter> enemies)
    {
        List<Fighter> enemiesTricked = new List<Fighter>();
        float subSkill = .5f * fighter.GetIntelligence() + .25f * fighter.GetSpellcraft() + .25f * subMove.GetLevel();

        foreach (Fighter enemy in enemies)
        {
            float skillDiff = enemy.GetIntelligence() - subSkill;

            if (enemy.CheckStatus(Enums.StatusType.PsychicControl) || enemy.CheckStatus(Enums.StatusType.PsychicParalysis) || enemy.CheckStatus(Enums.StatusType.Trapped))
            {
                enemiesTricked.Add(enemy);
            }
            else if (-2f < skillDiff && skillDiff < 2f)
            {
                int percentChance = (int)(50f - 25f * skillDiff + 0.5f);    // Percent chance = 50 - 25 * skillDiff     +0.5 is for rounding when converting to int
                                                                            // skillDiff = -2, chance = 100%        skillDiff = 2, chance = 0%      Linear in between
                int randomNumber = Random.Range(0, 100);

                if (randomNumber < percentChance)
                {
                    enemiesTricked.Add(enemy);
                }
            }
            else if (skillDiff <= -2f)
            {
                enemiesTricked.Add(enemy);
            }
        }

        return enemiesTricked;
    }

    public float GetDefensiveMovePowerChangeFromNatures(List<Enums.Nature> attackNatures, Fighter defender, Move defensiveMove)
    {
        float powerChange = 0.0f;
        List<Enums.Nature> defensiveMoveNatures = defensiveMove.GetNaturesList();

        if (attackNatures.Count == 1 && defensiveMoveNatures.Count == 1 && defensiveMove.GetMoveType() != Enums.MoveType.Avoid)
        {
            Enums.Nature attackNature = attackNatures[0];
            Enums.Nature defensiveMoveNature = defensiveMoveNatures[0];

            switch (attackNature)
            {
                case Enums.Nature.Water:
                    {
                        if (defensiveMoveNature == Enums.Nature.Earth)
                        {
                            powerChange += DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE;
                        }
                        else if (defensiveMoveNature == Enums.Nature.Fire)
                        {
                            powerChange -= DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE;
                        }
                        break;
                    }
                case Enums.Nature.Lightning:
                    {
                        if (defensiveMoveNature == Enums.Nature.Wind)
                        {
                            powerChange += DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE;
                        }
                        else if (defensiveMoveNature == Enums.Nature.Earth)
                        {
                            powerChange -= DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE;
                        }
                        break;
                    }
                case Enums.Nature.Earth:
                    {
                        if (defensiveMoveNature == Enums.Nature.Lightning)
                        {
                            powerChange += DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE;
                        }
                        else if (defensiveMoveNature == Enums.Nature.Water)
                        {
                            powerChange -= DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE;
                        }
                        break;
                    }
                case Enums.Nature.Wind:
                    {
                        if (defensiveMoveNature == Enums.Nature.Fire)
                        {
                            powerChange += DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE;
                        }
                        else if (defensiveMoveNature == Enums.Nature.Lightning)
                        {
                            powerChange -= DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE;
                        }
                        break;
                    }
                case Enums.Nature.Fire:
                    {
                        if (defensiveMoveNature == Enums.Nature.Water)
                        {
                            powerChange += DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE;
                        }
                        else if (defensiveMoveNature == Enums.Nature.Wind)
                        {
                            powerChange -= DEFENSIVE_MOVE_POWER_CHANGE_FROM_NATURE_ADVANTAGE;
                        }
                        break;
                    }
            }
        }

        return powerChange;
    }

    public Hit GetDefensiveMovesHit(Enums.MoveType moveType, float attackPower, List<Enums.Nature> attackNatures, List<Move> offensiveMoves, List<float> attackerRandomAdds, List<Fighter> attackers, Fighter target, float targetRandomAdd, List<Fighter> protectors, Dictionary<Fighter, float> protectorRandomAddMap, float subBonus)
    {
        Hit defensiveMovesHit = new Hit(); // Returned at end of function.

        float minAttackCastingSpeed = float.MaxValue;

        int offensiveMovesCount = offensiveMoves.Count;
        for (int index = 0; index < offensiveMovesCount; ++index)
        {
            Fighter attacker = attackers[index];
            Move offensiveMove = offensiveMoves[index];
            float randomAdd = attackerRandomAdds[index];
            float moveCastingSpeed = attacker.GetCastingSpeed(offensiveMove, randomAdd);

            moveCastingSpeed += subBonus;

            if (moveCastingSpeed < minAttackCastingSpeed)
            {
                minAttackCastingSpeed = moveCastingSpeed;
            }
        }

        // Attacking seal speed is penalized for being at a distance. Does not go negative.
        float attackCastingSpeed = Mathf.Max(0f, minAttackCastingSpeed - ATTACK_SEAL_SPEED_PENALTY);

        AttackBools attackBools = GetAttackBoolsForOffensiveMoves(offensiveMoves);
        bool attackTouchSuccess = attackBools.CheckTouchSuccess();
        bool attackIsOccular = attackBools.CheckOccular();
        bool attackCanBeAbsorbed = attackBools.CheckAbsorbed();
        bool attackIsMelee = (moveType == Enums.MoveType.Melee || moveType == Enums.MoveType.NinTai);

        DefensiveAction protectorBlockAction = null;
        int protectorManaAfterMove = -1;
        Dictionary<Fighter, DefensiveAction> protectorBestActionMap = new Dictionary<Fighter, DefensiveAction>();

        // See if any protectors can block the attack individually.
        int protectorsCount = protectors.Count;
        for (int index = 0; index < protectorsCount; ++index)
        {
            Fighter protector = protectors[index];

            if (protector.CheckIfAlive() == true && protector.CheckIfCanMove() == true) // Don't include protectors who cannot move.
            {
                float protectorRandomAdd = protectorRandomAddMap[protector];
                List<Move> defensiveMoves = protector.GetMoves(Enums.MoveType.Defensive);

                foreach (Move defensiveMove in defensiveMoves)
                {
                    bool defenseTouchFail = defensiveMove.CheckTouchFail();
                    bool defendsOccular = defensiveMove.CheckOccularSuccess();
                    bool defenseIsAbsorbing = defensiveMove.CheckAbsorbing();
                    bool defendsMelee = defensiveMove.CheckIfWorksAgainstMelee();
                    bool defendsRanged = defensiveMove.CheckIfWorksAgainstRanged();

                    Enums.TargetType defensiveMoveTargetType = defensiveMove.GetTargetType();

                    float defensiveMoveCastingSpeed = protector.GetCastingSpeed(defensiveMove, protectorRandomAdd);

                    if (defensiveMoveCastingSpeed >= attackCastingSpeed &&
                        defensiveMoveTargetType != Enums.TargetType.Self &&
                        defensiveMoveTargetType != Enums.TargetType.TeamMembersWithStatuses &&
                        protector.GetAI().CheckIfCanPerformMove(this, protector, defensiveMove) == true &&
                        (attackTouchSuccess == false || defenseTouchFail == false) &&
                        (attackIsOccular == false || defendsOccular == true) &&
                        (defenseIsAbsorbing == false || attackCanBeAbsorbed == true) &&
                        (attackIsMelee ? defendsMelee : defendsRanged))
                    {
                        float defensiveMovePower = MovePower(protector, defensiveMove, protectorRandomAdd);
                        defensiveMovePower += GetDefensiveMovePowerChangeFromNatures(attackNatures, protector, defensiveMove);
                        defensiveMovePower = Mathf.Max(defensiveMovePower, 0.0f); // Nothing negative.

                        DefensiveAction defensiveAction = new DefensiveAction();
                        defensiveAction.SetDefender(protector);
                        defensiveAction.SetDefensiveMove(defensiveMove);
                        defensiveAction.SetPower(defensiveMovePower);

                        // If no defensive action is set yet, set it.
                        if (protectorBestActionMap.ContainsKey(protector) == false)
                        {
                            protectorBestActionMap[protector] = defensiveAction;
                        }
                        else
                        {
                            DefensiveAction protectorBestAction = protectorBestActionMap[protector];
                            if (defensiveMovePower > protectorBestAction.GetPower())
                            {
                                protectorBestActionMap[protector] = defensiveAction;
                            }
                        }

                        if (defensiveMovePower >= attackPower)
                        {
                            int manaAfterMove = protector.GetMana() - defensiveMove.GetMana();

                            if (manaAfterMove > protectorManaAfterMove)
                            {
                                protectorBlockAction = defensiveAction;
                                protectorManaAfterMove = manaAfterMove;
                            }
                        }
                    }
                }
            }
        }

        if (protectorBlockAction != null) // A protector can block the attack by themself.
        {
            //protectorBlockAction.Get
            List<Fighter> listContainingProtector = new List<Fighter>() { protectorBlockAction.GetDefender() };
            List<Move> listContainingDefensiveMove = new List<Move>() { protectorBlockAction.GetDefensiveMove() };
            bool wasEasy = (protectorBlockAction.GetPower() - attackPower) >= EASY_BLOCK_POWER_DIFF;

            defensiveMovesHit.SetDamage(0);
            defensiveMovesHit.SetDefenders(listContainingProtector);
            defensiveMovesHit.SetDefensiveMoves(listContainingDefensiveMove);
            defensiveMovesHit.SetResult(Enums.HitResult.Blocked);
            defensiveMovesHit.SetWasEasy(wasEasy);
            defensiveMovesHit.SetWasProtected(true);
        }
        else // Keep adding protectors to see if they can fully block the attack together.
        {
            List<DefensiveAction> protectorBestActionList = new List<DefensiveAction>();

            foreach (KeyValuePair<Fighter, DefensiveAction> pair in protectorBestActionMap)
            {
                protectorBestActionList.Add(pair.Value);
            }

            // Sort in descending order of DefensiveMove Power.
            protectorBestActionList.Sort((left, right) => right.GetPower().CompareTo(left.GetPower()));

            List<float> defensiveMovePowers = new List<float>();
            List<Fighter> defenders = new List<Fighter>();
            List<Move> defendersDefensiveMoves = new List<Move>();
            float defensiveMovesPowerSum = 0.0f;
            bool defendersCanBlock = false;

            foreach (DefensiveAction action in protectorBestActionList)
            {
                defenders.Add(action.GetDefender());
                defensiveMovePowers.Add(action.GetPower());
                defendersDefensiveMoves.Add(action.GetDefensiveMove());

                defensiveMovesPowerSum = SumList(defensiveMovePowers);

                if (defensiveMovesPowerSum >= attackPower)
                {
                    defendersCanBlock = true;
                    break;
                }
            }

            bool targetCanMove = target.CheckIfCanMove();
            DefensiveAction targetBestAction = null;
            bool defendersCanBlockWithTarget = false;
            int targetManaAfterMove = -1;

            if (defendersCanBlock == false && targetCanMove == true) // Add the target to the defense along with protectors.
            {
                foreach (Move defensiveMove in target.GetMoves(Enums.MoveType.Defensive))
                {
                    bool defenseTouchFail = defensiveMove.CheckTouchFail();
                    bool defendsOccular = defensiveMove.CheckOccularSuccess();
                    bool defenseIsAbsorbing = defensiveMove.CheckAbsorbing();
                    bool defendsMelee = defensiveMove.CheckIfWorksAgainstMelee();
                    bool defendsRanged = defensiveMove.CheckIfWorksAgainstRanged();

                    float defensiveMoveCastingSpeed = target.GetCastingSpeed(defensiveMove, targetRandomAdd);

                    if (defensiveMoveCastingSpeed >= attackCastingSpeed &&
                        defensiveMove.GetTargetType() != Enums.TargetType.TeamMembersWithStatuses &&
                        target.GetAI().CheckIfCanPerformMove(this, target, defensiveMove) == true &&
                        (attackTouchSuccess == false || defenseTouchFail == false) &&
                        (attackIsOccular == false || defendsOccular == true) &&
                        (defenseIsAbsorbing == false || attackCanBeAbsorbed == true) &&
                        (attackIsMelee ? defendsMelee : defendsRanged))
                    {
                        float defensiveMovePower = MovePower(target, defensiveMove, targetRandomAdd);
                        defensiveMovePower += GetDefensiveMovePowerChangeFromNatures(attackNatures, target, defensiveMove);
                        defensiveMovePower = Mathf.Max(defensiveMovePower, 0.0f); // Nothing negative.

                        List<float> defensiveMovePowersIncludingTarget = new List<float>(defensiveMovePowers);
                        defensiveMovePowersIncludingTarget.Add(defensiveMovePower);

                        float defensiveMovesPowerWithTarget = SumList(defensiveMovePowersIncludingTarget);

                        DefensiveAction defensiveAction = new DefensiveAction();
                        defensiveAction.SetDefender(target);
                        defensiveAction.SetDefensiveMove(defensiveMove);
                        defensiveAction.SetPower(defensiveMovePower);

                        int manaAfterMove = target.GetMana() - defensiveMove.GetMana();

                        if (defendersCanBlockWithTarget == false && defensiveMovesPowerWithTarget < attackPower) // Cannot currently block the attack.
                        {
                            float currentTargetBestActionPower = (targetBestAction == null) ? -1.0f : targetBestAction.GetPower();

                            if (targetBestAction == null || defensiveMovePower > currentTargetBestActionPower)
                            {
                                targetBestAction = defensiveAction;
                                targetManaAfterMove = manaAfterMove;
                            }
                        }
                        else if (defensiveMovesPowerWithTarget >= attackPower) // Attack can be fully blocked with target's help.
                        {
                            if (targetBestAction == null || defendersCanBlockWithTarget == false || manaAfterMove > targetManaAfterMove)
                            {
                                targetBestAction = defensiveAction;
                                targetManaAfterMove = manaAfterMove;
                                defensiveMovesPowerSum = defensiveMovesPowerWithTarget;
                                defendersCanBlockWithTarget = true;
                            }
                        }
                    }
                }
            }

            if (defendersCanBlockWithTarget == true) // Defenders can fully block the attack with the target's help.
            {
                bool wasEasy = (defensiveMovesPowerSum - attackPower) >= EASY_BLOCK_POWER_DIFF;

                defenders.Add(target);
                defendersDefensiveMoves.Add(targetBestAction.GetDefensiveMove());
                //defensiveMovePowers.Add(targetBestAction.GetPower()); // The sum has already been calculated above.

                defensiveMovesHit.SetDamage(0);
                defensiveMovesHit.SetDefenders(defenders);
                defensiveMovesHit.SetDefensiveMoves(defendersDefensiveMoves);
                defensiveMovesHit.SetResult(Enums.HitResult.Blocked);
                defensiveMovesHit.SetWasEasy(wasEasy);
                defensiveMovesHit.SetWasProtected(defenders.Count > 1);
            }
            else // Defenders cannot block the attack. Determine possible damage reduction.
            {
                // Determine if target should be added or not.
                if (targetBestAction != null)
                {
                    defenders.Add(target);
                    defendersDefensiveMoves.Add(targetBestAction.GetDefensiveMove());
                    defensiveMovePowers.Add(targetBestAction.GetPower());
                    defensiveMovesPowerSum = SumList(defensiveMovePowers);
                }

                int fullDamage = AttackDamage(target, attackers, offensiveMoves, attackerRandomAdds);
                float attackPowerDiff = attackPower - defensiveMovesPowerSum;

                if (defenders.Count == 0 || attackPowerDiff > MAX_ATTACK_POWER_DIFF_TO_USE_DEFENSIVE_MOVE) // No defenders attempt to block.
                {
                    defensiveMovesHit.SetDamage(fullDamage);
                    defensiveMovesHit.SetDefenders(defenders);
                    defensiveMovesHit.SetDefensiveMoves(defendersDefensiveMoves);
                    defensiveMovesHit.SetResult(Enums.HitResult.Hit);
                    defensiveMovesHit.SetWasEasy(false);
                    defensiveMovesHit.SetWasProtected(false);
                }
                else // Blocking moves reduce damage.
                {
                    float partialDamageFloat = (float)fullDamage * attackPowerDiff; // 0.0 < attackPowerDiff < 1.0 , so partialDamage is 0-1 fraction of fullDamage.
                    int partialDamage = (int)(partialDamageFloat + .5f); // .5 is for rounding.

                    defensiveMovesHit.SetDamage(partialDamage);
                    defensiveMovesHit.SetDefenders(defenders);
                    defensiveMovesHit.SetDefensiveMoves(defendersDefensiveMoves);
                    defensiveMovesHit.SetResult(Enums.HitResult.PartiallyBlocked);
                    defensiveMovesHit.SetWasEasy(false);
                    defensiveMovesHit.SetWasProtected(defenders.Count > 1 || defenders.Contains(target) == false);
                }
            }
        }

        return defensiveMovesHit;
    }

    public Hit GetDeflectHitMelee(Enums.MoveType moveType, float attackPower, List<Enums.Nature> moveNatures, List<Move> offensiveMoves, List<float> attackerRandomAdds, List<Fighter> attackers, Fighter target, float targetRandomAdd, List<Fighter> protectors, Dictionary<Fighter, float> protectorRandomAddMap, float subBonus)
    {
        Hit deflectHit = new Hit(); // Returned at end of function.

        float attackSkill = 0.0f;
        switch (moveType)
        {
            case Enums.MoveType.Melee:
                attackSkill = AttackersMeleeSkill(attackers, offensiveMoves, attackerRandomAdds, subBonus);
                break;
            case Enums.MoveType.NinTai:
                attackSkill = attackPower;
                break;
            default:
                Debug.LogError("Error! Unexpected MoveType [" + moveType + "] in Fight.GetDefelctHitMelee!");
                break;
        }

        bool targetCanMove = target.CheckIfCanMove();
        float targetSkill = 0.0f;
        if (targetCanMove == true)
        {
            switch (moveType)
            {
                case Enums.MoveType.Melee:
                    targetSkill = target.GetMeleeDefenseSkill(targetRandomAdd);
                    break;
                case Enums.MoveType.NinTai:
                    targetSkill = target.GetNinTaiDefenseSkill(targetRandomAdd);
                    break;
                default:
                    Debug.LogError("Error! Unexpected MoveType [" + moveType + "] in Fight.GetDefelctHitMelee!");
                    break;
            }
        }

        float targetSkillDiff = targetSkill - attackSkill;
        bool targetCanDeflect = (targetSkillDiff >= 0 && targetCanMove);

        Fighter protectorWhoCanDeflect = null;
        bool protectorCanDeflectEasily = false;

        int protectorsCount = protectors.Count;
        for (int index = 0; index < protectorsCount; ++index)
        {
            Fighter protector = protectors[index];

            if (protector.CheckIfAlive() == true && protector.CheckIfCanMove() == true) // Don't include protectors who cannot move.
            {
                float protectorRandomAdd = protectorRandomAddMap[protector];
                float protectorSkill = 0.0f;
                switch (moveType)
                {
                    case Enums.MoveType.Melee:
                        protectorSkill = protector.GetMeleeDefenseSkill(protectorRandomAdd);
                        break;
                    case Enums.MoveType.NinTai:
                        targetSkill = protector.GetNinTaiDefenseSkill(protectorRandomAdd);
                        break;
                    default:
                        Debug.LogError("Error! Unexpected MoveType [" + moveType + "] in Fight.GetDefelctHitMelee!");
                        break;
                }

                float protectorSkillDiff = protectorSkill - attackSkill;
                bool wasEasy = protectorSkillDiff >= EASY_DEFLECT_SKILL_DIFF ? true : false;

                if (protectorSkillDiff >= 0) // Can successfully deflect the attack.
                {
                    if (protectorWhoCanDeflect == null ||
                        (!protector.CheckTrait(Enums.Trait.TeleportationMarkDeflect) && !protectorWhoCanDeflect.CheckTrait(Enums.Trait.TeleportationMarkDeflect) && protector.GetMana() > protectorWhoCanDeflect.GetMana()) || // When neither can mark the attacker, protector with the most mana will be used.
                        (protector.CheckTrait(Enums.Trait.TeleportationMarkDeflect) && wasEasy && (!protectorWhoCanDeflect.CheckTrait(Enums.Trait.TeleportationMarkDeflect) || !protectorCanDeflectEasily))) // New protector can mark the attacker.
                    {
                        protectorWhoCanDeflect = protector;
                        protectorCanDeflectEasily = wasEasy;
                    }
                }
            }
        }

        // Sort in descending order of MeleeDefenseSkill.
        protectors.Sort((left, right) => right.GetMeleeDefenseSkill(protectorRandomAddMap[right]).CompareTo(left.GetMeleeDefenseSkill(protectorRandomAddMap[left])));

        List<float> defenderSkills = new List<float>();
        bool defendersCanDeflect = false;
        List<Fighter> defenders = new List<Fighter>();
        float defendersSkillSum = 0.0f;

        foreach (Fighter protector in protectors) // Keep adding protectors to see if they can fully deflect the attack together.
        {
            if (protector.CheckIfAlive() == true && protector.CheckIfCanMove() == true) // Don't include protectors who cannot move.
            {
                defenders.Add(protector);
                float protectorRandomAdd = protectorRandomAddMap[protector];
                float protectorSkill = 0.0f;
                switch (moveType)
                {
                    case Enums.MoveType.Melee:
                        protectorSkill = protector.GetMeleeDefenseSkill(protectorRandomAdd);
                        break;
                    case Enums.MoveType.NinTai:
                        targetSkill = protector.GetNinTaiDefenseSkill(protectorRandomAdd);
                        break;
                    default:
                        Debug.LogError("Error! Unexpected MoveType [" + moveType + "] in Fight.GetDefelctHitMelee!");
                        break;
                }

                defenderSkills.Add(protectorSkill);
                defendersSkillSum = SumList(defenderSkills);

                if (defendersSkillSum >= attackSkill)
                {
                    defendersCanDeflect = true;
                    break;
                }
            }
        }

        if (defendersCanDeflect == false && defenders.Count > 0 && targetCanMove == true) // Add the target to the defense along with protectors.
        {
            defenders.Add(target);
            defenderSkills.Add(targetSkill);
            defendersSkillSum = SumList(defenderSkills);
            defendersCanDeflect = defendersSkillSum >= attackSkill;
        }

        if (protectorWhoCanDeflect != null) // A single protector can fully deflect the attack for no damage.
        {
            deflectHit.SetDamage(0);
            deflectHit.SetDefenders(new List<Fighter>() { protectorWhoCanDeflect });
            deflectHit.SetResult(Enums.HitResult.Deflected);
            deflectHit.SetWasProtected(true);
            deflectHit.SetWasEasy(protectorCanDeflectEasily);
        }
        else if (defendersCanDeflect == true) // Multiple defenders can fully deflect the attack together.
        {
            deflectHit.SetDamage(0);
            deflectHit.SetDefenders(defenders);
            deflectHit.SetResult(Enums.HitResult.Deflected);
            deflectHit.SetWasProtected(true);
            deflectHit.SetWasEasy(false);
        }
        else if (targetCanDeflect == true) // Target can fully deflect the attack.
        {
            deflectHit.SetDamage(0);
            deflectHit.SetDefenders(new List<Fighter>() { target });
            deflectHit.SetResult(Enums.HitResult.Deflected);
            deflectHit.SetWasProtected(false);
            deflectHit.SetWasEasy(targetSkillDiff >= EASY_DEFLECT_SKILL_DIFF);
        }
        else // The attack cannot be fully deflected.
        {
            int fullDamage = AttackDamage(target, attackers, offensiveMoves, attackerRandomAdds);
            float attackSkillDiff = attackSkill - defendersSkillSum;

            if (attackSkillDiff >= 1.0f) // Attack hits for full damage.
            {
                deflectHit.SetDamage(fullDamage);
                deflectHit.SetResult(Enums.HitResult.Hit);
                deflectHit.SetWasProtected(false);
                deflectHit.SetWasEasy(false);
            }
            else // Attack is partially deflected and hits for reduced damage.
            {
                float partialDamageFloat = (float)fullDamage * attackSkillDiff; // 0.0 < attackSkillDiff < 1.0 , so partialDamage is 0-1 fraction of fullDamage.
                int partialDamage = (int)(partialDamageFloat + .5f); // .5 is for rounding.

                deflectHit.SetDamage(partialDamage);
                deflectHit.SetResult(Enums.HitResult.PartiallyDeflected);
                deflectHit.SetDefenders(defenders);
                deflectHit.SetWasProtected(defenders.Count > 1 || defenders.Contains(target) == false);
                deflectHit.SetWasEasy(false);
            }
        }

        return deflectHit;
    }

    // Adds certain natures to the move if the fighter is relevantly powered up. Ex: Sage mana.
    // TODO Might want to only add certain natures.
    public List<Enums.Nature> GetFinalNaturesInMove(Fighter fighter, Move move)
    {
        List<Enums.Nature> moveNatures = new List<Enums.Nature>(move.GetNaturesArray());
        foreach(Enums.Nature nature in fighter.GetBonusNatures())
        {
            moveNatures.Add(nature);
        }

        return moveNatures;
    }

    public List<Enums.Nature> GetFinalNaturesInMoveEvent(MoveEvent moveEvent)
    {
        List<Enums.Nature> finalNatures = new List<Enums.Nature>();
        List<Fighter> fighters = moveEvent.GetFighters();
        List<Move> moves = moveEvent.GetMoves();

        int fightersCount = fighters.Count;
        for (int index = 0; index < fightersCount; ++index)
        {
            Fighter fighter = fighters[index];
            Move move = moves[index];
            List<Enums.Nature> moveNatures = GetFinalNaturesInMove(fighter, move);
            finalNatures.AddRange(moveNatures);
        }

        return finalNatures;
    }


    public Hit GetHitPsychic(Enums.MoveType moveType, float attackPower, List<Enums.Nature> attackNatures, List<Move> offensiveMoves, List<float> attackerRandomAdds, List<Fighter> attackers, Fighter target, float targetRandomAdd, List<Fighter> protectors)
    {
        Hit psychicHit = new Hit(); // Returned at end of function.
        psychicHit.SetDefenders(new List<Fighter>());
        psychicHit.SetDefensiveMoves(new List<Move>());
        psychicHit.SetDamage(0);
        psychicHit.SetWasEasy(false);
        psychicHit.SetWasProtected(false);

        Move offensiveMove = offensiveMoves[0]; // Psychic moves are currently all independent and not combined.
        int fullDamage = AttackDamage(target, attackers, offensiveMoves, attackerRandomAdds);
        float targetIntelligence = target.GetIntelligence();
        int randomNum = Random.Range(0, 100);
        float powerDiff = attackPower - (target.GetPsychic() + RandomAdd());

        if (target.CheckStatus(Enums.StatusType.PsychicControl) == true || target.CheckStatus(Enums.StatusType.PsychicParalysis) == true)
        {
            psychicHit.SetResult(Enums.HitResult.PsychicFailureAlreadyUnder);
        }
        else if (target.CheckIfSubbed() == true) // Substitutions don't move and will not be defended.
        {
            psychicHit.SetDamage(fullDamage);
            psychicHit.SetResult(Enums.HitResult.Substitution);
        }
        else if (offensiveMove.CheckNature(Enums.Nature.Sharingan) == true && 
                (CheckIfProtecting(target) == true ||
                target.CheckTrait(Enums.Trait.SharinganTrained) == true ||
                targetIntelligence >= 11f ||
                (10f >= targetIntelligence && targetIntelligence < 11f && randomNum < 67) ||
                (9f >= targetIntelligence && targetIntelligence < 10f && randomNum < 50) ||
                (8f >= targetIntelligence && targetIntelligence < 9f && randomNum < 33) ||
                (7f >= targetIntelligence && targetIntelligence < 8f && randomNum < 16)))
        {
            psychicHit.SetResult(Enums.HitResult.PsychicFailureSharinganAvoided);
        }
        else if (powerDiff > 0 && target.CheckTrait(Enums.Trait.PsychicImmune) == false)
        {
            psychicHit.SetResult(Enums.HitResult.PsychicSuccess);
            psychicHit.SetDamage(fullDamage);

            if (powerDiff >= EASY_GENJUTSU_POWER_DIFF)
            {
                psychicHit.SetWasEasy(true);
            }

            foreach (Fighter protector in protectors)
            {
                if (protector.GetIntelligence() >= Fighter.MIN_INTELLIGENCE_TO_REMOVE_PSYCHIC_CONTROL)
                {
                    List<Fighter> protectorList = new List<Fighter> { protector };

                    psychicHit.SetDefenders(protectorList);
                    psychicHit.SetWasProtected(true);
                    break;
                }
            }
        }
        else
        {
            psychicHit.SetResult(Enums.HitResult.PsychicFailureResist);

            if (-powerDiff >= EASY_GENJUTSU_POWER_DIFF || target.CheckTrait(Enums.Trait.PsychicImmune) == true)
            {
                psychicHit.SetWasEasy(true);
            }
        }

        return psychicHit;
    }

    public Hit GetHitMelee(Enums.MoveType moveType, float attackPower, List<Enums.Nature> attackNatures, List<Move> offensiveMoves, List<float> attackerRandomAdds, List<Fighter> attackers, Fighter target, float targetRandomAdd, List<Fighter> protectors, Dictionary<Fighter, float> protectorRandomAddMap)
    {
        Hit meleeHit = new Hit(); // Returned at end of function.

        if (target.CheckIfSubbed() == true) // Substitutions don't move and will not be defended.
        {
            int fullDamage = AttackDamage(target, attackers, offensiveMoves, attackerRandomAdds);

            meleeHit.SetDamage(fullDamage);
            meleeHit.SetResult(Enums.HitResult.Substitution);
            meleeHit.SetWasProtected(false);
            meleeHit.SetWasEasy(false);
        }
        else // Target is not a substitution and will therefore dodge, be defended, etc.
        {
            float subBonus = 0.0f;
            int attackersCount = attackers.Count;
            if (attackersCount == 1 && attackers[0].CheckIfSubbed() == true)
            {
                Fighter attacker = attackers[0];
                Substitution sub = attacker.GetSubstition();
                List<Fighter> enemiesTrickedBySub = sub.GetEnemiesTricked();

                if (enemiesTrickedBySub.Contains(target) == true)
                {
                    subBonus = SUBSTITUTION_BONUS;
                }
            }

            float targetSpeed = 0.0f;
            if (target.CheckIfCanMove() == true)
            {
                targetSpeed = target.GetSpeed() * target.GetHealthCo() + targetRandomAdd;
            }

            float minAttackerSpeed = float.MaxValue;
            //float minAttackCastingSpeed = float.MaxValue;
            List<float> attackSpeeds = new List<float>();
            for (int index = 0; index < attackersCount; ++index)
            {
                Fighter attacker = attackers[index];
                Move offensiveMove = offensiveMoves[index];
                float randomAdd = attackerRandomAdds[index];
                float castingSpeed = attacker.GetCastingSpeed(offensiveMove, randomAdd);

                float attackerSpeed = AttackerSpeed(attacker, offensiveMove, randomAdd, subBonus);
                attackSpeeds.Add(attackerSpeed);

                if (attackerSpeed < minAttackerSpeed)
                {
                    minAttackerSpeed = attackerSpeed;
                }
                /*if (castingSpeed < minAttackCastingSpeed)
                {
                    minAttackCastingSpeed = castingSpeed;
                }*/
            }

            /*float attackCastingSpeed = .5f * minAttackerSpeed + .5f * minAttackCastingSpeed;
            if (attackersCount == 1)
            {
                attackCastingSpeed += subBonus + offensiveMoves[0].GetAccuracy();
            }*/

            float attackSpeed = SumList(attackSpeeds);
            float attackSpeedDiff = attackSpeed - targetSpeed;

            // Try to avoid attack with pure speed.
            int randomNum = Random.Range(0, 100);
            if (attackSpeedDiff >= 0 ||                                                     // Target cannot avoid.
                (-.5f <= attackSpeedDiff && attackSpeedDiff < 0f && randomNum < 50) ||
                (-1.5f <= attackSpeedDiff && attackSpeedDiff < -.5f && randomNum < 33) ||
                (-2.5f <= attackSpeedDiff && attackSpeedDiff < -1.5f && randomNum < 17))
            {
                // Target cannot avoid attack with pure speed. Get deflect result.
                Hit deflectHit = GetDeflectHitMelee(moveType, attackPower, attackNatures, offensiveMoves, attackerRandomAdds, attackers, target, targetRandomAdd, protectors, protectorRandomAddMap, subBonus);

                if (deflectHit.GetResult() == Enums.HitResult.Deflected) // Attack was successfully deflected.
                {
                    meleeHit = deflectHit;
                }
                else // Not completely deflected. We have to consider defensive moves now.
                {
                    meleeHit = GetDefensiveMovesHit(moveType, attackPower, attackNatures, offensiveMoves, attackerRandomAdds, attackers, target, targetRandomAdd, protectors, protectorRandomAddMap, subBonus);
                }
            }
            else // Attack missed because target was too fast.
            {
                bool wasEasy = (targetSpeed - attackSpeed) >= EASY_DODGE_SPEED_DIFF;

                meleeHit.SetDamage(0);
                meleeHit.SetResult(Enums.HitResult.Miss);
                meleeHit.SetWasProtected(false);
                meleeHit.SetWasEasy(wasEasy);
            }
        }

        return meleeHit;
    }

    public Hit GetHitRanged(Enums.MoveType moveType, float attackPower, List<Enums.Nature> attackNatures, List<Move> offensiveMoves, List<float> attackerRandomAdds, List<Fighter> attackers, Fighter target, float targetRandomAdd, List<Fighter> protectors, Dictionary<Fighter, float> protectorRandomAddMap)
    {
        Hit rangedHit = new Hit(); // Returned at end of function.

        if (target.CheckIfSubbed() == true) // Substitutions don't move and will not be defended.
        {
            int fullDamage = AttackDamage(target, attackers, offensiveMoves, attackerRandomAdds);

            rangedHit.SetDamage(fullDamage);
            rangedHit.SetResult(Enums.HitResult.Substitution);
            rangedHit.SetWasProtected(false);
            rangedHit.SetWasEasy(false);
        }
        else // Target is not a substitution and will therefore dodge, be defended, etc.
        {
            float subBonus = 0.0f;
            int attackersCount = attackers.Count;
            if (attackersCount == 1 && attackers[0].CheckIfSubbed() == true)
            {
                Fighter attacker = attackers[0];
                Substitution sub = attacker.GetSubstition();
                List<Fighter> enemiesTrickedBySub = sub.GetEnemiesTricked();

                if (enemiesTrickedBySub.Contains(target) == true)
                {
                    subBonus = SUBSTITUTION_BONUS;
                }
            }

            float targetSpeed = 0.0f;
            if (target.CheckIfCanMove() == true)
            {
                targetSpeed = target.GetSpeed() * target.GetHealthCo() + targetRandomAdd;
            }

            List<float> attackAccuracies = new List<float>();
            for (int index = 0; index < attackersCount; ++index)
            {
                Fighter attacker = attackers[index];
                Move offensiveMove = offensiveMoves[index];
                float randomAdd = attackerRandomAdds[index];
                float accuracy = MoveAccuracyRanged(attacker, offensiveMove, subBonus, randomAdd);

                attackAccuracies.Add(accuracy);
            }

            float attackAccuracy = SumList(attackAccuracies);
            float attackSpeedDiff = attackAccuracy - targetSpeed;

            // Try to avoid attack with pure speed.
            int randomNum = Random.Range(0, 100);
            if (attackSpeedDiff >= 1)
            {
                // Target cannot avoid attack with speed.
                rangedHit = GetDefensiveMovesHit(moveType, attackPower, attackNatures, offensiveMoves, attackerRandomAdds, attackers, target, targetRandomAdd, protectors, protectorRandomAddMap, subBonus);
            }
            else if ((0f < attackSpeedDiff && attackSpeedDiff < 1f) ||
                (-1f <= attackSpeedDiff && attackSpeedDiff <= 0f && randomNum < 33) ||
                (-2f <= attackSpeedDiff && attackSpeedDiff < -1f && randomNum < 17))
            {
                // Cannot completely avoid attack with speed. Attack can partially hit (depends on defensive moves, etc).
                rangedHit = GetDefensiveMovesHit(moveType, attackPower, attackNatures, offensiveMoves, attackerRandomAdds, attackers, target, targetRandomAdd, protectors, protectorRandomAddMap, subBonus);

                // We need to account for the partial hit by reducing the damage.
                if (rangedHit.GetResult() == Enums.HitResult.PartialHit)
                {
                    float damageCo = attackSpeedDiff;

                    if (damageCo < MIN_DAMAGE_CO_FROM_PARTIAL_HIT)
                    {
                        damageCo = MIN_DAMAGE_CO_FROM_PARTIAL_HIT;
                    }

                    int reducedDamage = (int)(damageCo * rangedHit.GetDamage() + .5f); // .5 is for rounding.
                    rangedHit.SetDamage(reducedDamage);
                }
            }
            else // Attack missed because target was too fast.
            {
                bool wasEasy = (targetSpeed - attackAccuracy) >= EASY_DODGE_SPEED_DIFF;

                rangedHit.SetDamage(0);
                rangedHit.SetResult(Enums.HitResult.Miss);
                rangedHit.SetWasProtected(false);
                rangedHit.SetWasEasy(wasEasy);
            }
        }

        return rangedHit;
    }

    public IEnumerator GetMoveEvent(Fighter fighter, System.Action<MoveEvent> onMoveEventReady)
    {
        if (fighter.GetControlType() == Enums.ControlType.CPU)
        {
            onMoveEventReady(fighter.GetAI().GetCPUMoveEvent(this, fighter));
            yield break;
        }

        yield return SelectMoveUI.GetUserMoveEvent(fighter, onMoveEventReady);
    }

    public IEnumerator GetMoveEventList(System.Action<List<MoveEvent>> onMoveEventListReady)
    {
        List<MoveEvent> moveEvents = new List<MoveEvent>();
        foreach (Fighter fighter in Fighters)
        {
            MoveEvent moveEvent = null;
            yield return GetMoveEvent(fighter, result => moveEvent = result);
            moveEvents.Add(moveEvent);
        }

        mWriter.WriteLine("###################################################################");
        mWriter.WriteLine("###################################################################\n");

        List<MoveEvent> subMoveEvents = new List<MoveEvent>();
        List<MoveEvent> protectionMoveEvents = new List<MoveEvent>();
        List<MoveEvent> generalMoveEvents = new List<MoveEvent>();
        List<MoveEvent> finalMoveEvents = new List<MoveEvent>();

        foreach (MoveEvent moveEvent in moveEvents)
        {
            if (moveEvent.GetMoveType() != Enums.MoveType.Skip)
            {
                switch (moveEvent.GetMoveType())
                {
                    case Enums.MoveType.Substitution:
                        subMoveEvents.Add(moveEvent);
                        break;
                    case Enums.MoveType.Protect:
                        protectionMoveEvents.Add(moveEvent);
                        break;
                    default:
                        generalMoveEvents.Add(moveEvent);
                        break;
                }
            }
        }

        generalMoveEvents = CombineMoveEvents(generalMoveEvents);

        finalMoveEvents.AddRange(protectionMoveEvents);
        finalMoveEvents.AddRange(subMoveEvents);
        finalMoveEvents.AddRange(generalMoveEvents);

        onMoveEventListReady(finalMoveEvents);
    }

    public MoveEvent GetMoveEventWithActualAttackersAndTargets(MoveEvent originalMoveEvent)
    {
        MoveEvent moveEvent = new MoveEvent();
        moveEvent.SetTargetType(originalMoveEvent.GetTargetType());
        moveEvent.SetTargetTeam(originalMoveEvent.GetTargetTeam());
        moveEvent.SetMoveType(originalMoveEvent.GetMoveType());

        foreach (Fighter target in originalMoveEvent.GetTargets())
        {
            if (target.CheckIfAlive() == true)
            {
                moveEvent.AddTarget(target);
            }
        }

        List<Fighter> fighters = originalMoveEvent.GetFighters();
        int fightersCount = fighters.Count;
        for (int index = 0; index < fightersCount; ++index)
        {
            Fighter fighter = fighters[index];
            Move move = null;

            Enums.MoveType moveType = originalMoveEvent.GetMoveType();            
            if (moveType == Enums.MoveType.Protect)
            {
                if (fighter.CheckIfAlive() == true)
                {
                    moveEvent.AddFighter(fighter);
                    moveEvent.AddRandomAdd(originalMoveEvent.GetRandomAdds()[index]);
                }
            }
            else
            {
                move = originalMoveEvent.GetMoves()[index];

                if (fighter.CheckIfAlive() == true && fighter.GetAI().CheckIfCanPerformMove(this, fighter, move) == true)
                {
                    if (moveType == Enums.MoveType.PowerUp ||
                        moveType == Enums.MoveType.Protect ||
                        fighter.CheckIfCanMove() == true ||
                        (move.CheckOccular() == true && fighter.CheckStatus(Enums.StatusType.PsychicControl) == false && fighter.CheckStatus(Enums.StatusType.PsychicParalysis) == false))
                    {
                        moveEvent.AddFighter(fighter);
                        moveEvent.AddRandomAdd(originalMoveEvent.GetRandomAdds()[index]);
                        moveEvent.AddMove(originalMoveEvent.GetMoves()[index]);
                    }
                }
            }
        }

        return moveEvent;
    }

    public List<Fighter> GetFighters() { return Fighters; }

    public List<Fighter> GetProtectors(Fighter fighter)
    {
        List<Fighter> protectors = new List<Fighter>();

        foreach (Protection protection in Protections)
        {
            if (protection.GetProtected() == fighter && protection.GetProtector() != fighter)
            {
                protectors.Add(protection.GetProtector());
            }
        }

        return protectors;
    }

    public int GetRoundNumber() { return RoundNumber;}

    public List<Fighter> GetTeamList(int team)
    {
        switch (team)
        {
            case 1:
                return new List<Fighter>(Team1);
            case 2:
                return new List<Fighter>(Team2);
            case 3:
                return new List<Fighter>(Team3);
        }

        Debug.LogError("Error! Unknown team[" + team + "] in Fight.GetTeamList. Returning null.");
        return null;
    }

    public int GetTeams() { return Teams; }

    public void InitFight()
    {
        string debugOutputPath = "/Users/vincentroma/Fight/DebugOutput.txt"; //Application.persistentDataPath + " / DebugOutput.txt"; // "C:/Users/Asus/Documents/Unity/Fight/DebugOutput.txt";
        mWriter = new StreamWriter(debugOutputPath, false);
        RoundNumber = 1;
        Fighters = new List<Fighter>();
        Clones = new List<Clone>();
        Protections = new List<Protection>();
        Summons = new List<Summon>();

        foreach (Fighter fighter in Team1)
        {
            fighter.SetTeam(1);
            Fighters.Add(fighter);
        }
        foreach (Fighter fighter in Team2)
        {
            fighter.SetTeam(2);
            Fighters.Add(fighter);
        }
        foreach (Fighter fighter in Team3)
        {
            fighter.SetTeam(3);
            Fighters.Add(fighter);
        }

        OriginalFighters = new List<Fighter>(Fighters);
        OriginalTeam1 = new List<Fighter>(Team1);
        OriginalTeam2 = new List<Fighter>(Team2);
        OriginalTeam3 = new List<Fighter>(Team3);

        foreach (Fighter fighter in Fighters)
        {
            fighter.InitFighter();
        }
    }
    public void InsertCombinedTargetMoveEvents(List<MoveEvent> listToInsert, List<MoveEvent> moveEventList)
    {
        float moveEventCastingSpeed = float.MaxValue;

        foreach (MoveEvent moveEvent in listToInsert)   // Find the slowest MoveEvent in the list of MoveEvents.
        {
            float tempMoveEventCastingSpeed = moveEvent.GetMoveEventCastingSpeed();

            if (tempMoveEventCastingSpeed < moveEventCastingSpeed)
            {
                moveEventCastingSpeed = tempMoveEventCastingSpeed;
            }
        }

        int index = 0;
        int moveEventListCount = moveEventList.Count;
        bool foundInsertIndex = false;

        while (index < moveEventListCount && foundInsertIndex == false)   // Iterate through moveEventList until you find a slower effective move event speed.
        {
            float effectiveMoveEventCastingSpeed = moveEventList[index].GetEffectiveMoveEventCastingSpeed();

            if (moveEventCastingSpeed > effectiveMoveEventCastingSpeed)   // We found the index.
            {
                foundInsertIndex = true;
            }
            ++index;
        }

        moveEventList.InsertRange(index, listToInsert);
    }

    public void InsertPsychicBeforeMoveEvent(MoveEvent moveEvent, List<MoveEvent> moveEvents, List<MoveEvent> finalMoveEvents, int moveEventIndex, List<int> usedIndices)
    {
        Enums.MoveType moveType = moveEvent.GetMoveType();

        List<MoveEvent> earlierMoveEvents = moveEvents.GetRange(0, moveEventIndex + 1);  // MoveEvents that happen before moveEvent
        int earlierMoveEventsCount = earlierMoveEvents.Count;

        int earlierMoveEventIndex = 0;
        float earlierMoveEventPower = 0f;
        bool foundEarlierMoveEvent = false;

        for (int index = 0; index < earlierMoveEventsCount; ++index)
        {
            MoveEvent earlierMoveEvent = earlierMoveEvents[index];

            if (earlierMoveEvent.GetTargetType() == Enums.TargetType.OneEnemy &&
                earlierMoveEvent.CheckCombineAttacks() == true &&
                ((moveType == Enums.MoveType.Psychic && earlierMoveEvent.GetMoveType() != Enums.MoveType.Psychic) || (moveType != Enums.MoveType.Psychic && earlierMoveEvent.GetMoveType() == Enums.MoveType.Psychic)))
            {
                float moveEventPower = MoveEventPower(earlierMoveEvent);

                if (moveEventPower > earlierMoveEventPower)
                {
                    earlierMoveEventIndex = index;
                    earlierMoveEventPower = moveEventPower;
                    foundEarlierMoveEvent = true;
                }
            }
        }

        if (moveType == Enums.MoveType.Psychic && foundEarlierMoveEvent == true)
        {
            MoveEvent earlierMoveEvent = earlierMoveEvents[earlierMoveEventIndex];
            usedIndices.Add(earlierMoveEventIndex);
            finalMoveEvents.Add(earlierMoveEvent); // Added before moveEvent because it should happen after.
        }

        finalMoveEvents.Add(moveEvent);

        if (moveType != Enums.MoveType.Psychic && foundEarlierMoveEvent == true)
        {
            MoveEvent earlierMoveEvent = earlierMoveEvents[earlierMoveEventIndex];
            usedIndices.Add(earlierMoveEventIndex);
            finalMoveEvents.Add(earlierMoveEvent); // Added after moveEvent because it should happen first.
        }
    }

    // Spells/Projectiles. Melee are handled differently.
    public float MoveAccuracyRanged(Fighter fighter, Move offensiveMove, float subBonus, float randomAdd)
    {
        return offensiveMove.GetAccuracy() + subBonus + randomAdd;
    }

    // Spells/Projectiles
    public float MoveEventAccuracyRanged(MoveEvent moveEvent, float cloneBonus)
    {
        List<float> moveAccuracies = new List<float>();

        if (moveEvent.GetFighters().Count > 1)
        {
            cloneBonus = 0;
        }

        for (int index = 0; index < moveEvent.GetFighters().Count; index++)
        {
            Fighter fighter = moveEvent.GetFighters()[index];
            Move offensiveMove = moveEvent.GetMoves()[index];
            float randomAdd = moveEvent.GetRandomAdds()[index];
            float moveAccuracy = MoveAccuracyRanged(fighter, offensiveMove, cloneBonus, randomAdd);
            moveAccuracies.Add(moveAccuracy);
        }

        return SumList(moveAccuracies);
    }

    public float MoveEventPower(MoveEvent moveEvent)
    {
        List<float> movePowers = new List<float>();
        List<Fighter> fighters = moveEvent.GetFighters();
        int fightersCount = fighters.Count;
        List<Move> moves = moveEvent.GetMoves();
        List<float> randomAdds = moveEvent.GetRandomAdds();

        for (int index = 0; index < fightersCount; ++index)
        {
            Fighter fighter = fighters[index];
            Move move = moves[index];
            float randomAdd = randomAdds[index];
            movePowers.Add(MovePower(fighter, move, randomAdd));
        }

        List<Enums.Nature> natures = GetFinalNaturesInMoveEvent(moveEvent);
        float movePower = SumList(movePowers);

        return MovePowerWithNatures(movePower, natures); // return SumList(movePowers);   
    }

    public static float MovePower(Fighter fighter, Move move, float randomAdd)
    {
        float movePower;

        switch (move.GetMoveType())
        {
            case Enums.MoveType.Melee:
                {
                    float levelFactor = move.GetLevel() - TAIJUTSU_MOVE_POWER_PENALTY;
                    levelFactor = Mathf.Max(levelFactor, 0f);

                    float meleeFactor = fighter.GetMelee() - TAIJUTSU_MOVE_POWER_PENALTY;
                    meleeFactor = Mathf.Max(meleeFactor, 0f);

                    float strengthFactor = fighter.GetStrength() * fighter.GetHealthCo() - TAIJUTSU_MOVE_POWER_PENALTY;
                    strengthFactor = Mathf.Max(strengthFactor, 0f);

                    movePower = .05f * levelFactor + .05f * meleeFactor + .9f * strengthFactor + randomAdd;
                    break;
                }
            case Enums.MoveType.Psychic:
                movePower = .15f * fighter.GetPsychic() + .85f * move.GetLevel() + randomAdd;
                break;
            case Enums.MoveType.NinTai:
                movePower = .80f * move.GetLevel() + .10f * fighter.GetMelee() + .10f * fighter.GetSpellcraft() + randomAdd;
                break;
            case Enums.MoveType.Defensive:
                if (move.GetUseMeleeSkill() == true)
                {
                    movePower = move.GetLevel() + randomAdd;
                }
                else
                {
                    movePower = .15f * fighter.GetSpellcraft() + .85f * move.GetLevel() + randomAdd; // Default
                }
                break;
            default:
                movePower = .15f * fighter.GetSpellcraft() + .85f * move.GetLevel() + randomAdd;
                break;
        }

        return Mathf.Max(movePower, 0f);
    }

    public float MovePowerWithNatures(float movePower, List<Enums.Nature> natures)
    {
        // TODO: Figure this out. Should a multimove get stronger based on types included?
        if (natures.Count < 2)
        {
            return movePower;
        }

        float powerBonus = 0f;
        List<Enums.Nature> naturesAccountedFor = new List<Enums.Nature>();

        foreach (Enums.Nature nature in natures)
        {
            if (naturesAccountedFor.Contains(nature) == false)  // Ignore natures that have already been accounted for.
            {
                switch (nature)
                {
                    case Enums.Nature.Fire:
                        {
                            if (natures.Contains(Enums.Nature.Wind) == true)
                            {
                                powerBonus += MOVE_POWER_BONUS_FROM_NATURE_COMBO;
                                naturesAccountedFor.Add(Enums.Nature.Wind);
                            }
                            break;
                        }
                    case Enums.Nature.Wind:
                        {
                            if (natures.Contains(Enums.Nature.Fire) == true)
                            {
                                powerBonus += MOVE_POWER_BONUS_FROM_NATURE_COMBO;
                                naturesAccountedFor.Add(Enums.Nature.Fire);
                            }
                            break;
                        }
                    case Enums.Nature.Lightning:
                        {
                            if (natures.Contains(Enums.Nature.Water) == true)
                            {
                                powerBonus += MOVE_POWER_BONUS_FROM_NATURE_COMBO;
                                naturesAccountedFor.Add(Enums.Nature.Water);
                            }
                            break;
                        }
                    case Enums.Nature.Water:
                        {
                            if (natures.Contains(Enums.Nature.Lightning) == true)
                            {
                                powerBonus += MOVE_POWER_BONUS_FROM_NATURE_COMBO;
                                naturesAccountedFor.Add(Enums.Nature.Lightning);
                            }
                            break;
                        }
                }

                naturesAccountedFor.Add(nature);
            }
        }

        powerBonus = Mathf.Min(powerBonus, MAX_MOVE_POWER_BONUS_FROM_NATURES);

        return (movePower + powerBonus);
    }

    public float NaturesDamageCo(Fighter attacker, Fighter target, Move attackMove)
    {
        float damageCo = 1.0f;

        List<Enums.Nature> targetNatures = target.GetNatures();
        List<Enums.Nature> attackNatures = GetFinalNaturesInMove(attacker, attackMove);
        bool attackHasOnlyOneNature = attackNatures.Count == 1;
        List<Enums.Nature> naturesExamined = new List<Enums.Nature>(); // These have already been examined and shouldn't be checked again.

        foreach (Enums.Nature attackNature in attackNatures)
        {
            if (naturesExamined.Contains(attackNature) == false) // Don't look at the same Nature twice.
            {
                switch(attackNature)
                {
                    case Enums.Nature.Fire:
                        {
                            damageCo += AdjustNatureDamageCo(Enums.Nature.Fire, Enums.Nature.Water, Enums.Nature.Wind, targetNatures, attackHasOnlyOneNature);
                            break;
                        }
                    case Enums.Nature.Earth:
                        {
                            damageCo += AdjustNatureDamageCo(Enums.Nature.Earth, Enums.Nature.Lightning, Enums.Nature.Water, targetNatures, attackHasOnlyOneNature);
                            break;
                        }
                    case Enums.Nature.Wind:
                        {
                            damageCo += AdjustNatureDamageCo(Enums.Nature.Wind, Enums.Nature.Fire, Enums.Nature.Lightning, targetNatures, attackHasOnlyOneNature);
                            break;
                        }
                    case Enums.Nature.Lightning:
                        {
                            damageCo += AdjustNatureDamageCo(Enums.Nature.Lightning, Enums.Nature.Wind, Enums.Nature.Earth, targetNatures, attackHasOnlyOneNature);
                            break;
                        }
                    case Enums.Nature.Water:
                        {
                            damageCo += AdjustNatureDamageCo(Enums.Nature.Water, Enums.Nature.Earth, Enums.Nature.Fire, targetNatures, attackHasOnlyOneNature);
                            break;
                        }
                }

                naturesExamined.Add(attackNature);
            }
        }

        return Mathf.Max(damageCo, 0.0f); // We don't want anything negative.
    }

    /*public List<MoveEvent> PlacePsychicMoveEvents(List<MoveEvent> moveEvents)
    {
        List<Fighter> targetsHandled = new List<Fighter>();
        List<int> usedIndices = new List<int>();
        List<MoveEvent> finalMoveEvents = new List<MoveEvent>(); // This will be in reverse order initially

        for (int index = moveEvents.Count - 1; index >= 0; --index) // Iterating through the list from back to front
        {
            if (usedIndices.Contains(index) == false)   // If index has already been inserted into finalMoveEvents then we skip it
            {
                MoveEvent moveEvent = moveEvents[index];
                Fighter target = moveEvent.GetTargets()[0];   // We only care about OneEnemy MoveEvents, so we can assume 1 target.

                if (index > 0 &&
                    moveEvent.GetTargetType() == Enums.TargetType.OneEnemy &&
                    targetsHandled.Contains(target) == false && 
                    moveEvent.CheckCombineAttacks() == true)
                {
                    InsertPsychicBeforeMoveEvent(moveEvent, moveEvents, finalMoveEvents, index, usedIndices);
                    targetsHandled.Add(target);
                }
                else
                {
                    finalMoveEvents.Add(moveEvent);
                }
            }
        }

        finalMoveEvents.Reverse();  // MoveEvents were inserted in reverse order

        return finalMoveEvents;
    }*/

    public void PrintAttackString(MoveEvent moveEvent)
    {
        if (moveEvent.GetMoveType() != Enums.MoveType.Protect)  // No need to print anything for a Protect MoveEvent.
        {
            string printString = "";
            List<Fighter> fighters = moveEvent.GetFighters();
            int fightersCount = fighters.Count;
            List<Fighter> targets = moveEvent.GetTargets();
            Enums.TargetType targetType = moveEvent.GetTargetType();

            if (fightersCount == 1)
            {
                Fighter fighter = fighters[0];
                Move move = moveEvent.GetMoves()[0];

                bool cloneAttack = (fighter.CheckIfSubbed() == true && targetType == Enums.TargetType.OneEnemy);

                if (cloneAttack == true)
                {
                    Fighter target = targets[0];
                    printString += fighter.GetName() + " appears behind " + target.GetName() + " and uses ";
                }
                else
                {
                    printString += fighter.GetName() + " uses ";
                }

                printString += move.GetName();
            }
            else
            {
                List<Move> moves = moveEvent.GetMoves();

                printString += Util.ListString(fighters) + " use " + Util.ListString(moves);
            }

            // Target info printing
            if (targetType == Enums.TargetType.Self)
            {
                printString += "!";
            }
            else
            {
                Enums.MoveType moveType = moveEvent.GetMoveType();

                if (moveType == Enums.MoveType.Medical)
                {
                    printString += " on ";
                }
                else
                {
                    printString += " against ";
                }

                switch (targetType)
                {
                    case Enums.TargetType.OneEnemy:
                    case Enums.TargetType.OneTeamMember:
                        {
                            Fighter target = targets[0];
                            printString += target.GetName() + "!";
                            break;
                        }
                    case Enums.TargetType.EnemyTeam:
                        {
                            int targetTeam = moveEvent.GetTargetTeam();
                            printString += "Team " + targetTeam + "!";
                            break;
                        }
                    case Enums.TargetType.AllEnemies:
                        {
                            printString += "all enemies!";
                            break;
                        }
                    case Enums.TargetType.EnemiesWithStatuses:
                    case Enums.TargetType.TeamMembersWithStatuses:
                        {
                            printString += Util.ListString(targets) + "!";
                            break;
                        }
                    case Enums.TargetType.Team:
                        {
                            int team = moveEvent.GetTargetTeam();
                            printString += "all members of team " + team + "!";
                            break;
                        }
                    default:
                        Debug.LogError("Error! Unexpected TargetType [" + targetType + "] in Fight.PrintAttackString!");
                        break;
                }
            }

            mWriter.WriteLine(printString);
        }
    }

    static public float RandomAdd() 
    {
        float x = (float)Random.Range(0, RANDOM_ADD_NUM_POSSIBLE_RESULTS + 1); // x is within [0, 100]
        float numResults = (float)RANDOM_ADD_NUM_POSSIBLE_RESULTS;

        return ((x - (numResults / 2.0f)) / numResults) * RANDOM_ADD_RANGE; // randomAdd is within [-RANDOM_ADD_RANGE/2, RANDOM_ADD_RANGE/2] inclusive.
    }

    public void RemoveDefeatedFighters()
    {
        int index = 0;
        while (index < Fighters.Count)
        {
            Fighter fighter = Fighters[index];

            if (fighter.GetHealth() <= 0)
            {
                mWriter.WriteLine(fighter.GetName() + " is defeated!");
                RemoveFighter(fighter);
            }
            /*else if (fighter.GetMana() <= 0)
            {
                mWriter.WriteLine(fighter.GetName() + " has collapsed from exhausting all of their mana!");
                RemoveFighter(fighter);
            }*/
            else
            {
                ++index;
            }
        }
    }

    public void RemovePsychicControlForProtections()
    {
        foreach (Protection protection in Protections)
        {
            Fighter protectedFighter = protection.GetProtected();
            Fighter protector = protection.GetProtector();

            if (Fighters.Contains(protectedFighter) &&
                Fighters.Contains(protector) &&
                protector.CheckIfCanMove() == true &&
                (protectedFighter.CheckStatus(Enums.StatusType.PsychicControl) || protectedFighter.CheckStatus(Enums.StatusType.PsychicParalysis)) &&
                protector.GetIntelligence() >= Fighter.MIN_INTELLIGENCE_TO_REMOVE_PSYCHIC_CONTROL)
            {
                mWriter.WriteLine(protector.GetName() + " has released " + protectedFighter.GetName() + " from psychic control!");
                protectedFighter.RemoveStatus(Enums.StatusType.PsychicControl);
                protectedFighter.RemoveStatus(Enums.StatusType.PsychicParalysis);
            }
        }
    }

    public void RemoveFighter(Fighter fighter)
    {
        RemoveSummons(fighter);

        switch (fighter.GetTeam())
        {
            case 1:
                Team1.Remove(fighter);
                break;
            case 2:
                Team2.Remove(fighter);
                break;
            case 3:
                Team3.Remove(fighter);
                break;
        }

        Fighters.Remove(fighter);
    }

    public void RemoveSummons(Fighter fighter)
    {
        List<Fighter> theirSummons = new List<Fighter>();
        foreach (Summon summon in Summons)
        {
            if (summon.GetSummoner() == fighter)
            {
                theirSummons.Add(summon.GetSummoned());
            }
        }

        List<Fighter> teamList = GetTeamList(fighter.GetTeam());

        foreach (Fighter summoned in theirSummons)
        {
            mWriter.WriteLine(summoned.GetName() + " disappears!");
            teamList.Remove(summoned);
            Fighters.Remove(summoned);
        }
    }

    private IEnumerator Start()
    {
        InitFight();

        while (RoundNumber <= MAX_NUMBER_OF_ROUNDS)
        {
            UpdateSummons();
            UpdateStatuses();
            UpdatePowerUps();
            RemoveDefeatedFighters();
            Protections.Clear();

            mWriter.WriteLine("Round " + RoundNumber + "\n");
            DisplayTeamsText();

            List<MoveEvent> moveEventList = null;
            yield return GetMoveEventList(result => moveEventList = result);

            foreach (MoveEvent moveEvent in moveEventList)
            {
                if (CheckIfShouldExecuteMoveEvent(moveEvent) == true)
                {
                    yield return ExecuteMoveEvent(moveEvent);

                    if (CheckForEndOfFight() == true)
                    {
                        break;
                    }
                }
            }

            if (CheckForEndOfFight() == true)
            {
                break;
            }

            ApplyBonusHealth();

            if (CheckForEndOfFight() == true)
            {
                break;
            }

            RemovePsychicControlForProtections();
            mWriter.WriteLine("\n");
            ++RoundNumber;
        }

        DisplayFightResultsText();
    }

    public static float SumList(List<float> inList)
    {
        if (inList.Count == 0)
        {
            Debug.LogError("Error! Fight.SumList cannot sum a list with no elements.");
            return 0.0f;
        }
        else
        {
            List<float> list = new List<float>(inList);
            list.Sort();        // [3, 6, 8]
            list.Reverse();     // [8, 6, 3]

            float sum = list[0];
            float maximum = list[0];

            for (int index = 1; index < list.Count; index++)
            {
                float difference = maximum - list[index];
                sum += SUMLIST_MAX_INCREASE / Mathf.Pow(2.0f, Mathf.Max(difference + 1.0f, (float)index));
            }

            return sum;
        }
    }

    public float SumList(List<int> inList)
    {
        List<float> list = new List<float>();

        foreach (int x in inList)
        {
            list.Add((float)x);
        }

        return SumList(list);
    }

    public void UpdatePowerUps()
    {
        foreach (Fighter fighter in Fighters)
        {
            List<PowerUp> powerUps = fighter.GetPowerUps();

            int index = 0;
            while (index < powerUps.Count)
            {
                PowerUp powerUp = powerUps[index];

                if (RoundNumber == powerUp.GetEndingRoundNumber())
                {
                    Move powerUpMove = powerUp.GetPowerUpMove();
                    fighter.RemoveBonusData(powerUpMove.GetBonusData(), Enums.BonusSource.PowerUpMove, null, null, powerUpMove, null);
                    powerUps.RemoveAt(index);
                }
                else
                {
                    ++index;
                }
            }
        }
    }

    public void UpdateStatuses()
    {
        foreach (Fighter fighter in Fighters)
        {
            List<Status> statuses = fighter.GetStatuses();

            int index = 0;
            while (index < statuses.Count)
            {
                Status status = statuses[index];

                if (status.GetEndingRoundNumber() == RoundNumber)
                {
                    Enums.StatusType statusType = status.GetStatusType();
                    bool fighterIsAlive = Fighters.Contains(fighter);

                    if (fighterIsAlive && (statusType == Enums.StatusType.PsychicControl || statusType == Enums.StatusType.PsychicParalysis))
                    {
                        mWriter.WriteLine(fighter.GetName() + " is no longer under psychic control!");
                    }
                    else if (fighterIsAlive && statusType == Enums.StatusType.Trapped)
                    {
                        mWriter.WriteLine(fighter.GetName() + " is no longer trapped!");
                    }

                    statuses.RemoveAt(index);
                }
                else
                {
                    ++index;
                }
            }
        }
    }

    public void UpdateSummons()
    {
        foreach (Summon summon in Summons)
        {
            if (RoundNumber == summon.GetEndingRoundNumber())
            {
                mWriter.WriteLine(summon.GetSummoned().GetName() + " has disappeared!");
                int summonerTeam = summon.GetSummoner().GetTeam();
                switch (summonerTeam)
                {
                    case 1:
                        Team1.Remove(summon.GetSummoned());
                        break;
                    case 2:
                        Team2.Remove(summon.GetSummoned());
                        break;
                    case 3:
                        Team3.Remove(summon.GetSummoned());
                        break;
                }
                Fighters.Remove(summon.GetSummoned());
            }
        }
    }
}
