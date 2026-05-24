using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit
{
    protected int                   mDamage;
    protected List<Fighter>         mDefenders;
    protected List<DefensiveMove>   mDefensiveMoves;
    protected Enums.HitResult       mResult;
    protected bool                  mWasEasy;
    protected bool                  mWasProtected;

    public void AddDefender(Fighter defender) { mDefenders.Add(defender); }
    public void AddDefensiveMove(DefensiveMove defensiveMove) { mDefensiveMoves.Add(defensiveMove); }
    public bool CheckWasEasy() { return mWasEasy; }
    public bool CheckWasProtected() { return mWasProtected; }
    public int GetDamage() { return mDamage; }
    public List<DefensiveMove> GetDefensiveMoves() { return mDefensiveMoves; }

    public List<Move> GetMoves()
    {
        List<Move> moves = new List<Move>();
        foreach (DefensiveMove defensiveMove in mDefensiveMoves)
        {
            moves.Add(defensiveMove);
        }

        return moves;
    }

    public List<Fighter> GetDefenders() { return mDefenders; }
    public Enums.HitResult GetResult() { return mResult; }
    public void SetDamage(int damage) { mDamage = damage; }
    public void SetDefenders(List<Fighter> defenders) { mDefenders = defenders; }
    public void SetDefensiveMoves(List<DefensiveMove> defensiveMoves) { mDefensiveMoves = defensiveMoves; }
    public void SetResult(Enums.HitResult result) { mResult = result; }
    public void SetWasEasy(bool wasEasy) { mWasEasy = wasEasy; }
    public void SetWasProtected(bool wasProtected) { mWasProtected = wasProtected; }
}
