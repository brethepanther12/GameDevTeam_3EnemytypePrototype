using System.Collections;
using UnityEngine;
using UnityEngine.LowLevel;
using static UnityEngine.Rendering.DebugUI;
public class FlyingAI : MonoBehaviour, IDamage, Visibility
{
    [SerializeField] private Transform target;
    [SerializeField] private float lostPlayDelay;
    private GameObject playerTarget;
    private float playerLostTimer;

    [Header("\"--- Flying & Rotation ---\"")]
    [SerializeField] private float flyingSpeed;
    [SerializeField] private float rotationSpeed;
    Vector3 playerDirection;

    [SerializeField] private Rigidbody rigidBody;

    [Header("\"--- Damage ---\"")]
    [SerializeField] private float damageRate;
    [SerializeField] private int damageAmount;
    private bool isDamaging;
    damage Damage;
    IDamage iDamage;

    [Header("\"--- Hover ---\"")]
    [SerializeField] private float hoverHeight;
    [SerializeField] private float hoverClamp;

    [Header("\"--- Ceiling ---\"")]
    [SerializeField] private float ceilingInRadius;
    [SerializeField] private float ceilingAttachmentRange;
    //[SerializeField] private float ceilingHeightOff;
    [SerializeField] private LayerMask ceilingMask;
    [SerializeField] private SphereCollider bodyCollider;
    private bool returnToCeiling;
    private bool attachedToCeiling;
    private Vector3 ceilingTarget;
    private Vector3 ceilingPoint;
    private Vector3 attachedCeilingPoint;


    [Header("\"--- Retreat ---\"")]
    [SerializeField] private float retreatCooldown;
    [SerializeField] private float retreatSpeed;
    [SerializeField] private float retreatDistance;
    private bool isRetreating;
    private float retreatTimer;
    private Vector3 retreatDirection;

    [Header("\"--- Strafing ---\"")]
    [SerializeField] private float strafeSpeed;
    [SerializeField] private float strafeCooldown;
    [SerializeField] private float strafeDistance;
    private float strafeTimer;
    private Vector3 strafeDirection;

    [Header("\"--- Field of View ---\"")]
    [SerializeField] private float fovDistance;
    [SerializeField] private float fovAngle;
    [SerializeField] private LayerMask enviormentMask;
    private bool playerVisible;
    private bool InRange;
    private bool isBlind;

    [Header("\"--- Health ---\"")]
    [SerializeField] private int HP;
    private int currentHP;
    private bool Dead;

    [SerializeField] public int shield;
    [SerializeField] public int armor;

    [SerializeField] public GameObject shieldPrefab;
    [SerializeField] public GameObject armorPrefab;

    [Header("\"--- Audio ---\"")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float hitVolume;
    [SerializeField] private float deathVolume;

    [Header("\"--- Model ---\"")]
    [SerializeField] private Renderer modelRender;
    private Color originColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = HP;

        // Store original material color
        if (modelRender != null)
            originColor = modelRender.material.color;

        if (rigidBody == null) rigidBody = GetComponent<Rigidbody>();

        playerTarget = gamemanager.instance.player;

        Damage = GetComponent<damage>();
        if (Damage != null)
            Damage.enabled = false;
       gamemanager.instance.updateGameGoal(1);
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (Dead) return;

        Visibility();
        AssignTarget();

        Movement(); 
    }

    private void Movement()
    {
        // If player visible or in range
        if (target != null && (playerVisible || InRange))
        {
            MoveTowardsPlayer();
        }
        else
        {
            // Player not found, move toward nearest ceiling
            if (!returnToCeiling)
            {
                NearestCeiling();
                ceilingTarget = ceilingPoint;
                returnToCeiling = true;
            }
            else
            {
                MoveToCeiling();
            }
        }

        // Hover
        if (!returnToCeiling)
            Hover();
    }

    private void MoveTowardsPlayer()
    {
        Vector3 directionToPlayer = (target.position - transform.position).normalized;
        Vector3 finalVelocity = directionToPlayer * flyingSpeed + Strafing(directionToPlayer);

        if (isRetreating)
        {
            Retreat();
            return;
        }

        // Collision check
        if (!Physics.Raycast(transform.position, finalVelocity.normalized, 1f, enviormentMask))
            rigidBody.linearVelocity = finalVelocity;
        else
            rigidBody.linearVelocity = Vector3.zero;

        faceTarget();
    }

