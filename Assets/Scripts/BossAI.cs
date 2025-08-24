using System.Collections;
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

    [Header("Phase 1")]
    public float attackCooldownP1 = 1.5f;
    public int shotDamageP1 = 20;
    public float shotSpeedP1 = 14f;
    public int burstCountP1 = 1;
    public float burstSpacing = 0.12f;

    [Header("Phase 2")]
    public float attackCooldownP2 = 0.75f;
    public int burstCountP2 = 4;
    public float rampPerBurst = 0.12f;
    public float maxRamp = 0.8f;

    [Header("Projectile")]
    public FireballProjectile fireballPrefab;
    public Transform firePoint;

    [Header("Projectile Timing")]
    public float attackDelay = 0.5f; // delay to sync with throw animation

    int currentHealth;
    bool isDeadFlag;
    bool phase2;
    float ramp;
    float nextAttackTime;

    NavMeshAgent agent;
    Animator anim;

    bool isDodging;
    bool isAttacking;
    bool isBursting;

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
        StartCoroutine(ShuffleRoutine());

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
        if (isDeadFlag || player == null) return;

        if (!phase2 && currentHealth <= maxHealth / 2)
            EnterPhase2();

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= chaseRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (dist <= stopDistance)
                agent.isStopped = true;

            anim.SetBool("isMoving", !agent.isStopped);

            Vector3 to = (player.position - transform.position);
            to.y = 0f;
            if (to.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(to);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
            Debug.Log("Boss Speed: " + agent.velocity.magnitude); //debugger
        }
        else
        {
            agent.isStopped = true;
            anim.SetBool("isMoving", false);
        }

        // Attack logic
        if (dist <= attackRange && Time.time >= nextAttackTime && !isDodging)
        {
            isAttacking = true;
            anim.SetTrigger("Attack");
            StartCoroutine(AttackWithDelay());
            nextAttackTime = Time.time + (phase2 ? attackCooldownP2 : attackCooldownP1);

            // occasional dodge
            if (Random.value < 0.25f)
                StartCoroutine(DodgeRoutine());
        }
        else if (!isAttacking && !isDodging && dist <= chaseRange)
        {
            StrafeMovement();
        }
    }

    private IEnumerator AttackWithDelay()
    {
        yield return new WaitForSeconds(attackDelay);

        if (phase2)
            StartCoroutine(DoBurstFire()); // handles phase 2 burst properly
        else
            StartCoroutine(DoBurstFire()); // still used for phase 1 (single fire)
    }

    IEnumerator DoBurstFire()
    {
        if (isBursting) yield break; // prevent stacking
        isBursting = true;

        int shots = phase2 ? burstCountP2 : burstCountP1;

        for (int i = 0; i < shots; i++)
        {
            Vector3 dir = (player.position + Vector3.up * 1.2f - firePoint.position);
            float power = phase2 ? Mathf.Clamp01(1f + ramp) : 1f;

            int dmg = Mathf.RoundToInt(shotDamageP1 * power);
            float spd = shotSpeedP1 * power;

            SpawnFireball(dir, dmg, spd);

            if (i < shots - 1)
                yield return new WaitForSeconds(burstSpacing);
        }

        if (phase2)
            ramp = Mathf.Clamp(ramp + rampPerBurst, 0f, maxRamp);

        isBursting = false;
        isAttacking = false;
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
        if (isDeadFlag) return;

        currentHealth -= amount;
        if (gamemanager.instance != null)
            gamemanager.instance.UpdateBossHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void takeDamage(int amount, StatusEffectData effect)
    {
        takeDamage(amount);
    }

    public void slowDown(float magnitude, float duration)
    {
        if (isDeadFlag) return;
        StopAllCoroutines();
        StartCoroutine(SlowRoutine(magnitude, duration));
    }

    IEnumerator SlowRoutine(float magnitude, float duration)
    {
        float original = agent.speed;
        agent.speed = original * Mathf.Clamp01(1f - magnitude);
        yield return new WaitForSeconds(duration);
        agent.speed = original;
    }

    public bool isDead()
    {
        return isDeadFlag;
    }

    void Die()
    {
        if (isDeadFlag) return;
        isDeadFlag = true;

        // stop everything so nothing can kick us out of Die
        StopAllCoroutines();

        agent.isStopped = true;

        // freeze locomotion params so no transitions try to return to Locomotion
        anim.ResetTrigger("Attack");
        anim.SetBool("isMoving", false);
        anim.SetFloat("MoveX", 0f);
        anim.SetFloat("MoveZ", 0f);

        anim.SetTrigger("Die");             // enter Die state
        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(3f); // set to your death clip length
        if (gamemanager.instance != null)
            gamemanager.instance.TriggerWinScreen();
        Destroy(gameObject, 2f);
    }

    IEnumerator DodgeRoutine()
    {
        if (isDodging) yield break;
        isDodging = true;

        Vector3 dodgeDir = (Random.value > 0.5f ? transform.right : -transform.right);
        float dodgeTime = 0.4f;
        float dodgeSpeed = 15f;

        float t = 0f;
        while (t < dodgeTime)
        {
            transform.position += dodgeDir * dodgeSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

        isDodging = false;
    }

    void StrafeMovement()
    {
        float strafeSpeed = 5f;
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 strafeDir = Vector3.Cross(Vector3.up, toPlayer);

        // alternate sides
        if (Random.value > 0.5f)
            strafeDir *= -1f;

        transform.position += strafeDir * strafeSpeed * Time.deltaTime;

        Quaternion lookRot = Quaternion.LookRotation(toPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
    }

    IEnumerator ShuffleRoutine()
    {
        while (!isDeadFlag)
        {
            yield return new WaitForSeconds(Random.Range(2f, 4f));

            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            Vector3 target = transform.position + randomOffset;

            float t = 0f;
            while (t < 0.5f)
            {
                transform.position = Vector3.Lerp(transform.position, target, t / 0.5f);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, stopDistance);

    }

}