using System.Collections;
using UnityEngine;

public class FlyingAI : MonoBehaviour
{
    [SerializeField] private Transform target;
    private GameObject playerTarget;
    [SerializeField] private float flyingSpeed;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private Rigidbody rigidBody;

    [SerializeField] private float damageRate;
    [SerializeField] private int damageAmount;
    private bool isDamaging;
    damage Damage;
    IDamage iDamage;

    //Hover off floor
    [SerializeField] private float hoverHeight;
    [SerializeField] private float hoverClamp;

    //Ceiling varibles
    [SerializeField] private float ceilingInRadius;
    [SerializeField] private float ceilingAttachmentRange;
    [SerializeField] private LayerMask ceilingMask;
    private bool returnToCeiling;
    private Vector3 ceilingTarget;
    private Vector3 ceilingPoint;

    //Fov
    [SerializeField] private float fovDistance;
    [SerializeField] private float fovAngle;
    [SerializeField] private LayerMask enviormentMask;
    private bool playerVisible;
    private bool InRange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            }
            else
            {
                rigidBody.linearVelocity = Vector3.zero;
                return;
            }

            MoveToCeiling();
            return;
        }
        returnToCeiling = false;

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
            rigidBody.linearVelocity = direction * flyingSpeed;

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
        float angle = Vector3.Angle(transform.forward, direction);

        Debug.DrawRay(transform.position, direction.normalized * fovDistance, Color.red);

        //check if the player is far from the object
        if (direction.magnitude > fovDistance || angle > fovAngle)return false;

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
            Collider hit = hits[i];

            Vector3 ceilingBottom = hit.bounds.center - new Vector3(0, hit.bounds.extents.y,0);
            float distance = Vector3.Distance(transform.position, ceilingBottom);

            if (distance < closest)
            {
                closest = distance;
                ceilingPoint = ceilingBottom; 
            }
        }

        if (closest < Mathf.Infinity)
        {
            rigidBody.linearVelocity = (ceilingPoint - transform.position).normalized * flyingSpeed;
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

            // snap to point
            transform.position = ceilingTarget; 
                                                     
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            InRange = true;
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

    IEnumerator DOT (IDamage target)
    {
        isDamaging = true;

        target.takeDamage(damageAmount);

        yield return new WaitForSeconds(damageRate);

        isDamaging = false;
    }
}
