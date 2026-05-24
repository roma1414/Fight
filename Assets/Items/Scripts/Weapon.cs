using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Assets/Items/New Weapon")]
public class Weapon : ScriptableObject
{
    [Header("Weapon Fields")]
    [SerializeField] protected BonusData        BonusData;
    [SerializeField] protected Enums.EquipSlot  EquipSlot;
}
