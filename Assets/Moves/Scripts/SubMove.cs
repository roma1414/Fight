using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SubMove", menuName = "Assets/Moves/New SubMove")]
public class SubMove : Move
{
    [Header("SubMove Fields")]
    [SerializeField] protected int                  Damage;
    [SerializeField] protected int                  Duration;
    [SerializeField] protected Enums.DamageType     DamageType;
    [SerializeField] protected Enums.StatusType[]   StatusTypes;

    public int GetDamage() { return Damage; }
    public Enums.DamageType GetDamageType() { return DamageType; }
    public int GetDuration() { return Duration; }
    public Enums.StatusType[] GetStatusTypes() { return StatusTypes; }
}
