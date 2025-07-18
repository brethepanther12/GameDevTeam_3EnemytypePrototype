using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{

    enum damagetype { moving, stationary, DOT, homing }
    [SerializeField] damagetype type;
    [SerializeField] Rigidbody rb;

    [SerializeField] public int damageAmount;
    [SerializeField] public float damageRate;
    [SerializeField] public int speed;
    [SerializeField] public int destroyTime;

    [SerializeField] GameObject impactPrefab;

    bool isDamaging;
    private int weaponDMG;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.Play();
        }

        if (type == damagetype.moving || type == damagetype.homing)
        {
            Destroy(gameObject, destroyTime);
            if (type == damagetype.moving)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (type == damagetype.homing)
        {
            rb.linearVelocity = (gamemanager.instance.player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && type != damagetype.DOT)
        {
        
            dmg.takeDamage(damageAmount + weaponDMG);
            
        }

        if (type == damagetype.moving || type == damagetype.homing)
        {

            if (impactPrefab != null)
            {
                Instantiate(impactPrefab, transform.position, Quaternion.LookRotation(transform.forward));
            }

            Destroy(gameObject);
        }

        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        if (impactPrefab != null && !isDamaging)
        {
            Instantiate(impactPrefab, transform.position, Quaternion.LookRotation(transform.forward));
        }

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && type == damagetype.DOT && !isDamaging)
        {
            StartCoroutine(damageOther(dmg));
        }
    }

    public void SetWeaponDamage(int wepDmg)
    {
        weaponDMG = wepDmg;
    }

    IEnumerator damageOther(IDamage d)
    {
        isDamaging = true;

        d.takeDamage(damageAmount);

        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }
}