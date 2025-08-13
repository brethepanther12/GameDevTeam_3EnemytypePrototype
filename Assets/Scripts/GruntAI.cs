using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;

public class GruntAi : MonoBehaviour, IDamage, IGrapplable
{

    [SerializeField] SkinnedMeshRenderer[] modelParts;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;

    [SerializeField] float shootRange = 15;
    [SerializeField] private int maxAmmo = 5;
    [SerializeField] private float reloadTime = 3f;

    [SerializeField] Animator animator;

    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private float reloadVolume = 1f;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float deathVolume;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitVolume = 1f;

    [SerializeField] GameObject healthPickupPrefab;
    [SerializeField] GameObject ammoPickupPrefab;
    [SerializeField] float dropChance = 0.5f;

    [SerializeField] int HP;

    [SerializeField] public int shield;
    [SerializeField] public int armor;

    [SerializeField] public GameObject shieldPrefab;
    [SerializeField] public GameObject armorPrefab;

    [SerializeField] int fov;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int roamDistance;
    [SerializeField] int roamPauseTime;

    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float footstepDelay = 0.5f;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;

    Color colorOrig;

    public bool isBeingGrappled { get; set; }
    public bool canBeGrappled => true;
    private Coroutine reloadingRT;
    private bool isDead;
    private int currentAmmo;
    private bool isReloading;
    bool playerInTrigger;
    float shootTimer;
    float angleToPlayer;
    float roamTimer;
    float stoppingDistanceOrig;
    private float footstepTimer;

    Vector3 playerDir;
    Vector3 startingPos;

