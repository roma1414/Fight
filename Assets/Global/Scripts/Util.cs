using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static string DamageString(int damage, Enums.DamageType damageType, Fighter target, List<Fighter> fighters, bool includeIt)
    {
        string printString = "";
        
        switch (damageType)
        {
            case Enums.DamageType.Health:
                if (includeIt == true)
                {
                    printString += " and it does " + damage + " damage!";
                }
                else
                {
                    printString += " and does " + damage + " damage!";
                }
                break;
            case Enums.DamageType.AbsorbMana:
                if (includeIt == true)
                {
                    printString += " and it absorbs " + damage + " mana!";
                }
                else
                {
                    printString += " and absorbs " + damage + " mana!";
                }
                break;
            case Enums.DamageType.Mana:
                if (includeIt == true)
                {
                    printString += " and it does " + damage + " mana damage!";
                }
                else
                {
                    printString += " and does " + damage + " mana damage!";
                }
                break;
            case Enums.DamageType.AbsorbHealth:
                if (includeIt == true)
                {
                    printString += " and it absorbs " + damage + " health!";
                }
                else
                {
                    printString += " and absorbs " + damage + " health!";
                }
                break;
        }

        return printString;
    }
    
    public static string EnumToText(Enums.StatusType statusType)
    {
        switch (statusType)
        {
            case Enums.StatusType.Trapped:
                return "Trapped";
            case Enums.StatusType.PsychicControl:
                return "Psychic Control";
            case Enums.StatusType.PsychicParalysis:
                return "Psychic Paralysis";
            case Enums.StatusType.Cooldown:
                return "Cooldown";
            case Enums.StatusType.TeleportationMarked:
                return "Teleportation Marked";
            case Enums.StatusType.HidanMarked:
                return "Hidan Marked";
        }

        Debug.LogError("Error! Unexpected status [" + statusType + "] in Util.EnumToText(Enums.StatusType)!");
        return "?";
    }

    public static string ListString(List<string> stringList)
    {
        int stringListCount = stringList.Count;
        string printString = "";

        if (stringListCount == 1)
        {
            return stringList[0];
        }
        else
        {
            if (stringListCount == 2)
            {
                printString += stringList[0] + " and " + stringList[1];
            }
            else
            {
                for (int index = 0; index < stringListCount; index++)
                {
                    if (index == stringListCount - 1)
                    {
                        printString += "and " + stringList[index];
                    }
                    else
                    {
                        printString += stringList[index] + ", ";
                    }
                }
            }
        }

        return printString;
    }

    public static string ListString(List<Fighter> fighters)
    {
        List<string> names = new List<string>();

        foreach (Fighter fighter in fighters)
        {
            names.Add(fighter.GetName());
        }

        return ListString(names);
    }

    public static string ListString(List<Move> moves)
    {
        List<string> names = new List<string>();

        foreach (Move move in moves)
        {
            names.Add(move.GetName());
        }

        return ListString(names);
    }
}
