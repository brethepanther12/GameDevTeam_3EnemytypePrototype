using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Grenade : MonoBehaviour
{
    [SerializeField] private Rigidbody grenadeRigidB;

    [SerializeField] private int grenadeSpeed;
    [SerializeField] private int grenadeSpeedY;
    [SerializeField] private float destroyTimer;
    [SerializeField] private float maxHomingAngle = 30f;
    [SerializeField] public GameObject explosionPrefab;
    [SerializeField] public AudioClip explosionAudio;
    [SerializeField] public AudioSource grenadeAudio;
    [SerializeField] private bool OnStickyBomb;
    [SerializeField] private bool isTracking;
    [SerializeField] public bool canTrack;
    [SerializeField] private bool isCooked;
    private Transform playerTarget;
    private damage damageStats;
    
    bool OnSurface;
    private bool hasExploded = false;
    private Coroutine explosionRoutine;

    void Start()
    {
        damageStats = GetComponent<damage>();

        if (damageStats != null)
        {
            grenadeSpeed = damageStats.speed;
            destroyTimer = damageStats.destroyTime;
            grenadeRigidB = damageStats.rb;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            IDamage isEnemyDead = enemy.GetComponent<IDamage>();
            if (isEnemyDead != null && isEnemyDead.isDead()) continue;

            Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, toEnemy);
            if (angle > maxHomingAngle) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                closest = enemy;
                minDist = dist;
            }
        }

        playerTarget = closest != null ? closest.transform : null;
        isTracking = canTrack && playerTarget != null;

        if (!isTracking)
        {
            grenadeRigidB.useGravity = true;
            grenadeRigidB.linearVelocity = (transform.forward * grenadeSpeed) + (transform.up * grenadeSpeedY);
        }
        else
        {
            grenadeRigidB.useGravity = false;
            grenadeRigidB.linearVelocity = (transform.forward * grenadeSpeed * 0.5f);
        }

        explosionRoutine = StartCoroutine(Explode());
    }

    void Update()
    {
        if (isTracking && !OnSurface && playerTarget != null)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            grenadeRigidB.linearVelocity = direction * grenadeSpeed;

            float proximity = Vector3.Distance(transform.position, playerTarget.position);
            if (proximity <= 1f && !hasExploded)
            {
                if (explosionRoutine != null) StopCoroutine(explosionRoutine);
                ExplodeImmediate();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (OnStickyBomb && !OnSurface)
        {
            if (!collision.transform.CompareTag("Weapon"))
            {
                grenadeRigidB.linearVelocity = Vector3.zero;
                grenadeRigidB.angularVelocity = Vector3.zero;
                grenadeRigidB.isKinematic = true;
                transform.SetParent(collision.transform);
                OnSurface = true;
            }
        }
        else if ((collision.transform.CompareTag("Breakable") || collision.transform.CompareTag("Enemy")) && !isCooked && !hasExploded)
        {
            destroyTimer = 0;
            if (explosionRoutine != null) StopCoroutine(explosionRoutine);
            explosionRoutine = StartCoroutine(Explode());
        }
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(destroyTimer);

        if (hasExploded) yield break;
        hasExploded = true;

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (explosionAudio != null)
            AudioSource.PlayClipAtPoint(explosionAudio, transform.position);

        Destroy(gameObject);
    }

    public void RemoteDetonate()
    {
        if (hasExploded) return;

        if (explosionRoutine != null)
            StopCoroutine(explosionRoutine);

        ExplodeImmediate();
    }

    private void ExplodeImmediate()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (explosionAudio != null)
            AudioSource.PlayClipAtPoint(explosionAudio, transform.position);

        Destroy(gameObject);
    }

}

