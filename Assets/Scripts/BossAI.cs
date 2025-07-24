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
    [SerializeField] GameObject aoeEffectPrefab;
    [SerializeField] float aoeRange = 6f;
    [SerializeField] float aoeCooldown = 8f;
    [SerializeField] float burstCooldown = 10f;
    [SerializeField] int burstCount = 3;

    private float burstTimer = 0f;
    private bool isRetreating = false;

    private float aoeTimer = 0f;
    private bool isPhaseTwo = false;
    private float phaseTwoHealthThreshold => enemyHealthPointsMax * 0.5f;

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

        // Disable NavMeshAgent auto rotation to control rotation manually
        enemyNavAgent.updateRotation = false;

        gamemanager.instance.updateGameGoal(+1);
    }

    private void EnterPhaseTwo()
    {
        isPhaseTwo = true;
        Debug.Log("Boss has entered Phase 2!");
        attackCooldown *= 0.75f; // Attack faster
    }

    protected override void Update()
    {
        if (!isPhaseTwo && enemyCurrentHealthPoints <= phaseTwoHealthThreshold)
        {
            EnterPhaseTwo();
        }

        if (isDead)
            return;

        base.Update();

        if (enemyPlayerObject == null)
        {
            enemyPlayerObject = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (enemyPlayerObject == null) return;  // Can't do AI without player
        }

        float distanceToPlayer = Vector3.Distance(transform.position, enemyPlayerObject.position);
        SetPlayerInSight(distanceToPlayer <= detectionRange);

        if (enemyPlayerInSight && !isDodging && !isRetreating)
        {
            SmoothFacePlayer();

            if (distanceToPlayer > attackRange)
            {
                enemyNavAgent.isStopped = false;
                enemyNavAgent.SetDestination(enemyPlayerObject.position);
                bossAnimator.SetFloat("Speed", enemyNavAgent.velocity.magnitude);
            }
            else
            {
                enemyNavAgent.isStopped = true;
                bossAnimator.SetFloat("Speed", 0);

                if (attackTimer >= attackCooldown)
                {
                    BossAttack();
                    attackTimer = 0f;
                }
                else
                {
                    if (Random.value < 0.01f)  // 1% chance per frame to dodge
                    {
                        StartCoroutine(Dodge());
                    }
                }
            }
        }
        else
        {
            enemyNavAgent.isStopped = true;
            bossAnimator.SetFloat("Speed", 0);
        }

        attackTimer += Time.deltaTime;
        aoeTimer += Time.deltaTime;

        if (isPhaseTwo && Vector3.Distance(transform.position, enemyPlayerObject.position) < aoeRange && aoeTimer >= aoeCooldown)
        {
            StartCoroutine(PerformAOEAttack());
            aoeTimer = 0f;
        }

        if (isPhaseTwo && !isRetreating)
        {
            burstTimer += Time.deltaTime;

            if (burstTimer >= burstCooldown)
            {
                StartCoroutine(RetreatAndBurst());
                burstTimer = 0f;
            }
        }
    }

    private void SmoothFacePlayer()
    {
        Vector3 direction = enemyPlayerObject.position - transform.position;
        direction.y = 0; // Keep only horizontal direction

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    IEnumerator RetreatAndBurst()
    {
        isRetreating = true;

        Vector3 retreatDir = -(enemyPlayerObject.position - transform.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDir * 5f;

        enemyNavAgent.isStopped = false;
        enemyNavAgent.SetDestination(retreatTarget);
        bossAnimator.SetFloat("Speed", enemyNavAgent.velocity.magnitude);

        float timer = 0f;
        while (Vector3.Distance(transform.position, retreatTarget) > 0.5f && timer < 2f)
        {
            SmoothFacePlayer();
            timer += Time.deltaTime;
            yield return null;
        }

        enemyNavAgent.isStopped = true;
        bossAnimator.SetFloat("Speed", 0);

        for (int i = 0; i < burstCount; i++)
        {
            SmoothFacePlayer();

            // Randomly choose between homing or moving projectile
            damage.damagetype typeToFire = (Random.value > 0.5f)
                ? damage.damagetype.homing
                : damage.damagetype.moving;

            FireProjectile(typeToFire);

            yield return new WaitForSeconds(0.5f);
        }

        isRetreating = false;
    }

    IEnumerator PerformAOEAttack()
    {
        bossAnimator.SetTrigger("AOE");

        yield return new WaitForSeconds(0.5f); // Animation wind-up delay

        if (aoeEffectPrefab != null)
            Instantiate(aoeEffectPrefab, transform.position, Quaternion.identity);

        Debug.Log("Boss performed AOE attack!");
    }

    protected void BossAttack()
    {
        SmoothFacePlayer();
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

        enemyNavAgent.isStopped = false;
        enemyNavAgent.SetDestination(dodgeTarget);
        bossAnimator.SetFloat("Speed", enemyNavAgent.velocity.magnitude);

        float timer = 0f;
        while (Vector3.Distance(transform.position, dodgeTarget) > 0.5f && timer < 1.5f)
        {
            SmoothFacePlayer();
            timer += Time.deltaTime;
            yield return null;
        }

        isDodging = false;

        enemyNavAgent.isStopped = false;
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
            enemyNavAgent.isStopped = false;
            enemyNavAgent.SetDestination(enemyPlayerObject.position);

            if (enemyNavAgent.remainingDistance <= enemyNavAgent.stoppingDistance)
            {
                SmoothFacePlayer();
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

        // Update boss health bar via GameManager
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

    public void FireProjectile(damage.damagetype typeToUse)
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogWarning("Projectile prefab or spawn point is not assigned!");
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        damage dmg = proj.GetComponent<damage>();
        Rigidbody rb = proj.GetComponent<Rigidbody>();

        if (dmg != null)
        {
            dmg.SetDamageType(typeToUse);

            if (typeToUse == damage.damagetype.moving && rb != null)
            {
                rb.linearVelocity = proj.transform.forward * dmg.speed;
            }
            // For homing, Update() handles it
        }
        else
        {
            Debug.LogWarning("Projectile has no 'damage' component!");
        }
    }
}