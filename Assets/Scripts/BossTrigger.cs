using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public Animator gateAnimator;
    public GameObject enemyPortalPrefab;
    public Transform enemyPortalSpawnPoint;  // Where the portal appears
    public Transform enemySpawnPoint;         // Where the enemies spawn 
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    private bool hasActivated = false;

    public void ActivateBossFight()
    {
        if (hasActivated) return;
        hasActivated = true;

        // Open the gate
        if (gateAnimator != null)
        {
            Debug.Log("Playing gate animation");
            gateAnimator.Play("GateOpen");
        }

        // Spawn boss
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            GameObject spawnedBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
            BossAI bossAI = spawnedBoss.GetComponent<BossAI>();

            if (bossAI != null)
            {
                gamemanager.instance.currentBoss = bossAI;
            }
            else
            {
                Debug.LogWarning("Spawned boss is missing BossAI component!");
            }
        }

        // Spawn enemy portal
        if (enemyPortalPrefab != null && enemyPortalSpawnPoint != null)
        {
            GameObject portal = Instantiate(enemyPortalPrefab, enemyPortalSpawnPoint.position, enemyPortalSpawnPoint.rotation);

            // Tell the portal to start spawning at the correct enemy spawn location
            EnemySpawner spawner = portal.GetComponent<EnemySpawner>();
            if (spawner != null)
            {
                spawner.customSpawnPoint = enemySpawnPoint;  // assign enemySpawnPoint here
                spawner.StartSpawning();
            }
        }
    }
}