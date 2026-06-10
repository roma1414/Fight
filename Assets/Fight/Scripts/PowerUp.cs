using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp
{
    protected int           EndingRoundNumber;
    protected Fighter       Fighter;
    protected Move          PowerUpMove;

    public int GetEndingRoundNumber() { return EndingRoundNumber; }
    public Fighter GetFighter() { return Fighter; }
    public Move GetPowerUpMove() { return PowerUpMove; }
    public void SetEndingRoundNumber(int endingRoundNumber) { EndingRoundNumber = endingRoundNumber; }
    public void SetFighter(Fighter fighter) { Fighter = fighter; }
    public void SetPowerUpMove(Move powerUpMove) { PowerUpMove = powerUpMove; }
}
