using UnityEngine;
using System.Collections.Generic;

public class SpawnerTemp : MonoBehaviour
{
    [SerializeField] private GameObject[] objectToSpawn;
    [SerializeField] private int spawnRate;
    [SerializeField] private int baseSpawnAmount;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int enemiesPerDeath = 1;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    float spawnTimer;
    bool playerInRange;
    int spawnCount;
    int totalSpawnAmount;

    void Update()
    {
        if (playerInRange)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnRate && spawnCount < totalSpawnAmount)
            {
                spawn();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerInRange)
        {
            playerInRange = true;

            // We now ignore the death count and just use the base amount.
            totalSpawnAmount = baseSpawnAmount;

            Debug.Log($"Spawning initiated. Total to spawn: {totalSpawnAmount}");
        }
    }


    void spawn()
    {
        int arrayPosition = Random.Range(0, spawnPoints.Length);
        int enemyToSpawn = Random.Range(0, objectToSpawn.Length);

        GameObject newEnemy = Instantiate(objectToSpawn[enemyToSpawn], spawnPoints[arrayPosition].transform.position, spawnPoints[arrayPosition].transform.rotation);

        spawnedEnemies.Add(newEnemy);

        spawnCount++;
        spawnTimer = 0;
    }

    public void ResetSpawner()
    {
        Debug.Log(gameObject.name + " is resetting and destroying its children.");

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        spawnedEnemies.Clear();

        playerInRange = false;
        spawnCount = 0;
        spawnTimer = 0;
        totalSpawnAmount = 0;
    }
}