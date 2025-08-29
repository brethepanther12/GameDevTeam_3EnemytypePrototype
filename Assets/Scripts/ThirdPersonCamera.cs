using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float height = 2.0f;
    [SerializeField] private float smoothSpeed = 10f; 

    [Header("Rotation Settings")]
    [SerializeField] private int sens = 100;
    [SerializeField] private int lockVertMin = -45, lockVertMax = 45;
    [SerializeField] private bool invertY;
    private float rotX;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float collisionOffset = 0.2f;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        float mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;

        if (invertY)
            rotX += mouseY;
        else
            rotX -= mouseY;

        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);

        transform.localRotation = Quaternion.Euler(rotX, 0, 0);
        target.Rotate(Vector3.up * mouseX);

        Vector3 desiredPosition = target.position - (transform.forward * distance) + (Vector3.up * height);

        Vector3 targetPosition = target.position + (Vector3.up * height);
        Vector3 direction = desiredPosition - targetPosition;
        float desiredDistance = direction.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(targetPosition, direction.normalized, out hit, desiredDistance, collisionLayers))
        {
            transform.position = hit.point + (hit.normal * collisionOffset);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        if (Physics.Raycast(target.position, Vector3.up, out hit, height, collisionLayers))
        {
            Vector3 ceilingAdjustedPos = transform.position;
            float ceilingY = hit.point.y - collisionOffset;
            if (ceilingAdjustedPos.y > ceilingY)
            {
                ceilingAdjustedPos.y = ceilingY;
                transform.position = ceilingAdjustedPos;
            }
        }
    }
}