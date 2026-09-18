using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AimationData", menuName = "Assets/Moves/New AnimationData")]
public class AnimationData : ScriptableObject
{
    [SerializeField] private Sprite[]   Frames;
    [SerializeField] float              FramesPerSecond;
    [SerializeField] int                ImpactFrame;

    public float GetDuration() 
    { 
        if (FramesPerSecond > 0)
        {
            return Frames.Length / FramesPerSecond; 
        }

        Debug.LogError("Error! FramesPerSecond = 0 in GetDuration!");
        return 0f;
    }

    public Sprite[] GetFrames() { return Frames; }
    public float GetFramesPerSecond() { return FramesPerSecond; }
    public int GetImpactFrame() { return ImpactFrame; }
}