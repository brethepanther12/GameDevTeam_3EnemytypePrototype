using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{

    public enum damagetype { moving, stationary, DOT, homing, explosion}
    [SerializeField] damagetype type;
    [SerializeField] public Rigidbody rb;

    [SerializeField] public int damageAmount;
    [SerializeField] public float damageRate;
    [SerializeField] public int speed;
    [SerializeField] public float destroyTime;
    [SerializeField] GameObject projectileStraightPrefab;
    [SerializeField] GameObject projectileHomingPrefab;

    [SerializeField] GameObject impactPrefab;

    bool isDamaging;
    public int weaponDMG;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.Play();
        }

        if (type == damagetype.moving || type == damagetype.homing || type == damagetype.explosion)
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

            if (other.CompareTag("Enemy"))
            {
                GameObject reticle = GameObject.Find("Reticle");
                ReticleController rc = reticle.GetComponent<ReticleController>();
                rc.Pulse(true);
            }
        }

        if ((type == damagetype.moving || type == damagetype.homing) && impactPrefab != null)
        {
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + transform.forward * 0.2f;
            Vector3 rayDirection = -transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out hit, 1f, ~0, QueryTriggerInteraction.Ignore))
            {
                Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
            else
            {
                Instantiate(impactPrefab, transform.position, Quaternion.LookRotation(-transform.forward));
            }
        }
        if(type == damagetype.moving || type== damagetype.homing)
        {
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

    public void SetDamageType(damagetype newType)
    {
        type = newType;
    }

    IEnumerator damageOther(IDamage d)
    {
        isDamaging = true;

        d.takeDamage(damageAmount);

        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }
}