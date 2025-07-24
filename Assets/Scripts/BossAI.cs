using System.Collections;
using UnityEngine;

public class BossAI : EnemyAIBase, IGrapplable
{
    [Header("Boss Settings")]
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

    [SerializeField] AudioSource bossRoarSource;
    [SerializeField] AudioClip roarClip;

    public string bossName = "Boss 1";

    private float burstTimer = 0f;
    private float aoeTimer = 0f;
    private float attackTimer = 0f;
    private bool isDead = false;
    private bool isDodging = false;
    private bool isRetreating = false;
    private bool isPhaseTwo = false;
    private float detectionRange = 30f;

    private float phaseTwoHealthThreshold => enemyHealthPointsMax * 0.5f;

    public bool isBeingGrappled { get; set; }
    public bool canBeGrappled => false;

    protected override void Start()
    {
        bossRoarSource.PlayOneShot(roarClip);

        base.Start();

        attackTimer = 0f;

        // Ensure NavMesh works
        enemyNavAgent.enabled = false;
        enemyNavAgent.enabled = true;
        enemyNavAgent.updateRotation = false;

        gamemanager.instance.updateGameGoal(+1);
    }

    protected override void Update()
    {
        if (isDead) return;

        if (enemyPlayerObject == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) enemyPlayerObject = playerObj.transform;
            if (enemyPlayerObject == null) return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, enemyPlayerObject.position);
        SetPlayerInSight(distanceToPlayer <= detectionRange);

        if (!isPhaseTwo && enemyCurrentHealthPoints <= phaseTwoHealthThreshold)
            EnterPhaseTwo();

        if (enemyPlayerInSight && !isDodging && !isRetreating)
        {
            SmoothFacePlayer();

            // Boss tries to keep a bit of space before attacking
            float desiredDistance = attackRange - 3f;

            if (distanceToPlayer > desiredDistance)
            {
                // Move to maintain distance, don't rush player
                Vector3 targetPos = enemyPlayerObject.position - (enemyPlayerObject.position - transform.position).normalized * desiredDistance;
                enemyNavAgent.isStopped = false;
                enemyNavAgent.SetDestination(targetPos);
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
                else if (Random.value < 0.05f) // increased dodge chance
                {
                    StartCoroutine(Dodge());
                }
            }
        }
        else
        {
            // If player not in sight or dodging/retreating, boss stops moving
            enemyNavAgent.isStopped = true;
            bossAnimator.SetFloat("Speed", 0);
        }

        // Timers
        attackTimer += Time.deltaTime;
        aoeTimer += Time.deltaTime;
        burstTimer += Time.deltaTime;

        // AOE attack in phase 2
        if (isPhaseTwo && distanceToPlayer < aoeRange && aoeTimer >= aoeCooldown)
        {
            StartCoroutine(PerformAOEAttack());
            aoeTimer = 0f;
        }

        // Burst retreat & shoot in phase 2
        if (isPhaseTwo && !isRetreating && burstTimer >= burstCooldown)
        {
            StartCoroutine(RetreatAndBurst());
            burstTimer = 0f;
        }
    }

    void EnterPhaseTwo()
    {
        isPhaseTwo = true;
        attackCooldown *= 0.75f;
        Debug.Log("Boss has entered Phase 2");
    }

    void SmoothFacePlayer()
    {
        if (enemyPlayerObject == null) return;

        Vector3 direction = enemyPlayerObject.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    protected void BossAttack()
    {
        SmoothFacePlayer();
        bossAnimator.SetTrigger("Attack");
        FireProjectile(Random.value > 0.5f ? damage.damagetype.homing : damage.damagetype.moving);
    }

    IEnumerator Dodge()
    {
        isDodging = true;

        Vector3 toPlayer = (enemyPlayerObject.position - transform.position).normalized;
        Vector3 dodgeDir = Vector3.Cross(toPlayer, Vector3.up) * (Random.value > 0.5f ? 1 : -1);
        Vector3 dodgeTarget = transform.position + dodgeDir * 5f;

        enemyNavAgent.isStopped = false;
        enemyNavAgent.SetDestination(dodgeTarget);
        bossAnimator.SetFloat("Speed", enemyNavAgent.velocity.magnitude);

        float t = 0f;
        while (Vector3.Distance(transform.position, dodgeTarget) > 0.5f && t < 1.5f)
        {
            SmoothFacePlayer();
            t += Time.deltaTime;
            yield return null;
        }

        isDodging = false;
    }

    IEnumerator PerformAOEAttack()
    {
        bossAnimator.SetTrigger("AOE");
        yield return new WaitForSeconds(0.5f);
        if (aoeEffectPrefab)
            Instantiate(aoeEffectPrefab, transform.position, Quaternion.identity);
    }

    IEnumerator RetreatAndBurst()
    {
        isRetreating = true;

        Vector3 retreatDir = -(enemyPlayerObject.position - transform.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDir * 5f;

        enemyNavAgent.isStopped = false;
        enemyNavAgent.SetDestination(retreatTarget);
        bossAnimator.SetFloat("Speed", enemyNavAgent.velocity.magnitude);

        float t = 0f;
        while (Vector3.Distance(transform.position, retreatTarget) > 0.5f && t < 2f)
        {
            SmoothFacePlayer();
            t += Time.deltaTime;
            yield return null;
        }

        enemyNavAgent.isStopped = true;
        bossAnimator.SetFloat("Speed", 0);

        for (int i = 0; i < burstCount; i++)
        {
            SmoothFacePlayer();

            damage.damagetype type = (Random.value > 0.5f)
                ? damage.damagetype.homing
                : damage.damagetype.moving;

            FireProjectile(type);
            yield return new WaitForSeconds(0.5f);
        }

        isRetreating = false;
    }

    public void FireProjectile(damage.damagetype type)
    {
        if (!projectilePrefab || !projectileSpawnPoint) return;

        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        damage dmg = proj.GetComponent<damage>();
        Rigidbody rb = proj.GetComponent<Rigidbody>();

        if (dmg != null)
        {
            dmg.SetDamageType(type);

            if (rb != null)
            {
                // Use AddForce for smooth projectile velocity (no rb.velocity warning)
                rb.AddForce(proj.transform.forward * dmg.speed, ForceMode.VelocityChange);
            }
        }
    }

    protected override void enemyDeath()
    {
        if (isDead) return;

        isDead = true;
        bossAnimator.SetTrigger("IsDead");
        enemyNavAgent.isStopped = true;

        GetComponent<Collider>().enabled = false;
        StartCoroutine(DestroyAfterDeathAnim());

        if (gamemanager.instance.currentBoss == this)
            gamemanager.instance.EndBossFight();
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
                SmoothFacePlayer();
        }
    }

    public void SetPlayerInSight(bool inSight) => enemyPlayerInSight = inSight;

    public override void takeDamage(int amount)
    {
        enemyCurrentHealthPoints -= amount;

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

    protected override IEnumerator enemyFlashRead()
    {
        foreach (var part in enemyModel)
            part.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        foreach (var part in enemyModel)
            part.material.color = enemyColorOrigin;
    }
}