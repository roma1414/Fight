using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status
{
    protected int               EndingRoundNumber;
    protected Move              Move;
    protected Fighter           Fighter;
    protected float             Power;
    protected Enums.StatusType  StatusType;

    public int GetEndingRoundNumber() { return EndingRoundNumber; }
    public Move GetMove() { return Move; }
    public Fighter GetFighter() { return Fighter; }
    public float GetPower() { return Power; }
    public Enums.StatusType GetStatusType() { return StatusType; }
    public void SetEndingRoundNumber(int endingRoundNumber) { EndingRoundNumber = endingRoundNumber; }
    public void SetMove(Move move) { Move = move; }
    public void SetFighter(Fighter fighter) { Fighter = fighter; }
    public void SetPower(float power) { Power = power; }
    public void SetStatusType(Enums.StatusType statusToRemove) { StatusType = statusToRemove; }
}
