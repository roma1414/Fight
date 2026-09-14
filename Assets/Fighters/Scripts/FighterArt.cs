using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fighter Art", menuName = "Assets/Fighters/New Fighter Art")]
public class FighterArt : ScriptableObject
{
    [SerializeField]
    protected Sprite Body;

    public Sprite GetBody() { return Body; }
}