    private float originalSpeed;
    private Coroutine slowRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        originalSpeed = agent.speed;
        currentAmmo = maxAmmo;
        colorOrig = modelParts[0].material.color;
        gamemanager.instance.updateGameGoal(1);
        startingPos = transform.position;
        stoppingDistanceOrig = agent.stoppingDistance;

    }

    // Update is called once per frame
    void Update()
    {
        if (isDead || isBeingGrappled)
        {
            return;
        }
           
            animator.SetFloat("Speed", agent.velocity.magnitude);

        HandleFootsteps();

        if (agent.remainingDistance < 0.01f)
        {
            roamTimer += Time.deltaTime;
        }

        if (playerInTrigger && !CanSeePlayer())
        {
            RoamCheck();

        }
        else if (!playerInTrigger)
        {
            RoamCheck();
        }
    }

    void LateUpdate()
    {
        shootPos.LookAt(gamemanager.instance.player.transform.position);
    }

    void RoamCheck()
    {
        if (roamTimer >= roamPauseTime && agent.remainingDistance < 0.01f)
        {
            Roam();
        }
    }

    void Roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDistance + startingPos;

        NavMeshHit hit;
        bool foundHit = NavMesh.SamplePosition(ranPos, out hit, roamDistance, NavMesh.AllAreas);

        if (agent.isOnNavMesh && foundHit)
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("Failed to find valid NavMesh position or agent is not on NavMesh.");
        }
    }

    bool CanSeePlayer()
    {
        if (isDead)
        {
            return false;
        }


        playerDir = gamemanager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(headPos.position, playerDir);

        RaycastHit hit;

        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= fov)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, gamemanager.instance.player.transform.position);

                if (distanceToPlayer <= shootRange)
                {
                    shootTimer += Time.deltaTime;

                    if (shootTimer >= shootRate)
                    {
                        Shoot();
                    }

                    if (currentAmmo <= 0 && !isReloading && !isDead)
                    {
                        reloadingRT = StartCoroutine(Reload());
                    }
                }

                if (!isBeingGrappled && agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    if (distanceToPlayer > shootRange)
                    {
                        agent.SetDestination(gamemanager.instance.player.transform.position);
                    }
                    else
                    {
                        agent.ResetPath();
                    }

                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        FaceTarget();
                    }

                    agent.stoppingDistance = stoppingDistanceOrig;
                }

                return true;
            }
        }

        agent.stoppingDistance = 0;
        return false;
    }

    void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceTargetSpeed * Time.deltaTime);

        Quaternion gunRot = Quaternion.LookRotation(gamemanager.instance.player.transform.position - shootPos.position);
        shootPos.rotation = Quaternion.Lerp(shootPos.rotation, gunRot, faceTargetSpeed * Time.deltaTime);
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            agent.stoppingDistance = 0;
        }
    }

    public void takeDamage(int amount)
    {
        if (isDead)
        {
            return;
        }

        if (shield > 0)
        {
            shield -= amount;
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
            agent.SetDestination(gamemanager.instance.player.transform.position);

            if (shield <= 0)
            {
                shield = 0;

                shieldPrefab.SetActive(false);
                armor -= amount;
                AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
                agent.SetDestination(gamemanager.instance.player.transform.position);

            }

        }

        else if (armor > 0)
        {
            armor -= amount;

            if (armor <= 0 && shield <= 0)
            {

                armor = 0;
                shield = 0;
                armorPrefab.SetActive(false);
                AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
                agent.SetDestination(gamemanager.instance.player.transform.position);
            }
        }
        else
        {
            HP -= amount;
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
            agent.SetDestination(gamemanager.instance.player.transform.position);
        }


        if (HP <= 0)
        {
            isDead = true;


            if (reloadingRT != null)
            {
                StopCoroutine(reloadingRT);
                reloadingRT = null;
            }

            animator.SetBool("isdead", true);
            StartCoroutine(Die());
            ScoreManager.instance.AddPointsForEnemy(gameObject.tag);

        }
        else
        {

            StartCoroutine(FlashRed());
        }
    }

    public void takeDamage(int amount, StatusEffectData effect)
    {
        switch (effect.statusType)
        {

            case DamageStatus.None:

                takeDamage(amount);
                break;

            case DamageStatus.Fire:

                if (shield <= 0 && armor <= 0 && HP > 0)
                {
                    takeDamage(amount + 1);
                }
                else
                {
                    takeDamage(amount);
                }
                break;

            case DamageStatus.Corrosive:

                if (shield <= 0 && armor > 0)
                {
                    takeDamage(amount + 1);
                }
                else
                {
                    takeDamage(amount);
                }
                break;

            case DamageStatus.Cryo:

                takeDamage(amount);
                break;

            default:
                break;
        }



    }

    IEnumerator FlashRed()
    {
        foreach (var part in modelParts)
        {
            part.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var part in modelParts)
        {
            part.material.color = colorOrig;
        }
    }

    void Shoot()
    {
        if (isReloading || currentAmmo <= 0)
        {
            return;
        }

        shootTimer = 0;

        currentAmmo--;

        animator.SetTrigger("Shoot");

        Instantiate(bullet, shootPos.position, transform.rotation);

        AudioSource.PlayClipAtPoint(shootSound, shootPos.position);
    }

    IEnumerator Reload()
    {
        if (isDead)
        {
            yield break;
        }
        isReloading = true;
        AudioSource.PlayClipAtPoint(reloadSound, transform.position, reloadVolume);
        animator.SetTrigger("Reload");


        yield return new WaitForSeconds(reloadTime - .25f);

        if (isDead)
        {
            yield break;
        }

        isReloading = false;
        yield return new WaitForSeconds(.25f);
        currentAmmo = maxAmmo;
    }

    IEnumerator Die()
    {
        isReloading = false;
        animator.ResetTrigger("Reload");

        isDead = true;
        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
        animator.Play("Death", 0, 0);

        AudioSource.PlayClipAtPoint(deathSound, transform.position);

        yield return new WaitForSeconds(3.5f);

        gamemanager.instance.updateGameGoal(-1);

        TryDropPickup();

        Destroy(gameObject);
    }

    void TryDropPickup()
    {
        float roll = Random.value; // 0 to 1
        if (roll <= dropChance)
        {
            int itemType = Random.Range(0, 2); // 0 = health, 1 = ammo

            GameObject drop = null;
            if (itemType == 0 && healthPickupPrefab != null)
            {
                drop = Instantiate(healthPickupPrefab, transform.position + Vector3.up, Quaternion.identity);
            }
            else if (itemType == 1 && ammoPickupPrefab != null)
            {
                drop = Instantiate(ammoPickupPrefab, transform.position + Vector3.up, Quaternion.identity);
            }
        }
    }

    void HandleFootsteps()
    {
        bool isMoving = agent.velocity.magnitude > 0.2f && agent.remainingDistance > agent.stoppingDistance;

        if (isMoving)
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= footstepDelay)
            {
                PlayFootstep();
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClip != null && footstepSource != null)
        {
            footstepSource.pitch = Random.Range(0.95f, 1.05f);
            footstepSource.PlayOneShot(footstepClip);
        }
    }

    public void FootStep()
    {
        PlayFootstep();
    }

    public void slowDown(float magnitude, float duration)
    {
        if (slowRoutine != null)
        {
            StopCoroutine(slowRoutine);
        }

        slowRoutine = StartCoroutine(SlowRoutine(magnitude, duration));
    }

    private IEnumerator SlowRoutine(float magnitude, float duration)
    {
        if (agent == null) yield break;

        if (originalSpeed == 0f)
            originalSpeed = agent.speed;

        float slowedSpeed = originalSpeed * (1f - magnitude);
        agent.speed = slowedSpeed;

        yield return new WaitForSeconds(duration);

        agent.speed = originalSpeed;
        slowRoutine = null;
    }
}
