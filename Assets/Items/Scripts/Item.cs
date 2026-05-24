using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Assets/Moves/New Item (Unused)")]
public class Item : ScriptableObject
{
    [Header("Item Fields")]
    protected Enums.Rarity Rarity;
    protected int Value;
}