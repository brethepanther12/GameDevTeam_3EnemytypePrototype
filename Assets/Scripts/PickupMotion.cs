using UnityEngine;

public class PickupMotion : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 45f;

    [Header("Bounce")]
    public float bounceAmplitude = 0.2f;
    public float bounceFrequency = 2f;

    private Vector3 startLocalPos;

    void Start()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float offsetY = Mathf.Sin(Time.time * bounceFrequency) * bounceAmplitude;
        transform.localPosition = startLocalPos + Vector3.up * offsetY;
    }
}
