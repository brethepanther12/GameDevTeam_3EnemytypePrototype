using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    private BossAI boss;

    public GameObject[] fireBowls;
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

            // Enable fire bowls and lights
            foreach (GameObject bowl in fireBowls)
            {
                if (bowl != null) continue;

                // Start flame particles
                Transform flameTransform = bowl.transform.Find("Flame");
                if (flameTransform != null)
                {
                    ParticleSystem flame = flameTransform.GetComponent<ParticleSystem>();
                    if (flame != null) flame.Play();
                }

                // Enable fire light
                Transform lightTransform = bowl.transform.Find("FireLight");
                if (lightTransform != null)
                {
                    Light fireLight = lightTransform.GetComponent<Light>();
                    if (fireBowls != null) fireLight.enabled = true;
                }
            }
        }
    }
}