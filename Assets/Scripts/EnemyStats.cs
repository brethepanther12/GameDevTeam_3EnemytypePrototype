using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float health = 100f;
    public float damage = 10f;

    // Called by spawner immediately after Instantiate
    public void ApplyMultiplier(float multiplier)
    {
        health *= multiplier;
        damage *= multiplier;
    }
}