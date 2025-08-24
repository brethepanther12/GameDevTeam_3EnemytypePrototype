using UnityEngine;

public class EnemyWeakPoints : MonoBehaviour , IDamage
{
    [SerializeField] Enemy enemy;
    [SerializeField] float bodyModifier;

    public void slowDown(float magnitude, float duration)
    {
        throw new System.NotImplementedException();
    }


    public void takeDamage(int amount)
    {
        enemy.takeDamage(Mathf.RoundToInt(amount * bodyModifier));
    }

    public void takeDamage(int amount, StatusEffectData effect)
    {
        enemy.takeDamage(Mathf.RoundToInt(amount * bodyModifier), effect);
    }

    bool IDamage.isDead()
    {
        throw new System.NotImplementedException();
    }
}
