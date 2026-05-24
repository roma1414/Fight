using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Clothing", menuName = "Assets/Items/New Clothing (Unused)")]
public class Clothing : Item
{
    [Header("Clothing Fields")]
    [SerializeField] protected Enums.EquipSlot  EquipSlot;
    [SerializeField] protected BonusData        BonusData;
}
