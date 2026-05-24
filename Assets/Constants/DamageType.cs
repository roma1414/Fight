namespace Enums
{
    public enum DamageType : short
    {
        Health,             // Lowers the target's health.
        Mana,             // Lowers the target's mana.
        AbsorbHealth,       // Absorbs health from the target. Attacker can gain health.
        AbsorbMana        // Absorbs mana from the target. Attacker can gain mana.
    }
}