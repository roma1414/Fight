using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Animation Frames", menuName = "Assets/Fighters/New Animation Frames")]
public class AnimationFrames : ScriptableObject
{
    [SerializeField] private Sprite[] Frames;
    
    public Sprite[] GetFrames() { return Frames; }
}
