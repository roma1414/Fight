public class Target
{
    protected string            Name, Level, Health, Mana;
    protected Fighter           FighterTarget;
    protected Enums.TargetType  TargetType;
    protected int TargetTeam;

    public Fighter GetFighterTarget() { return FighterTarget; }
    public string GetHealth() { return Health; }
    public string GetLevel() { return Level; }
    public string GetName() { return Name; }
    public string GetMana() { return Mana; }
    public Enums.TargetType GetTargetType() { return TargetType; }
    public int GetTargetTeam() { return TargetTeam; }
    public void SetFighterTarget(Fighter fighterTarget) { FighterTarget = fighterTarget; }
    public void SetHealth(string health) { Health = health; }
    public void SetLevel(string level) { Level = level; }
    public void SetName(string name) { Name = name; }
    public void SetMana(string mana) { Mana = mana; }
    public void SetTargetType(Enums.TargetType targetType) { TargetType = targetType; }
    public void SetTargetTeam(int targetTeam) { TargetTeam = targetTeam; }
}
