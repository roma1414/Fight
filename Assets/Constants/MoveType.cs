namespace Enums
{
    public enum MoveType : short
    {
        Melee,           // Melee/hand-to-hand attacks with no mana manipulation.
        Spell,           // Mana-based ranged attacks.
        Psychic,           // Mind-related ranged attacks.
        NinTai,             // Melee/hand-to-hand attacks with mana manipulation. Rasengan, chidori, etc. Melee ability is relevant to hit chance and damage.
        Projectile,         // Ranged projectile attacks with physical weapons. Kunai throws, shadow-shuriken jutsu, etc.
        Offensive,          // General term for the types above when multiple moves are used simultaneously
        PowerUp,            // PowerUp jutsus that commonly boost attributes, add jutsus/abilities, change appearance, etc, such as sage mode or Sasuke's cursed seal power.
        Medical,            // Healing actions. Typically jutsus.
        Defensive,          // Protective jutsus/abilities/actions that defend yourself or teammates. Water wall, iron-skin, etc.
        Avoid,              // Jutsus/abilities that avoid attacks. Hiraishin teleportation, flight, etc.
        Protect,            // Protecting self, a teammate, or a team. When targeting a teammate or team, user will use jutsus/abilities as if protecting themself.
        Substitution,       // Jutsu/ability that surreptitiously replaces the user. First attack towards the user that round is always avoided. If user is not attacked, 
                            // user can remain hidden. Opponents may or may not be tricked depending on intelligence, move level, etc.
        Clone,              // Jutsu/ability that produces a clone. Advanced jutsus can produce multiple clones. Enemies may attack a clone if they are fooled.
        Summon,             // Summons an ally for a set number of turns, or until summon is defeated.
        Skip                // No move should be performed. Sometimes the result of an error during move selection, etc.
    }
}
