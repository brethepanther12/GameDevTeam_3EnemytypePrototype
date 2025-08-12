using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public Rigidbody rb;
    public int damageAmount = 10;
    public float speed = 20f;
    public float destroyTime = 5f;
    public GameObject impactPrefab;

    [System.Obsolete]
    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("BossProjectile: No Rigidbody found on projectile prefab!");
            return; 
        }

        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
            audio.Play();

        rb.velocity = transform.forward * speed;

        Destroy(gameObject, destroyTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null)
        {
            dmg.takeDamage(damageAmount);
        }

        if (impactPrefab != null)
        {
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + transform.forward * 0.2f;
            Vector3 rayDirection = -transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out hit, 1f, ~0, QueryTriggerInteraction.Ignore))
                Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            else
                Instantiate(impactPrefab, transform.position, Quaternion.LookRotation(-transform.forward));
        }

        Destroy(gameObject);
    }
}