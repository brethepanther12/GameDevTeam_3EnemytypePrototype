using UnityEngine;

public class MineTrap : MonoBehaviour
{
    [SerializeField] int damageAmount = 50;
    [SerializeField] GameObject explosionEffect;
    [SerializeField] bool destroyAfterTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Mine triggered!");

            playerController player = other.GetComponent<playerController>();
            if (player != null)
                player.takeDamage(damageAmount);

            if (explosionEffect != null)
                Instantiate(explosionEffect, transform.position, Quaternion.identity);

            if (destroyAfterTrigger)
                Destroy(gameObject);
        }
    }
}