using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEvent
{
    protected float                 EffectiveMoveEventCastingSpeed = 0f;
    protected Enums.MoveType        MoveType;
    protected List<Fighter>         Fighters;
    protected List<Move>            Moves;
    protected List<float>           RandomAdds;
    protected List<Fighter>         Targets;
    protected int                   TargetTeam;
    protected Enums.TargetType      TargetType;

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public MoveEvent()
    {
        MoveType = Enums.MoveType.Skip;
        Fighters = new List<Fighter>();
        Moves = new List<Move>();
        RandomAdds = new List<float>();
        Targets = new List<Fighter>();
    }

    public void AddMove(Move move) { Moves.Add(move); }
    public void AddFighter(Fighter fighter) { Fighters.Add(fighter); }
    public void AddRandomAdd(float randomAdd) { RandomAdds.Add(randomAdd); }
    public void AddTarget(Fighter target) { Targets.Add(target); }

    public void AddTargets(List<Fighter> targets)
    {
        foreach (Fighter target in targets)
        {
            Targets.Add(target);
        }
    }

    public bool CheckCombineAttacks()
    {
        foreach (Fighter fighter in Fighters)
        {
            if (fighter.CheckCombineAttacks() == false)
            {
                return false;
            }
        }
        return true;
    }

    public AnimationTimes GetAnimationTimes(Hit hit)
    {
        AnimationTimes result = new AnimationTimes();

        float maxImpactTime = 0f;
        float maxAttackDuration = 0f;
        float maxDefenseDuration = 0f;
        List<float> attackDelays = new List<float>();
        List<float> defenseDelays = new List<float>();

        // First iteration through attacking moves
        foreach (Move move in GetMoves())
        {
            AnimationData animationData = move.GetAnimationData();

            if (animationData != null)
            {
                float duration = animationData.GetDuration();
                if (duration > maxAttackDuration)
                {
                    maxAttackDuration = duration;
                }

                float impactTime = animationData.GetImpactTime();
                if (impactTime > maxImpactTime)
                {
                    maxImpactTime = impactTime;
                }
            }
            else
            {
                Debug.LogError($"Error! {move.GetName()} has no AnimationData in GetAnimationTimes!");
            }
        }

        // First iteration through defensive moves
        foreach (Move move in hit.GetDefensiveMoves())
        {
            AnimationData animationData = move.GetAnimationData();

            if (animationData != null)
            {
                float duration = animationData.GetDuration();
                if (duration > maxDefenseDuration)
                {
                    maxDefenseDuration = duration;
                }

                float impactTime = animationData.GetImpactTime();
                if (impactTime > maxImpactTime)
                {
                    maxImpactTime = impactTime;
                }
            }
            else
            {
                Debug.LogError($"Error! {move.GetName()} has no AnimationData in GetAnimationTimes!");
            }
        }

        // Second iteration through attacking moves
        foreach (Move move in GetMoves())
        {
            AnimationData animationData = move.GetAnimationData();
            if (animationData != null)
            {
                float delay = maxImpactTime - animationData.GetImpactTime();
                attackDelays.Add(delay);
            }
        }

        // Second iteration through defensive moves
        foreach (Move move in hit.GetDefensiveMoves())
        {
            AnimationData animationData = move.GetAnimationData();
            if (animationData != null)
            {
                float delay = maxImpactTime - animationData.GetImpactTime();
                defenseDelays.Add(delay);
            }
        }
        
        result.SetImpactTime(maxImpactTime);
        result.SetMaxAttackDuration(maxAttackDuration);
        result.SetMaxDefenseDuration(maxDefenseDuration);
        result.SetAttackDelays(attackDelays);
        result.SetDefenseDelays(defenseDelays);

        return result;
    }

    public float GetEffectiveMoveEventCastingSpeed() 
    {
        if (EffectiveMoveEventCastingSpeed == 0f) // If EffectiveMoveEventSpeed has not been set
        {
            EffectiveMoveEventCastingSpeed = GetMoveEventCastingSpeed();
            return EffectiveMoveEventCastingSpeed;
        }
        return EffectiveMoveEventCastingSpeed; 
    }

    //public static float GetMeleeMoveCastingSpeed(Fighter fighter, Move move, float randomAdd) { return (.5f * fighter.GetSpeed() * fighter.GetHealthCo() + .5f * fighter.GetCastingSpeed(move) + randomAdd); }
    public Enums.MoveType GetMoveType() { return MoveType; }
    public List<Move> GetMoves() { return Moves; }

    public float GetMoveEventCastingSpeed()
    {
        if (MoveType == Enums.MoveType.Protect || MoveType == Enums.MoveType.Skip)    // These are handled separately when assembling a round's move event list.
        {
            return 0f;
        }
        
        List<Move> moves = GetMoves();
        int movesCount = moves.Count;

        if (movesCount == 1)
        {
            Fighter fighter = Fighters[0];
            Move move = moves[0];
            float randomAdd = RandomAdds[0];

            return fighter.GetCastingSpeed(move, randomAdd);
        }

        float minMoveSpeed = float.MaxValue;    // When there's more than 1 move they all attack with the slowest.

        for (int index = 0; index < movesCount; ++index)
        {
            Fighter fighter = Fighters[index];
            Move move = moves[index];
            float randomAdd = RandomAdds[index];
            float moveSpeed = fighter.GetCastingSpeed(move, randomAdd);

            if (moveSpeed < minMoveSpeed)
            {
                minMoveSpeed = moveSpeed;
            }
        }
        
        return minMoveSpeed;
    }

    public List<Fighter> GetFighters() { return Fighters; }
    public List<float> GetRandomAdds() { return RandomAdds; }
    public List<Fighter> GetTargets() { return Targets; }
    public int GetTargetTeam() { return TargetTeam; }
    public Enums.TargetType GetTargetType() { return TargetType; }
    public void SetEffectiveMoveEventCastingSpeed(float moveEventCastingSpeed) { EffectiveMoveEventCastingSpeed = moveEventCastingSpeed; }
    public void SetMoveType(Enums.MoveType moveType) { MoveType = moveType; }
    public void SetTargetTeam(int targetTeam) { TargetTeam = targetTeam; }
    public void SetTargetType(Enums.TargetType targetType) { TargetType = targetType; }
}
