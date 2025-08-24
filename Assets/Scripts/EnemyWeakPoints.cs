using UnityEngine;

public class EnemyWeakPoints : MonoBehaviour , IDamage
{
    [SerializeField]

    public void slowDown(float magnitude, float duration)
    {
        throw new System.NotImplementedException();
    }


    public void takeDamage(int amount)
    {
        throw new System.NotImplementedException();
    }

    public void takeDamage(int amount, StatusEffectData effect)
    {
        throw new System.NotImplementedException();
    }
}
