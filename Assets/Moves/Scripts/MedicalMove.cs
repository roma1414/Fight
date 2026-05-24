using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New MedicalMove", menuName = "Assets/Moves/New MedicalMove")]
public class MedicalMove : Move
{
    [Header("MedicalMove Fields")]
    [SerializeField] protected int                  HealthAmount;
    [SerializeField] protected int                  ManaRestoreAmount;
    [SerializeField] protected int                  Duration;
    [SerializeField] protected Enums.HealType       HealType;
    [SerializeField] protected Enums.StatusType[]   StatusTypes;

    public int GetManaRestoreAmount() { return ManaRestoreAmount; }
    public int GetDuration() { return Duration; }
    public int GetHealthAmount() { return HealthAmount; }
    public Enums.HealType GetHealType() { return HealType; }
    public Enums.StatusType[] GetStatusTypes() { return StatusTypes; }
}
