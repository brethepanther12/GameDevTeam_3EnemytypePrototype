using UnityEngine;
using System.Collections;

public class BatAI : EnemyAIBase
{
    // Bat enemy's ability to detect the player and retreat at a certain distance.
    [SerializeField] public float batAttackRange;
    [SerializeField] public float batRetreatDistance;
    bool batHasAttacke;
    bool batIsReatreating;

    //Bat enemy's damage
    [SerializeField] public float batDamage;

    private Transform currentCeiling;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}
