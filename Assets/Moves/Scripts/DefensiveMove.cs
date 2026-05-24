using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DefensiveMove", menuName = "Assets/Moves/New DefensiveMove")]
public class DefensiveMove : Move
{
    [Header("DefensiveMove Fields")]
    [SerializeField] protected int  UsesPerRound;
    [SerializeField] protected int  MaxLevelAvoided;
    [SerializeField] protected bool WorksAgaintsMelee;
    [SerializeField] protected bool WorksAgainstRanged;
    [SerializeField] protected bool Absorbing;
    [SerializeField] protected bool OccularSuccess;
    [SerializeField] protected bool TouchFail;

    public bool CheckAbsorbing() { return Absorbing; }
    public bool CheckOccularSuccess() { return OccularSuccess; }
    public bool CheckTouchFail() { return TouchFail; }
    public bool CheckIfWorksAgainstMelee() { return WorksAgaintsMelee; }
    public bool CheckIfWorksAgainstRanged() { return WorksAgainstRanged; }
    public int GetMaxLevelAvoided() { return MaxLevelAvoided; }
    public int GetUsesPerRound() { return UsesPerRound; }
}
