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

    public Sprite GetBackground(int team) 
    { 
        switch (team)
        {
            case 1:
                return Background_1;
            case 2:
                return Background_2;
            case 3:
                return Background_3;
            default:
                Debug.LogError($"Unknown team {team} in GetBackground!");
                break;
        }

        return null;
    }
}