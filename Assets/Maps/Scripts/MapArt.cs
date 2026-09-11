using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Map Art", menuName = "Assets/Maps/New Map Art")]
public class MapArt : ScriptableObject
{
    [SerializeField]
    protected Sprite Background_1;
    [SerializeField]
    protected Sprite Background_2;
    [SerializeField]
    protected Sprite Background_3;

    public Sprite GetBackground_1() { return Background_1; }
    public Sprite GetBackground_2() { return Background_2; }
    public Sprite GetBackground_3() { return Background_3; }
}