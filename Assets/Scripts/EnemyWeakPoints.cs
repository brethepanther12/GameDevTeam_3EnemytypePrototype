using UnityEngine;

public class EnemyWeakPoints : MonoBehaviour , IDamage
{
    [SerializeField] MonoBehaviour enemyScript;
    [SerializeField] float bodyModifier;
     IDamage enemy;
    private void Awake()
    {
        enemy = enemyScript as IDamage;
       
    }

    public void takeDamage(int amount)
    {
        if (enemy == null) return;
        enemy.takeDamage(Mathf.RoundToInt(amount * bodyModifier));
    }

    public void takeDamage(int amount, StatusEffectData effect)
    {
        if (enemy == null) return;
        enemy.takeDamage(Mathf.RoundToInt(amount * bodyModifier), effect);
    }

    public bool isDead()
    {
        return enemy != null && enemy.isDead();
    }

    public void slowDown(float magnitude, float duration)
    {
        throw new System.NotImplementedException();
    }
}
