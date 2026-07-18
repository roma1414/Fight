using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fighter Art", menuName = "Assets/Fighters/New Fighter Art")]
public class FighterArt : ScriptableObject
{
    [SerializeField]
    protected Texture2D Portrait;

    public Texture2D GetPortrait() { return Portrait; }
}
