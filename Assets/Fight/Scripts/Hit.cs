using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit
{
    protected int                   Damage;
    protected List<Fighter>         Defenders = new List<Fighter>();
    protected List<Move>            DefensiveMoves = new List<Move>();
    protected Enums.HitResult       Result;
    protected bool                  WasEasy;
    protected bool                  WasProtected;

    public void AddDefender(Fighter defender) { Defenders.Add(defender); }
    public void AddDefensiveMove(Move defensiveMove) { DefensiveMoves.Add(defensiveMove); }

    public bool CheckFlightAvoided()
    {
        if (DefensiveMoves.Count == 1 && DefensiveMoves[0].GetNaturesList().Contains(Enums.Nature.Flight))
        {
            return true;
        }
        return false;
    }

    public bool CheckWasEasy() { return WasEasy; }
    public bool CheckWasProtected() { return WasProtected; }
    public int GetDamage() { return Damage; }
    public List<Move> GetDefensiveMoves() { return DefensiveMoves; }
    public List<Fighter> GetDefenders() { return Defenders; }
    public Enums.HitResult GetResult() { return Result; }
    public void SetDamage(int damage) { Damage = damage; }
    public void SetDefenders(List<Fighter> defenders) { Defenders = defenders; }
    public void SetDefensiveMoves(List<Move> defensiveMoves) { DefensiveMoves = defensiveMoves; }
    public void SetResult(Enums.HitResult result) { Result = result; }
    public void SetWasEasy(bool wasEasy) { WasEasy = wasEasy; }
    public void SetWasProtected(bool wasProtected) { WasProtected = wasProtected; }
}
