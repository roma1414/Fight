using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEvent
{
    protected List<CloneMove>       CloneMoves;
    protected float                 EffectiveMoveEventCastingSpeed = 0f;
    protected List<MedicalMove>     MedicalMoves;
    protected Enums.MoveType        MoveType;
    protected List<Fighter>         Fighters;
    protected List<OffensiveMove>   OffensiveMoves;
    protected List<PowerUpMove>     PowerUpMoves;
    protected List<float>           RandomAdds;
    protected List<SubMove>         SubMoves;
    protected List<Fighter>         Targets;
    protected int                   TargetTeam;
    protected Enums.TargetType      TargetType;
    protected List<SummonMove>      SummonMoves;

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public MoveEvent()
    {
        CloneMoves = new List<CloneMove>();
        MedicalMoves = new List<MedicalMove>();
        MoveType = Enums.MoveType.Skip;
        Fighters = new List<Fighter>();
        OffensiveMoves = new List<OffensiveMove>();
        PowerUpMoves = new List<PowerUpMove>();
        RandomAdds = new List<float>();
        SubMoves = new List<SubMove>();
        Targets = new List<Fighter>();
        SummonMoves = new List<SummonMove>();
    }

    public void AddMove(CloneMove cloneMove) { CloneMoves.Add(cloneMove); }
    public void AddMove(MedicalMove medicalMove) { MedicalMoves.Add(medicalMove); }
    public void AddMove(OffensiveMove offensiveMove) { OffensiveMoves.Add(offensiveMove); }
    public void AddMove(PowerUpMove powerUpMove) { PowerUpMoves.Add(powerUpMove); }
    public void AddMove(SubMove subMove) { SubMoves.Add(subMove); }
    public void AddMove(SummonMove summonMove) { SummonMoves.Add(summonMove); }
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

    public List<CloneMove> GetCloneMoves() { return CloneMoves; }

    public float GetEffectiveMoveEventCastingSpeed() 
    {
        if (EffectiveMoveEventCastingSpeed == 0f) // If EffectiveMoveEventSpeed has not been set
        {
            return GetMoveEventCastingSpeed();
        }
        return EffectiveMoveEventCastingSpeed; 
    }

    public List<MedicalMove> GetMedicalMoves() { return MedicalMoves; }
    //public static float GetMeleeMoveCastingSpeed(Fighter fighter, Move move, float randomAdd) { return (.5f * fighter.GetSpeed() * fighter.GetHealthCo() + .5f * fighter.GetCastingSpeed(move) + randomAdd); }
    public Enums.MoveType GetMoveType() { return MoveType; }

    public List<Move> GetMoves()
    {
        List<Move> moves;
        
        switch (MoveType)
        {
            case Enums.MoveType.Melee:
            case Enums.MoveType.Spell:
            case Enums.MoveType.Psychic:
            case Enums.MoveType.NinTai:
            case Enums.MoveType.Projectile:
                moves = new List<Move>(OffensiveMoves);
                break;
            case Enums.MoveType.PowerUp:
                moves = new List<Move>(PowerUpMoves);
                break;
            case Enums.MoveType.Medical:
                moves = new List<Move>(MedicalMoves);
                break;
            case Enums.MoveType.Substitution:
                moves = new List<Move>(SubMoves);
                break;
            case Enums.MoveType.Clone:
                moves = new List<Move>(CloneMoves);
                break;
            case Enums.MoveType.Summon:
                moves = new List<Move>(SummonMoves);
                break;
            default:
                Debug.LogError("Error! Unexpected move type [" + MoveType + "] in MoveEvent.GetMoves!");
                moves = new List<Move>();
                break;
        }

        return moves;
    }

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
    public List<OffensiveMove> GetOffensiveMoves() { return OffensiveMoves; }
    public List<PowerUpMove> GetPowerUpMoves() { return PowerUpMoves; }
    public List<float> GetRandomAdds() { return RandomAdds; }
    public List<SubMove> GetSubMoves() { return SubMoves; }
    public List<SummonMove> GetSummonMoves() { return SummonMoves; }
    public List<Fighter> GetTargets() { return Targets; }
    public int GetTargetTeam() { return TargetTeam; }
    public Enums.TargetType GetTargetType() { return TargetType; }
    public void SetEffectiveMoveEventCastingSpeed(float moveEventCastingSpeed) { EffectiveMoveEventCastingSpeed = moveEventCastingSpeed; }
    public void SetMoveType(Enums.MoveType moveType) { MoveType = moveType; }
    public void SetTargetTeam(int targetTeam) { TargetTeam = targetTeam; }
    public void SetTargetType(Enums.TargetType targetType) { TargetType = targetType; }
}
