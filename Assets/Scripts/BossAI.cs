using System.Collections;
using UnityEngine;

public class BossAI : EnemyAIBase
{
    [Header("Boss Settings")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float attackRange = 20f;
    [SerializeField] float attackCooldown = 2f;

    float attackTimer;

    protected override void Start()
    {
        base.Start();
        attackTimer = 0;
    }

    protected override void Update()
    {
        base.Update();
        attackTimer += Time.deltaTime;

        if (enemyPlayerInSight)
        {
            float distance = Vector3.Distance(transform.position, enemyPlayerObject.position);
            if (distance <= attackRange && attackTimer >= attackCooldown)
            {
                BossAttack();
            }
        }
    }

    protected void BossAttack()
    {
        attackTimer = 0f;

        // Instantiate projectile
        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(enemyPlayerObject.position - projectileSpawnPoint.position));

        // Get Rigidbody safely
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(proj.transform.forward * 20f, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogWarning("Boss projectile has no Rigidbody attached!");
        }
    }

    protected override void enemyDeath()
    {
        Debug.Log($"{gameObject.name} the Boss has been defeated!");
        Destroy(gameObject);
        // optionally trigger win screen or animation here
    }

    protected override void enemyMoveToPlayer()
    {
        if (enemyPlayerInSight)
        {
            Debug.Log("Boss is moving toward player!");
            enemyNavAgent.SetDestination(enemyPlayerObject.position);

            if (enemyNavAgent.remainingDistance <= enemyNavAgent.stoppingDistance)
            {
                enemyFacePlayer();
            }
        }
    }

    public void SetPlayerInSight(bool isInSight)
    {
        enemyPlayerInSight = isInSight;
    }
}