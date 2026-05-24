using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SummonMove", menuName = "Assets/Moves/New SummonMove")]
public class SummonMove : Move
{
    [Header("SummonMove Fields")]
    [SerializeField] protected int      Duration;
    [SerializeField] protected Fighter[]  Summons;

    public int GetDuration() { return Duration; }
    public Fighter[] GetSummons() { return Summons; }
}
