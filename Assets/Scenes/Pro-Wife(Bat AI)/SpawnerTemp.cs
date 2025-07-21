using UnityEngine;

public class SpawnerTemp : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private int spawnRate;
    [SerializeField] private int spawnAmount;
    [SerializeField] private Transform spawnPoints;

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
        
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
    void spawn()
    {

    }
}
