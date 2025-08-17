using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DroneBoss : MonoBehaviour
{
    [Header("Activation")]
    public bool isActive = false; // Boss idle until activated

    [Header("Player")]
    public Transform player; // Assign player in Inspector

    [Header("Movement")]
    public float hoverHeight = 2f;       // Y position
    public float floatAmplitude = 0.5f;  // Up/down float motion
    public float floatSpeed = 2f;        // Float speed
    public float rotationSpeed = 5f;     // Smooth rotation speed
    private Vector3 startPosition;

    [Header("Projectile Prefabs")]
    public GameObject straightPrefab;
    public GameObject spreadPrefab;
    public GameObject spiralPrefab;
    public Transform firePoint;
    public float attackInterval = 2f;

    [Header("Health")]
    public int maxHealth = 500;
    private int currentHealth;
    public Slider healthBar;

    [System.Obsolete]
    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.value = 1f;

        startPosition = transform.position; // Save initial location
        StartCoroutine(AttackRoutine());
    }

    [System.Obsolete]
    void Update()
    {
        Hover();
        FacePlayer();
    }

    // --- Activation ---
    [System.Obsolete]
    public void ActivateBoss()
    {
        if (isActive) return;
        isActive = true;

        // Snap rotation immediately to player
        if (player != null)
        {
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    // --- Hover ---
    [System.Obsolete]
    void Hover()
    {
        if (!isActive) return;

        // Only adjust Y to prevent flipping
        Vector3 pos = transform.position;
        pos.y = hoverHeight + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = pos;
    }

    // --- Face Player ---
    [System.Obsolete]
    void FacePlayer()
    {
        if (!isActive || player == null) return;

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(lookDir) * Quaternion.Euler(0, 180f, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime * 360f);
    }

    // --- Shooting ---
    [System.Obsolete]
    IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (isActive && player != null)
            {
                if (straightPrefab != null) ShootStraight();
                yield return new WaitForSeconds(0.3f);

                if (spreadPrefab != null) ShootSpread();
                yield return new WaitForSeconds(0.3f);

                if (spiralPrefab != null) ShootSpiral();
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(attackInterval);
        }
    }

    [System.Obsolete]
    void ShootStraight()
    {
        Vector3 dir = (player.position - firePoint.position).normalized;
        firePoint.rotation = Quaternion.LookRotation(dir);

        GameObject proj = Instantiate(straightPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = dir * 25f; // fire directly at player
        Destroy(proj, 5f);
    }

    [System.Obsolete]
    void ShootSpread()
    {
        float angleStep = 15f;
        Vector3 dir = (player.position - firePoint.position).normalized;

        for (int i = -2; i <= 2; i++)
        {
            Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(0, i * angleStep, 0);
            GameObject proj = Instantiate(spreadPrefab, firePoint.position, rot);
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = rot * Vector3.forward * 20f; // fire spread toward player
            Destroy(proj, 5f);
        }
    }

    [System.Obsolete]
    void ShootSpiral()
    {
        int bullets = 8;
        float angleStep = 360f / bullets;
        Vector3 dir = (player.position - firePoint.position).normalized;

        for (int i = 0; i < bullets; i++)
        {
            Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(0, i * angleStep, 0);
            GameObject proj = Instantiate(spiralPrefab, firePoint.position, rot);
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = rot * Vector3.forward * 15f; // fire spiral toward player
            Destroy(proj, 5f);
        }
    }

    // --- Health System ---
    [System.Obsolete]
    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (healthBar != null)
            healthBar.value = (float)currentHealth / maxHealth;

        if (currentHealth <= 0) Die();
    }

    [System.Obsolete]
    void Die()
    {
        Destroy(gameObject);
    }
}