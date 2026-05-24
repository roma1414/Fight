using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensiveAction
{
    protected Fighter       Defender;
    protected DefensiveMove DefensiveMove;
    protected float         Power;

    public Fighter GetDefender() { return Defender; }
    public DefensiveMove GetDefensiveMove() { return DefensiveMove; }
    public float GetPower() { return Power; }
    public void SetDefender(Fighter defender) { Defender = defender; }
    public void SetDefensiveMove(DefensiveMove defensiveMove) { DefensiveMove = defensiveMove; }
    public void SetPower(float power) { Power = power; }
}
