using System.Collections.Generic;
using UnityEngine;

public class SmokeCloud : MonoBehaviour
{
    // Duration the smoke lasts in seconds
    [SerializeField] public float smokeDuration;

    // List to keep track of enemies inside the cloud
    private List<FlyingAI> enemiesInSmoke = new List<FlyingAI>();

    private void Start()
    {
        // Destroy the smoke cloud after duration
        Destroy(gameObject, smokeDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            FlyingAI enemy = other.GetComponent<FlyingAI>();
            if (enemy != null && !enemiesInSmoke.Contains(enemy))
            {
                enemiesInSmoke.Add(enemy);
                enemy.SetInvisible(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            FlyingAI enemy = other.GetComponent<FlyingAI>();
            if (enemy != null && enemiesInSmoke.Contains(enemy))
            {
                enemiesInSmoke.Remove(enemy);
                enemy.SetInvisible(false);
            }
        }
    }

    private void OnDestroy()
    {
        // When smoke disappears, reset invisibility on all enemies inside
        foreach (var enemy in enemiesInSmoke)
        {
            if (enemy != null)
                enemy.SetInvisible(false);
        }
        enemiesInSmoke.Clear();
    }
}
