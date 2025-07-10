using System.Collections;
using UnityEngine;

public class BossAI : EnemyAIBase, IDamage
{
    [Header("Boss Settings")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float attackRange = 20f;
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] GameObject deathEffect;

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

        // Rotate spawn point to aim at player
        projectileSpawnPoint.LookAt(enemyPlayerObject.position);

        // Instantiate projectile
        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        // Apply force to projectile
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

        // Spawn death effect
        Instantiate(deathEffect, transform.position, Quaternion.identity);

        gamemanager.instance.updateGameGoal(-1);
        Destroy(gameObject);
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

    // Boss takes damage and flashes red
    public override void takeDamage(int amount)
    {
        enemyCurrentHealthPoints -= amount;

        if (enemyCurrentHealthPoints <= 0)
        {
            enemyDeath();
        }
        else
        {
            StartCoroutine(enemyFlashRead());
        }
    }

    // Flash boss red briefly on hit
    protected override IEnumerator enemyFlashRead()
    {
        enemyModel.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        enemyModel.material.color = enemyColorOrigin;
    }
}