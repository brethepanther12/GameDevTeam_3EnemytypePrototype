using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;

public class Enemy : MonoBehaviour, IDamage, IGrapplable, Visibility, IEnemyAI
{

    [SerializeField] SkinnedMeshRenderer[] modelParts;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;

    [SerializeField] float shootRange =15;
    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private float reloadTime = 1.5f;

    [SerializeField] Animator animator;

    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float footstepDelay = 0.5f;

    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private float reloadVolume = 1f;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float deathVolume;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitVolume = 1f;

    [SerializeField] int HP;
    [SerializeField] int shield;
    [SerializeField] int armor;
    [SerializeField] int fov;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int roamDistance;
    [SerializeField] int roamPauseTime;

    public GameObject componentPrefab;
    public GameObject mutagenPickupPrefab;
    [SerializeField] GameObject healthPickupPrefab;
    [SerializeField] GameObject ammoPickupPrefab;
    [SerializeField] GameObject shieldPrefab;
    [SerializeField] GameObject armorPrefab;
  //  [SerializeField] float dropChance = 0.5f;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] StatusEffectData statusEffectData;

    [Header("AI - Call for Help")]
    [SerializeField] private float helpRadius = 15f;
    [SerializeField] private LayerMask enemyLayer;
    private bool hasCalledForHelp = false;
    private Transform playerTarget;

    Color colorOrig;

    public bool isBeingGrappled { get; set; }
    public bool canBeGrappled => true;
    private float footstepTimer;
    private Coroutine reloadingRT;
    public bool isDead;
    private int currentAmmo;
    private bool isReloading;
    bool playerInTrigger;
    float shootTimer;
    float angleToPlayer;
    float roamTimer;
    float stoppingDistanceOrig;

    private float originalSpeed;
    private Coroutine slowRoutine;

    //private List<Collider> smokeZone = new List<Collider>();
    bool IsBlind;

    Vector3 playerDir;
    Vector3 startingPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalSpeed = agent.speed;
        shootTimer = 0f;
        currentAmmo = maxAmmo;
        colorOrig = modelParts[0].material.color;
        gamemanager.instance.updateGameGoal(1);
        startingPos = transform.position;
        stoppingDistanceOrig = agent.stoppingDistance;

        if(shield == 0)
        {
            shieldPrefab.SetActive(false);
        }

        if (armor == 0)
        {
            armorPrefab.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isDead || isBeingGrappled)
            return;

        if (!isReloading)
        {
            animator.ResetTrigger("Reload");
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);

        HandleFootsteps();

        if (agent.remainingDistance < 0.01f)
            roamTimer += Time.deltaTime;

        if (!playerInTrigger || (playerInTrigger && !CanSeePlayer()))
            RoamCheck();
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

