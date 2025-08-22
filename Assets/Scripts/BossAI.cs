using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class BossAI : MonoBehaviour, IDamage
{
    [Header("Boss Info")]
    public string bossName = "Mutant Boss";
    public int maxHealth = 500;

    [Header("Targeting")]
    public Transform player;
    public float chaseRange = 40f;
    public float attackRange = 18f; 
    public float stopDistance = 10f; 
    public float turnSpeed = 720f;

    [Header("Phase 1 (baseline)")]
    public float attackCooldownP1 = 1.5f;
    public int shotDamageP1 = 20;
    public float shotSpeedP1 = 14f;
    public int burstCountP1 = 1;
    public float burstSpacing = 0.12f;

    [Header("Phase 2 (enraged)")]
    public float attackCooldownP2 = 0.75f;
    public int burstCountP2 = 4;
    public float rampPerBurst = 0.12f;     // each burst increases power/speed
    public float maxRamp = 0.8f;           // cap the ramp

    [Header("Projectile")]
    public FireballProjectile fireballPrefab;
    public Transform firePoint;

    // Runtime
    int currentHealth;
    bool isDead;
    bool phase2;
    float ramp;             // scales damage/speed in phase 2
    float nextAttackTime;

    NavMeshAgent agent;
    Animator anim;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;

        agent.stoppingDistance = stopDistance;
        agent.updateRotation = false;
    }

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (gamemanager.instance != null)
        {
            if (gamemanager.instance.BossHealthBarUI)
                gamemanager.instance.BossHealthBarUI.SetActive(true);
            if (gamemanager.instance.BossNameText)
                gamemanager.instance.BossNameText.text = bossName;

            gamemanager.instance.UpdateBossHealthBar(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Phase swap
        if (!phase2 && currentHealth <= maxHealth / 2)
            EnterPhase2();

        float dist = Vector3.Distance(transform.position, player.position);

        // Movement + facing
        if (dist <= chaseRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            // keep distance
            if (dist <= stopDistance)
                agent.isStopped = true;

            anim.SetBool("isMoving", agent.isStopped ? false : true);

            Vector3 to = (player.position - transform.position);
            to.y = 0f;
            if (to.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(to);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }
        else
        {
            agent.isStopped = true;
            anim.SetBool("isMoving", false);
        }

        // Attack window (fixed to shoot without animation events)
        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            anim.SetTrigger("Attack");             // plays attack animation
            StartCoroutine(DoBurstFire());         // shoot projectiles directly
            nextAttackTime = Time.time + (phase2 ? attackCooldownP2 : attackCooldownP1);
        }
    }
    // Called by an Animation Event on the Attack clip
    public void AnimEvent_AttackShoot()
    {
        if (isDead || player == null) return;
        StartCoroutine(DoBurstFire());
    }

    System.Collections.IEnumerator DoBurstFire()
    {
        int shots = phase2 ? burstCountP2 : burstCountP1;

        for (int i = 0; i < shots; i++)
        {
            Vector3 dir = (player.position + Vector3.up * 1.2f - firePoint.position); // aim chest-height
            float power = phase2 ? Mathf.Clamp01(1f + ramp) : 1f;

            int dmg = Mathf.RoundToInt((phase2 ? shotDamageP1 : shotDamageP1) * power);
            float spd = (phase2 ? shotSpeedP1 : shotSpeedP1) * power;

            SpawnFireball(dir, dmg, spd);

            if (i < shots - 1) yield return new WaitForSeconds(burstSpacing);
        }

        if (phase2)
            ramp = Mathf.Clamp(ramp + rampPerBurst, 0f, maxRamp);
    }

    void SpawnFireball(Vector3 dir, int damage, float speed)
    {
        if (fireballPrefab == null || firePoint == null) return;

        var fb = Instantiate(fireballPrefab, firePoint.position, Quaternion.LookRotation(dir));
        fb.Init(dir, damage, speed, transform);
    }

    void EnterPhase2()
    {
        phase2 = true;
        ramp = 0f;
    }

    public void takeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (gamemanager.instance != null)
            gamemanager.instance.UpdateBossHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    public void takeDamage(int amount, StatusEffectData effect)
    {
        takeDamage(amount);
    }

    public void slowDown(float magnitude, float duration)
    {
        if (isDead) return;
        StopAllCoroutines();
        StartCoroutine(SlowRoutine(magnitude, duration));
    }

    System.Collections.IEnumerator SlowRoutine(float magnitude, float duration)
    {
        float original = agent.speed;
        agent.speed = original * Mathf.Clamp01(1f - magnitude);
        yield return new WaitForSeconds(duration);
        agent.speed = original;
    }

    void Die()
    {
        isDead = true;
        agent.isStopped = true;
        anim.SetTrigger("Die");

        if (gamemanager.instance != null)
            gamemanager.instance.TriggerWinScreen();

        Destroy(gameObject, 5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, stopDistance);
    }


}