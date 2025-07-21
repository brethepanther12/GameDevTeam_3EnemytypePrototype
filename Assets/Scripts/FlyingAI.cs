using System.Collections;
using UnityEngine;

public class FlyingAI : MonoBehaviour, IDamage
{
    [SerializeField] private Transform target;
    private GameObject playerTarget;
    [SerializeField] private float flyingSpeed;
    [SerializeField]
    private float rotationSpeed;
    // Vector3 playerDirection;

    [SerializeField] private Rigidbody rigidBody;

    //Damage
    [SerializeField] private float damageRate;
    [SerializeField] private int damageAmount;
    private bool isDamaging;
    damage Damage;
    IDamage iDamage;

    //Hover off floor
    [SerializeField] private float hoverHeight;
    [SerializeField] private float hoverClamp;

    //Ceiling variables
    [SerializeField] private float ceilingInRadius;
    [SerializeField] private float ceilingAttachmentRange;
    //[SerializeField] private float ceilingHeightOff;
    [SerializeField] private LayerMask ceilingMask;
    [SerializeField] private SphereCollider bodyCollider;
    private bool returnToCeiling;
    private Vector3 ceilingTarget;
    private Vector3 ceilingPoint;

    //Fov
    [SerializeField] private float fovDistance;
    [SerializeField] private float fovAngle;
    [SerializeField] private LayerMask enviormentMask;
    private bool playerVisible;
    private bool InRange;

    //Health
    [SerializeField] private int HP;
    private int currentHP;
    private bool Dead;

    //Upon getting hit
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float hitVolume;
    [SerializeField] private float deathVolume;

    //Render
    [SerializeField] private Renderer modelRender;
    private Color originColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = HP;

        //playerDirection = gamemanager.instance.player.transform.position - transform.position;

        // Store original material color
        if (modelRender != null)
            originColor = modelRender.material.color;

        if (rigidBody == null) rigidBody = GetComponent<Rigidbody>();

        playerTarget = gamemanager.instance.player;
        if (playerTarget != null)
            target = playerTarget.transform;

        Damage = GetComponent<damage>();
        if (Damage != null)
            Damage.enabled = false;
        gamemanager.instance.updateGameGoal(1);
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        playerVisible = PlayerInFieldOfView();

        if (target == null || !InRange || !playerVisible)
        {

            if (!returnToCeiling)
            {
                NearestCeiling();
                ceilingTarget = ceilingPoint;
                returnToCeiling = true;
            }

            //returnToCeiling = false;

            MoveToCeiling();
            return;
        }

        if (rigidBody.isKinematic)
            rigidBody.isKinematic = false;

        // Direction toward the target
        Vector3 direction = (target.position - transform.position).normalized;

        Debug.DrawRay(transform.position, direction * 1f, Color.red);


        // Maintain hover height
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, hoverHeight))
        {
            float hoverError = hoverHeight - hit.distance;
            float upwardForce = hoverClamp * hoverError;

            // Boost if very close to the ground
            if (hit.distance < 0.2f)
                upwardForce *= 3f;

            rigidBody.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
        }
        /*
         
         // Smooth rotation
        Quaternion targetRot = Quaternion.LookRotation(direction);
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));

        // Move forward
        rigidBody.linearVelocity = transform.forward * flyingSpeed;
         
         */

        //Checking if there is a wall in flying enemy direction
        if (!Physics.Raycast(transform.position, direction, out RaycastHit wallHit, 1f, enviormentMask))

            // Move directly toward the player
            rigidBody.linearVelocity = transform.forward * flyingSpeed;

        else
            rigidBody.linearVelocity = Vector3.zero;

        // Smooth rotation
        Quaternion targetRot = Quaternion.LookRotation(direction);
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
    }


    //Logic if the player is in view or not
    private bool PlayerInFieldOfView()
    {
        if (target == null) return false;

        //Locate player
        Vector3 direction = target.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);

        Debug.DrawRay(transform.position, direction.normalized * fovDistance, Color.red);

        //check if the player is far from the object
        if (direction.magnitude > fovDistance || angle > fovAngle) return false;

        if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, fovDistance))
        {
            if (hit.collider.CompareTag("Player"))
                return true;

            else if (((1 << hit.collider.gameObject.layer) & enviormentMask) != 0)
                return false;
        }


        return false;
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

    //void faceTarget()
    //{
    //    Quaternion rotate = Quaternion.LookRotation(new Vector3(playerDirection.x, 0, playerDirection.z));
    //    transform.rotation = Quaternion.Lerp(transform.rotation, rotate, Time.deltaTime * faceTargetSpeed);
    //}

    public void takeDamage(int amount)
    {

        if (Dead) return;

        currentHP -= amount;

        if (currentHP <= 0)
        {
            //Die method
            Die();
        }
        else
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
            StartCoroutine(FlashRed());
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
