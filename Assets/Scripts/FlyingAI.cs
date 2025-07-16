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
        if (target == null) return;

        // Direction toward the target
        Vector3 direction = (target.position - transform.position).normalized;

        // Smooth rotation
        Quaternion targetRot = Quaternion.LookRotation(direction);
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));

        // Move forward
        rigidBody.linearVelocity = transform.forward * flyingSpeed;
    }
}
