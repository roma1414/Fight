using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New DialogueData", menuName = "Assets/AI/New DialogueData")]
public class DialogueData : ScriptableObject
{
    [SerializeField] protected List<string> AttackingStatements, AngryStatements, BlockedStatements, DeflectedStatements, HurtStatements, TauntingStatements, VictoryStatements;
    [SerializeField] protected List<Statement> StatementsWithRequirements;
    protected HashSet<int> SpokenAttackingStatements = new HashSet<int>();
    protected HashSet<int> SpokenAngryStatements = new HashSet<int>();
    protected HashSet<int> SpokenBlockedStatements = new HashSet<int>();
    protected HashSet<int> SpokenDeflectedStatements = new HashSet<int>();
    protected HashSet<int> SpokenHurtStatements = new HashSet<int>();
    protected HashSet<int> SpokenTauntingStatements = new HashSet<int>();
    protected List<int> SpokenVictoryStatements = new List<int>();

    public string GetAttackingStatement()
    {
        if (AttackingStatements.Count == 0)
        {
            return null;
        }

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < AttackingStatements.Count; i++)
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
            SpokenAttackingStatements.Clear();
            return GetAttackingStatement();
        }
    }
}
