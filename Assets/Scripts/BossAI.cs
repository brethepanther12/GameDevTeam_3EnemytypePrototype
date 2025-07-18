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

    public bool isBeingGrappled { get; set; }

    public bool canBeGrappled => false;

    protected override void Start()
    {
        bossRoarSource.PlayOneShot(roarClip);

        base.Start();
        attackTimer = 0;

        gamemanager.instance.updateGameGoal(+1);
    }

    protected override void Update()
    {
        if (isDead)
        {
            return;
        }

        base.Update();
        attackTimer += Time.deltaTime;
        bossAnimator.SetFloat("Speed", enemyNavAgent.velocity.magnitude);

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
        bossAnimator.SetTrigger("Attack");
        attackTimer = 0f;
    }

    protected override void enemyDeath()
    {
        if (isDead)
        {
            return; 
        }

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
        if (enemyPlayerInSight)
        {
           // Debug.Log("Boss is moving toward player!");
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

        //vUpdate the centralized boss health bar via GameManager
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