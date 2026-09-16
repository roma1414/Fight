using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Talking Frames", menuName = "Assets/Fighters/New Talking Frames")]
public class TalkingFrames : ScriptableObject
{
    [SerializeField] private Sprite[] Frames;
    
    public Sprite[] GetFrames() { return Frames; }
}
