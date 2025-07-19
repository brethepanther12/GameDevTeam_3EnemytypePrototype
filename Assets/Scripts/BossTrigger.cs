using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public Animator gateAnimator;
    public GameObject enemyPortalPrefab;
    public Transform enemyPortalSpawnPoint;
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    private bool hasActivated = false;

    public void ActivateBossFight()
    {
        if (hasActivated) return;

        hasActivated = true;

        // Spawn boss
        if (bossPrefab && bossSpawnPoint)
        {
            Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        }

        // Spawn enemy portal
        if (enemyPortalPrefab && enemyPortalSpawnPoint)
        {
            GameObject portal = Instantiate(enemyPortalPrefab, enemyPortalSpawnPoint.position, enemyPortalSpawnPoint.rotation);

            // Tell the portal to start spawning
            EnemySpawner spawner = portal.GetComponent<EnemySpawner>();
            if (spawner != null)
            {
                spawner.StartSpawning();
            }
        }
    }

    }