    private Vector3 Strafing(Vector3 direction)
    {
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
        {
            strafeTimer = strafeCooldown;

            Vector3 strafing = Vector3.Cross(Vector3.up, direction).normalized;
            if (Random.value > 0.5f)
                strafeDirection = strafing;
            else
                strafeDirection = -strafing;

            strafeDistance = Random.Range(1f, 3f);
        }
        
        return strafeDirection * strafeSpeed;
    }

    private void Retreat()
    {
        retreatTimer -= Time.deltaTime;

        if (strafeDirection == Vector3.zero)
        {
            Vector3 strafing = Vector3.Cross(Vector3.up, retreatDirection).normalized;

            if (Random.value > 0.3f)
                strafeDirection = strafing;
            else
                strafeDirection = -strafing;
        }

        Vector3 velocity = (retreatDirection + strafeDirection).normalized * retreatSpeed;
        rigidBody.linearVelocity = velocity;

        Quaternion targetRotation = Quaternion.LookRotation(retreatDirection);
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

        if (retreatTimer <= 0f ||
            Vector3.Distance(transform.position, playerTarget.transform.position) >= retreatDistance)
        {
            isRetreating = false;
            strafeDirection = Vector3.zero;
            rigidBody.linearVelocity = Vector3.zero;
        }
    }

