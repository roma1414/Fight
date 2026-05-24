using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CloneMove", menuName = "Assets/Moves/New CloneMove")]
public class CloneMove : Move
{
    [Header("CloneMove Fields")]
    [SerializeField] protected int      Number;
    [SerializeField] protected int      Duration;
    [SerializeField] protected float    CloneStrength;

    public int GetDuration() { return Duration; }
    public int GetNumber() { return Number; }
    public float GetCloneStrength() { return CloneStrength; }
}
