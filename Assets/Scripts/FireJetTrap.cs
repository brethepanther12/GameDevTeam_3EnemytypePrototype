using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class FireJetTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damagePerTick = 10;    // how much damage per tick
    [SerializeField] private float tickRate = 0.25f;    // how often damage is applied
    [SerializeField] private int burstTicks = 4;        // number of ticks per activation

    [Header("Trigger Settings")]
    [SerializeField] private float activationDelay = 0.5f; // delay before trap fires

    [Header("Effects")]
    [SerializeField] private ParticleSystem flameFX;   
    [SerializeField] private AudioSource flameSound;   
    private bool playerInside = false;
    private playerController player;

    private void Awake()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        player = other.GetComponent<playerController>();

        StartCoroutine(FireTrap());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (flameFX != null) flameFX.Stop();
        if (flameSound != null) flameSound.Stop();
    }

    private IEnumerator FireTrap()
    {
        yield return new WaitForSeconds(activationDelay); // delay before firing

        if (!playerInside) yield break;

        if (flameFX != null) flameFX.Play();
        if (flameSound != null) flameSound.Play();

        for (int i = 0; i < burstTicks; i++)
        {
            if (playerInside && player != null)
                player.takeDamage(damagePerTick);

            yield return new WaitForSeconds(tickRate);
        }

        if (flameFX != null) flameFX.Stop();
        if (flameSound != null) flameSound.Stop();
    }
}