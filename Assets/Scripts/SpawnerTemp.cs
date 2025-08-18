using UnityEngine;
using System.Collections.Generic;

public class SpawnerTemp : MonoBehaviour
{
    [SerializeField] private GameObject[] objectToSpawn;
    [SerializeField] private float spawnRate = 2f; // seconds between spawns (float is nicer)
    [SerializeField] private int baseSpawnAmount;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int enemiesPerDeath = 1;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    float spawnTimer;
    bool playerInRange;
    int spawnCount;
    int totalSpawnAmount;

    private void OnEnable()
    {
        // subscribe to difficulty changes
        gamemanager.OnDifficultyChanged += OnDifficultyChanged;
    }

    private void OnDisable()
    {
        // unsubscribe
        gamemanager.OnDifficultyChanged -= OnDifficultyChanged;
    }

    private void OnDifficultyChanged(gamemanager.DifficultyLevels newDifficulty)
    {
        // If player already triggered this spawner, update total spawn amount
        if (playerInRange)
        {
            int alreadySpawned = spawnCount;
            totalSpawnAmount = GetSpawnAmountBasedOnDifficulty();
            // ensure we don't drop below already spawned
            totalSpawnAmount = Mathf.Max(totalSpawnAmount, spawnCount);
            Debug.Log($"{gameObject.name} difficulty updated. New total to spawn: {totalSpawnAmount} (already spawned: {spawnCount})");
        }
    }

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

            // use difficulty-aware total
            totalSpawnAmount = GetSpawnAmountBasedOnDifficulty();

            Debug.Log($"Spawning initiated. Total to spawn: {totalSpawnAmount}");
        }
    }

    private int GetSpawnAmountBasedOnDifficulty()
    {
        switch (gamemanager.instance.currentDifficulty)
        {
            case gamemanager.DifficultyLevels.easy:
                return baseSpawnAmount;
            case gamemanager.DifficultyLevels.normal:
                return baseSpawnAmount + 2;
            case gamemanager.DifficultyLevels.hard:
                return baseSpawnAmount + 5;
            default:
                return baseSpawnAmount;
        }
    }

    void spawn()
    {
        int arrayPosition = Random.Range(0, spawnPoints.Length);
        int enemyToSpawn = Random.Range(0, objectToSpawn.Length);

        GameObject newEnemy = Instantiate(objectToSpawn[enemyToSpawn],
                                          spawnPoints[arrayPosition].position,
                                          spawnPoints[arrayPosition].rotation);

        // Scale enemy stats for harder difficulties (if the prefab has EnemyStats)
        var stats = newEnemy.GetComponent<EnemyStats>();
        if (stats != null)
        {
            float multiplier = 1f;
            switch (gamemanager.instance.currentDifficulty)
            {
                case gamemanager.DifficultyLevels.normal:
                    multiplier = 1.2f; break;
                case gamemanager.DifficultyLevels.hard:
                    multiplier = 1.5f; break;
            }
            stats.ApplyMultiplier(multiplier);
        }

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