using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class FireballProjectile : MonoBehaviour
{
    [Header("Runtime (set by spawner)")]
    public int damage;
    public float speed;
    public Transform owner;

    [Header("Tuning")]
    public float lifeTime = 6f;
    public GameObject hitVFX;

    Vector3 dir;

    public void Init(Vector3 direction, int dmg, float spd, Transform ownerRoot)
    {
        dir = direction.normalized;
        damage = dmg;
        speed = spd;
        owner = ownerRoot;
    }

    void Start()
    {
        // Ensure physics setup
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        GetComponent<SphereCollider>().isTrigger = true;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignore the owner & its children
        if (owner != null && (other.transform == owner || other.transform.IsChildOf(owner))) return;

        // Apply damage if the hit target supports IDamage
        var d = other.GetComponent<IDamage>();
        if (d != null)
        {
            d.takeDamage(damage);
        }

        if (hitVFX) Instantiate(hitVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}