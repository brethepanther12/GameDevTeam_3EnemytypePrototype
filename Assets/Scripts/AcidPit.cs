using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class AcidPit : MonoBehaviour
{
    [Header("Damage Over Time")]
    [SerializeField] private float damagePerSecond = 20f;
    [SerializeField] private float tickRate = 0.25f; 

    [Header("Slow (optional via Status System)")]
    [SerializeField] private StatusEffectData slowStatus; 
    [SerializeField] private bool refreshSlowEachTick = true;

    [Header("FX (optional)")]
    [SerializeField] private ParticleSystem acidFX;  // bubbling/steam particles
    [SerializeField] private AudioSource loopAudio;  // looping sizzle sound

    private bool playerInside;
    private playerController player;
    private StatusEffectHandler statusHandler;
    private IDamage idamage; 
    private Coroutine dotRoutine;
    private BoxCollider trigger;

    private void Reset()
    {
        trigger = GetComponent<BoxCollider>();
        if (trigger != null) trigger.isTrigger = true;
    }

    private void Awake()
    {
        trigger = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        player = other.GetComponent<playerController>();
        statusHandler = other.GetComponent<StatusEffectHandler>();
        idamage = other.GetComponent<IDamage>(); // may be null; that's fine

        if (acidFX != null) acidFX.Play();
        if (loopAudio != null) loopAudio.Play();

        if (slowStatus != null && statusHandler != null)
            statusHandler.ApplyStatusEffect(slowStatus, idamage);

        if (dotRoutine == null)
            dotRoutine = StartCoroutine(DamageLoop());
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (acidFX != null) acidFX.Stop();
        if (loopAudio != null) loopAudio.Stop();

        if (dotRoutine != null)
        {
            StopCoroutine(dotRoutine);
            dotRoutine = null;
        }
    }
    private IEnumerator DamageLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(tickRate);

        while (playerInside)
        {
            if (player != null)
            {
                int dmg = Mathf.RoundToInt(damagePerSecond * tickRate);
                player.takeDamage(dmg);
            }

            if (refreshSlowEachTick && slowStatus != null && statusHandler != null)
                statusHandler.ApplyStatusEffect(slowStatus, idamage);

            yield return wait;
        }
    }
    private void OnDrawGizmosSelected()
    {
        var box = GetComponent<BoxCollider>();
        if (!box) return;

        Gizmos.color = Color.green;
        // Draw correctly in world-space
        var worldCenter = transform.TransformPoint(box.center);
        var worldSize = Vector3.Scale(box.size, transform.lossyScale);
        Gizmos.DrawWireCube(worldCenter, worldSize);
    }
}