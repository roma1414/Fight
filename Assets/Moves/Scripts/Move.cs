using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Move", menuName = "Assets/Moves/New Move")]
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

    [Header("CloneMove Fields")]
    [SerializeField] protected int                  Number;
    [SerializeField] protected float                CloneStrength;

    [Header("DefensiveMove Fields")]
    [SerializeField] protected int                  UsesPerRound;
    [SerializeField] protected int                  MaxLevelAvoided;
    [SerializeField] protected bool                 WorksAgaintsMelee;
    [SerializeField] protected bool                 WorksAgainstRanged;
    [SerializeField] protected bool                 Absorbing;
    [SerializeField] protected bool                 OccularSuccess;
    [SerializeField] protected bool                 TouchFail;

    [Header("Medical Move Fields")]
    [SerializeField] protected int                  HealthAmount;
    [SerializeField] protected int                  ManaRestoreAmount;
    [SerializeField] protected Enums.HealType       HealType;

    [Header("Offensive Move Fields")]
    [SerializeField] protected int                  Damage;
    [SerializeField] protected int                  Duration;
    [SerializeField] protected float                Accuracy;
    [SerializeField] protected bool                 Absorbed;
    [SerializeField] protected bool                 TouchSuccess;
    [SerializeField] protected Enums.DamageType     DamageType;
    [SerializeField] protected Enums.StatusType[]   StatusTypes;

    [Header("Power Up Move Fields")]
    [SerializeField] protected BonusData            BonusData;

    [Header("Summon Move Fields")]
    [SerializeField] protected Fighter[]  Summons;
    
    public bool CheckAbsorbed() { return Absorbed; }
    public bool CheckAbsorbing() { return Absorbing; }
    public bool CheckIfWorksAgainstMelee() { return WorksAgaintsMelee; }
    public bool CheckIfWorksAgainstRanged() { return WorksAgainstRanged; }
    public bool CheckOccularSuccess() { return OccularSuccess; }

    public bool CheckOffensive()
    { 
        bool result = false;
        switch (MoveType)
        {
            case Enums.MoveType.Melee:
            case Enums.MoveType.Spell:
            case Enums.MoveType.Psychic:
            case Enums.MoveType.NinTai:
            case Enums.MoveType.Projectile:
            case Enums.MoveType.Offensive:
            result = true;
            break;
        }

        return result;
    }

    public bool CheckNature(Enums.Nature nature) { return GetNaturesList().Contains(nature); }
    public bool CheckOccular() { return Occular; }
    public bool CheckTouchFail() { return TouchFail; }
    public bool CheckTouchSuccess() { return TouchSuccess; }
    public float GetAccuracy() { return Accuracy; }
    public MoveAnimations GetAnimations() { return MoveAnimations; }
    public BonusData GetBonusData() { return BonusData; }
    public float GetCastingSpeed() { return CastingSpeed; }
    public float GetCloneStrength() { return CloneStrength; }
    public int GetDamage() { return Damage; }
    public Enums.DamageType GetDamageType() { return DamageType; }
    public int GetDuration() { return Duration; }
    public int GetHealthAmount() { return HealthAmount; }
    public Enums.HealType GetHealType() { return HealType; }
    public ulong GetID() { return ID; }
    public int GetLevel() { return Level; }
    public int GetMana() { return Mana; }
    public int GetManaRestoreAmount() { return ManaRestoreAmount; }
    public int GetMaxLevelAvoided() { return MaxLevelAvoided; }
    public int GetNumber() { return Number; }
    public string GetName() { return Name; }
    public Enums.Nature[] GetNaturesArray() { return Natures; }
    public List<Enums.Nature> GetNaturesList() { return new List<Enums.Nature>(Natures); }
    public Enums.TargetType GetTargetType() { return TargetType; }
    public Enums.MoveType GetMoveType() { return MoveType; }
    public Enums.StatusType[] GetRequiredTargetStatuses() { return RequiredTargetStatuses; }
    public List<Enums.StatusType> GetRequiredTargetStatusesList() { return new List<Enums.StatusType>(RequiredTargetStatuses); }
    public Enums.StatusType[] GetStatusTypes() { return StatusTypes; }
    public Fighter[] GetSummons() { return Summons; }
    public bool GetUseMeleeSkill() { return UseMeleSkill; }
    public int GetUsesPerFight() { return UsesPerFight; }
    public int GetUsesPerRound() { return UsesPerRound; }
}

