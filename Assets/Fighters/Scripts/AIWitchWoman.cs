using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New WitchWomanAI", menuName = "Assets/AIs/New UchihaItachiAI")]
public class AIWitchWoman : AI
{
    public override MoveEvent GetCustomAIMoveEvent(Fight fight, Fighter fighter)
    {
        return GetStandardAIMoveEvent(fight, fighter);
    }
}
