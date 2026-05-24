using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New OffensiveMove", menuName = "Assets/Moves/New OffensiveMove")]
public class OffensiveMove : Move
{
    [Header("OffensiveMove Fields")]
    [SerializeField] protected int                  Damage;
    [SerializeField] protected int                  Duration;
    [SerializeField] protected float                Accuracy;
    [SerializeField] protected bool                 Absorbed;
    [SerializeField] protected bool                 TouchSuccess;
    [SerializeField] protected Enums.DamageType     DamageType;
    [SerializeField] protected Enums.StatusType[]   StatusTypes;

    public bool CheckAbsorbed() { return Absorbed; }
    public bool CheckTouchSuccess() { return TouchSuccess; }
    public float GetAccuracy() { return Accuracy; }
    public int GetDamage() { return Damage; }
    public Enums.DamageType GetDamageType() { return DamageType; }
    public int GetDuration() { return Duration; }
    public Enums.StatusType[] GetStatusTypes() { return StatusTypes; }
}
