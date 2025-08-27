using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class FireballProjectile : MonoBehaviour
{
    public int damage;
    public float speed;
    public Transform owner;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var col = GetComponent<Collider>();
        col.isTrigger = false; // <-- real collision now
    }

    [System.Obsolete]
    public void Init(Vector3 direction, int dmg, float spd, Transform ownerRoot)
    {
        damage = dmg;
        speed = spd;
        owner = ownerRoot;

        direction.Normalize();
        rb.velocity = direction * speed; // <-- use velocity
        // Debug.Log($"Fireball launched! Vel: {rb.velocity}");
    }

    void Start()
    {
        Destroy(gameObject, 10f);
    }

    [System.Obsolete]
    void Update()
    {
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity.normalized, Vector3.up);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ignore the owner & its children
        if (owner != null && (collision.transform == owner || collision.transform.IsChildOf(owner)))
            return;

        var health = collision.gameObject.GetComponent<IDamage>();
        if (health != null)
        {
            health.takeDamage(damage);
        }

        Destroy(gameObject);
    }
}