using UnityEngine;
using System.Collections;
using System;

public class damage : MonoBehaviour
{

    public enum damagetype { moving, stationary, DOT, homing, explosion }

    [SerializeField] damagetype type;
    [SerializeField] public Rigidbody rb;
    [SerializeField] private float maxHomingAngle = 30f;
    [SerializeField] public int damageAmount;
    [SerializeField] public float damageRate;
    [SerializeField] public int speed;
    [SerializeField] public float destroyTime;
    [SerializeField] public float blastRadius;
    [SerializeField] private float homingDelay;
    private float homingTimer = 0f;
    private bool homingActive = false;
    private bool hasExploded = false;

    [SerializeField] public GameObject impactPrefab;

    private Transform homingTarget;
    bool isDamaging;
    public int weaponDMG;
    public DamageStatus currentStatus;
    public StatusEffectData currentStatusData;

    public StatusEffectHandler statusTarget;


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
        if (type != damagetype.homing) return;

        homingTimer += Time.deltaTime;
        if (homingTimer < homingDelay) return;

        homingActive = true;

        if (homingTarget != null && homingActive)
        {
            Vector3 direction = (homingTarget.position - transform.position).normalized;

            // Smooth rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            rb.linearVelocity = transform.forward * speed;
            return;
        }

        // Fallback auto-targeting
        if (gameObject.layer == LayerMask.NameToLayer("Enemy Bullet"))
        {
            Vector3 direction = (gamemanager.instance.player.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else if (gameObject.layer == LayerMask.NameToLayer("Player Bullet"))
        {
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

            if (closest != null)
            {
                Vector3 direction = (closest.transform.position - transform.position).normalized;

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

                rb.linearVelocity = transform.forward * speed;
            }
            else
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by {other.name}");
        if (other.isTrigger)
            return;

        // Spawn impact effect immediately
        if (impactPrefab != null)
        {
            Vector3 hitPoint = transform.position;
            Vector3 hitNormal = transform.forward;

            GameObject impact = Instantiate(impactPrefab, hitPoint, Quaternion.LookRotation(hitNormal));

            // Parent to the enemy if valid
            if (other.CompareTag("Enemy"))
            {
                impact.transform.SetParent(other.transform, true); // true keeps world position
                impact.transform.position += hitNormal * 0.01f; // slight offset to avoid z-fighting
            }

            // Assign status duration
            ImpactEffectLifetime lifetime = impact.GetComponent<ImpactEffectLifetime>();
            if (lifetime != null)
                lifetime.duration = currentStatusData.statusDuration;
        }

        // Damage logic
        if (blastRadius > 0f && !hasExploded)
        {
            hasExploded = true;
            ApplyAOEDamage();
        }
        else
        {
            IDamage dmg = other.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(damageAmount + weaponDMG);

            StatusEffectHandler statusTarget = other.GetComponent<StatusEffectHandler>();
            if (statusTarget != null)
                statusTarget.ApplyStatusEffect(currentStatusData, dmg);

            if (other.CompareTag("Enemy"))
            {
                GameObject reticle = GameObject.Find("Reticle");
                ReticleController rc = reticle.GetComponent<ReticleController>();
                rc.Pulse(true);
            }
        }

        // Destroy projectile
        if (type == damagetype.moving || type == damagetype.homing)
        {
            Destroy(gameObject);
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        if (type == damagetype.DOT && impactPrefab != null && !isDamaging)
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

    public void SetBlastRadius(float radius)
    {
        blastRadius = radius;
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

    public void SetStatusData(StatusEffectData statusData)
    {
        currentStatusData = statusData;
    }

    public void ApplyDamageTo(Collider other)
    {
        IDamage dmg = other.GetComponent<IDamage>();

        if (other.GetComponent<StatusEffectHandler>() != null)
        {
            statusTarget = other.GetComponent<StatusEffectHandler>();
            statusTarget.ApplyStatusEffect(currentStatusData, dmg);
        }

        if (dmg != null)
        {
            dmg.takeDamage(damageAmount + weaponDMG);
        }
    }

    private void OnDestroy()
    {

        if (type == damagetype.explosion && blastRadius > 0f && !hasExploded)
        {
            hasExploded = true;
            ApplyAOEDamage();
        }

    }

    private void ApplyAOEDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, blastRadius, LayerMask.GetMask("Enemy", "Default", "Player", "Environment", "Enemy Bullet", "Bullet"));

        foreach (Collider target in hits)
        {
            IDamage aoeDmg = target.GetComponent<IDamage>();
            if (aoeDmg != null)
                aoeDmg.takeDamage(damageAmount + weaponDMG);

            StatusEffectHandler statusTarget = target.GetComponent<StatusEffectHandler>();
            if (statusTarget != null)
                statusTarget.ApplyStatusEffect(currentStatusData, aoeDmg);

            if (target.CompareTag("Enemy"))
            {
                GameObject reticle = GameObject.Find("Reticle");
                ReticleController rc = reticle.GetComponent<ReticleController>();
                rc.Pulse(true);
            }
        }

        Debug.Log($"Explosion triggered at {transform.position} with radius {blastRadius}");
    }

    public void SetHomingTarget(Transform target)
    {
        homingTarget = target;
    }

}