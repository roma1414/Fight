using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Move", menuName = "Assets/Moves/New Move (Unused)")]
public class Move : ScriptableObject
{
    [Header("Move Fields")]
    [SerializeField] protected string               Name;
    [SerializeField] protected int                  Level;
    [SerializeField] protected int                  Mana;
    [SerializeField] protected int                  RoundsToCast;
    [SerializeField] protected int                  UsesPerFight;
    [SerializeField] protected float                CastingSpeed;
    [SerializeField] protected ulong                ID;
    [SerializeField] protected bool                 Occular, UseMeleSkill;
    [SerializeField] protected Enums.StatusType[]   RequiredTargetStatuses;
    [SerializeField] protected Enums.TargetType     TargetType;
    [SerializeField] protected Enums.MoveType       MoveType;
    [SerializeField] protected Enums.Nature[]       Natures;
    [SerializeField] protected MoveAnimations       MoveAnimations;

    public bool CheckNature(Enums.Nature nature) { return GetNaturesList().Contains(nature); }
    public bool CheckOccular() { return Occular; }
    public MoveAnimations GetAnimations() { return MoveAnimations; }
    public float GetCastingSpeed() { return CastingSpeed; }
    public ulong GetID() { return ID; }
    public int GetLevel() { return Level; }
    public int GetMana() { return Mana; }
    public string GetName() { return Name; }
    public Enums.Nature[] GetNaturesArray() { return Natures; }
    public List<Enums.Nature> GetNaturesList() { return new List<Enums.Nature>(Natures); }
    public Enums.TargetType GetTargetType() { return TargetType; }
    public Enums.MoveType GetMoveType() { return MoveType; }
    public Enums.StatusType[] GetRequiredTargetStatuses() { return RequiredTargetStatuses; }
    public List<Enums.StatusType> GetRequiredTargetStatusesList() { return new List<Enums.StatusType>(RequiredTargetStatuses); }
    public bool GetUseMeleeSkill() { return UseMeleSkill; }
    public int GetUsesPerFight() { return UsesPerFight; }
}

