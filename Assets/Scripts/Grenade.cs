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

    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject smokeCloudPrefab;

    [SerializeField] private bool OnStickyBomb;
    [SerializeField] private bool isTracking;
    private Transform playerTarget;
    private damage damageStats;
    
    bool OnSurface;
    
    void Start()
    {
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
        {
            GameObject closest = enemies[0];
            float minDist = Vector3.Distance(transform.position, closest.transform.position);

            foreach (GameObject enemy in enemies)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    closest = enemy;
                    minDist = dist;

                    StartCoroutine(explode());

                }
            }

            playerTarget = closest.transform;
        }

        
        damageStats = GetComponent<damage>();

        if (damageStats != null)
        {
            grenadeSpeed = damageStats.speed;
            destroyTimer = damageStats.destroyTime;
            grenadeRigidB = damageStats.rb;
        }

        
        if (!isTracking)
        {
            grenadeRigidB.useGravity = true;
            grenadeRigidB.linearVelocity = (transform.forward * grenadeSpeed )  + (transform.up * grenadeSpeedY);
        }

        
        StartCoroutine(explode());

    }

    // Update is called once per frame
    void Update()
    {
        
        if (isTracking && !OnSurface && playerTarget != null)
        {
            grenadeRigidB.linearVelocity = (playerTarget.position - transform.position) * grenadeSpeed;

            float proximity = Vector3.Distance(transform.position, playerTarget.transform.position);

            if (proximity <= 1f)
            {
                destroyTimer = 0;
                StartCoroutine(explode());
            }
        }

    }
    

    private void OnCollisionEnter(Collision collision)
    {
        if (OnStickyBomb && !OnSurface)
        {   
            //Making it stationary
            

            grenadeRigidB.linearVelocity = Vector3.zero;
            grenadeRigidB.angularVelocity = Vector3.zero;

            grenadeRigidB.isKinematic = true;

            //Making it stick to a surface; Moving with the object it parents
            transform.SetParent(collision.transform);

            //Setting it true that it is on a surface
            OnSurface = true;
        }

        
    }

    IEnumerator explode()
    {
        yield return new WaitForSeconds(destroyTimer);
        if (explosionPrefab != null )
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (smokeCloudPrefab != null)
         Instantiate(smokeCloudPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }


}

