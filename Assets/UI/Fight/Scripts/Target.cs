public class Target
{
    protected string Name, Level, Health, Mana;

    public string GetHealth() { return Health; }
    public string GetLevel() { return Level; }
    public string GetName() { return Name; }
    public string GetMana() { return Mana; }
    public void SetHealth(string health) { Health = health; }
    public void SetLevel(string level) { Level = level; }
    public void SetName(string name) { Name = name; }
    public void SetMana(string mana) { Mana = mana; }
}
