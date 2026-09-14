using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fighter Art", menuName = "Assets/Fighters/New Fighter Art")]
public class FighterArt : ScriptableObject
{
    [SerializeField]
    protected Sprite        Body;
    [SerializeField]
    protected Sprite        Back;
    [SerializeField]
    protected List<Sprite>  TalkingSprites;
    [SerializeField]
    protected Sprite        TalkingAngry;
    protected int           PreviousTalkingSprite = -1;

    public Sprite GetBack() { return Back; }
    public Sprite GetBody() { return Body; }
    public Sprite GetTalkingAngry() { return TalkingAngry; }

    public Sprite GetTalkingSprite()
    {
        if (TalkingSprites.Count == 0)
        {
            return null;
        }

        List<Sprite> possibleSprites = new List<Sprite>();
        for (int i = 0; i < TalkingSprites.Count; i++)
        {
            if (i != PreviousTalkingSprite)
            {
                possibleSprites.Add(TalkingSprites[i]);
            }
        }
        
        if (possibleSprites.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleSprites.Count);
            Sprite selectedSprite = possibleSprites[randomIndex];
            PreviousTalkingSprite = TalkingSprites.IndexOf(selectedSprite);
            return selectedSprite;
        }
        
        return TalkingSprites[0];
    }

    public List<Sprite> GetTalkingSprites() { return TalkingSprites; }
}
