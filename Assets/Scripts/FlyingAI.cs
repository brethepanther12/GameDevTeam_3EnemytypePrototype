using System.Collections;
using UnityEditor;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.LowLevel;
using static UnityEngine.Rendering.DebugUI;
public class FlyingAI : MonoBehaviour, IDamage, Visibility
{
    [Header("--- Target & Timers ---")]
    [SerializeField] private Transform target;
    [SerializeField] private float lostPlayDelay;
    private GameObject playerTarget;
    private float playerLostTimer;

    [Header("--- Flying & Rotation ---")]
    [SerializeField] private float flyingSpeed;
    [SerializeField] private float rotationSpeed;
    private Vector3 playerDirection;
    [SerializeField] private Rigidbody rigidBody;

    [Header("--- Damage ---")]
    [SerializeField] private float damageRate;
    [SerializeField] private int damageAmount;
    [SerializeField] private float attackRange;
    private bool isDamaging;
    private IDamage iDamage;

    [Header("--- Shooting ---")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootPos;
    [SerializeField] private float shootRange;
    [SerializeField] private float shootRate;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float reloadTime;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private Animator animator;
    [SerializeField] private StatusEffectData statusEffectData;

    private int currentAmmo;
    private float shootTimer;
    private bool isReloading;
    private Coroutine reloadingRT;

    [Header("--- Hover ---")]
    [SerializeField] private float hoverHeight;
    [SerializeField] private float hoverClamp;

    [Header("--- Strafing ---")]
    [SerializeField] private float strafeSpeed;
    [SerializeField] private float strafeCooldown;
    private float strafeTimer;
    private Vector3 strafeDirection;

    [Header("--- Retreat ---")]
    [SerializeField] private float retreatSpeed;
    [SerializeField] private float retreatDistance;
    private bool isRetreating;
    private float retreatTimer;
    private Vector3 retreatDirection;

    [Header("--- Ceiling ---")]
    [SerializeField] private float ceilingInRadius;
    [SerializeField] private float ceilingAttachmentRange;
    [SerializeField] private LayerMask ceilingMask;
    [SerializeField] private SphereCollider bodyCollider;
    private bool attachedToCeiling;
    private Vector3 ceilingPoint;

    [Header("--- Field of View ---")]
    [SerializeField] private float fovDistance;
    [SerializeField] private float fovAngle;
    [SerializeField] private LayerMask environmentMask;
    private bool playerVisible;
    private bool InRange;
    private bool isBlind;

    [Header("--- Health ---")]
    [SerializeField] private int HP;
    private int currentHP;
    private bool Dead;
    [SerializeField] public int shield;
    [SerializeField] public int armor;
    [SerializeField] public GameObject shieldPrefab;
    [SerializeField] public GameObject armorPrefab;

    [Header("--- Audio ---")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float hitVolume;
    [SerializeField] private float deathVolume ;


    [Header("--- Drops ---")]
    public GameObject healthPickupPrefab;
    public GameObject ammoPickupPrefab;
    public GameObject mutagenPickupPrefab;
    public GameObject componentPrefab;

    [Header("--- Model ---")]
    [SerializeField] private Renderer modelRender;
    private Color originColor;

    // Drone state
    private enum DroneState { Idle, Chasing, Retreating, ReturningToCeiling }
    private DroneState currentState;

    //Slowdown
    private float originalSpeed;
    private Coroutine slowRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = HP;
		currentAmmo = maxAmmo;
        originalSpeed = flyingSpeed;
        gamemanager.instance.updateGameGoal(1); //DO NOT REMOVE THIS FOR ANY REASON. IT HAS SINGLE-HANDEDLY MADE OUR LAST BUILD UNPLAYABLE AND ALMOST DID THE FIRST TIME TOO

        if (shield == 0)
        {
            shieldPrefab.SetActive(false);
        }

        if (armor == 0)
        {
            armorPrefab.SetActive(false);
        }
        // Store original material color
        if (modelRender != null)
            originColor = modelRender.material.color;
		
        if (rigidBody == null) rigidBody = GetComponent<Rigidbody>();

        playerTarget = gamemanager.instance.player;
        rigidBody.isKinematic = false;
    }

    void FixedUpdate()
    {
        if (Dead) return;

        UpdateVisibility();
        PlayerLost();
        AssignTarget();

        Hover();
        Movement();
        HandleShooting();
    }

    private void Movement()
    {
        switch (currentState)
        {
            case DroneState.Chasing:
                if (target != null) MoveTowardsPlayer();
                break;
            case DroneState.Retreating:
                Retreat();
                break;
            case DroneState.ReturningToCeiling:
                MoveToCeiling();
                break;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (target == null || rigidBody == null) return;

        if (rigidBody.isKinematic) rigidBody.isKinematic = false;

        Vector3 direction = (target.position - transform.position);
        Vector3 horizontalDir = new Vector3(direction.x, 0, direction.z).normalized;

        Vector3 horizontalVelocity = horizontalDir * flyingSpeed + Strafing(horizontalDir);
        float verticalVelocity = rigidBody.linearVelocity.y;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, hoverHeight * 2f))
        {
            float heightError = hoverHeight - hit.distance;
            float proportionalLift = heightError * hoverClamp;
            float damping = -rigidBody.linearVelocity.y * 0.5f;
            verticalVelocity = proportionalLift + damping;
        }

        rigidBody.linearVelocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
        FaceTarget();
    }

