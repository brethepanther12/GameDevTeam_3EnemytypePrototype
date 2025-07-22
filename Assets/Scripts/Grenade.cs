using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    [SerializeField] private Rigidbody grenadeRigidB;

    [SerializeField] private int grenadeSpeed;
    [SerializeField] private int grenadeSpeedY;
    [SerializeField] private float destroyTimer;

    [SerializeField] private GameObject explosionPrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grenadeRigidB.linearVelocity = (transform.forward * grenadeSpeed) + (transform.up * grenadeSpeedY);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerable explode()
    {
        yield return new WaitForSeconds(destroyTimer);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}

