using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DroneBoss : MonoBehaviour, IDamage
{
    [Header("Activation")]
    public bool isActive = false;

    [Header("Player")]
    public Transform player;

    [Header("Movement")]
    public float hoverHeight = 2f;
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 2f;
    public float rotationSpeed = 5f;
    private Vector3 startPosition;

    [Header("Projectile Prefabs")]
    public GameObject straightPrefab;
    public GameObject spreadPrefab;
    public GameObject spiralPrefab;
    public Transform firePoint;
    public float attackInterval = 2f;

    [Header("Health")]
    public int maxHealth = 40;
    private int currentHealth;
    public Slider healthBar;          // assign in inspector
    public Canvas healthCanvas;       // assign in inspector

    [Header("Damage Feedback")]
    public Renderer bossRenderer;     // assign in inspector
    public Color damageFlashColor = Color.red;
    public float flashDuration = 0.15f;
    private Color originalColor;

    [Header("Death Effect")]
    public GameObject explosionPrefab;

    [System.Obsolete]
    void Start()
    {
        gamemanager.instance.updateGameGoal(1);

        currentHealth = maxHealth;

        // init health bar
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = 1f;
        }

        if (healthCanvas != null)
            healthCanvas.enabled = false; // hidden until boss activates

        if (bossRenderer != null)
            originalColor = bossRenderer.material.color;

        startPosition = transform.position;
        StartCoroutine(AttackRoutine());
    }

    void Update()
    {
        Hover();
        FacePlayer();
    }

    public void ActivateBoss()
    {
        if (isActive) return;
        isActive = true;

        if (healthCanvas != null)
            healthCanvas.enabled = true; // show health bar now

        // snap rotation to face player immediately
        if (player != null)
        {
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    void Hover()
    {
        if (!isActive) return;
        Vector3 pos = transform.position;
        pos.y = hoverHeight + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = pos;
    }

    void FacePlayer()
    {
        if (!isActive || player == null) return;
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(lookDir) * Quaternion.Euler(0, 180f, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime * 360f);
    }

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
        if (rb != null) rb.velocity = dir * 25f;
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
            if (rb != null) rb.velocity = rot * Vector3.forward * 20f;
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
            if (rb != null) rb.velocity = rot * Vector3.forward * 15f;
            Destroy(proj, 5f);
        }
    }

    // ---- DAMAGE SYSTEM ----
    public void takeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        // update health bar
        if (healthBar != null)
            healthBar.value = (float)currentHealth / maxHealth;

        // flash red
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
            Die();
    }

    // optional status effect version (not needed for simple damage, but must exist)
    public void takeDamage(int amount, StatusEffectData effect)
    {
        takeDamage(amount);
        // you can add effect logic here later
    }

    // slow down method (stub)
    public void slowDown(float magnitude, float duration)
    {
        // not needed for boss unless you want slowdown effects
    }

    // check dead
    public bool isDead()
    {
        return currentHealth <= 0;
    }
    IEnumerator DamageFlash()
    {
        if (bossRenderer != null)
        {
            bossRenderer.material.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            bossRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        Debug.Log("Drone Boss defeated!");

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (healthCanvas != null)
            healthCanvas.enabled = false;

        Destroy(gameObject);
    }
}