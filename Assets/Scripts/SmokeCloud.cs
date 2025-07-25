using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeCloud : MonoBehaviour
{
    [SerializeField] private float smokeDuration;
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private int damageAmount;
    [SerializeField] private float damageRate;

    // Track enemies and their DOT coroutines
    private Dictionary<IDamage, Coroutine> damageCoroutines = new Dictionary<IDamage, Coroutine>();
    private List<FlyingAI> enemiesInSmoke = new List<FlyingAI>();

    private void Start()
    {
        Destroy(gameObject, smokeDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            FlyingAI enemy = other.GetComponent<FlyingAI>();
            if (enemy != null && !enemiesInSmoke.Contains(enemy))
            {
                enemiesInSmoke.Add(enemy);
                enemy.SetInvisible(true);
            }

            IDamage dmg = other.GetComponent<IDamage>();
            if (dmg != null && !damageCoroutines.ContainsKey(dmg))
            {
                Coroutine co = StartCoroutine(DamageOverTime(dmg));
                damageCoroutines.Add(dmg, co);
            }
        }

        if (other.CompareTag("Player"))
        {
            Visibility visible = other.GetComponent<Visibility>();
            if (visible != null)
            {
                visible.SetInvisible(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            FlyingAI enemy = other.GetComponent<FlyingAI>();
            if (enemy != null && enemiesInSmoke.Contains(enemy))
            {
                enemiesInSmoke.Remove(enemy);
                enemy.SetInvisible(false);
            }

            IDamage dmg = other.GetComponent<IDamage>();
            if (dmg != null && damageCoroutines.ContainsKey(dmg))
            {
                StopCoroutine(damageCoroutines[dmg]);
                damageCoroutines.Remove(dmg);
            }
        }

        if (other.CompareTag("Player"))
        {
            Visibility visible = other.GetComponent<Visibility>();
            if (visible != null)
            {
                visible.SetInvisible(false);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var enemy in enemiesInSmoke)
        {
            if (enemy != null)
                enemy.SetInvisible(false);
        }
        enemiesInSmoke.Clear();

        // Stop all damage coroutines
        foreach (var co in damageCoroutines.Values)
        {
            if (co != null)
                StopCoroutine(co);
        }
        damageCoroutines.Clear();

        if (gamemanager.instance.player != null)
        {
            Visibility visible = gamemanager.instance.player.GetComponent<Visibility>();
            if (visible != null)
            {
                visible.SetInvisible(false);
            }
        }
    }

    private IEnumerator DamageOverTime(IDamage target)
    {
        while (true)
        {
            target.takeDamage(damageAmount);

            if (smokePrefab != null)
                Instantiate(smokePrefab, target as Component != null ? ((Component)target).transform.position : transform.position, Quaternion.identity);

            yield return new WaitForSeconds(damageRate);
        }
    }
}
