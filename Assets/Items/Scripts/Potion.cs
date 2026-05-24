using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Potion", menuName = "Assets/Items/New Potion")]
public class Potion : Item
{
    [Header("Potion Fields")]
    [SerializeField] protected BonusData    BonusData;
    [SerializeField] protected int          Duration;
}
