using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeCloud : MonoBehaviour
{
    [SerializeField] private float smokeDuration;
    [SerializeField] private GameObject smokePrefab;

    private void Start()
    {
        if (smokePrefab != null)
        {
            GameObject smoke = Instantiate(smokePrefab, transform.position, Quaternion.identity);

            // Destroy the smoke after its duration
            Destroy(smoke, smokeDuration);
        }

        // Optionally destroy this parent (e.g., grenade shell)
        Destroy(gameObject);
    }
}