    //Logic if the player is in view or not
    private bool PlayerInFieldOfView()
    {
        //playerDirection = gamemanager.instance.player.transform.position - transform.position;

        if (playerTarget == null || isBlind) return false;

        Vector3 direction = playerTarget.transform.position - transform.position;
        float distanceToPlayer = direction.magnitude;
        float angle = Vector3.Angle(direction, transform.forward);

        // Check FOV angle and distance first
        if (distanceToPlayer > fovDistance || angle > fovAngle) return false;

        // Raycast to check occlusion
        if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, fovDistance))
        {
            // Only visible if the first thing hit is the player
            if (hit.collider.CompareTag("Player"))
                return true;

            // Check if hit something like smoke, walls, or environment
            if (hit.collider.CompareTag("Smoke"))
                return false;

            if (((1 << hit.collider.gameObject.layer) & enviormentMask) != 0)
                return false;

            return false;
        }

        return false;
    }

    private void Visibility()
    {
        if (isBlind)
        {
            playerVisible = false;
            target = null;
        }
        else
        {
            //  check if the player is in range and visible
            playerVisible = PlayerInFieldOfView();
        }
    }

    private void PlayerLost()
    {
        if (target == null)
            playerLostTimer += Time.deltaTime;
        else
            playerLostTimer = 0f;

        if (playerLostTimer >= lostPlayDelay && !InRange && !playerVisible && !isRetreating)
        {
            if (!returnToCeiling)
            {
                NearestCeiling();
                ceilingTarget = ceilingPoint;
                returnToCeiling = true;
            }
        }
    }

    public void SetInvisible(bool invisible)
    {
        isBlind = invisible;
        if (invisible)
        {
            target = null;
            playerVisible = false;
            InRange = false;
        }
    }

    private void AssignTarget()
    {
        // Assign or clear the target based on FOV + trigger
        if (InRange || playerVisible)
        {
            target = playerTarget.transform;
            attachedToCeiling = false;
        }
    }
    void NearestCeiling()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ceilingInRadius, ceilingMask);

        float closest = Mathf.Infinity;
        ceilingPoint = Vector3.zero;

        for (int i = 0; i < hits.Length; i++)
        {
            Bounds bounds = hits[i].bounds;

            // Bottom Y of the ceiling
            float ceilingY = bounds.center.y - bounds.extents.y;

            // Margin to stay inside the collider
            float margin = 0.5f;
            float minX = bounds.min.x + margin;
            float maxX = bounds.max.x - margin;
            float minZ = bounds.min.z + margin;
            float maxZ = bounds.max.z - margin;

            if (minX >= maxX || minZ >= maxZ) continue;

            // Pick a random point fully inside the bounds
            float ceilingX = Random.Range(minX, maxX);
            float ceilingZ = Random.Range(minZ, maxZ);

            Vector3 ceilingBottom = new Vector3(ceilingX, ceilingY, ceilingZ);
            float distance = Vector3.Distance(transform.position, ceilingBottom);

            if (distance < closest)
            {
                closest = distance;
                ceilingPoint = ceilingBottom;
            }
        }

        if (closest == Mathf.Infinity)
        {
            Debug.Log("No ceiling found in range!");
        }

        // Reset the attached flag so the drone will pick a new local offset
        attachedToCeiling = false;
    }

    void MoveToCeiling()
    {
        if (!returnToCeiling) return;

        if (rigidBody.isKinematic) rigidBody.isKinematic = false;

        Vector3 toCeiling = ceilingTarget - transform.position;
        float distance = toCeiling.magnitude;

        float ceilingHeightOff = bodyCollider.bounds.extents.y;

        // Pick a random point on the ceiling once
        if (!attachedToCeiling)
        {
            float offsetX = Random.Range(-1f, 1f);
            float offsetZ = Random.Range(-1f, 1f);
            attachedCeilingPoint = ceilingTarget - new Vector3(0, ceilingHeightOff, 0) + new Vector3(offsetX, 0, offsetZ);

            attachedToCeiling = true;
        }

        // Smoothly move toward the attached ceiling point
        Vector3 desiredPosition = attachedCeilingPoint;
        rigidBody.position = Vector3.MoveTowards(rigidBody.position, desiredPosition, Time.fixedDeltaTime * flyingSpeed);

        // Smoothly rotate toward ceiling
        Vector3 direction = (attachedCeilingPoint - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));

        // If close enough, attach
        if (distance < ceilingAttachmentRange)
        {
            rigidBody.position = attachedCeilingPoint;
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.isKinematic = true;
            returnToCeiling = false;
            attachedToCeiling = true;
        }
    }

    private void Hover()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, hoverHeight))
        {
            float hoverError = hoverHeight - hit.distance;
            float upwardForce = hoverClamp * hoverError;

            if (hit.distance < 0.2f)
                upwardForce *= 3f;

            rigidBody.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
        }
    }
    
    void faceTarget()
    {
        if (target == null) return;

        playerDirection = (target.position - transform.position).normalized;

        if (playerDirection.sqrMagnitude > 0.01f)
        {
            Quaternion rotate = Quaternion.LookRotation(playerDirection);
            transform.rotation = Quaternion.Lerp(rigidBody.rotation, rotate, Time.deltaTime * rotationSpeed);

        }

    }

    public void takeDamage(int amount)
    {

        if (Dead) return;

        if (shield > 0)
        {
            shield -= amount;
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
            StartCoroutine(FlashRed());

            if (shield <= 0)
            {
                shield = 0;

                shieldPrefab.SetActive(false);
                armor -= amount;
                AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
                StartCoroutine(FlashRed());

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
                StartCoroutine(FlashRed());
            }
        }
        else
        {
            currentHP -= amount;
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
            StartCoroutine(FlashRed());
        }
        if (!Dead && playerTarget != null) 
        {
            isRetreating = true;
            retreatTimer = retreatCooldown;
            SetRetreatPoint();

        }

        if (currentHP <= 0)
        {
            //Die method
            Die();
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

            default:
                break;
        }



    }

    private void SetRetreatPoint()
    {
        Vector3 retreatPoint = (transform.position - playerTarget.transform.position).normalized;

        Vector3 randomPoint = Vector3.Cross(Vector3.up, retreatPoint).normalized;
        float retreatRange = Random.Range(-1f, 1f);

        Vector3 direction = (retreatPoint + randomPoint * retreatRange).normalized;

        Vector3 retreatTarget = transform.position + direction * retreatDistance;

        retreatTarget.y = transform.position.y;

        retreatDirection = (retreatTarget - transform.position).normalized;
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
        if (!isRetreating && !isDamaging && other.gameObject.CompareTag("Player"))
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

        Destroy(gameObject);
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

}