using UnityEngine;
using UnityEngine.AI;

public class GruntAi : EnemyAIBase
{
    [Header("Grunt Settings")]
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
                GruntAttack();
            }
        }
    }

    protected void GruntAttack()
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
            Debug.LogWarning("Grunt projectile has no Rigidbody attached!");
        }
    }

    protected override void enemyDeath()
    {
        Debug.Log($"{gameObject.name} the Grunt has been defeated!");
        Destroy(gameObject);
        // optionally trigger win screen or animation here
    }
}