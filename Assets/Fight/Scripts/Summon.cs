using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon
{
    protected int           mEndingRoundNumber;
    protected List<Fighter> mSummoned, mSummoners;
    protected Move          mMove;

    public int GetEndingRoundNumber() { return mEndingRoundNumber; }
    public Move GetMove() { return mMove; }
    public List<Fighter> GetSummoned() { return mSummoned; }
    public List<Fighter> GetSummoners() { return mSummoners; }
    public void SetEndingRoundNumber(int endingRoundNumber) { mEndingRoundNumber = endingRoundNumber; }
    public void SetMove(Move summonMove) { mMove = summonMove; }
    public void SetSummoned(List<Fighter> summoned) { mSummoned = summoned; }
    public void SetSummoners(List<Fighter> summoners) { mSummoners = summoners; }
}
