using UnityEngine;
using System.Collections;

public class ImpactEffectLifetime : MonoBehaviour
{
    public float duration = 5f;

    void Start()
    {
        StartCoroutine(StopAndDestroy());
    }

    IEnumerator StopAndDestroy()
    {
        yield return new WaitForSeconds(duration);

        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        Destroy(gameObject, 1f);
    }
}
