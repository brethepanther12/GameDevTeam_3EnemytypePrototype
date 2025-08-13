using UnityEngine;

public interface IDamage
{
    public void takeDamage(int amount);
    public void takeDamage(int amount, StatusEffectData effect);
}
