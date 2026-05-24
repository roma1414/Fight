using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEvent
{
    protected List<CloneMove>       mCloneMoves;
    protected float                 mEffectiveMoveEventCastingSpeed = 0f;
    protected List<MedicalMove>     mMedicalMoves;
    protected Enums.MoveType        mMoveType;
    protected List<Fighter>         mFighters;
    protected List<OffensiveMove>   mOffensiveMoves;
    protected List<PowerUpMove>     mPowerUpMoves;
    protected List<float>           mRandomAdds;
    protected List<SubMove>         mSubMoves;
    protected List<Fighter>         mTargets;
    protected int                   mTargetTeam;
    protected Enums.TargetType      mTargetType;
    protected List<SummonMove>      mSummonMoves;

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public MoveEvent()
    {
        mCloneMoves = new List<CloneMove>();
        mMedicalMoves = new List<MedicalMove>();
        mMoveType = Enums.MoveType.Skip;
        mFighters = new List<Fighter>();
        mOffensiveMoves = new List<OffensiveMove>();
        mPowerUpMoves = new List<PowerUpMove>();
        mRandomAdds = new List<float>();
        mSubMoves = new List<SubMove>();
        mTargets = new List<Fighter>();
        mSummonMoves = new List<SummonMove>();
    }

    public void AddMove(CloneMove cloneMove) { mCloneMoves.Add(cloneMove); }
    public void AddMove(MedicalMove medicalMove) { mMedicalMoves.Add(medicalMove); }
    public void AddMove(OffensiveMove offensiveMove) { mOffensiveMoves.Add(offensiveMove); }
    public void AddMove(PowerUpMove powerUpMove) { mPowerUpMoves.Add(powerUpMove); }
    public void AddMove(SubMove subMove) { mSubMoves.Add(subMove); }
    public void AddMove(SummonMove summonMove) { mSummonMoves.Add(summonMove); }
    public void AddFighter(Fighter fighter) { mFighters.Add(fighter); }
    public void AddRandomAdd(float randomAdd) { mRandomAdds.Add(randomAdd); }
    public void AddTarget(Fighter target) { mTargets.Add(target); }

    public bool CheckCombineAttacks()
    {
        foreach (Fighter fighter in mFighters)
        {
            if (fighter.CheckCombineAttacks() == false)
            {
                return false;
            }
        }
        return true;
    }

    public List<CloneMove> GetCloneMoves() { return mCloneMoves; }

    public float GetEffectiveMoveEventCastingSpeed() 
    {
        if (mEffectiveMoveEventCastingSpeed == 0f) // If mEffectiveMoveEventSpeed has not been set
        {
            return GetMoveEventCastingSpeed();
        }
        return mEffectiveMoveEventCastingSpeed; 
    }

    public List<MedicalMove> GetMedicalMoves() { return mMedicalMoves; }
    //public static float GetMeleeMoveCastingSpeed(Fighter fighter, Move move, float randomAdd) { return (.5f * fighter.GetSpeed() * fighter.GetHealthCo() + .5f * fighter.GetCastingSpeed(move) + randomAdd); }
    public Enums.MoveType GetMoveType() { return mMoveType; }

    public List<Move> GetMoves()
    {
        List<Move> moves;
        
        switch (mMoveType)
        {
            case Enums.MoveType.Melee:
            case Enums.MoveType.Spell:
            case Enums.MoveType.Psychic:
            case Enums.MoveType.NinTai:
            case Enums.MoveType.Projectile:
                moves = new List<Move>(mOffensiveMoves);
                break;
            case Enums.MoveType.PowerUp:
                moves = new List<Move>(mPowerUpMoves);
                break;
            case Enums.MoveType.Medical:
                moves = new List<Move>(mMedicalMoves);
                break;
            case Enums.MoveType.Substitution:
                moves = new List<Move>(mSubMoves);
                break;
            case Enums.MoveType.Clone:
                moves = new List<Move>(mCloneMoves);
                break;
            case Enums.MoveType.Summon:
                moves = new List<Move>(mSummonMoves);
                break;
            default:
                Debug.LogError("Error! Unexpected move type [" + mMoveType + "] in MoveEvent.GetMoves!");
                moves = new List<Move>();
                break;
        }

        return moves;
    }

    public float GetMoveEventCastingSpeed()
    {
        if (mMoveType == Enums.MoveType.Protect || mMoveType == Enums.MoveType.Skip)    // These are handled separately when assembling a round's move event list.
        {
            return 0f;
        }
        
        List<Move> moves = GetMoves();
        int movesCount = moves.Count;

        if (movesCount == 1)
        {
            Fighter fighter = mFighters[0];
            Move move = moves[0];
            float randomAdd = mRandomAdds[0];

            return fighter.GetCastingSpeed(move, randomAdd);
        }

        float minMoveSpeed = float.MaxValue;    // When there's more than 1 move they all attack with the slowest.

        for (int index = 0; index < movesCount; ++index)
        {
            Fighter fighter = mFighters[index];
            Move move = moves[index];
            float randomAdd = mRandomAdds[index];

            float moveSpeed = fighter.GetCastingSpeed(move, randomAdd);

            if (moveSpeed < minMoveSpeed)
            {
                minMoveSpeed = moveSpeed;
            }
        }
        
        return minMoveSpeed;
    }

    public List<Fighter> GetFighters() { return mFighters; }
    public List<OffensiveMove> GetOffensiveMoves() { return mOffensiveMoves; }
    public List<PowerUpMove> GetPowerUpMoves() { return mPowerUpMoves; }
    public List<float> GetRandomAdds() { return mRandomAdds; }
    public List<SubMove> GetSubMoves() { return mSubMoves; }
    public List<SummonMove> GetSummonMoves() { return mSummonMoves; }
    public List<Fighter> GetTargets() { return mTargets; }
    public int GetTargetTeam() { return mTargetTeam; }
    public Enums.TargetType GetTargetType() { return mTargetType; }
    public void SetEffectiveMoveEventCastingSpeed(float moveEventCastingSpeed) { mEffectiveMoveEventCastingSpeed = moveEventCastingSpeed; }
    public void SetMoveType(Enums.MoveType moveType) { mMoveType = moveType; }
    public void SetTargetTeam(int targetTeam) { mTargetTeam = targetTeam; }
    public void SetTargetType(Enums.TargetType targetType) { mTargetType = targetType; }
}
