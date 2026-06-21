using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    [SerializeField] protected string               Name;
    [SerializeField] protected float                Spellcraft, Melee, Psychic, Speed, Strength, Intelligence, DamageResistance;
    [SerializeField] protected int                  Mana, MaxMana, Health, Level, Team;
    [SerializeField] protected Enums.ControlType    ControlType;
    [SerializeField] protected Enums.FightingStyle  FightingStyle;
    [SerializeField] protected AI                   AI;
    [SerializeField] protected ulong                ID;
    [SerializeField] protected Images               Images;
    [SerializeField] protected Weapon               Weapon;
    [SerializeField] protected List<Enums.Nature>   Natures;
    [SerializeField] protected List<Enums.Trait>    Traits;

    [SerializeField] protected List<Move>           Moves;

    protected List<AttributeBonus>                  AttributeBonuses;
    protected List<Enums.Nature>                    BonusNatures;
    protected List<Enums.Trait>                     BonusTraits;
    protected List<Move>                            BonusMoves;

    protected List<PowerUp>                         PowerUps;      // Active PowerUps.
    protected List<Status>                          Statuses;
    protected List<Substitution>                    Subs;
    protected List<UsedMove>                        UsedMoves;

    public const float  CASTING_TRAIT_CASTING_SPEED_INCREASE = 1.5f;
    public const int    LOW_MANA_VALUE = 25;
    public const int    LOW_HEALTH_VALUE = 30;
    public const float  MIN_ATTRIBUTE_VALUE = 0f;
    public const float  MIN_DAMAGE_RESISTANCE_CO = 0.00001f;
    public const float  MIN_INTELLIGENCE_TO_COMBINE_ATTACKS = 4f;
    public const float  MIN_INTELLIGENCE_TO_REMOVE_PSYCHIC_CONTROL = 4f;
    public const float  QUICK_DRAW_TRAIT_CASTING_SPEED_INCREASE = 1.5f;
    public const float  WEAKENED_HEALTH_CO = 0.94f;

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //public static bool operator ==(Fighter a, Fighter b) { return (a.GetInstanceID() == b.GetInstanceID()); }
    //public static bool operator !=(Fighter a, Fighter b) { return (a.GetInstanceID() != b.GetInstanceID()); }

    public void AddBonusData(BonusData bonusData, float movePower, Enums.BonusSource source, Clothing clothing, Potion potion, Move powerUpMove, Weapon weapon, int endingRoundNumber)
    {
        if (bonusData.GetHealth() != 0)
        {
            int healthBonus = bonusData.GetHealth();
            AttributeBonus attributeBonus = new AttributeBonus();
            attributeBonus.SetAmount((float)healthBonus);
            attributeBonus.SetAttribute(Enums.Attribute.Health);
            SetAttributeBonusSource(attributeBonus, source, clothing, potion, powerUpMove, weapon);

            AttributeBonuses.Add(attributeBonus);
            // TODO It's possible that we don't want this to happen immediately and only in ApplyBonusHealth
            /*if (healthBonus > 0)
            {
                AddHealth(healthBonus);
            }
            else
            {
                RemoveHealth(healthBonus);
            }*/
        }
        if (bonusData.GetMana() != 0)
        {
            int manaBonus = bonusData.GetMana();
            AttributeBonus attributeBonus = new AttributeBonus();
            attributeBonus.SetAmount((float)manaBonus);
            attributeBonus.SetAttribute(Enums.Attribute.Mana);
            SetAttributeBonusSource(attributeBonus, source, clothing, potion, powerUpMove, weapon);

            AttributeBonuses.Add(attributeBonus);

            if (manaBonus > 0)
            {
                AddMana(manaBonus);
            }
            else
            {
                RemoveMana(manaBonus);
            }
        }
        if (bonusData.GetSpeed() != 0)
        {
            AttributeBonus attributeBonus = new AttributeBonus();
            attributeBonus.SetAmount(bonusData.GetSpeed());
            attributeBonus.SetAttribute(Enums.Attribute.Speed);
            SetAttributeBonusSource(attributeBonus, source, clothing, potion, powerUpMove, weapon);

            AttributeBonuses.Add(attributeBonus);
        }
        if (bonusData.GetStrength() != 0)
        {
            AttributeBonus attributeBonus = new AttributeBonus();
            attributeBonus.SetAmount(bonusData.GetStrength());
            attributeBonus.SetAttribute(Enums.Attribute.Strength);
            SetAttributeBonusSource(attributeBonus, source, clothing, potion, powerUpMove, weapon);

            AttributeBonuses.Add(attributeBonus);
        }
        if (bonusData.GetIntelligence() != 0)
        {
            AttributeBonus attributeBonus = new AttributeBonus();
            attributeBonus.SetAmount(bonusData.GetIntelligence());
            attributeBonus.SetAttribute(Enums.Attribute.Intelligence);
            SetAttributeBonusSource(attributeBonus, source, clothing, potion, powerUpMove, weapon);

            AttributeBonuses.Add(attributeBonus);
        }
        if (bonusData.GetSpellcraft() != 0)
        {
            AttributeBonus attributeBonus = new AttributeBonus();
            attributeBonus.SetAmount(bonusData.GetSpellcraft());
            attributeBonus.SetAttribute(Enums.Attribute.Spellcraft);
            SetAttributeBonusSource(attributeBonus, source, clothing, potion, powerUpMove, weapon);

            AttributeBonuses.Add(attributeBonus);
        }
        if (bonusData.GetMelee() != 0)
        {
            AttributeBonus attributeBonus = new AttributeBonus();
            attributeBonus.SetAmount(bonusData.GetMelee());
            attributeBonus.SetAttribute(Enums.Attribute.Melee);
            SetAttributeBonusSource(attributeBonus, source, clothing, potion, powerUpMove, weapon);

            AttributeBonuses.Add(attributeBonus);
        }
        if (bonusData.GetPsychic() != 0)
        {
            AttributeBonus attributeBonus = new AttributeBonus();
            attributeBonus.SetAmount(bonusData.GetPsychic());
            attributeBonus.SetAttribute(Enums.Attribute.Psychic);
            SetAttributeBonusSource(attributeBonus, source, clothing, potion, powerUpMove, weapon);

            AttributeBonuses.Add(attributeBonus);
        }
        if (bonusData.GetDamageResistance() != 0)
        {
            AttributeBonus attributeBonus = new AttributeBonus();
            attributeBonus.SetAmount(bonusData.GetDamageResistance());
            attributeBonus.SetAttribute(Enums.Attribute.DamageResistance);
            SetAttributeBonusSource(attributeBonus, source, clothing, potion, powerUpMove, weapon);

            AttributeBonuses.Add(attributeBonus);
        }

        AddBonusNatures(bonusData.GetNatures());
        AddBonusTraits(bonusData.GetTraits());
        AddBonusMoves(bonusData.GetMoves());
    }

    public void AddBonusMoves(Move[] moves) { BonusMoves.AddRange(moves); }
    public void AddBonusNatures(Enums.Nature[] natures) { BonusNatures.AddRange(natures); }
    public void AddBonusTraits(Enums.Trait[] traits) { BonusTraits.AddRange(traits); }
    public void AddMana(int amount) { Mana += amount; }
    public void AddHealth(int amount)
    {
        Health += amount;
        Health = Mathf.Min(100, Health);
    }

    public void AddMove(Move move)
    {
        if (CheckIfOwnMove(move) == false)
        {
            Moves.Add(move);
        }
    }

    public void AddNature(Enums.Nature nature)
    {
        if (CheckNatureOwned(nature) == false)
        {
            Natures.Add(nature);
        }
    }

    public void AddPowerUp(PowerUp powerUp) { PowerUps.Add(powerUp); }

    public void AddStatus(Status status)
    {
        Enums.StatusType statusType = status.GetStatusType();
        if (CheckStatus(statusType) == true)
        {
            RemoveStatus(statusType);
        }

        Statuses.Add(status);
    }

    public void AddStatus(Enums.StatusType statusType, Move move, float movePower, int endingRoundNumber)
    {
        if (CheckStatus(statusType) == true)
        {
            RemoveStatus(statusType);
        }

        Status status = new Status();
        status.SetFighter(this);
        status.SetMove(move);
        status.SetPower(movePower);
        status.SetStatusType(statusType);
        status.SetEndingRoundNumber(endingRoundNumber);

        Statuses.Add(status);
    }

    public void AddStatuses(List<Status> statuses)
    {
        foreach (Status status in statuses)
        {
            Enums.StatusType statusType = status.GetStatusType();
            if (CheckStatus(statusType) == false)
            {
                AddStatus(status);
            }
        }
    }

    public void AddSub(Substitution sub)
    {
        Subs.Clear();
        Subs.Add(sub);
    }

    public void AddTrait(Enums.Trait trait)
    {
        if (CheckTraitOwned(trait) == false)
        {
            Traits.Add(trait);
        }
    }

    public void AddUsedMove(Move move, int roundNumber)
    {
        UsedMove usedMove = new UsedMove();
        usedMove.SetRoundNumber(roundNumber);
        usedMove.SetFighter(this);
        usedMove.SetMove(move);

        UsedMoves.Add(usedMove);
    }

    public void AddUsedMove(UsedMove usedMove) { UsedMoves.Add(usedMove); }
    public bool CheckCombineAttacks() { return GetIntelligence() >= MIN_INTELLIGENCE_TO_COMBINE_ATTACKS; }
    public bool CheckCustomAI() { return AI.CheckCustomAI() == true; }
    public bool CheckIfAlive() { return Health > 0; }

    public bool CheckIfCanMove() 
    { 
        return CheckStatus(Enums.StatusType.PsychicControl) == false && CheckStatus(Enums.StatusType.PsychicParalysis) == false
            && CheckStatus(Enums.StatusType.Trapped) == false && CheckStatus(Enums.StatusType.Cooldown) == false; 
    }

    public bool CheckIfCapableOfMove(Move move)
    {
        if (move.GetMana() > Mana)
        {
            return false;
        }

        float skill = 0;
        float requiredSkill = (float)move.GetLevel();
        Enums.MoveType moveType = move.GetMoveType();

        switch (moveType)
        {
            case Enums.MoveType.Melee:
                skill = GetMelee();
                break;
            case Enums.MoveType.Psychic:
                skill = GetPsychic();
                break;
            case Enums.MoveType.Defensive:
                if (move.GetUseMeleeSkill() == true)
                {
                    skill = GetMelee();
                }
                else
                {
                    skill = GetSpellcraft();
                }
                break;
            default:
                skill = GetSpellcraft();
                break;
        }

        return skill >= requiredSkill;
    }

    public bool CheckIfManaIsLow() { return Mana <= LOW_MANA_VALUE; }
    public bool CheckIfHealthIsLow() { return Health <= LOW_HEALTH_VALUE; }
    public bool CheckIfMissingXMana(int x) { return (MaxMana - Mana) >= x; }
    public bool CheckIfMissingXHealth(int x) { return GetHealth() <= (100 - x); }
    public bool CheckIfNearDefeat() { return (CheckIfHealthIsLow() == true || (CheckIfManaIsLow() == true && GetFightingStyle() != Enums.FightingStyle.Melee)); }
    public bool CheckIfOwnMove(Move move) { return Moves.Contains(move); }
    public bool CheckIfSubbed() { return Subs.Count > 0; }
    public bool CheckIfWeakened() { return GetHealthCo() <= WEAKENED_HEALTH_CO; }

    public bool CheckNature(Enums.Nature nature)
    {
        foreach (Enums.Nature tempNature in Natures)
        {
            if (tempNature == nature)
            {
                return true;
            }
        }

        foreach (Enums.Nature tempNature in BonusNatures)
        {
            if (tempNature == nature)
            {
                return true;
            }
        }

        return false;
    }

    // Checks if they possess the nature (not bonus natures)
    public bool CheckNatureOwned(Enums.Nature nature) { return Natures.Contains(nature); }

    public bool CheckStatus(Enums.StatusType statusType)
    {
        foreach (Status status in Statuses)
        {
            if (status.GetStatusType() == statusType)
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckStatuses(List<Enums.StatusType> statuses)
    {
        foreach (Enums.StatusType status in statuses)
        {
            if (CheckStatus(status) == false)
            {
                return false;
            }
        }

        return true;
    }

    public bool CheckStatuses(Enums.StatusType[] statuses)
    {
        foreach (Enums.StatusType status in statuses)
        {
            if (CheckStatus(status) == false)
            {
                return false;
            }
        }

        return true;
    }

    public bool CheckTrait(Enums.Trait trait)
    {
        foreach (Enums.Trait tempTrait in Traits)
        {
            if (tempTrait == trait)
            {
                return true;
            }
        }

        foreach (Enums.Trait tempTrait in BonusTraits)
        {
            if (tempTrait == trait)
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckTraitOwned(Enums.Trait trait) { return Traits.Contains(trait); }
    public AI GetAI() { return AI; }
    public List<AttributeBonus> GetAttributeBonuses() { return AttributeBonuses; }
    public List<Enums.Nature> GetBonusNatures() { return BonusNatures; }
    public List<Enums.Trait> GetBonusTraits() { return BonusTraits; }
    public int GetMana() { return Mana; }

    public static float GetManaRating(int mana)
    {
        return ((float)mana / 100.0f) * 5f;
    }

    public Enums.ControlType GetControlType() { return ControlType; }
    public float GetDamageCo() { return Mathf.Max(1 - GetDamageResistance(), MIN_DAMAGE_RESISTANCE_CO); }
    public float GetDamageResistance() { return DamageResistance + GetTotalAttributeBonus(Enums.Attribute.DamageResistance); }

    // This is currently not used. Avoid moves need to be added to the getdefensivemove logic.
    public List<Move> GetDefensiveMoves() 
    {
        List<Move> defensiveMoves = new List<Move>();
        foreach (Move move in Moves)
        {
            if (move.GetMoveType() == Enums.MoveType.Defensive || move.GetMoveType() == Enums.MoveType.Avoid)
            {
                defensiveMoves.Add(move);
            }
        }

        return defensiveMoves;
    }

    public Enums.FightingStyle GetFightingStyle() { return FightingStyle; }
    public float GetPsychic() { return Psychic + GetTotalAttributeBonus(Enums.Attribute.Psychic); }
    public int GetHealth() { return Health; }
    public float GetHealthCo() { return .2f + .8f * Mathf.Min(1.0f, Health / 75.0f); } //+ .4f * Mathf.Min(1.0f, Mana / 75.0f); } Not considering mana anymore since it's not chakra
    public ulong GetID() { return ID; }
    public float GetIntelligence() { return Intelligence + GetTotalAttributeBonus(Enums.Attribute.Intelligence); }
    public int GetMaxMana() { return MaxMana + (int)(GetTotalAttributeBonus(Enums.Attribute.Mana) + 0.5f); } // .5 is for rounding
    public float GetMaxManaRating() { return GetManaRating(MaxMana); }

    // Multiple move types are considered "Offensive", so that logic is treated uniquely.
    public List<Move> GetMoves(Enums.MoveType moveType)
    {
        List<Move> moves = new List<Move>();
        foreach (Move move in Moves)
        {
            if (moveType == Enums.MoveType.Offensive)
            {
                Enums.MoveType tempMoveType = move.GetMoveType();
                switch (tempMoveType)
                {
                    case Enums.MoveType.Melee:
                    case Enums.MoveType.NinTai:
                    case Enums.MoveType.Offensive:
                    case Enums.MoveType.Projectile:
                    case Enums.MoveType.Psychic:
                        {
                            moves.Add(move);
                            break;
                        }
                }
            }
            else
            {
                if (move.GetMoveType() == moveType)
                {
                    moves.Add(move);
                }
            }
        }

        return moves;
    }

    public string GetName() { return Name; }
    public List<Enums.Nature> GetNatures() { return Natures; }
    public float GetSpellcraft() { return Spellcraft + GetTotalAttributeBonus(Enums.Attribute.Spellcraft); }
    public float GetNinTaiDefenseSkill(float randomAdd) { return GetMelee() * GetHealthCo() + randomAdd; }

    public float GetOverallRating()
    {
        float healthCo = GetHealthCo(); // Health coefficient
        float overall = 0;

        switch (GetFightingStyle())
        {
            case Enums.FightingStyle.Balanced:
                overall = GetSpellcraft() * .35f + GetMelee() * .25f * healthCo + GetPsychic() * .05f + GetSpeed() * .10f * healthCo + GetStrength() * .10f * healthCo + GetManaRating(Mana) * .10f + GetIntelligence() * .05f;
                break;
            case Enums.FightingStyle.Melee:
                overall = GetSpellcraft() * .01f + GetMelee() * .40f * healthCo + GetPsychic() * .01f + GetSpeed() * .24f * healthCo + GetStrength() * .23f * healthCo + GetManaRating(Mana) * .08f + GetIntelligence() * .03f;
                break;
            case Enums.FightingStyle.Psychic:
                overall = GetSpellcraft() * .15f + GetMelee() * .05f * healthCo + GetPsychic() * .65f + GetSpeed() * .05f * healthCo + GetManaRating(Mana) * .05f + GetIntelligence() * .05f;
                break;
            case Enums.FightingStyle.Medical:
            case Enums.FightingStyle.Spellslinger:
                overall = GetSpellcraft() * .65f + GetPsychic() * .03f + GetSpeed() * .02f * healthCo + GetStrength() * .02f * healthCo + GetManaRating(Mana) * .18f + GetIntelligence() * .05f;
                break;
            default:
                Debug.LogError("Error! Unrecognized FightingStyle in Fighter.GetOverallRating for Fighter: " + GetName() + " with ID: " + GetID());
                overall = GetSpellcraft() * .35f + GetMelee() * .25f * healthCo + GetPsychic() * .05f + GetSpeed() * .10f * healthCo + GetStrength() * .10f * healthCo + GetManaRating(Mana) * .10f + GetIntelligence() * .05f;
                break;
        }

        return overall;
    }

    public List<PowerUp> GetPowerUps() { return PowerUps; }

    public float GetCastingSpeed(Move move, float randomAdd)
    {
        float castingSpeed = 0.0f;
        Enums.MoveType moveType = move.GetMoveType();

        switch (moveType)
        {
            case Enums.MoveType.Melee:
            case Enums.MoveType.NinTai:
                {
                    castingSpeed = .5f * GetSpeed() * GetHealthCo() + .5f * move.GetCastingSpeed() + randomAdd;
                    break;
                }
            case Enums.MoveType.Projectile:
                {
                    castingSpeed = move.GetCastingSpeed();
                    if (CheckTrait(Enums.Trait.QuickDraw) == true)
                    {
                        castingSpeed += QUICK_DRAW_TRAIT_CASTING_SPEED_INCREASE;
                    }
                    break;
                }
            default:
                {
                    castingSpeed = move.GetCastingSpeed();
                    if (CheckTrait(Enums.Trait.QuickCasting) == true)
                    {
                        castingSpeed += CASTING_TRAIT_CASTING_SPEED_INCREASE;
                    }
                    break;
                }
        }

        return castingSpeed;
    }

    public float GetSpeed() { return Speed + GetTotalAttributeBonus(Enums.Attribute.Speed); }
    public List<Status> GetStatuses() { return Statuses; }
    public float GetStrength() { return Strength + GetTotalAttributeBonus(Enums.Attribute.Strength); }

    public Substitution GetSubstition()
    {
        if (Subs.Count > 0)
        {
            return Subs[0];
        }

        Debug.LogError("Error! Calling Fighter.GetSubstitution() when there is no existing Substitution! Returning an empty Substitution object.");
        Substitution sub = new Substitution();
        sub.SetFighter(this);

        return sub;
    }

    public float GetMelee() { return Melee + GetTotalAttributeBonus(Enums.Attribute.Melee); }
    public float GetMeleeDefenseSkill(float randomAdd) { return (GetMelee() + GetStrength()) * 0.5f * GetHealthCo() + randomAdd; }
    public int GetTeam() { return Team; }

    public float GetTotalAttributeBonus(Enums.Attribute attribute)
    {
        float totalAttributeBonus = 0;

        foreach (AttributeBonus attributeBonus in AttributeBonuses)
        {
            if (attributeBonus.GetAttribute() == attribute)
            {
                totalAttributeBonus += attributeBonus.GetAmount();
            }
        }
        
        return totalAttributeBonus;
    }

    public List<Enums.Trait> GetTraits() { return Traits; }
    public List<UsedMove> GetUsedMoves() { return UsedMoves; }
    public Weapon GetWeapon() { return Weapon; }

    public void InitFighter()
    {
        AttributeBonuses = new List<AttributeBonus>();
        BonusNatures = new List<Enums.Nature>();
        BonusTraits = new List<Enums.Trait>();
        BonusMoves = new List<Move>();

        PowerUps = new List<PowerUp>();
        Statuses = new List<Status>();
        Subs = new List<Substitution>();
        UsedMoves = new List<UsedMove>();
    }

    public void RemoveBonusData(BonusData bonusData, Enums.BonusSource source, Clothing clothing, Potion potion, Move powerUpMove, Weapon weapon)
    {
        int index = 0;
        while (index < AttributeBonuses.Count)
        {
            bool shouldDelete = false;
            AttributeBonus attributeBonus = AttributeBonuses[index];

            switch (source)
            {
                case Enums.BonusSource.Clothing:
                    if (attributeBonus.GetSource() == Enums.BonusSource.Clothing && attributeBonus.GetClothing() == clothing)
                    {
                        shouldDelete = true;
                    }
                    break;
                case Enums.BonusSource.Potion:
                    if (attributeBonus.GetSource() == Enums.BonusSource.Potion && attributeBonus.GetPotion() == potion)
                    {
                        shouldDelete = true;
                    }
                    break;
                case Enums.BonusSource.PowerUpMove:
                    if (attributeBonus.GetSource() == Enums.BonusSource.PowerUpMove && attributeBonus.GetPowerUpMove() == powerUpMove)
                    {
                        shouldDelete = true;
                    }
                    break;
                case Enums.BonusSource.Weapon:
                    if (attributeBonus.GetSource() == Enums.BonusSource.Weapon && attributeBonus.GetWeapon() == weapon)
                    {
                        shouldDelete = true;
                    }
                    break;
                default:
                    Debug.Log("Error! Invalid BonusSource in RemoveBonusData!");
                    break;
            }

            if (shouldDelete)
            {
                AttributeBonuses.RemoveAt(index);
            }
            else
            {
                index++;
            }
        }

        RemoveBonusNatures(bonusData.GetNatures());
        RemoveBonusTraits(bonusData.GetTraits());
        RemoveBonusMoves(bonusData.GetMoves());
    }

    public void RemoveBonusMoves(Move[] moves)
    {
        foreach (Move moveToRemove in moves)
        {
            int index = 0;
            while (index < BonusMoves.Count)
            {
                if (BonusMoves[index] == moveToRemove)
                {
                    BonusMoves.RemoveAt(index);
                    break;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    public void RemoveBonusNatures(Enums.Nature[] natures)
    {
        foreach(Enums.Nature natureToRemove in natures)
        {
            int index = 0;
            while(index < BonusNatures.Count)
            {
                if (BonusNatures[index] == natureToRemove)
                {
                    BonusNatures.RemoveAt(index);
                    break;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    public void RemoveBonusTraits(Enums.Trait[] traits)
    {
        foreach (Enums.Trait traitToRemove in traits)
        {
            int index = 0;
            while (index < BonusTraits.Count)
            {
                if (BonusTraits[index] == traitToRemove)
                {
                    BonusTraits.RemoveAt(index);
                    break;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    public void RemoveMana(int amount)
    {
        Mana -= Mathf.Abs(amount); // Removes absolute value of input to reduce ambiguity
        Mana = Mathf.Max(Mana, 0);
    }

    public void RemoveHealth(int amount)
    {
        Health -= Mathf.Abs(amount); // Removes absolute value of input to reduce ambiguity
        Health = Mathf.Max(Health, 0);
    }

    public void RemoveNature(Enums.Nature nature) { Natures.Remove(nature); }

    public void RemovePowerUp(Move move)
    {
        int index = 0;
        while (index < PowerUps.Count)
        {
            PowerUp powerUp = PowerUps[index];
            
            if (powerUp.GetPowerUpMove().GetID() == move.GetID())   // This PowerUp has the correct move attached to it.
            {
                PowerUps.RemoveAt(index);
                break;
            }
        }
    }

    public void RemoveStatus(Enums.StatusType statusTypeToRemove)
    {
        int index = 0;
        while (index < Statuses.Count)
        {
            Enums.StatusType statusType = Statuses[index].GetStatusType();

            if (statusType == statusTypeToRemove)
            {
                Statuses.RemoveAt(index);
            }
            else
            {
                ++index;
            }
        }
    }

    public void RemoveSubstitution() { Subs.Clear(); }
    public void RemoveTrait(Enums.Trait trait) { Traits.Remove(trait); }

    public void SetAttributeBonusSource(AttributeBonus attributeBonus, Enums.BonusSource source, Clothing clothing, Potion potion, Move powerUpMove, Weapon weapon)
    {
        attributeBonus.SetSource(source);

        switch (source)
        {
            case Enums.BonusSource.Clothing:
                attributeBonus.SetClothing(clothing);
                break;
            case Enums.BonusSource.Potion:
                attributeBonus.SetPotion(potion);
                break;
            case Enums.BonusSource.PowerUpMove:
                attributeBonus.SetPowerUpMove(powerUpMove);
                break;
            case Enums.BonusSource.Weapon:
                attributeBonus.SetWeapon(weapon);
                break;
            default:
                Debug.LogError("Error! Invalid BonusSource in SetAttributeBonusSource!");
                break;
        }
    }

    public void SetDamageResistance(float damageResistance) { DamageResistance = damageResistance; }
    public void SetFightingStyle(Enums.FightingStyle style) { FightingStyle = style; }
    public void SetPsychic(float psychic) { Psychic = psychic; }
    public void SetHealth(int health) { Health = health; }
    public void SetIntelligence(float intelligence) { Intelligence = intelligence; }
    public void SetMaxMana(int maxMana) { MaxMana = maxMana; }
    public void SetNinjutsu(float spellcraft) { Spellcraft = spellcraft; }
    public void SetSpeed(float speed) { Speed = speed; }
    public void SetStatuses(List<Status> statuses) { Statuses = statuses; }
    public void SetStrength(float strength) { Strength = strength; }
    public void SetMelee(float melee) { Melee = melee; }
    public void SetTeam(int team) { Team = team; }
    public void SetControlType(Enums.ControlType controlType) { ControlType = controlType; }
}