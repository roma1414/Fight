using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fighter Art", menuName = "Assets/Fighters/New Fighter Art")]
public class FighterArt : ScriptableObject
{
    [SerializeField] protected Sprite           Body;
    [SerializeField] protected Sprite           Back;
    [SerializeField] protected Sprite           TalkingAngry;
    [SerializeField] protected TalkingFrames[]  TalkingFrameList;
    protected int                               PreviousTalkingFrames = -1;

    public Sprite GetBack() { return Back; }
    public Sprite GetBody() { return Body; }
    public Sprite GetTalkingAngry() { return TalkingAngry; }

    public TalkingFrames GetTalkingFrames()
    {
        if (TalkingFrameList.Length == 0)
        {
            return null;
        }

        List<TalkingFrames> possibleSpriteFrames = new List<TalkingFrames>();
        for (int i = 0; i < TalkingFrameList.Length; i++)
        {
            if (i != PreviousTalkingFrames)
            {
                possibleSpriteFrames.Add(TalkingFrameList[i]);
            }
        }
        
        if (possibleSpriteFrames.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleSpriteFrames.Count);
            TalkingFrames selectedSpriteFrames = possibleSpriteFrames[randomIndex];
            PreviousTalkingFrames = System.Array.IndexOf(TalkingFrameList, selectedSpriteFrames);
            return selectedSpriteFrames;
        }
        
        return TalkingFrameList[0];
    }
}
