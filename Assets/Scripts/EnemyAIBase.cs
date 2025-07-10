
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIBase : MonoBehaviour, IDamage
{
    //Enemy health
    [SerializeField] protected int enemyCurrentHealthPoints;
    [SerializeField] public int enemyHealthPointsMax;
    public int CurrentHealthPoints => enemyCurrentHealthPoints;
    public int MaxHealthPoints => enemyHealthPointsMax;
    //Enemy model
    [SerializeField] public SkinnedMeshRenderer[] enemyModel;
    protected Color enemyColorOrigin;

    //Enemy movement
    [SerializeField] public float enemySpeed = 3;

    //player detection
    //Navigation Mesh is used instead -v-
    //public [SerializeField] float enemyDetectionMeshRange;
     [SerializeField] public NavMeshAgent enemyNavAgent;
    [SerializeField] public Transform enemyPlayerObject;
    protected bool enemyPlayerInSight;


    protected Vector3 enemyPlayerDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()

    { 
        //To save the enemy's max health to currently.
        enemyCurrentHealthPoints = enemyHealthPointsMax;

        EnemyHealthUI ui = GetComponent<EnemyHealthUI>();
        if (ui != null)
        {
            ui.UpdateHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);
        }

        //Assigning the object with "Player" string tag to the Transform var
        enemyPlayerObject = GameObject.FindGameObjectWithTag("Player").transform;

        //Assigning the 3d vector of the player's position
        enemyPlayerDirection = enemyPlayerObject.transform.position - transform.position;

        //Fetching navigation mesh attached to 'this' game object
        enemyNavAgent = GetComponent<NavMeshAgent>();

        //This assigns the original color of the placed model in the Unity Inspector

        enemyColorOrigin = enemyModel.material.color;

        gamemanager.instance.updateGameGoal(1);

        enemyColorOrigin = enemyModel[0].material.color;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Since the enemy is moving, it will constantly update
        enemyMoveToPlayer();
    }
    protected virtual void enemyDeath()
    {
        //Debug will message the debugger that an enemy dies
        //by getting the string name of the gameObject
        Debug.Log($"{gameObject.name} has died");

        //Then it destroy 'this' object after
        Destroy(gameObject);
        
    }
    protected virtual void enemyMoveToPlayer()
    {
        //If check checks the toggled bool enemyPlayerInSight
        if (enemyPlayerInSight)
        {
            //The navigation mesh calls SetDestination, which will be the Player's position
            enemyNavAgent.SetDestination(enemyPlayerObject.transform.position);

            //This check is for the enemy's position to the target is
            //less than the allowed distance before stopping
            if(enemyNavAgent.remainingDistance <= enemyNavAgent.stoppingDistance)
            {
                //This calls for the enemy to face forward to the player
                enemyFacePlayer();
            }
        }

    }

    //The OnTriggers toggle the player going in and out of the NavMesh
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyPlayerInSight = true;
        }
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyPlayerInSight = false;
        }
    }

    protected virtual void enemyFacePlayer()
    {
        //rotate holds the LookRotation() that creates
        //a rotation based on a specific forward(z-axis) and upwards(y-axis)
        // A Vector# can be placed with the x an y axis of the player's direction
        // -For 'flying' enemy's the z can be placed in as well.
        Quaternion rotate = Quaternion.LookRotation(new Vector3(enemyPlayerDirection.x, 0, enemyPlayerDirection.z));

        //Lerp takes the estimated value of the rotation of the enemy and the player
        transform.rotation = Quaternion.Lerp(transform.rotation, rotate, Time.deltaTime * enemySpeed);
    }
    public virtual void takeDamage(int amount)
    {
        enemyCurrentHealthPoints -= amount;

        GetComponent<EnemyHealthUI>().UpdateHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);

        if (enemyCurrentHealthPoints <= 0)
        {
            gamemanager.instance.updateGameGoal(-1);
            enemyDeath();
        }
        else
        {
           StartCoroutine(enemyFlashRead());
        }
    }

    protected virtual IEnumerator enemyFlashRead()
    {
        foreach (var part in enemyModel)
        {
            part.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var part in enemyModel)
        {
            part.material.color = enemyColorOrigin;
        }
    }
}
