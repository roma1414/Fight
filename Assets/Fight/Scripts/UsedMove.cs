using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsedMove
{
    protected Move      mMove;
    protected Fighter   mFighter;
    protected int       mRoundNumber;

    public Move GetMove() { return mMove; }
    public Fighter GetFighter() { return mFighter; }
    public int GetRoundNumber() { return mRoundNumber; }
    public void SetMove(Move move) { mMove = move; }
    public void SetFighter(Fighter fighter) { mFighter = fighter; }
    public void SetRoundNumber(int roundNumber) { mRoundNumber = roundNumber; }
}
