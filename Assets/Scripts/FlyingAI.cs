using System.Collections;
using UnityEngine;
using UnityEngine.AI;
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
    private Vector3 ceilingTarget;
    private Vector3 ceilingPoint;

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

    [Header("\"--- Drops ---\"")]
    public GameObject healthPickupPrefab;
    public GameObject ammoPickupPrefab;
    public GameObject mutagenPickupPrefab;
    public GameObject componentPrefab;

    [Header("\"--- Model ---\"")]
    [SerializeField] private Renderer modelRender;
    private Color originColor;

    private float originalSpeed;
    private Coroutine slowRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = HP;
        originalSpeed = flyingSpeed;

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

        Damage = GetComponent<damage>();
        if (Damage != null)
            Damage.enabled = false;
       gamemanager.instance.updateGameGoal(1);
    }


    // Update is called once per frame
    void FixedUpdate()
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


        // Assign or clear the target based on FOV + trigger
        if (InRange && playerVisible && target == null)
        {
            target = playerTarget.transform;
        }
        else if ((!InRange || !playerVisible) && target != null)
        {
            target = null;
        }

        // Determine if the player is "lost"
        bool playerLost = target == null;

        // Handle player loss timer
        if (playerLost)
        {
            playerLostTimer += Time.fixedDeltaTime;
        }
        else
        {
            playerLostTimer = 0f;
        }

        // If lost for long enough, go to ceiling
        if (playerLostTimer >= lostPlayDelay && !returnToCeiling)
        {
            NearestCeiling();
            ceilingTarget = ceilingPoint;
            returnToCeiling = true;
        }

        if (!playerLost && returnToCeiling)
        {
            returnToCeiling = false;
        }

        // If returning to ceiling, override all movement
        if (returnToCeiling)
        {
            MoveToCeiling();
            return;
        }

        // Hover logic (always active)
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, hoverHeight))
        {
            float hoverError = hoverHeight - hit.distance;
            float upwardForce = hoverClamp * hoverError;

            if (hit.distance < 0.2f)
            {
                upwardForce *= 3f;
            }

            rigidBody.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
        }

        // Movement logic
        if (!playerLost && target != null)
        {
            if (rigidBody.isKinematic)
            {
                rigidBody.isKinematic = false;
            }

            Vector3 direction = (target.position - transform.position).normalized;
            Debug.DrawRay(transform.position, direction * 1f, Color.red);

            if (!Physics.Raycast(transform.position, direction, 1f, enviormentMask))
            {
                rigidBody.linearVelocity = direction * flyingSpeed;
            }
            else
            {
                rigidBody.linearVelocity = Vector3.zero;
            }

            faceTarget();
        }
        else
        {
            rigidBody.linearVelocity = Vector3.zero;
        }

    }


    //Logic if the player is in view or not
    private bool PlayerInFieldOfView()
    {
        //playerDirection = gamemanager.instance.player.transform.position - transform.position;

        if (playerTarget == null || isBlind) return false;

        //Locate player
        Vector3 direction = playerTarget.transform.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);

        Debug.DrawRay(transform.position, direction.normalized * fovDistance, Color.red);

        //check if the player is far from the object
        if (direction.magnitude > fovDistance || angle > fovAngle) return false;

        if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, fovDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
            else if (hit.collider.CompareTag("Smoke"))
            {
                return false;
            }
            else if (((1 << hit.collider.gameObject.layer) & enviormentMask) != 0)
            {
                return false;
            }

        }
        return false;
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
    void NearestCeiling()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ceilingInRadius, ceilingMask);

        float closest = Mathf.Infinity;
        ceilingPoint = Vector3.zero;

        for (int i = 0; i < hits.Length; i++)
        {
            /*
             Collider hit = hits[i];

            Vector3 ceilingBottom = hit.bounds.center - new Vector3(0, hit.bounds.extents.y,0);
            float distance = Vector3.Distance(transform.position, ceilingBottom);
             */

            Bounds bounds = hits[i].bounds;

            //Y of the ceiling
            float ceilingY = bounds.center.y - bounds.extents.y;

            float margin = 0.5f;

            float minX = bounds.min.x + margin;
            float maxX = bounds.max.x - margin;
            float minZ = bounds.min.z + margin;
            float maxZ = bounds.max.z - margin;

            if (minX >= maxX || minZ >= maxZ) continue;
            ////Random z and x points
            float ceilingX = Random.Range(minX, maxX);
            float ceilngZ = Random.Range(minZ, maxZ);

            Vector3 ceilingBottom = new Vector3(ceilingX, ceilingY, ceilngZ);
            float distance = Vector3.Distance(transform.position, ceilingBottom);
            if (distance < closest)
            {
                closest = distance;
                ceilingPoint = ceilingBottom;
            }
        }

        if (closest < Mathf.Infinity)
        {
            //rigidBody.linearVelocity = (ceilingPoint - transform.position).normalized * flyingSpeed;
        }
    }

    void MoveToCeiling()
    {
        if (!returnToCeiling) return;

        if (rigidBody.isKinematic) return;

        Vector3 direction = (ceilingTarget - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, ceilingTarget);

        if (distance > ceilingAttachmentRange)
        {
            rigidBody.linearVelocity = direction * flyingSpeed;

            Quaternion targetRot = Quaternion.LookRotation(direction);
            rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }
        else
        {
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            rigidBody.isKinematic = true;

            float ceilingHeightOff = bodyCollider.bounds.extents.y; //bodyCollider.radius * transform.localScale.y;
            // snap to point
            transform.position = ceilingTarget - new Vector3(0, ceilingHeightOff, 0);

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
                } else
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
}