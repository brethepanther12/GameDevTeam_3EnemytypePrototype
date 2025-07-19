using UnityEngine;

public class PickupMotion : MonoBehaviour
{
    [Header("Rotation Range")]
    public float minRotationSpeed = 30f;
    public float maxRotationSpeed = 90f;

    [Header("Bounce Range")]
    public float minBounceAmplitude = 0.15f;
    public float maxBounceAmplitude = 0.25f;

    public float minBounceFrequency = 1.5f;
    public float maxBounceFrequency = 3.0f;

    private Vector3 startLocalPos;
    private float rotationSpeed;
    private float bounceAmplitude;
    private float bounceFrequency;
    private float timeOffset;

    void Start()
    {
        startLocalPos = transform.localPosition;

        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        bounceAmplitude = Random.Range(minBounceAmplitude, maxBounceAmplitude);
        bounceFrequency = Random.Range(minBounceFrequency, maxBounceFrequency);
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float offsetY = Mathf.Sin(Time.time * bounceFrequency + timeOffset) * bounceAmplitude;
        transform.localPosition = startLocalPos + Vector3.up * offsetY;
    }
}
