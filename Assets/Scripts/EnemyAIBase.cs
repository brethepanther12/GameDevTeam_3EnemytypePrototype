using System.Collections;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI;

public class EnemyAIBase : MonoBehaviour, IDamage
{
    //Enemy health
    [SerializeField] protected int enemyCurrentHealthPoints;
    [SerializeField] public int enemyHealthPointsMax;

    [SerializeField] public int shield;
    [SerializeField] public int armor;

    [SerializeField] public GameObject shieldPrefab;
    [SerializeField] public GameObject armorPrefab;
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

    private float originalSpeed;
    private Coroutine slowRoutine;


    protected Vector3 enemyPlayerDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()

    {

        originalSpeed = enemyNavAgent.speed;
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
            if (enemyNavAgent.remainingDistance <= enemyNavAgent.stoppingDistance)
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

    protected virtual bool HasLineOfSightOfPlayer()
    {
        if (enemyPlayerObject == null) return false;

        Vector3 target = (enemyPlayerObject.position + Vector3.up * 1f);
        Vector3 direction = (transform.position - target).normalized;

        float distance = Vector3.Distance(transform.position, target);

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    public virtual void takeDamage(int amount)
    {

        if (shield > 0)
        {
            shield -= amount;
            GetComponent<EnemyHealthUI>().UpdateHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);

            if (shield <= 0)
            {
                shield = 0;

                shieldPrefab.SetActive(false);
                armor -= amount;
                GetComponent<EnemyHealthUI>().UpdateHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);
            }

        }

        else if (armor > 0)
        {
            armor -= amount;
            GetComponent<EnemyHealthUI>().UpdateHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);

            if (armor <= 0 && shield <= 0)
            {

                armor = 0;
                shield = 0;
                armorPrefab.SetActive(false);
                GetComponent<EnemyHealthUI>().UpdateHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);
            }
        }
        else
        {
            enemyCurrentHealthPoints -= amount;
            GetComponent<EnemyHealthUI>().UpdateHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);
        }
        //GetComponent<EnemyHealthUI>().UpdateHealthBar(enemyCurrentHealthPoints, enemyHealthPointsMax);

        if (enemyCurrentHealthPoints <= 0)
        {
            gamemanager.instance.updateGameGoal(-1);
            enemyDeath();
            ScoreManager.instance.AddPointsForEnemy(gameObject.tag);
        }
        else
        {
            StartCoroutine(enemyFlashRead());
        }
    }

    public void takeDamage(int amount, StatusEffectData effect)
    {
        switch (effect.statusType)
        {

            case DamageStatus.None:

                takeDamage(amount);
                break;

            case DamageStatus.Fire:

                if (shield <= 0 && armor <= 0 && enemyCurrentHealthPoints > 0)
                {
                    takeDamage(amount + 1);
                }
                else
                {
                    takeDamage(amount);
                }
                break;

            case DamageStatus.Corrosive:

                if (shield <= 0 && armor > 0)
                {
                    takeDamage(amount + 1);
                }
                else
                {
                    takeDamage(amount);
                }
                break;

            case DamageStatus.Cryo:

                takeDamage(amount);
                break;

            case DamageStatus.Electric:

                if (shield > 0)
                {
                    takeDamage(amount + 1);
                }
                else
                {
                    takeDamage(amount);
                }
                break;

            default:
                break;
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

    public void slowDown(float magnitude, float duration)
    {
        if (slowRoutine != null)
        {
            StopCoroutine(slowRoutine);
        }

        slowRoutine = StartCoroutine(SlowRoutine(magnitude, duration));
    }

    private IEnumerator SlowRoutine(float magnitude, float duration)
    {
        //NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (enemyNavAgent == null) yield break;

        if (originalSpeed == 0f)
            originalSpeed = enemyNavAgent.speed;

        float slowedSpeed = originalSpeed * (1f - magnitude);
        enemyNavAgent.speed = slowedSpeed;

        yield return new WaitForSeconds(duration);

        enemyNavAgent.speed = originalSpeed;
        slowRoutine = null;
    }
}

