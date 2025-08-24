using UnityEngine;

public class PlayerPushback : MonoBehaviour
{
    public CharacterController controller;
    public float pushForce = 5f;
    private Vector3 pushDirection;
    private float pushDuration = 0.2f;
    private float pushTimer;

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
    }

    public void ApplyPush(Vector3 direction)
    {
        pushDirection = direction.normalized * pushForce;
        pushTimer = pushDuration;
    }

    void Update()
    {
        if (pushTimer > 0)
        {
            controller.Move(pushDirection * Time.deltaTime);
            pushTimer -= Time.deltaTime;
        }
    }
}
