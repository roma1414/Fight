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

    [SerializeField] protected Move[]               Moves;

    [SerializeField] protected Enums.Nature[]       Natures;
    [SerializeField] protected Enums.Trait[]        Traits;

    public int GetMana() { return Mana; }
    public float GetDamageResistance() { return DamageResistance; }
    public float GetPsychic() { return Psychic; }
    public int GetHealth() { return Health; }
    public float GetIntelligence() { return Intelligence; }
    public float GetMelee() { return Melee; }
    public Move[] GetMoves() { return Moves; }
    public Enums.Nature[] GetNatures() { return Natures; }
    public float GetSpellcraft() { return Spellcraft; }
    public float GetSpeed() { return Speed; }
    public float GetStrength() { return Strength; }
    public Enums.Trait[] GetTraits() { return Traits; }
}
