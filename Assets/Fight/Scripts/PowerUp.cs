using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp
{
    protected int           EndingRoundNumber;
    protected Fighter       Fighter;
    protected PowerUpMove   PowerUpMove;

    public int GetEndingRoundNumber() { return EndingRoundNumber; }
    public Fighter GetFighter() { return Fighter; }
    public PowerUpMove GetPowerUpMove() { return PowerUpMove; }
    public void SetEndingRoundNumber(int endingRoundNumber) { EndingRoundNumber = endingRoundNumber; }
    public void SetFighter(Fighter fighter) { Fighter = fighter; }
    public void SetPowerUpMove(PowerUpMove powerUpMove) { PowerUpMove = powerUpMove; }
}
