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

    public float GetEffectiveMoveEventCastingSpeed() 
    {
        if (EffectiveMoveEventCastingSpeed == 0f) // If EffectiveMoveEventSpeed has not been set
        {
            return GetMoveEventCastingSpeed();
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
