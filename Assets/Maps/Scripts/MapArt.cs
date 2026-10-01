using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Map Art", menuName = "Assets/Maps/New Map Art")]
public class MapArt : ScriptableObject
{
    [SerializeField] protected Sprite               Background_1, Background_2, Background_3;
    [SerializeField] protected AnimationFrames[]    MotionFrameList;
    protected int                                   PreviousMotionFrames = -1;

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

    public AnimationFrames GetMotionFrames()
    {
        if (MotionFrameList.Length == 0)
        {
            return null;
        }

        List<AnimationFrames> possibleSpriteFrames = new List<AnimationFrames>();
        for (int i = 0; i < MotionFrameList.Length; i++)
        {
            if (i != PreviousMotionFrames)
            {
                possibleSpriteFrames.Add(MotionFrameList[i]);
            }
        }
        
        if (possibleSpriteFrames.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleSpriteFrames.Count);
            AnimationFrames selectedSpriteFrames = possibleSpriteFrames[randomIndex];
            PreviousMotionFrames = System.Array.IndexOf(MotionFrameList, selectedSpriteFrames);
            return selectedSpriteFrames;
        }
        
        return MotionFrameList[0];
    }
}