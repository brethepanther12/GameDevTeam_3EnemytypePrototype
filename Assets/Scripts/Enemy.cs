using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class Enemy : MonoBehaviour, IDamage
{

    [SerializeField] SkinnedMeshRenderer[] modelParts;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;

    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private float reloadTime = 1.5f;

    [SerializeField] Animator animator;

    [SerializeField] int HP;
    [SerializeField] int fov;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int roamDistance;
    [SerializeField] int roamPauseTime;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;

    Color colorOrig;

    private int currentAmmo;
    private bool isReloading;
    bool playerInTrigger;
    float shootTimer;
    float angleToPlayer;
    float roamTimer;
    float stoppingDistanceOrig;

    Vector3 playerDir;
    Vector3 startingPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmo = maxAmmo;
        colorOrig = modelParts[0].material.color;
        gamemanager.instance.updateGameGoal(1);
        startingPos = transform.position;
        stoppingDistanceOrig = agent.stoppingDistance;

    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (agent.remainingDistance < 0.01f)
        {
            roamTimer += Time.deltaTime;
        }

        if (playerInTrigger && !CanSeePlayer())
        {
            RoamCheck();

        }
        else if (!playerInTrigger)
        {
            RoamCheck();
        }
    }

    void LateUpdate()
    {
        shootPos.LookAt(gamemanager.instance.player.transform.position);
    }

    void RoamCheck()
    {
        if (roamTimer >= roamPauseTime && agent.remainingDistance < 0.01f)
        {
            Roam();
        }
    }

    void Roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDistance;
        ranPos += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDistance, 1);
        agent.SetDestination(hit.position);

    }

    bool CanSeePlayer()
    {
        playerDir = gamemanager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(headPos.position, playerDir);

        RaycastHit hit;

        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= fov)
            {
                shootTimer += Time.deltaTime;

                if (shootTimer >= shootRate)
                {
                    Shoot();
                }

                if(currentAmmo <=0 && !isReloading)
                {
                    StartCoroutine(Reload());
                }

                agent.SetDestination(gamemanager.instance.player.transform.position);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    FaceTarget();
                }

                agent.stoppingDistance = stoppingDistanceOrig;

                return true;
            }
        }

        agent.stoppingDistance = 0;
        return false;
    }

    void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceTargetSpeed * Time.deltaTime);

        Quaternion gunRot = Quaternion.LookRotation(gamemanager.instance.player.transform.position - shootPos.position);
        shootPos.rotation = Quaternion.Lerp(shootPos.rotation, gunRot, faceTargetSpeed * Time.deltaTime);
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            agent.stoppingDistance = 0;
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        agent.SetDestination(gamemanager.instance.player.transform.position);


        if (HP <= 0)
        {

            gamemanager.instance.updateGameGoal(-1);
            Destroy(gameObject);

        }
        else
        {

            StartCoroutine(FlashRed());
        }
    }

    IEnumerator FlashRed()
    {
        foreach (var part in modelParts)
        {
            part.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var part in modelParts)
        {
            part.material.color = colorOrig;
        }
    }

    void Shoot()
    {
        if(isReloading || currentAmmo <=0)
        {
            return;
        }

        shootTimer = 0;

        currentAmmo--;

        animator.SetTrigger("Shoot");

        Instantiate(bullet, shootPos.position, transform.rotation);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        animator.SetTrigger("Reload");
        yield return new WaitForSeconds(reloadTime -.25f);
        isReloading = false;
        yield return new WaitForSeconds(.25f);
        currentAmmo = maxAmmo;
    }
}
