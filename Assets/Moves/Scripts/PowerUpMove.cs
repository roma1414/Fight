using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PowerUpMove", menuName = "Assets/Moves/New PowerUpMove")]
public class PowerUpMove : Move
{
    [Header("PowerUpMove Fields")]
    [SerializeField] protected int          Duration;
    [SerializeField] protected BonusData    BonusData;

    public BonusData GetBonusData() { return BonusData; }
    public int GetDuration() { return Duration; }
}
