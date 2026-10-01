using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New DialogueData", menuName = "Assets/AI/New DialogueData")]
public class DialogueData : ScriptableObject
{
    [SerializeField] protected string[] AttackingStatements, AngryStatements, BlockedStatements, DeflectedStatements,
                                        HurtStatements, TooFastStatements, VictoryStatements;
    [SerializeField] protected bool LoopDialogue = true;
    protected HashSet<int> SpokenAttackingStatements = new HashSet<int>();
    protected HashSet<int> SpokenAngryStatements = new HashSet<int>();
    protected HashSet<int> SpokenBlockedStatements = new HashSet<int>();
    protected HashSet<int> SpokenDeflectedStatements = new HashSet<int>();
    protected HashSet<int> SpokenHurtStatements = new HashSet<int>();
    protected HashSet<int> SpokenTooFastStatements = new HashSet<int>();
    protected List<int> SpokenVictoryStatements = new List<int>();

    public string GetAttackingStatement()
    {
        if (AttackingStatements.Length == 0)
        {
            return null;
        }

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < AttackingStatements.Length; i++)
        {
            if (!SpokenAttackingStatements.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            int statementIndex = availableIndices[randomIndex];
            SpokenAttackingStatements.Add(statementIndex);
            return AttackingStatements[statementIndex];
        }
        else
        {
            // All statements have been spoken, reset the spoken set and choose again
            if (LoopDialogue == true)
            {
                SpokenAttackingStatements.Clear();
                return GetAttackingStatement();
            }
        }

        return "";
    }

    public string GetTooFastStatement()
    {
        if (TooFastStatements.Length == 0)
        {
            return null;
        }

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < TooFastStatements.Length; i++)
        {
            if (!SpokenTooFastStatements.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            int statementIndex = availableIndices[randomIndex];
            SpokenTooFastStatements.Add(statementIndex);
            return TooFastStatements[statementIndex];
        }
        else
        {
            // All statements have been spoken, reset the spoken set and choose again
            if (LoopDialogue == true)
            {
                SpokenTooFastStatements.Clear();
                return GetTooFastStatement();
            }
        }

        return "";
    }

    public string GetStatement(Enums.StatementType statementType)
    {
        string[] statementArray;
        HashSet<int> spokenSet;
        switch (statementType)
        {
            case Enums.StatementType.Attacking:
                statementArray = AttackingStatements;
                spokenSet = SpokenAttackingStatements;
                break;
            case Enums.StatementType.TooFast:
                statementArray = TooFastStatements;
                spokenSet = SpokenTooFastStatements;
                break;
            default:
                Debug.LogError($"Error! Unknown StatementType {statementType} in GetStatement!");
                return null;
        }

        if (statementArray.Length == 0)
        {
            return null;
        }

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < statementArray.Length; i++)
        {
            if (!SpokenTooFastStatements.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            int statementIndex = availableIndices[randomIndex];
            spokenSet.Add(statementIndex);
            return statementArray[statementIndex];
        }
        else
        {
            // All statements have been spoken, reset the spoken set and choose again
            if (LoopDialogue == true)
            {
                spokenSet.Clear();
                return GetStatement(statementType);
            }
        }

        return null;
    }
}
