using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ElectricPylonTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 30;
    [SerializeField] private float shockInterval = 1f;

    [Header("Visuals")]
    [SerializeField] private GameObject electricEffect;  
    [SerializeField] private LineRenderer lightningLine;  
    [SerializeField] private float lightningFlashDuration = 0.1f;

    private bool playerInside = false;
    private float shockTimer = 0f;
    private Transform playerTransform;

    private void Start()
    {
        // Get reference to player
        if (gamemanager.instance != null && gamemanager.instance.player != null)
            playerTransform = gamemanager.instance.player.transform;

        if (lightningLine != null)
            lightningLine.enabled = false;
    }

    private void Update()
    {
        if (playerInside)
        {
            shockTimer += Time.deltaTime;
            if (shockTimer >= shockInterval)
            {
                ShockPlayer();
                shockTimer = 0f;
            }
        }
    }

    private void ShockPlayer()
    {
        if (playerTransform == null) return;

        if (electricEffect != null)
            Instantiate(electricEffect, transform.position, Quaternion.identity);

        // Apply damage
        Collider[] hits = Physics.OverlapBox(transform.position, GetComponent<BoxCollider>().size / 2f);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var player = hit.GetComponent<playerController>();
                if (player != null)
                    player.takeDamage(damageAmount);
            }
        }

        // Draw lightning from top of pylon to player's middle
        if (lightningLine != null)
        {
            Vector3 top = transform.position + Vector3.up * 2f;           
            Vector3 playerMid = playerTransform.position + Vector3.up;   

            lightningLine.SetPosition(0, top);
            lightningLine.SetPosition(1, playerMid);

            lightningLine.enabled = true;
            Invoke(nameof(TurnOffLightning), lightningFlashDuration);
        }
    }
    private void TurnOffLightning()
    {
        if (lightningLine != null)
            lightningLine.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + box.center, box.size);
        }
    }
}