        Vector3 ranPos = Random.insideUnitSphere * roamDistance;
        ranPos += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDistance, 1);
        agent.SetDestination(hit.position);

    }

    bool CanSeePlayer()
    {
        if (isDead || IsBlind)
        {
            return false;
        }
           

        playerDir = gamemanager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

      //  Debug.DrawRay(headPos.position, playerDir);

        RaycastHit hit;

        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Smoke"))
                return false;
            
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

                    if (currentAmmo <= 0 && !isReloading && !isDead && reloadingRT == null)
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

                    return true;
                }
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
        //else if (other.CompareTag("Smoke"))
        //{
        //    smokeZone.Add(other);
        //}
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            agent.stoppingDistance = 0;
        }
        //else if (other.CompareTag("Smoke"))
        //{
        //    smokeZone.Remove(other);
        //}
        //
    }

    public void takeDamage(int amount)
    {

        if (isDead || amount <= 0)
            return;

        int remainingDamage = amount;

        if (!hasCalledForHelp)
        {
            playerTarget = gamemanager.instance.player.transform;
            CallForHelp();
        }

        if (shield > 0)
        {
            int damageToShield = Mathf.Min(remainingDamage, shield);
            shield -= damageToShield;
            remainingDamage -= damageToShield;

            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
            agent.SetDestination(gamemanager.instance.player.transform.position);

            if (shield <= 0)
            {
                shield = 0;
                shieldPrefab.SetActive(false);
            }
        }

        if (remainingDamage > 0 && armor > 0)
        {
            int damageToArmor = Mathf.Min(remainingDamage, armor);
            armor -= damageToArmor;
            remainingDamage -= damageToArmor;

            if (armor <= 0)
            {
                armor = 0;
                armorPrefab.SetActive(false);
            }

            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
            agent.SetDestination(gamemanager.instance.player.transform.position);
        }

        if (remainingDamage > 0)
        {
            HP -= remainingDamage;
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
            agent.SetDestination(gamemanager.instance.player.transform.position);
        }

        if (HP <= 0)
        {
            shieldPrefab.SetActive(false);
            armorPrefab.SetActive(false);
            isDead = true;

            if (reloadingRT != null)
            {
                StopCoroutine(reloadingRT);
                reloadingRT = null;
            }

            animator.SetBool("isdead", true);
            animator.ResetTrigger("Reload");
            animator.CrossFade("Death", 0f);
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
                break;

            case DamageStatus.Corrosive:

                if (shield <= 0 && armor > 0)
                {
                    takeDamage(amount + 1);
                }
                    break;

            case DamageStatus.Cryo:

                takeDamage(amount);
                break;

            case DamageStatus.Electric:

                if (shield > 0)
                {
                    takeDamage(amount + 1);
                }
                else
                {
                    takeDamage(amount);
                }
                break;

            case DamageStatus.Explosive:

                takeDamage(amount);
                break;

            case DamageStatus.Plasma:

                takeDamage(amount + 1);
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
        if(isReloading || currentAmmo <=0)
        {
            return;
        }

        shootTimer = 0;

        currentAmmo--;

        animator.SetTrigger("Shoot");

        Instantiate(bullet, shootPos.position, transform.rotation);

        AudioSource.PlayClipAtPoint(shootSound, shootPos.position);

        damage dmgScript = bullet.GetComponent<damage>();

        if (statusEffectData.statusType != DamageStatus.None)
        {
            dmgScript.SetStatusData(statusEffectData);
        }
    }

    IEnumerator Reload()
    {
        if (isDead || isReloading)
            yield break;

        isReloading = true;
        animator.SetTrigger("Reload");

        AudioSource.PlayClipAtPoint(reloadSound, transform.position, reloadVolume);

        float timer = 0f;
        while (timer < reloadTime)
        {
            if (isDead)
            {
                isReloading = false; // safety reset
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        currentAmmo = maxAmmo;
        isReloading = false;
        reloadingRT = null;
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
            int itemType = Random.Range(0, 4); // 0 = health, 1 = ammo, 2 = mutagen, 3 = component.

            GameObject drop = null;
        if (itemType == 0 && healthPickupPrefab != null)
        {
            drop = Instantiate(healthPickupPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        else if (itemType == 1 && ammoPickupPrefab != null)
        {
            drop = Instantiate(ammoPickupPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        else if (itemType == 2 && mutagenPickupPrefab != null)
        {
            drop = Instantiate(mutagenPickupPrefab, transform.position + Vector3.up, Quaternion.identity);        
        }
        else if (itemType == 3 && componentPrefab != null)
        {
            drop = Instantiate(componentPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
    }

    public void FootStep()
    {
        PlayFootstep();
    }

    

    public void SetInvisible(bool state)
    {
        IsBlind = state;

        foreach (var part in modelParts)
        {
            part.material.color = state ? Color.gray : colorOrig;
        }
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
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent == null) yield break;

        if (originalSpeed == 0f)
            originalSpeed = agent.speed;

        float slowedSpeed = originalSpeed * (1f - magnitude);
        agent.speed = slowedSpeed;

        yield return new WaitForSeconds(duration);

        agent.speed = originalSpeed;
        slowRoutine = null;
    }

    private void CallForHelp()
    {
        hasCalledForHelp = true;
        //Debug.Log(gameObject.name + " is calling for help!");

        Collider[] nearbyAllies = Physics.OverlapSphere(transform.position, helpRadius, enemyLayer);

        //Debug.Log("Found " + nearbyAllies.Length + " potential allies in range.");

        foreach (Collider allyCollider in nearbyAllies)
        {

            //Debug.Log("Checking ally: " + allyCollider.name + " on layer: " + LayerMask.LayerToName(allyCollider.gameObject.layer));

            if (allyCollider.gameObject == this.gameObject) continue;

            IEnemyAI allyAI = allyCollider.GetComponent<IEnemyAI>();
            if (allyAI != null)
            {
                allyAI.RespondToHelpCall(playerTarget);
            }
            else
            {

                //Debug.LogWarning(allyCollider.name + " is on the Enemy layer but is missing the 'Enemy' script!");
            }
        }
    }

    public void RespondToHelpCall(Transform target)
    {

        if (hasCalledForHelp || isDead)
        {
            return;
        }

        //Debug.Log(gameObject.name + " is responding to a help call!");

        playerTarget = target;
        hasCalledForHelp = true;

        if (agent.isActiveAndEnabled)
        {
            agent.SetDestination(playerTarget.position);
        }
    }

    bool IDamage.isDead()
    {
        return isDead;
    }
}
