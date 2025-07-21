using UnityEngine;

public class SpawnerTemp : MonoBehaviour
{
    [SerializeField] private GameObject[] objectToSpawn;
    [SerializeField] private int spawnRate;
    [SerializeField] private int spawnAmount;
    [SerializeField] private Transform[] spawnPoints;

    float spawnTimer;
    bool IsSpawning;
    int spawnCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gamemanager.instance.updateGameGoal(spawnAmount);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsSpawning)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnRate && spawnCount < spawnAmount) 
            {
                spawn();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) IsSpawning = true;
    }
    void spawn()
    {
        int arrayPosition = Random.Range(0, spawnPoints.Length);
        int enemyToSpawn = Random.Range(0, objectToSpawn.Length);

        Instantiate(objectToSpawn[enemyToSpawn], spawnPoints[arrayPosition].transform.position, spawnPoints[arrayPosition].transform.rotation);
        spawnCount++;
        spawnTimer = 0;
    }
}
