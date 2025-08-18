using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Setup")]
    public Transform turretHead;   // The part that rotates
    public Transform firePoint;    // Where bullets spawn
    public GameObject bulletPrefab; // Your rifle bullet prefab
    public Transform target;        // Usually the player

    [Header("Stats")]
    public float range = 20f;
    public float fireRate = 1f;
    public float rotationSpeed = 5f;

    private float fireCooldown = 0f;

    [System.Obsolete]
    void Update()
    {
        if (target == null) return;

        // Check if player is in range
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= range)
        {
            AimAtTarget();

            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = 1f / fireRate;
            }
        }

        fireCooldown -= Time.deltaTime;
    }

    void AimAtTarget()
    {
        Vector3 dir = target.position - turretHead.position;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(turretHead.rotation, lookRot, Time.deltaTime * rotationSpeed).eulerAngles;
        turretHead.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    [System.Obsolete]
    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Spawn the bullet at FirePoint
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Make the bullet move forward
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * bullet.GetComponent<damage>().speed;
        }
    }
}