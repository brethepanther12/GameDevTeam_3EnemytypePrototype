using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BatAI : EnemyAIBase
{
    public enum BatState { ReturningToCeiling, Chasing, Attacking, Retreating }

    public Transform[] ceilingPoints;
    public float attackRange;
    public float retreatDistance;
    public int attackDamage;
    public float attackCooldown;
    [SerializeField] private LayerMask visionLayers;

    public float hoverHeight;           
    public float attackHoverHeight;   
    public float verticalLerpSpeed;     

    private BatState currentState;
    private Vector3 ceilingTarget;
    private Vector3 retreatTarget;
    private bool isAttacking = false;

    protected override void Start()
    {
        base.Start();
        BatChooseNearestCeiling();
        currentState = BatState.ReturningToCeiling;
        enemyNavAgent.stoppingDistance = 0.5f;
        enemyNavAgent.updateUpAxis = false;
    }

    protected override void Update()
    {
        base.Update();

        switch (currentState)
        {
            case BatState.ReturningToCeiling:
                MoveToTarget(ceilingTarget, hoverHeight);
                if (Vector3.Distance(GetXZ(transform.position), GetXZ(ceilingTarget)) <= 0.5f)
                {
                    enemyNavAgent.isStopped = true;
                }
                break;

            case BatState.Chasing:
                if (enemyPlayerInSight && HasLineOfSightOfPlayer())
                {
                    Vector3 playerPosXZ = GetXZ(enemyPlayerObject.position);
                    MoveToTarget(playerPosXZ, attackHoverHeight);

                    float distance = Vector3.Distance(transform.position, enemyPlayerObject.position);
                    if (distance <= attackRange && !isAttacking)
                    {
                        StartCoroutine(BatAttack());
                    }
                }
                else
                {
                    Debug.Log("[Chasing] Lost line of sight, returning to ceiling.");
                    currentState = BatState.ReturningToCeiling;
                }
                break;

            case BatState.Retreating:
                MoveToTarget(GetXZ(retreatTarget), hoverHeight);
                if (Vector3.Distance(GetXZ(transform.position), GetXZ(retreatTarget)) <= 0.5f)
                {
                    if (enemyPlayerInSight && HasLineOfSightOfPlayer())
                        currentState = BatState.Chasing;
                    else
                        currentState = BatState.ReturningToCeiling;
                }
                break;
        }
    }
    protected override bool HasLineOfSightOfPlayer()
    {
        if (enemyPlayerObject == null)
        {
            return false;
        }

        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 target = enemyPlayerObject.position + Vector3.up * 1f;
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, visionLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }



    private void MoveToTarget(Vector3 horizontalTarget, float targetHeight)
    {
       
        enemyNavAgent.isStopped = false;
        enemyNavAgent.SetDestination(horizontalTarget);

      
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, targetHeight, Time.deltaTime * verticalLerpSpeed);
        transform.position = pos;

       
        Vector3 moveDir = horizontalTarget - GetXZ(transform.position);
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    private IEnumerator BatAttack()
    {
        isAttacking = true;
        enemyNavAgent.isStopped = true;

        if (enemyPlayerObject != null)
        {
            playerController pc = enemyPlayerObject.GetComponent<playerController>();
            if (pc != null)
            {
                pc.takeDamage(attackDamage);
                Debug.Log("[BatAttack] Dealt " + attackDamage + " to player.");
            }
        }

        yield return new WaitForSeconds(0.2f); // pause before retreat
        BatStartRetreat();
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void BatStartRetreat()
    {
        enemyNavAgent.isStopped = false;
        Vector3 awayDirection = (transform.position - enemyPlayerObject.position);
        awayDirection.y = 0;
        if (awayDirection.sqrMagnitude < 0.01f)
            awayDirection = Vector3.right;

        retreatTarget = transform.position + awayDirection.normalized * retreatDistance;
        currentState = BatState.Retreating;
    }

    private void BatChooseNearestCeiling()
    {
        float closestDist = float.MaxValue;
        foreach (var ceiling in ceilingPoints)
        {
            float dist = Vector3.Distance(transform.position, ceiling.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                ceilingTarget = ceiling.position;
            }
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.CompareTag("Player"))
        {
            currentState = BatState.Chasing;
            enemyNavAgent.isStopped = false;
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (other.CompareTag("Player"))
        {
            currentState = BatState.ReturningToCeiling;
            enemyNavAgent.isStopped = false;
        }
    }

    private Vector3 GetXZ(Vector3 v)
    {
        return new Vector3(v.x, transform.position.y, v.z);
    }
}
