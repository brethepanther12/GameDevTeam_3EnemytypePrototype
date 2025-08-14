using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TripWire : MonoBehaviour
{
    [SerializeField] int damageAmount = 20;
    [SerializeField] bool destroyAfterTrigger = true;

    private LineRenderer lr;
    public Transform postA;
    public Transform postB;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, postA.position);
        lr.SetPosition(1, postB.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Tripwire triggered!");

            playerController player = other.GetComponent<playerController>();
            if (player != null)
            {
                player.takeDamage(damageAmount);
            }

            if (destroyAfterTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}