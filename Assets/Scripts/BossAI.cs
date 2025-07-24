using System.Collections;
using UnityEngine;

public class BossAI : EnemyAIBase, IGrapplable
{
    [Header("Boss Settings")]
    [Header("Boss Info")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float attackRange = 20f;
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] GameObject deathEffect;
    [SerializeField] Animator bossAnimator;

    [SerializeField] AudioSource bossRoarSource;
    [SerializeField] AudioClip roarClip;

    public string bossName = "Boss 1";

    private bool isDead = false;
    float attackTimer;

    private bool isDodging = false;

    public bool isBeingGrappled { get; set; }

    public bool canBeGrappled => false;

    private float detectionRange = 30f;  // How far boss can see player

    protected override void Start()
    {
        bossRoarSource.PlayOneShot(roarClip);

        base.Start();
        attackTimer = 0;

        // Try to find player if not set in base class
        if (enemyPlayerObject == null)
            enemyPlayerObject = GameObject.FindGameObjectWithTag("Player")?.transform;

        gamemanager.instance.updateGameGoal(+1);
    }

    protected override void Update()
    {
        if (isDead)
            return;

        base.Update();

        if (enemyPlayerObject == null)
        {
            enemyPlayerObject = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (enemyPlayerObject == null) return;  // Can't do AI without player
        }

        float distanceToPlayer = Vector3.Distance(transform.position, enemyPlayerObject.position);

        // Detect player based on distance
        SetPlayerInSight(distanceToPlayer <= detectionRange);

        if (enemyPlayerInSight)
        {
            // If dodging, just continue dodge movement
            if (isDodging) return;

            if (distanceToPlayer > attackRange)
            {
                // Chase player
                enemyNavAgent.SetDestination(enemyPlayerObject.position);
                bossAnimator.SetFloat("Speed", enemyNavAgent.velocity.magnitude);
            }
            else
            {
                // In attack range
                enemyNavAgent.SetDestination(transform.position); // Stop moving to attack

                if (attackTimer >= attackCooldown)
                {
                    BossAttack();
                    attackTimer = 0f;
                }
                else
                {
                    // Chance to dodge while waiting to attack
                    if (Random.value < 0.01f)  // 1% chance per frame to dodge
                    {
                        StartCoroutine(Dodge());
                    }
                }
            }
        }
        else
        {
            // Player not in sight, idle or patrol could go here
            bossAnimator.SetFloat("Speed", 0);
        }

        attackTimer += Time.deltaTime;
    }

    protected void BossAttack()
    {
        bossAnimator.SetTrigger("Attack");
        attackTimer = 0f;
    }

    IEnumerator Dodge()
    {
        isDodging = true;

        float dodgeDistance = 5f;
        Vector3 directionToPlayer = (enemyPlayerObject.position - transform.position).normalized;

        // Dodge direction perpendicular to player direction (left or right)
        Vector3 dodgeDirection = Vector3.Cross(directionToPlayer, Vector3.up);

        if (Random.value > 0.5f)
            dodgeDirection = -dodgeDirection;

        Vector3 dodgeTarget = transform.position + dodgeDirection * dodgeDistance;

        enemyNavAgent.SetDestination(dodgeTarget);
        bossAnimator.SetFloat("Speed", enemyNavAgent.velocity.magnitude);

        // Wait until close to dodgeTarget or timeout
        float timer = 0f;
        while (Vector3.Distance(transform.position, dodgeTarget) > 0.5f && timer < 1.5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isDodging = false;

        // Resume chasing player after dodge
        enemyNavAgent.SetDestination(enemyPlayerObject.position);
        bossAnimator.SetFloat("Speed", enemyNavAgent.velocity.magnitude);
    }

    protected override void enemyDeath()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log($"{gameObject.name} the Boss has been defeated!");

        bossAnimator.SetTrigger("IsDead");

        enemyNavAgent.isStopped = true;

        GetComponent<Collider>().enabled = false;

        StartCoroutine(DestroyAfterDeathAnim());

        if (gamemanager.instance.currentBoss == this)
        {
            gamemanager.instance.EndBossFight();
        }
    }

    IEnumerator DestroyAfterDeathAnim()
    {
        yield return new WaitForSeconds(3.5f);

        gamemanager.instance.TriggerWinScreen();

        Destroy(gameObject);
    }

    protected override void enemyMoveToPlayer()
    {
        if (enemyPlayerInSight && !isDodging)
        {
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

        // Update the centralized boss health bar via GameManager
        if (gamemanager.instance.currentBoss == this)
        {
            gamemanager.instance.UpdateBossHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);
        }

        if (enemyCurrentHealthPoints <= 0)
        {
            bossAnimator.SetBool("IsDead", true);
            enemyDeath();
        }
        else
        {
            bossAnimator.SetTrigger("Hit");
            StartCoroutine(enemyFlashRead());
        }
    }

    // Flash boss red briefly on hit
    protected override IEnumerator enemyFlashRead()
    {
        foreach (var part in enemyModel)
        {
            part.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var part in enemyModel)
        {
            part.material.color = enemyColorOrigin;
        }
    }

    public void FireProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogWarning("Projectile prefab or spawn point is not assigned!");
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

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
}