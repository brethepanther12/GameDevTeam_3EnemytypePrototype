using UnityEngine;
using System.Collections;

public class BatAI : EnemyAIBase
{
    // Bat enemy's ability to detect the player and retreat at a certain distance.
    [SerializeField] public float batAttackRange;
    [SerializeField] public float batRetreatDistance;
    bool batHasAttacked;
    bool batIsRetreating;

    //Bat enemy's damage
    [SerializeField] public float batAttackCoolDown;
    [SerializeField] public int batDamage;
    private playerController batPlayer;


    //Ceiling detection and attachment
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
    }

    // Update is called once per frame
    protected override void Update()
    {
        
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
        enemyNavAgent.SetDestination(batRetreatTarget);

        //Than start the yield return, (for this is going to be a routine, Attack-Retreat!) 
        yield return new WaitForSeconds(batAttackCoolDown);

        //toggle the that the bat has attacked to be false
        batHasAttacked = false;
    }

    private void AttachToNearestCeiling()
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

        //Starting a loop at the second ceiling and checking each remaining
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
        
        //Setting the enemy Nav to the ceiling point
        enemyNavAgent.SetDestination(batCeilingAttachPoint);
    }
}
