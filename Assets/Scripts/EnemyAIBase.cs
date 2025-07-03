using UnityEngine;

public class EnemyAIBase : MonoBehaviour, IDamage
{
    //Enemy health
    protected [SerializeField] int enemyCurrentHealthPoints;
    public [SerializeField] int enemyHealthPointsMax;

    //Enemy movment
    public [SerializeField] float enemySpeed;

    //player detection
    public [SerializeField] float enemyDetectionMeshRange;
    protected Transform enemyPlayerObject;
    protected bool enemyPlayerInSight;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {   //To save the enemy's max health to currently.
        enemyCurrentHealthPoints = enemyHealthPointsMax;
        //Assigning the object with "Player" string tag to the Transform var
        enemyPlayerObject = GameObject.FindGameObjectWithTag("Player").transform;

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    public void takeDamage(int amount)
    {
       
    }

}
