using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Statement", menuName = "Assets/AI/New Statement")]
public class Statement : ScriptableObject
{
    [SerializeField]
    protected string Text;

    public string GetText() { return Text; }
}