    private Vector3 Strafing(Vector3 direction)
    {
        strafeTimer -= Time.fixedDeltaTime;
        if (strafeTimer <= 0f)
        {
            strafeTimer = strafeCooldown;
            Vector3 strafe = Vector3.Cross(Vector3.up, direction).normalized;
            strafeDirection = Random.value > 0.5f ? strafe : -strafe;
        }
        return strafeDirection * strafeSpeed;
    }

    private void Retreat()
    {
        if (rigidBody.isKinematic) rigidBody.isKinematic = false;

        retreatTimer -= Time.fixedDeltaTime;

        if (strafeDirection == Vector3.zero)
        {
            Vector3 strafe = Vector3.Cross(Vector3.up, retreatDirection).normalized;
            strafeDirection = Random.value > 0.5f ? strafe : -strafe;
        }

        Vector3 horizontalVelocity = (retreatDirection + strafeDirection).normalized * retreatSpeed;
        rigidBody.linearVelocity = new Vector3(horizontalVelocity.x, rigidBody.linearVelocity.y, horizontalVelocity.z);

        Quaternion targetRotation = Quaternion.LookRotation(retreatDirection);
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

        if (retreatTimer <= 0f || Vector3.Distance(transform.position, playerTarget.transform.position) >= retreatDistance)
        {
            strafeDirection = Vector3.zero;
            rigidBody.linearVelocity = Vector3.zero;
            NearestCeiling();
            currentState = DroneState.ReturningToCeiling;
        }
    }

    private void AssignTarget()
    {
        if (Dead) return;

        InRange = playerTarget && Vector3.Distance(transform.position, playerTarget.transform.position) <= fovDistance;

        if (!isRetreating && (playerVisible || InRange))
        {
            target = playerTarget.transform;
            attachedToCeiling = false;
            currentState = DroneState.Chasing;
            playerLostTimer = 0f;

            if (rigidBody.isKinematic)
            {
                rigidBody.isKinematic = false;
                rigidBody.linearVelocity = Vector3.zero;
                rigidBody.angularVelocity = Vector3.zero;
            }
        }
        else if (!isRetreating && currentState != DroneState.ReturningToCeiling)
        {
            target = null;
            NearestCeiling();
            currentState = DroneState.ReturningToCeiling;
        }
    }

    private void UpdateVisibility()
    {
        if (isBlind)
        {
            playerVisible = false;
            target = null;
        }
        else
        {
            playerVisible = PlayerInFieldOfView();
        }
    }

    private bool PlayerInFieldOfView()
    {
        if (!playerTarget) return false;

        Vector3 dir = playerTarget.transform.position - transform.position;
        float distance = dir.magnitude;

        if (distance > fovDistance) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > fovAngle) return false;

