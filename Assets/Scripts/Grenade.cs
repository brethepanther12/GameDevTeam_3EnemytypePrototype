using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    [SerializeField] private Rigidbody grenadeRigidB;

    [SerializeField] private int grenadeSpeed;
    [SerializeField] private int grenadeSpeedY;
    [SerializeField] private float destroyTimer;

    [SerializeField] private GameObject explosionPrefab;

    [SerializeField] private bool OnStickyBomb;
    [SerializeField] private bool isTracking;
    bool OnSurface;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!OnStickyBomb)
        {
            grenadeRigidB.useGravity = true;
            grenadeRigidB.linearVelocity = (transform.forward * grenadeSpeed) + (transform.up * grenadeSpeedY);
        }
        else
        {
            grenadeRigidB.useGravity = false;
            grenadeRigidB.linearVelocity = (transform.forward * grenadeSpeed);
        }
        StartCoroutine(explode());
    }

    // Update is called once per frame
    void Update()
    {
        if (isTracking && OnStickyBomb && !OnSurface)
        {
            grenadeRigidB.useGravity = false;
            grenadeRigidB.linearVelocity = (gamemanager.instance.player.transform.position - transform.position).normalized * grenadeSpeed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (OnStickyBomb && !OnSurface)
        {   
            //Making it stationary
            grenadeRigidB.isKinematic = true;

            grenadeRigidB.linearVelocity = Vector3.zero;
            grenadeRigidB.angularVelocity = Vector3.zero;


            //Making it stick to a surface; Moving with the object it parents
            transform.SetParent(collision.transform);

            //Setting it true that it is on a surface
            OnSurface = true;
        }
    }

    IEnumerator explode()
    {
        yield return new WaitForSeconds(destroyTimer);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}

