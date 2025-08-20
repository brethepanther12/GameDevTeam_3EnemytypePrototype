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

    [SerializeField] private bool OnStickyBomb;
    [SerializeField] private bool isTracking;
    [SerializeField] public bool canTrack;
    [SerializeField] private bool isCooked;
    private Transform playerTarget;
    private damage damageStats;
    
    bool OnSurface;
    
    void Start()
    {
        Transform player = gamemanager.instance.player.transform;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
        {
            if (!isCooked && !OnStickyBomb)
            {
                isTracking = true;
            }


            GameObject closest = null;
            float minDist = Mathf.Infinity;

            foreach (GameObject enemy in enemies)
            {
                IDamage isEnemyDead = enemy.GetComponent<IDamage>();
                if (isEnemyDead != null && isEnemyDead.isDead()) continue;

                Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, toEnemy);
                if (angle > maxHomingAngle) continue; ;

                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    closest = enemy;
                    minDist = dist;
                }
            }

            playerTarget = closest != null ? closest.transform : null;
            isTracking = canTrack && playerTarget != null;

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
                grenadeRigidB.linearVelocity = (transform.forward * grenadeSpeed) + (transform.up * grenadeSpeedY);
            }

            if (isTracking)
            {
                grenadeRigidB.useGravity = false;
                grenadeRigidB.linearVelocity = (transform.forward * grenadeSpeed * 0.5f);
            }


            StartCoroutine(explode());
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (isTracking && !OnSurface && playerTarget != null)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            grenadeRigidB.linearVelocity = direction * grenadeSpeed;

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
            if (!collision.transform.CompareTag("Weapon"))
            {
                grenadeRigidB.linearVelocity = Vector3.zero;
                grenadeRigidB.angularVelocity = Vector3.zero;

                grenadeRigidB.isKinematic = true;

                //Making it stick to a surface; Moving with the object it parents
                transform.SetParent(collision.transform);

                //Setting it true that it is on a surface
                OnSurface = true;
            }

            
        } 
        else if (collision.transform.CompareTag("Breakable") || collision.transform.CompareTag("Untagged") || collision.transform.CompareTag("Enemy"))
        {
            if (!isCooked)
            {
                destroyTimer = 0;
                StartCoroutine(explode());
            }
            
            
        }

        
    }

    IEnumerator explode()
    {
        yield return new WaitForSeconds(destroyTimer);
        if (explosionPrefab != null )
        Instantiate(explosionPrefab, transform.position, Quaternion.identity); 
        Destroy(gameObject);
    }

    public void RemoteDetonate()
    {
        destroyTimer = 0;
        StartCoroutine(explode());
    }


}

