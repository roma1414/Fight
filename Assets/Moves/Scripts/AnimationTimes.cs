using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTimes
{
    protected float                 MaxAttackDuration = 0f, ImpactTime = 0f, MaxDefenseDuration = 0f;
    protected List<float>           AttackDelays = new List<float>(), DefenseDelays = new List<float>();

    /*public AnimationTimes()
    {
        MoveType = Enums.MoveType.Skip;
        Fighters = new List<Fighter>();
        Moves = new List<Move>();
        AttackDelays = new List<float>();
        Targets = new List<Fighter>();
    }*/

    public List<float> GetAttackDelays() { return AttackDelays; }
    public List<float> GetDefenseDelays() { return DefenseDelays; }
    public float GetImpactTime() { return ImpactTime; }
    public float GetMaxAttackDuration() { return MaxAttackDuration; }
    public float GetMaxDefenseDuration() { return MaxDefenseDuration; }
    public void SetAttackDelays(List<float> attackDelays) { AttackDelays = attackDelays; }
    public void SetDefenseDelays(List<float> defenseDelays) { DefenseDelays = defenseDelays; }
    public void SetImpactTime(float impactTime) { ImpactTime = impactTime; }
    public void SetMaxAttackDuration(float maxAttackDuration) { MaxAttackDuration = maxAttackDuration; }
    public void SetMaxDefenseDuration(float maxDefenseDuration) { MaxDefenseDuration = maxDefenseDuration; }
}
