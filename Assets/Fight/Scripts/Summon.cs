using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon
{
    protected int           mEndingRoundNumber;
    protected Fighter       mSummoned, mSummoner;
    protected SummonMove    mSummonMove;

    public int GetEndingRoundNumber() { return mEndingRoundNumber; }
    public Fighter GetSummoned() { return mSummoned; }
    public Fighter GetSummoner() { return mSummoner; }
    public SummonMove GetSummonMove() { return mSummonMove; }
    public void SetEndingRoundNumber(int endingRoundNumber) { mEndingRoundNumber = endingRoundNumber; }
    public void SetSummoned(Fighter summoned) { mSummoned = summoned; }
    public void SetSummoner(Fighter summoner) { mSummoner = summoner; }
    public void SetSummonMove(SummonMove summonMove) { mSummonMove = summonMove; }
}
