using UnityEngine;
using System.Collections;

public class BatAI : EnemyAIBase, IDamage
{
    // Bat enemy's ability to detect the player and retreat at a certain distance.
    [SerializeField] public float batAttackRange;
    [SerializeField] public float batAttackPosition;
    [SerializeField] public float batRetreatDistance;
    bool batHasAttacked;
    bool batIsRetreating;

    //Bat enemy's damage
    [SerializeField] public float batAttackCoolDown;
    [SerializeField] public int batDamage;
    private playerController batPlayer;


    //Ceiling detection and attachment
    [SerializeField] public float batCeilingRange;
    [SerializeField] public float batCeilingRangeThreshold;
    private Transform batCurrentCeiling;
    private Vector3 batRetreatTarget;
    private Vector3 batCeilingAttachPoint;
    private bool batIsReturningToCeiling;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        //Gives the Navigation mesh agent an update to give the bat the ability to fly freely
       enemyNavAgent.updateUpAxis = false;
       enemyNavAgent.updateRotation = false;
        enemyNavAgent.updateUpAxis = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (enemyPlayerInSight && !batIsRetreating)
        {
            enemyMoveToPlayer();
        }
        else if (!enemyPlayerInSight && !batIsRetreating && !batIsReturningToCeiling)
        {   
            // Just sets the target point ONCE
            batAttachToNearestCeiling(); 
        }

