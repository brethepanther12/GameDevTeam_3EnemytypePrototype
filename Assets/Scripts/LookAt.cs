using UnityEngine;

public class LookAt : MonoBehaviour
{

    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
}