        if (Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, fovDistance, environmentMask | (1 << LayerMask.NameToLayer("Player"))))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    private void PlayerLost()
    {
        if (target == null) playerLostTimer += Time.fixedDeltaTime;
        else playerLostTimer = 0f;

        if (playerLostTimer >= lostPlayDelay && !playerVisible && !isRetreating)
        {
            NearestCeiling();
            currentState = DroneState.ReturningToCeiling;
        }
    }

    private void NearestCeiling()
    {
        attachedToCeiling = false;

        Collider[] hits = Physics.OverlapSphere(transform.position, ceilingInRadius, ceilingMask);
        float best = Mathf.Infinity;
        Vector3 bestPoint = transform.position + Vector3.up * hoverHeight;

        foreach (var c in hits)
        {
            Vector3 fromAbove = c.bounds.center + Vector3.up * (c.bounds.extents.y + 0.1f);
            if (c.Raycast(new Ray(fromAbove, Vector3.down), out RaycastHit h, c.bounds.size.y + 1f))
            {
                float d = (h.point - transform.position).sqrMagnitude;
                if (d < best) { best = d; bestPoint = h.point; }
            }
        }
        ceilingPoint = bestPoint;
    }

    private void MoveToCeiling()
    {
        if (attachedToCeiling) return;

        float radius = bodyCollider != null ? bodyCollider.radius : 0.5f;
        float dangleOffset = 1f;
        Vector3 targetPos = ceilingPoint - Vector3.up * (radius + dangleOffset);

        if (!rigidBody.isKinematic)
        {
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.isKinematic = true;
        }

        rigidBody.MovePosition(Vector3.MoveTowards(
            rigidBody.position,
            targetPos,
            flyingSpeed * Time.fixedDeltaTime
        ));

        if (Vector3.Distance(rigidBody.position, targetPos) < 0.05f)
        {
            attachedToCeiling = true;
            rigidBody.position = targetPos;
            currentState = DroneState.Idle;
        }
    }

    private void Hover()
    {
        if (attachedToCeiling || rigidBody.isKinematic) return;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, hoverHeight * 2f))
        {
            float heightError = hoverHeight - hit.distance;
            float lift = heightError * hoverClamp - rigidBody.linearVelocity.y * 0.5f;
            rigidBody.AddForce(Vector3.up * lift, ForceMode.Acceleration);
        }
    }

    private void FaceTarget()
    {
        if (target == null) return;

        playerDirection = (target.position - transform.position).normalized;
        if (playerDirection.sqrMagnitude > 0.01f)
        {
            Quaternion rotate = Quaternion.LookRotation(playerDirection);
            transform.rotation = Quaternion.Lerp(rigidBody.rotation, rotate, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    private void HandleShooting()
    {
        if (currentState != DroneState.Chasing || target == null || isReloading) return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > fovDistance) return;

        shootTimer += Time.fixedDeltaTime;

        if (shootTimer >= shootRate && currentAmmo > 0)
        {
            Shoot();
        }
        else if (currentAmmo <= 0 && reloadingRT == null)
        {
            reloadingRT = StartCoroutine(Reload());
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || shootPos == null) return;

        currentAmmo--;
        shootTimer = 0f;

        Instantiate(bulletPrefab, shootPos.position, shootPos.rotation);

        if (shootSound != null)
            AudioSource.PlayClipAtPoint(shootSound, shootPos.position);
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        if (reloadSound != null)
            AudioSource.PlayClipAtPoint(reloadSound, transform.position);

        float timer = 0f;
        while (timer < reloadTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        currentAmmo = maxAmmo;
        isReloading = false;
        reloadingRT = null;
    }


    public void takeDamage(int amount)
    {

        if (Dead || amount <= 0)
            return;

        int remainingDamage = amount;

        if (shield > 0)
        {
            int damageToShield = Mathf.Min(remainingDamage, shield);
            shield -= damageToShield;
            remainingDamage -= damageToShield;

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
        }

        if (remainingDamage > 0)
        {
            currentHP -= remainingDamage;
            if (currentHP < 0) currentHP = 0;
        }

        AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
        StartCoroutine(FlashRed());

        if (currentHP <= 0)
        {
            Die();
            ScoreManager.instance.AddPointsForEnemy(gameObject.tag);

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
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            int bulletDmg = 0;

            damage dmgScript = other.gameObject.GetComponent<damage>();
            if (dmgScript != null)
            {
                if (dmgScript.weaponDMG > 0)
                    bulletDmg = dmgScript.damageAmount + dmgScript.weaponDMG;
                else
                    bulletDmg = dmgScript.damageAmount;
            }
            takeDamage(bulletDmg);

            Destroy(other.gameObject);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            InRange = false;
    }

    private void OnCollisionStay(Collision other)
    {
        if (!isDamaging && other.gameObject.CompareTag("Player"))
        {
            iDamage = other.gameObject.GetComponent<IDamage>();
            if (iDamage != null)
            {
                StartCoroutine(DOT(iDamage));
            }
        }
    }


    void Die()
    {
        Dead = true;


        rigidBody.linearVelocity = Vector3.zero;

        gamemanager.instance.updateGameGoal(-1);

        AudioSource.PlayClipAtPoint(deathSound, transform.position, deathVolume);

        TryDropPickup();

        Destroy(gameObject);
    }

    void TryDropPickup()
    {
        int itemType = Random.Range(0, 4); // 0 = health, 1 = ammo, 2 = mutagen, 3 = component

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

    IEnumerator DOT(IDamage target)
    {
        isDamaging = true;

        target.takeDamage(damageAmount);

        yield return new WaitForSeconds(damageRate);

        isDamaging = false;
    }
    IEnumerator FlashRed()
    {
        if (modelRender == null) yield break;
        //foreach (var part in modelRender)
        modelRender.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        //  foreach (var part in modelRender)
        modelRender.material.color = originColor;
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

        if (originalSpeed == 0f)
            originalSpeed = flyingSpeed;

        float slowedSpeed = originalSpeed * (1f - magnitude);
        flyingSpeed = slowedSpeed;

        yield return new WaitForSeconds(duration);

        flyingSpeed = originalSpeed;
        slowRoutine = null;
    }
    public void SetInvisible(bool state)
    {
       // throw new System.NotImplementedException();
    }

    bool IDamage.isDead()
    {
        return Dead;
    }
}