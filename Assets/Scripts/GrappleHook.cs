using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GrappleHook : MonoBehaviour
{
    [Header("Grapple Settings")]
    public Transform grappleOrigin;
    public float grappleRange = 15f;
    public float pullSpeed = 15f;
    public LayerMask grappleLayer;
    [SerializeField] private float grappleCD = 5f;
    [SerializeField] private Image grappleCooldownUI;

    private Rigidbody grappledRigidbody;
    private GameObject grappledEnemy;
    private bool isPulling = false;
    private float lastGrapple = -Mathf.Infinity;
    private LineRenderer grappleLine;
    private NavMeshAgent grappledAgent;
    private IGrapplable grappledAI;
    private bool isReleased = false;

    void Start()
    {
        grappleLine = gameObject.AddComponent<LineRenderer>();
        grappleLine.startWidth = 0.05f;
        grappleLine.endWidth = 0.05f;
        grappleLine.material = new Material(Shader.Find("Sprites/Default"));
        grappleLine.startColor = Color.red;
        grappleLine.endColor = Color.red;
        grappleLine.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= lastGrapple + grappleCD)
        {
            TryGrapple();
        }

        if (isPulling && grappledEnemy != null)
        {
            PullEnemy();

            if(grappledEnemy != null)
            {
                grappleLine.enabled = true;
                grappleLine.SetPosition(0, grappleOrigin.position);
                grappleLine.SetPosition(1, grappledEnemy.transform.position);
            }
   
        }
        else
        {
            grappleLine.enabled = false;
        }

        if (grappleCooldownUI != null)
        {
            float timeSinceLast = Time.time - lastGrapple;
            float cooldownRatio = 1 - Mathf.Clamp01(timeSinceLast / grappleCD);

            if (cooldownRatio > 0f)
            {
                grappleCooldownUI.enabled = true;
                grappleCooldownUI.fillAmount = cooldownRatio;
            }
            else
            {
                grappleCooldownUI.enabled = false;
            }
        }
    }

    void TryGrapple()
    {
        RaycastHit hit;
        Vector3 rayStart = grappleOrigin.position + Camera.main.transform.forward * 0.5f;

        Debug.DrawRay(rayStart, Camera.main.transform.forward * grappleRange, Color.cyan, 1f);

        if (Physics.Raycast(rayStart, Camera.main.transform.forward, out hit, grappleRange, grappleLayer, QueryTriggerInteraction.Collide))
        {
            GameObject target = hit.collider.gameObject;

            if (target.TryGetComponent<IGrapplable>(out IGrapplable ai))
            {
                if (!ai.canBeGrappled)
                {
                    Debug.Log("This target cannot be grappled: " + target.name);
                    return;
                }

                grappledEnemy = target;
                grappledAI = ai;
                grappledAI.isBeingGrappled = true;
                isPulling = true;
                lastGrapple = Time.time;

                if (target.TryGetComponent<PickupMotion>(out PickupMotion motion))
                {
                    motion.isBeingPulled = true;
                }

                if (target.TryGetComponent<NavMeshAgent>(out grappledAgent))
                {
                    grappledAgent.enabled = false;
                }

                Debug.Log("Grapple hit: " + target.name);
            }
            else
            {
                Debug.Log("Hit something without IGrapplable: " + target.name);
            }
        }
        else
        {
            Debug.Log("Grapple missed.");
        }
    }

    void PullEnemy()
    {
        if (grappledEnemy == null) return;

        Vector3 newPos = Vector3.MoveTowards(
            grappledEnemy.transform.position,
            grappleOrigin.position,
            pullSpeed * Time.deltaTime
        );

        if (grappledRigidbody != null)
        {
            grappledRigidbody.MovePosition(newPos);
        }
        else
        {
            grappledEnemy.transform.position = newPos;
        }

        float distance = Vector3.Distance(grappledEnemy.transform.position, grappleOrigin.position);

        if (distance <= 1.5f && !isReleased)
        {
            isPulling = false;
            isReleased = true;


            if (grappledEnemy.TryGetComponent<ItemPickup>(out ItemPickup pickup))
            {

                grappledEnemy = null;
                grappledAgent = null;
                grappledAI = null;
                isReleased = false;
                return;
            }

            StartCoroutine(ResumeEnemyAI());
        }
    }

    private IEnumerator ResumeEnemyAI()
    {
        yield return new WaitForSeconds(0.5f);

        if (grappledAgent != null)
        {
            if (!grappledAgent.enabled)
                grappledAgent.enabled = true;

            if (!grappledAgent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(grappledEnemy.transform.position, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
                    grappledAgent.Warp(navHit.position);
                }
            }
        }

        if (grappledAI != null)
        {
            grappledAI.isBeingGrappled = false;
        }

        grappledEnemy = null;
        grappledAgent = null;
        grappledAI = null;        
        isReleased = false;
    }
}