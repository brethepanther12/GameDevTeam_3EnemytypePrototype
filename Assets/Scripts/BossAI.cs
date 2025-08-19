using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : EnemyAIBase, IGrapplable
{
    [Header("Boss Settings")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float attackRange = 30f;
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
    //private bool isDead = false;
    private bool isDodging = false;
    private bool isRetreating = false;
    private bool isPhaseTwo = false;
    private float detectionRange = 60f;

    private float phaseTwoHealthThreshold => enemyHealthPointsMax * 0.5f;

    public bool isBeingGrappled { get; set; }
    public bool canBeGrappled => false;

    protected override void Start()
    {
        base.Start();
        bossAnimator.applyRootMotion = false;
        //bossAnimator.applyRootMotion = false;
        if (bossRoarSource && roarClip)
            bossRoarSource.PlayOneShot(roarClip);

        StartCoroutine(DelayedNavStart());

        gamemanager.instance.updateGameGoal(+1);
    }

    IEnumerator DelayedNavStart()
    {
        yield return null;
        enemyNavAgent.enabled = true;
        enemyNavAgent.updateRotation = false;
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
            float desiredDistance = Mathf.Max(attackRange - 1.5f, 2f); // closes in better

            if (distanceToPlayer > desiredDistance)
            {
                MoveTowardPlayer(desiredDistance);
            }
            else
            {
                bossAnimator.SetFloat("Speed", 0f); // stop running when attacking
                if (attackTimer >= attackCooldown)
                {
                    BossAttack();
                    attackTimer = 0f;
                }
                else if (Random.value < 0.05f)
                {
                    StartCoroutine(Dodge());
                }
            }
        }

        if (enemyNavAgent.enabled && enemyNavAgent.velocity.magnitude > 0.1f && !isDodging && !isRetreating)
        {
            Quaternion targetRot = Quaternion.LookRotation(enemyNavAgent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 7f);
        }

        bossAnimator.SetFloat("Speed", enemyNavAgent.velocity.magnitude); // makes idle/walk/run work

        // Timers
        attackTimer += Time.deltaTime;
        aoeTimer += Time.deltaTime;
        burstTimer += Time.deltaTime;

        if (isPhaseTwo && distanceToPlayer < aoeRange && aoeTimer >= aoeCooldown)
        {
            StartCoroutine(PerformAOEAttack());
            aoeTimer = 0f;
        }

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

    void MoveTowardPlayer(float desiredDistance)
    {
        if (enemyPlayerObject == null) return;

        enemyNavAgent.isStopped = false;

        Vector3 targetPos = enemyPlayerObject.position - (enemyPlayerObject.position - transform.position).normalized * desiredDistance;
        enemyNavAgent.SetDestination(targetPos);
    }

    void SmoothFacePlayer()
    {
        enemyNavAgent.isStopped = false;
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

    public void FireProjectile(damage.damagetype type)
    {
        if (!projectilePrefab || !projectileSpawnPoint) return;

        Vector3 target = enemyPlayerObject.GetComponent<Collider>()?.bounds.center ?? enemyPlayerObject.position;
        Vector3 shootDir = (target - projectileSpawnPoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(shootDir);

        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, rot);
        damage dmg = proj.GetComponent<damage>();
        Rigidbody rb = proj.GetComponent<Rigidbody>();

        if (dmg != null)
        {
            dmg.SetDamageType(type);
            if (rb != null)
                rb.AddForce(shootDir * dmg.speed, ForceMode.VelocityChange);
        }
    }

    IEnumerator Dodge()
    {
        enemyNavAgent.isStopped = false;
        isDodging = true;

        Vector3 dodgeDir = Vector3.Cross((enemyPlayerObject.position - transform.position).normalized, Vector3.up) *
                           (Random.value > 0.5f ? 1 : -1);
        Vector3 dodgeTarget = transform.position + dodgeDir * 5f;

        enemyNavAgent.SetDestination(dodgeTarget);

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
        enemyNavAgent.isStopped = false;
        isRetreating = true;

        Vector3 retreatDir = -(enemyPlayerObject.position - transform.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDir * 5f;

        enemyNavAgent.SetDestination(retreatTarget);

        float t = 0f;
        while (Vector3.Distance(transform.position, retreatTarget) > 0.5f && t < 2f)
        {
            SmoothFacePlayer();
            t += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < burstCount; i++)
        {
            SmoothFacePlayer();
            FireProjectile(Random.value > 0.5f ? damage.damagetype.homing : damage.damagetype.moving);
            yield return new WaitForSeconds(0.5f);
        }

        isRetreating = false;
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

    public void SetPlayerInSight(bool inSight) => enemyPlayerInSight = inSight;

    public override void takeDamage(int amount)
    {


        if (shield > 0)
        {
            shield -= amount;
            if (gamemanager.instance.currentBoss == this)
                gamemanager.instance.UpdateBossHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);

            if (shield <= 0)
            {
                shield = 0;

                shieldPrefab.SetActive(false);
                armor -= amount;
                if (gamemanager.instance.currentBoss == this)
                    gamemanager.instance.UpdateBossHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);
            }

        }

        else if (armor > 0)
        {
            armor -= amount;
            if (gamemanager.instance.currentBoss == this)
                gamemanager.instance.UpdateBossHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);

            if (armor <= 0 && shield <= 0)
            {

                armor = 0;
                shield = 0;
                armorPrefab.SetActive(false);
                if (gamemanager.instance.currentBoss == this)
                    gamemanager.instance.UpdateBossHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);
            }
        }
        else
        {
            enemyCurrentHealthPoints -= amount;
            if (gamemanager.instance.currentBoss == this)
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