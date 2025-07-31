using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeCloud : MonoBehaviour
{
    [SerializeField] private float smokeDuration;
    [SerializeField] private GameObject smokePrefab;

    private void Start()
    {
        if(smokePrefab == null)
        {
            Debug.LogWarning("Smoke prefab is null!");
            return;
        }

        GameObject smoke = Instantiate(smokePrefab, transform.position, Quaternion.identity);
        Debug.Log("Smoke instantiated: " + smoke.name);

        damage smokeDamage = smoke.GetComponent<damage>();
        float duration = smokeDuration;

        if (smokeDamage != null)
        {
            duration = smokeDamage.destroyTime;
        }

        Destroy(smoke, duration);
        Destroy(gameObject, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        Visibility visible = other.GetComponent<Visibility>();
        if (visible != null) { visible.SetInvisible(true); }
    }
    
    private void OnTriggerExit(Collider other)
    {
        Visibility visible = other.GetComponent<Visibility>();
        if (visible != null) { visible.SetInvisible(false); }
    }

    private void OnDestroy()
    {
        if (gamemanager.instance.player != null)
        {
            Visibility visible = gamemanager.instance.playerScript.GetComponent<Visibility>();
            if (visible != null) { visible.SetInvisible(false); }
        }
    }
}
