using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeBonus
{
    protected Enums.Attribute   Attribute;
    protected float             Amount;
    protected Clothing          Clothing;
    protected Potion            Potion;
    protected Move              PowerUpMove;
    protected Enums.BonusSource Source;
    protected Weapon            Weapon;

    public float GetAmount() { return Amount; }
    public Enums.Attribute GetAttribute() { return Attribute; }
    public Clothing GetClothing() { return Clothing; }
    public Potion GetPotion() { return Potion; }
    public Move GetPowerUpMove() { return PowerUpMove; }
    public Enums.BonusSource GetSource() { return Source; }
    public Weapon GetWeapon() { return Weapon; }
    public void SetAmount(float amount) { Amount = amount; }
    public void SetAttribute(Enums.Attribute attribute) { Attribute = attribute; }
    public void SetClothing(Clothing clothing) { Clothing = clothing; }
    public void SetPotion(Potion potion) { Potion = potion; }
    public void SetPowerUpMove(Move powerUpMove) { PowerUpMove = powerUpMove; }
    public void SetSource(Enums.BonusSource source) { Source = source; }
    public void SetWeapon(Weapon weapon) { Weapon = weapon; }
}
