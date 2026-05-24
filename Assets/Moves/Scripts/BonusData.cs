using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BonusData", menuName = "Assets/Items/New BonusData")]
public class BonusData : ScriptableObject
{
    [SerializeField] protected int                  Health;
    [SerializeField] protected int                  Mana;
    [SerializeField] protected float                Speed;
    [SerializeField] protected float                Strength;
    [SerializeField] protected float                Intelligence;
    [SerializeField] protected float                Spellcraft;
    [SerializeField] protected float                Melee;
    [SerializeField] protected float                Psychic;
    [SerializeField] protected float                DamageResistance;

    [SerializeField] protected OffensiveMove[]      OffensiveMoves;
    [SerializeField] protected DefensiveMove[]      DefensiveMoves;
    [SerializeField] protected CloneMove[]          CloneMoves;
    [SerializeField] protected MedicalMove[]        MedicalMoves;
    [SerializeField] protected PowerUpMove[]        PowerUpMoves;
    [SerializeField] protected SubMove[]            SubMoves;
    [SerializeField] protected SummonMove[]         SummonMoves;

    [SerializeField] protected Enums.Nature[]       Natures;
    [SerializeField] protected Enums.Trait[]        Traits;

    public int GetMana() { return Mana; }
    public CloneMove[] GetCloneMoves() { return CloneMoves; }
    public float GetDamageResistance() { return DamageResistance; }
    public DefensiveMove[] GetDefensiveMoves() { return DefensiveMoves; }
    public float GetPsychic() { return Psychic; }
    public int GetHealth() { return Health; }
    public float GetIntelligence() { return Intelligence; }
    public MedicalMove[] GetMedicalMoves() { return MedicalMoves; }
    public Enums.Nature[] GetNatures() { return Natures; }
    public float GetSpellcraft() { return Spellcraft; }
    public OffensiveMove[] GetOffensiveMoves() { return OffensiveMoves; }
    public PowerUpMove[] GetPowerUpMoves() { return PowerUpMoves; }
    public float GetSpeed() { return Speed; }
    public float GetStrength() { return Strength; }
    public SubMove[] GetSubMoves() { return SubMoves; }
    public SummonMove[] GetSummonMoves() { return SummonMoves; }
    public float GetMelee() { return Melee; }
    public Enums.Trait[] GetTraits() { return Traits; }
}
