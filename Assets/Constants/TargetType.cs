namespace Enums
{
    public enum TargetType : short
    {
        Enemy,
        EnemyTeam,
        AllEnemies,             // Enemies from all opposing teams.
        EnemiesWithStatuses,
        TeamMember,
        Team,
        TeamMembersWithStatuses,
        Self
    }
}