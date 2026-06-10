using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Substitution
{
    protected List<Fighter> mEnemiesTricked;
    protected Fighter       mFighter;
    protected float         mPower;
    protected Move          mSubMove;

    public List<Fighter> GetEnemiesTricked() { return mEnemiesTricked; }
    public Fighter GetFighter() { return mFighter; }
    public float GetPower() { return mPower; }
    public Move GetSubMove() { return mSubMove; }
    public void SetEnemiesTricked(List<Fighter> enemiesTricked) { mEnemiesTricked = enemiesTricked; }
    public void SetFighter(Fighter fighter) { mFighter = fighter; }
    public void SetPower(float power) { mPower = power; }
    public void SetSubMove(Move subMove) { mSubMove = subMove; }
}
