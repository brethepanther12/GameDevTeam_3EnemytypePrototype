using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public Animator gateAnimator;
    private bool hasOpened = false;


    [Header("Enemy Spawner Settings")]
    public EnemySpawner[] spawners;

    private void OnTriggerEnter(Collider other)
    {
        if (hasOpened) return;

        if (other.CompareTag("Player"))
        {
            gateAnimator.Play("BossGate");  // Use "BossGate" since that's animation state's name
            hasOpened = true;

            foreach (EnemySpawner spawner in spawners)
            {
                spawner.StartSpawning();
            }
        }
    }
}