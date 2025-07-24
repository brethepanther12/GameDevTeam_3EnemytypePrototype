using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Spawn Settings")]
    public GameObject enemyPrefab;
    public int numberToSpawn = 3;
    public float minDelay = 0.5f;
    public float maxDelay = 1.5f;

    [Header("Spawn Location")]
    public Transform customSpawnPoint;  // ? NEW

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
            Vector3 spawnPosition = customSpawnPoint != null ? customSpawnPoint.position : transform.position;

            Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f);

            Instantiate(enemyPrefab, spawnPosition, spawnRotation);

            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
        }
    }
}