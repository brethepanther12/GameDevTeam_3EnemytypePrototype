using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Spawn Settings")]
    public GameObject enemyPrefab;        
    public int numberToSpawn = 3;        
    public float minDelay = 0.5f;          
    public float maxDelay = 1.5f;          

    private bool hasSpawned = false;

    public void StartSpawning()
    {
        if (!hasSpawned)
        {
            hasSpawned = true;
            StartCoroutine(SpawnEnemies());
        }
    }

    IEnumerator SpawnEnemies()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
        }
    }
}