        //Moves toward retreat target if currently retreating
        if (batIsRetreating)
        {
            transform.position = Vector3.MoveTowards(transform.position, batRetreatTarget, enemySpeed * Time.deltaTime);
            batFaceTarget(batRetreatTarget);
            if (Vector3.Distance(transform.position, batRetreatTarget) <= batCeilingRangeThreshold)
            {
                batIsRetreating = false;
            }
        }
        // Move to the ceiling 
        if (batIsReturningToCeiling)
        {
            transform.position = Vector3.MoveTowards(transform.position, batCeilingAttachPoint, enemySpeed * Time.deltaTime);
            batFaceTarget(batCeilingAttachPoint);

            if (Vector3.Distance(transform.position, batCeilingAttachPoint) <= batCeilingRangeThreshold)
            {
                transform.rotation = Quaternion.Euler(180f, transform.rotation.eulerAngles.y, 0f);
                batIsReturningToCeiling = false;
            }
        }
        if (batIsReturningToCeiling)
        {
            Debug.DrawLine(transform.position, batCeilingAttachPoint, Color.red);
        }
    }

    private IEnumerator batAttackPlayer()
    {
        batHasAttacked = false;
        //The if check is to simulate the damage the player will take.
        batPlayer = enemyPlayerObject.GetComponent<playerController>();

        if (batPlayer != null) 
        {
            batPlayer.takeDamage(batDamage);
        }

        //toggle the retreat bool and direct the bat away from the players position
        batIsRetreating = true;

        //Add a Vector3 so the bat can reposition in a different area, away from the player
        Vector3 directionAwayFromPlayer = (transform.position - enemyPlayerObject.position).normalized;

        //Setting the target retreat position for the bat to locate and track towards.
        batRetreatTarget = transform.position + directionAwayFromPlayer * batRetreatDistance;

        //Then setting the destination of the Nav Agent of the retreat target
        //enemyNavAgent.SetDestination(batRetreatTarget);
        //transform.position = Vector3.MoveTowards(transform.position, batRetreatTarget, enemySpeed * Time.deltaTime);
        //batFaceTarget(batRetreatTarget);

        while (Vector3.Distance(transform.position, batRetreatTarget) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, batRetreatTarget, enemySpeed * Time.deltaTime);
            batFaceTarget(batRetreatTarget);
            yield return null;
        }
        //Than start the yield return, (for this is going to be a routine, Attack-Retreat!) 
        yield return new WaitForSeconds(batAttackCoolDown);

        //toggle the that the bat has attacked to be false
        batHasAttacked = false;
    }

    private void batAttachToNearestCeiling()
    {
        // Makes an object array that finds the 'ceiling' objects tagged with that name
        GameObject[] ceilings = GameObject.FindGameObjectsWithTag("Ceiling");
        // If no game objects are tagged, the function ends here
        if (ceilings.Length == 0) return;

        //toggle the bool that has the bat return to the ceiling
        batIsReturningToCeiling = true;

        //Stores the index of the closest ceiling in the array
        int nearestIndex = 0;
        //Assigning the calculated distance between the bat and ceiling.
        float minDistance = Vector3.Distance(transform.position, ceilings[0].transform.position);

        //Starting a loop at the second ceiling and checking each remaining in the array
        for (int i = 1; i < ceilings.Length; i++) 
        {
            //Assigning measurements on how far the bat is from the current ceiling
            float distance = Vector3.Distance(transform.position, ceilings[i].transform.position);
            //Checking if the distance is closer the one instantiated(ceiling[0]))
            if (distance < minDistance)
            {
                //update minDistance
                minDistance = distance;
                nearestIndex = i;
            }
        }

        //Once the loop is over, the loop has found which indexed ceiling is closest
        // Assigning the ceiling array transform from the nearest ceiling index
        batCurrentCeiling = ceilings[nearestIndex].transform;

        //Call the method to for the bat attaching to the bottom of the ceilings
        batCeilingAttachPoint = batGetBottomOfCeiling(batCurrentCeiling);

        //Setting the enemy Nav to the ceiling point
        //enemyNavAgent.SetDestination(batCeilingAttachPoint);
        //transform.position = Vector3.MoveTowards(transform.position, batCeilingAttachPoint, enemySpeed * Time.deltaTime);
        //batFaceTarget(batCeilingAttachPoint);
    }

    //Gets the bottom of ceiling object position from inputted parameter variable
    private Vector3 batGetBottomOfCeiling(Transform ceiling)
    {
        Renderer rend = ceiling.GetComponent<Renderer>();
        //If the parameter is not empty
        if (rend != null) 
        { 
         // A bounding box so the bat can attach only at the bottom
        Bounds bounds = rend.bounds;

            //Variables for the x, z, and y axis of the ceiling plane
            float minX, maxX, minZ, maxZ, randomX, randomZ, bottomY;

            //The x-axis of the bottom of the object ceiling
            minX = bounds.min.x + batCeilingRange;
            maxX = bounds.max.x - batCeilingRange;

            //The z-axis of the bottom of the object ceiling
            minZ = bounds.min.z + batCeilingRange;
            maxZ  = bounds.max.z - batCeilingRange;

            //Setting the random x and z value for the axis
            randomX = Random.Range(minX, maxX);
            randomZ = Random.Range(minZ, maxZ);

            //Assigning the y-axis to the bounds y
            bottomY = bounds.min.y;

            return new Vector3(randomX, bottomY, randomZ);
        }
        return ceiling.position;
    }

    protected override void enemyMoveToPlayer()
    {
        if(enemyPlayerInSight && !batIsRetreating)
        {
            float distance = Vector3.Distance(transform.position, enemyPlayerObject.position);

            // If the distance is less than the bat range and the bat has not attacked
            if (distance <= batAttackRange && !batHasAttacked) 
            {
                //Start attacking
                StartCoroutine(batAttackPlayer());
            }
            else 
            {
                //Aiming slightly below player
                Vector3 targetPos = enemyPlayerObject.position;
                targetPos.y += batAttackPosition;
                //Manual flying towards the player
                transform.position = Vector3.MoveTowards(transform.position, enemyPlayerObject.position, enemySpeed * Time.deltaTime);
                batFaceTarget(enemyPlayerObject.position);
            }
        }
    }

    protected void batFaceTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;

        if (direction == Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            //To interpolate between the bat and the looking direction in a spherical linear area 
            transform.rotation = Quaternion.Slerp(transform.rotation , lookRotation, Time.deltaTime * 5f);
        }
    }

    protected override void enemyDeath()
    {
        base.enemyDeath();
    }

    public override void takeDamage(int amount)
    {
       base.takeDamage(amount);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        //When the player enders the radius, bat will attack
        if (other.CompareTag("Player"))
        {
            batIsReturningToCeiling = false;
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if (other.CompareTag("Player"))
        {
            //Start attaching to ceiling when out of range
            batAttachToNearestCeiling();
        }
    }
}
