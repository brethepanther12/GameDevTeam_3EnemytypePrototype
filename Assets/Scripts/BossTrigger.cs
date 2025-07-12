using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    private BossAI boss;

    void Start()
    {
        boss = GetComponentInParent<BossAI>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered boss trigger!");
            boss.SetPlayerInSight(true);

            gamemanager.instance.StartBossFight(boss);
        }
    }
}