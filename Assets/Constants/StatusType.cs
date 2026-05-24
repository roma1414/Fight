namespace Enums
{
    public enum StatusType : short
    {
        Cooldown,           // Fighter is in cooldown from a jutsu/ability and cannot do a move this round.
        HidanMarked,        // Hidan has performed his ritual with this fighter's blood. He can now attack them via his own body.
        PsychicParalysis,   // User is under a psychic and cannot choose a move this round.
        PsychicControl,     // User is being controlled by an enemy
        TeleportationMarked,    // Fighter has been marked by another fighter's hiraishin seal. They can now be attacked through teleportation.
        Trapped,            // Fighter is unable to move or trapped in some physical way.
    }
}