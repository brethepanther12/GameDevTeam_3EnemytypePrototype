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

    //[SerializeField] private float fovDistance;
   // [SerializeField] private float fovAngle;
   // [SerializeField] private LayerMask enviormentMask;
   // private bool playerVisible;
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

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null || !InRange)
        {
            rigidBody.linearVelocity = Vector3.zero;
            return;
        }

        // Direction toward the target
        Vector3 direction = (target.position - transform.position).normalized;

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

        // Move directly toward the player
        rigidBody.linearVelocity = direction * flyingSpeed;

        // Smooth rotation
        Quaternion targetRot = Quaternion.LookRotation(direction);
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
    }

    /*
    //Logic if the player is in view or not
    private bool PlayerInFieldOfView()
    {
        if (target == null) return false;

        //Locate player
        Vector3 direction = target.position - transform.position;
        float angle = Vector3.Angle(transform.forward, direction);

        Debug.DrawRay(transform.position, direction.normalized * fovDistance, Color.red);

        //check if the player is far from the object
        if (direction.magnitude > fovDistance || angle > fovAngle / 2f)return false;

        if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, fovDistance))
        {
            if (hit.collider.CompareTag("Player"))
                return true;
            else if (((1 << hit.collider.gameObject.layer) & enviormentMask) != 0)
                return false;
        }

        
        return false;
    }
    */

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
